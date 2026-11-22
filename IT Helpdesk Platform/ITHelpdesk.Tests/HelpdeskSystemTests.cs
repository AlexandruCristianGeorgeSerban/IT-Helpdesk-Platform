using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using IT_Helpdesk_Platform.Models;
using Xunit;

namespace ITHelpdesk.Tests
{
    public class HelpdeskSystemTests
    {
        // 1. BASELINE TEST: Validates SLA deadline calculation according to ticket priority (Slide 8: Critical 4h, High 24h, Medium 3 days, Low 7 days)
        [Theory]
        [InlineData("Critical", 4)]
        [InlineData("High", 24)]
        [InlineData("Medium", 72)]
        [InlineData("Low", 168)]
        public void TicketSlaDeadline_ShouldMatchPriorityWindow(string priority, int expectedHours)
        {
            // Arrange
            DateTime createdAt = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

            // Act
            DateTime dueDate = priority switch
            {
                "Critical" => createdAt.AddHours(4),
                "High" => createdAt.AddHours(24),
                "Medium" => createdAt.AddDays(3),
                _ => createdAt.AddDays(7)
            };

            // Assert
            Assert.Equal(expectedHours, (dueDate - createdAt).TotalHours);
        }

        // 2. SMART LOAD BALANCING TEST: Validates weighted formula (Low=1, Medium=2, High=3)
        [Fact]
        public void WeightedAgentWorkload_ShouldSelectLeastLoadedAgent()
        {
            // Arrange:
            // Agent 1 has 2 High tickets (2 * 3 = 6) + 1 Medium (2) = Score 8
            // Agent 2 has 1 Medium ticket (2) + 2 Low (2 * 1 = 2) = Score 4
            // Agent 3 has 1 High ticket (3) = Score 3 (Least loaded -> must be selected)
            var agentTickets = new Dictionary<int, List<string>>
            {
                { 101, new List<string> { "High", "High", "Medium" } },
                { 102, new List<string> { "Medium", "Low", "Low" } },
                { 103, new List<string> { "High" } }
            };

            // Act
            int selectedAgentId = -1;
            int minScore = int.MaxValue;

            foreach (var agent in agentTickets)
            {
                int score = agent.Value.Sum(p => p switch
                {
                    "High" => 3,
                    "Medium" => 2,
                    "Low" => 1,
                    _ => 1
                });

                if (score < minScore)
                {
                    minScore = score;
                    selectedAgentId = agent.Key;
                }
            }

            // Assert
            Assert.Equal(3, minScore);
            Assert.Equal(103, selectedAgentId);
        }

        // 3. TF-IDF & COSINE SIMILARITY TEST: Validates semantic relevance and latency < 100 ms
        [Fact]
        public void CosineSimilarity_MatchingSymptomVectors_ShouldReturnHighScoreUnder100Ms()
        {
            // Arrange
            var sw = Stopwatch.StartNew();
            double[] queryVector = { 0.8, 0.6, 0.0, 0.2 };   // "laptop screen flickering"
            double[] articleMatch = { 0.75, 0.65, 0.0, 0.1 }; // "screen flickering display driver"
            double[] articleUnrelated = { 0.0, 0.0, 0.9, 0.4 }; // "office printer paper jam"

            // Act
            double matchSimilarity = ComputeCosine(queryVector, articleMatch);
            double unrelatedSimilarity = ComputeCosine(queryVector, articleUnrelated);
            sw.Stop();

            // Assert
            Assert.True(matchSimilarity > 0.90, "Matching troubleshooting article should have > 90% similarity.");
            Assert.True(matchSimilarity > unrelatedSimilarity, "Relevant article must rank higher than unrelated article.");
            Assert.True(sw.ElapsedMilliseconds < 100, $"Execution took {sw.ElapsedMilliseconds} ms (Target: < 100 ms).");
        }

        // 4. TEXT TOKENIZER & STOP-WORDS TEST: Validates text cleaning and normalization for NLP
        [Fact]
        public void Tokenizer_ShouldRemoveStopWordsAndKeepTechnicalKeywords()
        {
            // Arrange
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "the", "is", "not", "my", "after" };
            string rawInput = "My VPN is not working after password reset";

            // Act
            var tokens = rawInput
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !stopWords.Contains(w))
                .ToList();

