using IT_Helpdesk_Platform.Models.ML;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace IT_Helpdesk_Platform.Services
{
    public class TicketPredictionService : ITicketPredictionService
    {
        private readonly MLContext _mlContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private ITransformer? _trainedModelCategory;
        private ITransformer? _trainedModelDept;
        private ITransformer? _trainedModelPriority;
        private PredictionEngine<TicketTrainingData, SingleFieldPrediction>? _catEngine;
        private PredictionEngine<TicketTrainingData, SingleFieldPrediction>? _deptEngine;
        private PredictionEngine<TicketTrainingData, SingleFieldPrediction>? _prioEngine;
        private readonly object _lock = new();

        public TicketPredictionService(IServiceScopeFactory scopeFactory)
        {
            _mlContext = new MLContext(seed: 42);
            _scopeFactory = scopeFactory;
        }

        public async Task TrainModelFromDatabaseAsync()
        {
            var trainingList = GetSeedBaselineSamples();

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                    var dbTickets = await repo.Ticket.FindAll().ToListAsync();
                    var categories = (await repo.Category.FindAll().ToListAsync())
                        .ToDictionary(c => c.Id, c => c.Name ?? "General");
                    var departments = (await repo.Department.FindAll().ToListAsync())
                        .ToDictionary(d => d.Id, d => d.Name ?? "IT Support");

                    foreach (var t in dbTickets)
                    {
                        if (categories.TryGetValue(t.CategoryId, out var catName))
                        {
                            string deptName = departments.Values.FirstOrDefault() ?? "IT Support";
                            trainingList.Add(new TicketTrainingData
                            {
                                Title = t.Title ?? "",
                                Description = t.Description ?? "",
                                Category = catName,
                                Dept = deptName,
                                Priority = string.IsNullOrEmpty(t.Priority) ? "Medium" : t.Priority
                            });
                        }
                    }
                }
            }
            catch
            {
                // Fall back gracefully to baseline samples if database is not yet ready
            }

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingList);

            var featurePipeline = _mlContext.Transforms.Text.FeaturizeText("TitleFeats", nameof(TicketTrainingData.Title))
                .Append(_mlContext.Transforms.Text.FeaturizeText("DescFeats", nameof(TicketTrainingData.Description)))
                .Append(_mlContext.Transforms.Concatenate("Features", "TitleFeats", "DescFeats"));

            lock (_lock)
            {
                // 1. Multiclass Classification Model for Category
                var catPipeline = featurePipeline
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TicketTrainingData.Category)))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                _trainedModelCategory = catPipeline.Fit(dataView);

                // 2. Multiclass Classification Model for Department
                var deptPipeline = featurePipeline
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TicketTrainingData.Dept)))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                _trainedModelDept = deptPipeline.Fit(dataView);

                // 3. Multiclass Classification Model for Priority
                var prioPipeline = featurePipeline
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TicketTrainingData.Priority)))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                _trainedModelPriority = prioPipeline.Fit(dataView);

                // Cache prediction engines for high-performance sub-millisecond inference
                _catEngine = _mlContext.Model.CreatePredictionEngine<TicketTrainingData, SingleFieldPrediction>(_trainedModelCategory);
                _deptEngine = _mlContext.Model.CreatePredictionEngine<TicketTrainingData, SingleFieldPrediction>(_trainedModelDept);
                _prioEngine = _mlContext.Model.CreatePredictionEngine<TicketTrainingData, SingleFieldPrediction>(_trainedModelPriority);
            }
        }

        public TicketPredictionOutput PredictMetadata(string title, string desc)
        {
            if (_catEngine == null || _deptEngine == null || _prioEngine == null)
            {
                TrainModelFromDatabaseAsync().GetAwaiter().GetResult();
            }

            var sample = new TicketTrainingData
            {
                Title = title ?? "",
                Description = desc ?? ""
            };

            lock (_lock)
            {
                var catPred = _catEngine!.Predict(sample);
                var deptPred = _deptEngine!.Predict(sample);
                var prioPred = _prioEngine!.Predict(sample);

                double confidence = 85.0;
                if (catPred.Score != null && catPred.Score.Length > 0)
                {
                    float maxScore = catPred.Score.Max();
                    if (maxScore > 0f && maxScore <= 1.0f)
                    {
                        confidence = Math.Round(maxScore * 100.0, 1);
                    }
                    else
                    {
                        double sumExp = catPred.Score.Sum(s => Math.Exp(s));
                        if (sumExp > 0)
                        {
                            confidence = Math.Round((Math.Exp(maxScore) / sumExp) * 100.0, 1);
                        }
                    }
                }
                confidence = Math.Max(70.0, Math.Min(98.5, confidence));

                return new TicketPredictionOutput
                {
                    PredictedCategory = catPred.PredictedValue,
                    PredictedDepartment = deptPred.PredictedValue,
                    PredictedPriority = prioPred.PredictedValue,
                    ConfidencePercentage = confidence,
                    Score = prioPred.Score
                };
            }
        }

        private List<TicketTrainingData> GetSeedBaselineSamples() => new()
        {
            // Network Support
            new() { Title = "VPN connection timed out", Description = "Cannot connect to company VPN after password reset network error", Category = "Network", Dept = "Network Support", Priority = "High" },
            new() { Title = "Wi-Fi dropping signal repeatedly", Description = "Office router internet connection disconnects repeatedly on 5GHz band", Category = "Network", Dept = "Network Support", Priority = "High" },
            new() { Title = "Slow wireless network in conference room", Description = "Internet speed test shows high latency and packet loss across multiple laptops", Category = "Network", Dept = "Network Support", Priority = "Medium" },
            new() { Title = "DNS resolution failed", Description = "Cannot reach internal intranet websites hostname cannot be resolved", Category = "Network", Dept = "Network Support", Priority = "High" },

            // Hardware & Devices
            new() { Title = "Laptop screen flickering and black display", Description = "Monitor display flashes black every few seconds hardware graphics driver issue", Category = "Hardware", Dept = "Hardware & Devices", Priority = "High" },
            new() { Title = "Broken keyboard key needs replacement", Description = "Need replacement external keyboard and mouse for workstation", Category = "Hardware", Dept = "Hardware & Devices", Priority = "Low" },
            new() { Title = "Printer paper jam in tray 2", Description = "Office floor printer is jammed and print spooler queue is blocked with jobs", Category = "Hardware", Dept = "Hardware & Devices", Priority = "Medium" },
            new() { Title = "Dual monitor not detected on HDMI", Description = "Second monitor shows no signal when connected via USB-C docking station", Category = "Hardware", Dept = "Hardware & Devices", Priority = "Medium" },
            new() { Title = "Laptop battery swollen and overheating", Description = "Laptop battery drains in 15 minutes and chassis fan runs at maximum loud noise", Category = "Hardware", Dept = "Hardware & Devices", Priority = "Critical" },

            // Software & Apps
            new() { Title = "Install Visual Studio Enterprise and SQL Server", Description = "Requesting software installation license for development tools and IDE", Category = "Software", Dept = "Software & Apps", Priority = "Medium" },
            new() { Title = "Outlook email not syncing and crashing", Description = "Application crashes when opening attachments in mail client corrupted OST", Category = "Software", Dept = "Software & Apps", Priority = "Medium" },
            new() { Title = "Excel freezes on large macros spreadsheet", Description = "Spreadsheet calculation hangs indefinitely need COM add-ins disabled", Category = "Software", Dept = "Software & Apps", Priority = "Low" },
            new() { Title = "Windows update error 0x80070002", Description = "Cumulative security update installation failed and stuck in reboot loop", Category = "Software", Dept = "Software & Apps", Priority = "High" },

            // Access & Security
            new() { Title = "Account locked out password expired", Description = "Forgot domain credentials cannot log in to workstation active directory lockout", Category = "Access & Security", Dept = "System Administration", Priority = "High" },
            new() { Title = "New employee folder permissions request", Description = "Grant read and write access to shared network drive for HR department member", Category = "Access & Security", Dept = "System Administration", Priority = "Low" },
            new() { Title = "BitLocker recovery key prompt at startup", Description = "Workstation boots into blue screen asking for 48 digit BitLocker recovery key", Category = "Access & Security", Dept = "System Administration", Priority = "Critical" },
            new() { Title = "MFA push notification not received on phone", Description = "Microsoft Authenticator prompt does not show up unable to complete two factor login", Category = "Access & Security", Dept = "System Administration", Priority = "High" },

            // General Inquiry
            new() { Title = "Question about IT equipment return policy", Description = "General question on how to return old monitor when working remotely", Category = "General Inquiry", Dept = "IT Support", Priority = "Low" },
            new() { Title = "Requesting IT setup for summer intern", Description = "Inquiry regarding standard workstation bundle and onboarding schedule", Category = "General Inquiry", Dept = "IT Support", Priority = "Low" }
        };
    }
}
