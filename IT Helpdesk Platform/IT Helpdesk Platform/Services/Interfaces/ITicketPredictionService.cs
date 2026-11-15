using IT_Helpdesk_Platform.Models.ML;

namespace IT_Helpdesk_Platform.Services.Interfaces
{
    public interface ITicketPredictionService
    {
        TicketPredictionOutput PredictMetadata(string title, string desc);
        Task TrainModelFromDatabaseAsync();
    }
}

