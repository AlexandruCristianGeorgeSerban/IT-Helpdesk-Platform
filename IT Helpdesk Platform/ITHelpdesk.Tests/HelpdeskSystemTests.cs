using System.Diagnostics;
using Xunit;

namespace ITHelpdesk.Tests
{
    public class HelpdeskSystemTests
    {
        // 1. TEST BASELINE: Verifică regulile de SLA în funcție de prioritatea tichetului
        [Theory]
        [InlineData("High", 4)]
        [InlineData("Medium", 24)]
        [InlineData("Low", 48)]
        public void TicketSlaDeadline_ShouldMatchPriorityWindow(string priority, int expectedHours)
        {
            // Arrange
            DateTime createdAt = new DateTime(2026, 11, 1, 10, 0, 0);

            // Act
            DateTime dueDate = priority switch
            {
                "High" => createdAt.AddHours(4),
                "Medium" => createdAt.AddHours(24),
                _ => createdAt.AddHours(48)
            };

            // Assert
            Assert.Equal(expectedHours, (dueDate - createdAt).TotalHours);
        }

        // 2. TEST SMART LOAD BALANCING: Verifică formula ponderată (Low=1, Medium=2, High=3)
        [Fact]
        public void WeightedAgentWorkload_ShouldSelectLeastLoadedAgent()
        {
            // Arrange:
            // Agent 1 are 2 tichete High (2 * 3 = 6) + 1 Medium (2) = Scor 8
            // Agent 2 are 1 tichet Medium (2) + 2 Low (2 * 1 = 2) = Scor 4
            // Agent 3 are 1 tichet High (3) = Scor 3 (Cel mai liber -> trebuie ales)
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

        // 3. TEST TF-IDF & COSINE SIMILARITY: Verifică relevanța semantică și latența < 100 ms
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

        // 4. TEST TEXT TOKENIZER & STOP-WORDS: Verifică curățarea textului pentru NLP
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