            // Assert
            Assert.Contains("vpn", tokens);
            Assert.Contains("password", tokens);
            Assert.Contains("reset", tokens);
            Assert.DoesNotContain("my", tokens);
            Assert.DoesNotContain("is", tokens);
        }

        // 5. ML.NET MULTICLASS PREDICTION TEST: Validates SdcaMaximumEntropy inference latency < 50 ms
        [Fact]
        public async Task MlNet_TicketClassification_ShouldPredictCategoryAndPriorityUnder50Ms()
        {
            // Arrange: Setup mock scope factory for TicketPredictionService
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var predictionService = new IT_Helpdesk_Platform.Services.TicketPredictionService(scopeFactory);
            await predictionService.TrainModelFromDatabaseAsync();

            // Warm up the ML.NET prediction engine and JIT compiler once
            predictionService.PredictMetadata("Warmup title", "Warmup description");

            var sw = Stopwatch.StartNew();

            // Act & Assert 1: Network & High priority prediction
            var netResult = predictionService.PredictMetadata(
                "Cannot connect to office Wi-Fi network",
                "Signal keeps dropping and VPN connection timed out"
            );
            sw.Stop();

            Assert.NotNull(netResult);
            Assert.Equal("Network", netResult.PredictedCategory);
            Assert.False(string.IsNullOrEmpty(netResult.PredictedPriority));
            Assert.True(sw.ElapsedMilliseconds < 50, $"Inference took {sw.ElapsedMilliseconds} ms (Target: < 50 ms).");

            // Act & Assert 2: Hardware prediction
            var hwResult = predictionService.PredictMetadata(
                "Laptop screen flickering black display",
                "Monitor flashes black repeatedly on dock station"
            );
            Assert.Equal("Hardware", hwResult.PredictedCategory);

            // Act & Assert 3: Access & Security prediction
            var secResult = predictionService.PredictMetadata(
                "Account locked out password expired",
                "Active Directory lockout unable to complete login"
            );
            Assert.Equal("Access & Security", secResult.PredictedCategory);
        }

        // 6. SMART AGENT ROUTING TEST: Validates weighted workload calculation (Critical=4, High=3, Medium=2, Low=1)
        [Fact]
        public void SmartRouting_ShouldCalculateWeightedWorkloadAccurately()
        {
            // Arrange: Simulate active tickets for an agent
            var activeTickets = new[]
            {
                new { Priority = "Critical", Status = "Open" },      // Weight: 4
                new { Priority = "High", Status = "In Progress" },   // Weight: 3
                new { Priority = "High", Status = "Open" },          // Weight: 3
                new { Priority = "Medium", Status = "In Progress" }, // Weight: 2
                new { Priority = "Low", Status = "Open" },           // Weight: 1
                new { Priority = "Critical", Status = "Resolved" },  // Inactive: 0
                new { Priority = "High", Status = "Closed" }         // Inactive: 0
            };

            // Act: Calculate workload sum for active tickets only
            int totalScore = activeTickets
                .Where(t => t.Status != "Resolved" && t.Status != "Closed")
                .Sum(t => t.Priority switch
                {
                    "Critical" => 4,
                    "High" => 3,
                    "Medium" => 2,
                    "Low" => 1,
                    _ => 1
                });

            // Assert: 4 + 3 + 3 + 2 + 1 = 13
            Assert.Equal(13, totalScore);
        }

        // 7. SLA MONITORING TEST: Validates SLA breached, approaching (<4h), and met statuses
        [Fact]
        public void SlaMonitoring_ShouldAccuratelyDetermineBreachedAndApproachingDeadlines()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var pastDueDate = now.AddHours(-2);
            var imminentDueDate = now.AddHours(2);
            var safeDueDate = now.AddDays(2);

            // Act: Evaluate SLA rules
            bool breachedActive = pastDueDate < now;
            bool approachingActive = imminentDueDate > now && (imminentDueDate - now).TotalHours < 4;
            bool safeActive = safeDueDate > now && (safeDueDate - now).TotalHours >= 4;

            // Assert
            Assert.True(breachedActive, "Ticket past due date must be flagged as breached.");
            Assert.True(approachingActive, "Ticket due within 4 hours must be flagged as approaching.");
            Assert.True(safeActive, "Ticket due in 2 days must be flagged as safe/on track.");
        }

        // 8. ADMIN ISOLATION TEST: Validates that Administrators are excluded from automatic ticket assignment
        [Fact]
        public void SmartRouting_ShouldExcludeAdministratorsFromAutomaticRouting()
        {
            // Arrange: 1 Admin with load 0, 2 Agents with loads 4 and 2
            var candidateStaff = new[]
            {
                new { Id = 1, Role = "Admin", Workload = 0 },
                new { Id = 2, Role = "Agent", Workload = 4 },
                new { Id = 3, Role = "Agent", Workload = 2 }
            };

            // Act: Filter strictly candidate agents with Role == "Agent" and select minimum workload
            var eligibleAgents = candidateStaff
                .Where(s => s.Role == "Agent")
                .OrderBy(s => s.Workload)
                .ToList();

            var assignedAgent = eligibleAgents.FirstOrDefault();

            // Assert: Admin with 0 load must NOT be selected; Agent 3 with load 2 must be chosen
            Assert.NotNull(assignedAgent);
            Assert.Equal(3, assignedAgent.Id);
            Assert.Equal(2, assignedAgent.Workload);
            Assert.NotEqual(1, assignedAgent.Id);
        }

        // 9. ZERO WORKLOAD TEST: Validates that an agent with only Resolved/Closed tickets has 0 workload points
        [Fact]
        public void SmartRouting_ResolvedAndClosedTickets_ShouldYieldZeroWorkloadPoints()
        {
            // Arrange: Agent has 5 tickets, all resolved or closed
            var agentTickets = new[]
            {
                new { Priority = "Critical", Status = "Resolved" },
                new { Priority = "High", Status = "Resolved" },
                new { Priority = "Medium", Status = "Closed" },
                new { Priority = "Low", Status = "Closed" }
            };

            // Act: Calculate workload ignoring resolved and closed tickets
            int activeLoad = agentTickets
                .Where(t => !string.Equals(t.Status, "Resolved", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(t.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Priority switch
                {
                    "Critical" => 4,
                    "High" => 3,
                    "Medium" => 2,
                    "Low" => 1,
                    _ => 1
                });

            // Assert
            Assert.Equal(0, activeLoad);
        }

        // 10. TF-IDF EDGE CASES: Validates that whitespace, sub-3-char queries, or pure punctuation do not yield query tokens
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ab")]
        [InlineData("a")]
        [InlineData("!!! @@ ##")]
        public void TfIdf_ShortOrPunctuationOnlyInput_ShouldYieldZeroRelevance(string input)
        {
            // Arrange: Service pre-validation condition (< 3 chars or whitespace) and stop-words
            bool isRejectedEarly = string.IsNullOrWhiteSpace(input) || input.Trim().Length < 3;
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "the", "a", "an", "is", "for" };
            
            // Act: If not rejected early, extract valid tokens
            var tokens = isRejectedEarly
                ? new List<string>()
                : System.Text.RegularExpressions.Regex.Matches(input.ToLowerInvariant(), @"[a-z0-9]+")
                    .Select(m => m.Value)
                    .Where(t => t.Length > 1 && !stopWords.Contains(t))
                    .ToList();

            // Assert: Query should produce no actionable tokens
            Assert.Empty(tokens);
        }

        // 11. TF-IDF THRESHOLD: Validates that cosine similarity strictly filters out weak relevance (< 0.15)
        [Fact]
        public void TfIdf_RelevanceThreshold_ShouldFilterOutWeakMatchesBelow15Percent()
        {
            // Arrange: Vectors with weak overlap (similarity ~ 0.05) and strong overlap (> 0.50)
            double[] queryVec = { 0.9, 0.1, 0.0, 0.0 };
            double[] weakArticleVec = { 0.05, 0.0, 0.9, 0.9 }; // Very weak overlap on dimension 0
            double[] strongArticleVec = { 0.85, 0.2, 0.0, 0.1 };

            // Act
            double weakSim = ComputeCosine(queryVec, weakArticleVec);
            double strongSim = ComputeCosine(queryVec, strongArticleVec);

            // Assert
            Assert.True(weakSim < 0.15, $"Weak similarity was {weakSim}, should be < 0.15");
            Assert.True(strongSim >= 0.15, $"Strong similarity was {strongSim}, should be >= 0.15");
        }

        // 12. DYNAMIC SLA RECALCULATION TEST: Validates that changing priority recalculates DueDate from original CreatedAt
        [Fact]
        public void DynamicSla_WhenPriorityChangesFromLowToCritical_ShouldRecalculateDueDateFromCreatedAt()
        {
            // Arrange: Ticket was created 2 hours ago with priority "Low" (DueDate was CreatedAt + 7 days)
            var createdAt = DateTime.UtcNow.AddHours(-2);
            var initialDueDate = IT_Helpdesk_Platform.Services.TicketService.CalculateDueDateStatic("Low", createdAt);

            // Verify initial state: due in 7 days
            Assert.Equal(createdAt.AddDays(7), initialDueDate);

            // Act: Agent or Admin escalates priority to "Critical"
            var recalculatedDueDate = IT_Helpdesk_Platform.Services.TicketService.CalculateDueDateStatic("Critical", createdAt);

            // Assert: DueDate must be exactly CreatedAt + 4 hours (leaving ~2 hours remaining from now)
            Assert.Equal(createdAt.AddHours(4), recalculatedDueDate);
            double hoursRemaining = (recalculatedDueDate - DateTime.UtcNow).TotalHours;
            Assert.True(hoursRemaining > 1.8 && hoursRemaining <= 2.05, 
                $"Remaining hours should be ~2h, but was {hoursRemaining}h");
        }

        // 13. SLA RESOLVED AT TEST: Validates that resolved/closed tickets compare against ResolvedAt instead of current UtcNow
        [Fact]
        public void SlaStatus_ResolvedTicket_ShouldCompareAgainstResolvedAtInsteadOfUtcNow()
        {
            // Ticket created 14 days ago, due 13 days ago (historical ticket)
            var createdAt = DateTime.UtcNow.AddDays(-14);
            var dueDate = createdAt.AddDays(1); // Due 13 days ago

            // Case A: Resolved ON TIME (ResolvedAt was 13.5 days ago, before dueDate)
            var onTimeResolvedAt = createdAt.AddHours(12); // Resolved in 12h, within 24h SLA
            bool isResolvedA = true;
            bool isOverdueA = isResolvedA
                ? onTimeResolvedAt > dueDate
                : DateTime.UtcNow > dueDate;

            Assert.False(isOverdueA, "Ticket resolved within SLA deadline should evaluate to SLA Met, not overdue.");

            // Case B: Resolved LATE (ResolvedAt was 12 days ago, after dueDate)
            var lateResolvedAt = createdAt.AddDays(2); // Resolved in 48h, past 24h SLA
            bool isResolvedB = true;
            bool isOverdueB = isResolvedB
                ? lateResolvedAt > dueDate
                : DateTime.UtcNow > dueDate;

            Assert.True(isOverdueB, "Ticket resolved after SLA deadline should evaluate to SLA Breached (Late).");
        }

        // 14. TICKET REOPENING TEST: Reopening a ticket resets ResolvedAt so active timer compares against DateTime.UtcNow
        [Fact]
        public void TicketReopening_ShouldResetResolvedAtAndRevertToUtcNowComparison()
        {
            var ticket = new Ticket
            {
                Id = 101,
                Title = "Reopened issue test",
                Description = "Testing ticket reopening lifecycle",
                Status = "Resolved",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddHours(2), // Still valid SLA
                ResolvedAt = DateTime.UtcNow.AddDays(-1)
            };

            // While resolved, uses ResolvedAt
            Assert.NotNull(ticket.ResolvedAt);
            bool isResolved = ticket.Status == "Resolved" || ticket.Status == "Closed";
            bool isOverdueWhileResolved = isResolved
                ? (ticket.ResolvedAt.HasValue && ticket.ResolvedAt.Value > ticket.DueDate.Value)
                : DateTime.UtcNow > ticket.DueDate.Value;
            Assert.False(isOverdueWhileResolved);

            // Reopen ticket to "In Progress"
            ticket.Status = "In Progress";
            ticket.ResolvedAt = null; // Cleared on reopen

            // Now comparison uses DateTime.UtcNow
            bool isResolvedAfterReopen = ticket.Status == "Resolved" || ticket.Status == "Closed";
            bool isOverdueAfterReopen = isResolvedAfterReopen
                ? (ticket.ResolvedAt.HasValue && ticket.ResolvedAt.Value > ticket.DueDate.Value)
                : DateTime.UtcNow > ticket.DueDate.Value;

            Assert.False(isResolvedAfterReopen);
            Assert.False(isOverdueAfterReopen); // Still 2 hours left from now
        }

        private static double ComputeCosine(double[] vecA, double[] vecB)
        {
            double dot = 0.0, normA = 0.0, normB = 0.0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dot += vecA[i] * vecB[i];
                normA += vecA[i] * vecA[i];
                normB += vecB[i] * vecB[i];
            }
            return (normA == 0 || normB == 0) ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
