using IT_Helpdesk_Platform.Models.ML;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace IT_Helpdesk_Platform.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IRepositoryWrapper _repository;
        private readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "an", "and", "or", "but", "is", "are", "was", "were", "be", "been", "being",
            "to", "of", "in", "on", "for", "with", "at", "by", "from", "up", "about", "into", "over",
            "after", "my", "i", "me", "we", "our", "you", "your", "it", "its", "this", "that", "these",
            "those", "not", "can", "cannot", "cant", "wont", "please", "help", "issue", "issues",
            "problem", "problems", "error", "errors", "need", "needs", "working", "work", "works",
            "tried", "trying", "having", "have", "has", "had", "also", "just", "get", "getting",
            "when", "how", "what", "where", "which", "who", "whom", "there", "here", "am", "do",
            "does", "did", "doing", "would", "should", "could"
        };

        public RecommendationService(IRepositoryWrapper repository)
        {
            _repository = repository;
        }

        public async Task<List<ArticleRecommendationDto>> GetRecommendedArticlesAsync(string text, int topK = 3)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 3)
                return new List<ArticleRecommendationDto>();

            var articles = await _repository.KnowledgeBase.FindAll().ToListAsync();
            if (!articles.Any())
                return new List<ArticleRecommendationDto>();

            // Index 0 = user input query; Index 1..N = Knowledge Base articles
            // Weight article title 3x so primary keywords dominate over incidental body mentions
            var corpus = new List<string> { text };
            corpus.AddRange(articles.Select(a => $"{a.Title} {a.Title} {a.Title} {a.Category} {a.Content}"));

            var tfidfVectors = ComputeTfIdfVectors(corpus);
            var queryVector = tfidfVectors[0];

            var results = new List<ArticleRecommendationDto>();
            for (int i = 0; i < articles.Count; i++)
            {
                double sim = CalculateCosineSimilarity(queryVector, tfidfVectors[i + 1]);
                if (sim >= 0.15) // Filter out weak/spurious matches (minimum 15% relevance)
                {
                    results.Add(new ArticleRecommendationDto
                    {
                        ArticleId = articles[i].Id,
                        Title = articles[i].Title ?? "Untitled Article",
                        Category = articles[i].Category ?? "General",
                        Similarity = Math.Round(sim * 100, 1),
                        Content = articles[i].Content ?? ""
                    });
                }
            }

            return results.OrderByDescending(r => r.Similarity).Take(topK).ToList();
        }

        public async Task<List<SimilarTicketDto>> GetSimilarResolvedTicketsAsync(int ticketId, int topK = 3, int? requesterId = null)
        {
            var allTickets = await _repository.Ticket.FindAll().ToListAsync();
            var currentTicket = allTickets.FirstOrDefault(t => t.Id == ticketId);
            if (currentTicket == null)
                return new List<SimilarTicketDto>();

            var resolvedTicketsQuery = allTickets
                .Where(t => t.Id != ticketId && (t.Status == "Resolved" || t.Status == "Closed"));

            // If a specific requester is provided (e.g., standard employee), restrict suggestions to their own tickets
            if (requesterId.HasValue)
            {
                resolvedTicketsQuery = resolvedTicketsQuery.Where(t => t.RequesterId == requesterId.Value);
            }

            var resolvedTickets = resolvedTicketsQuery.ToList();

            if (!resolvedTickets.Any())
                return new List<SimilarTicketDto>();

            // Weight ticket titles 2x so primary symptoms dominate
            var corpus = new List<string> { $"{currentTicket.Title} {currentTicket.Title} {currentTicket.Description}" };
            corpus.AddRange(resolvedTickets.Select(t => $"{t.Title} {t.Title} {t.Description}"));

            var tfidfVectors = ComputeTfIdfVectors(corpus);
            var queryVector = tfidfVectors[0];

            var results = new List<SimilarTicketDto>();
            for (int i = 0; i < resolvedTickets.Count; i++)
            {
                double sim = CalculateCosineSimilarity(queryVector, tfidfVectors[i + 1]);
                if (sim >= 0.15) // Filter out weak/spurious matches (minimum 15% relevance)
                {
                    results.Add(new SimilarTicketDto
                    {
                        TicketId = resolvedTickets[i].Id,
                        Title = resolvedTickets[i].Title ?? "Untitled Ticket",
                        Status = resolvedTickets[i].Status ?? "Resolved",
                        Similarity = Math.Round(sim * 100, 1)
                    });
                }
            }

            return results.OrderByDescending(r => r.Similarity).Take(topK).ToList();
        }

        private List<string> TokenizeAndClean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var matches = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9]+");
            return matches
                .Select(m => m.Value)
                .Where(token => token.Length > 1 && !_stopWords.Contains(token))
                .ToList();
        }

        private double[][] ComputeTfIdfVectors(List<string> corpus)
        {
            var tokenizedDocs = corpus.Select(TokenizeAndClean).ToList();
            var vocabulary = tokenizedDocs.SelectMany(d => d).Distinct().ToList();
            int docCount = corpus.Count;
            int vocabSize = vocabulary.Count;

            if (vocabSize == 0)
                return new double[docCount][];

            // IDF calculation: log(1 + N / (1 + df))
            var idf = new double[vocabSize];
            for (int v = 0; v < vocabSize; v++)
            {
                string term = vocabulary[v];
                int docsWithTerm = tokenizedDocs.Count(d => d.Contains(term));
                idf[v] = Math.Log(1.0 + (double)docCount / (1.0 + docsWithTerm));
            }

            // TF-IDF vector computation for each document
            var vectors = new double[docCount][];
            for (int d = 0; d < docCount; d++)
            {
                vectors[d] = new double[vocabSize];
                var docTokens = tokenizedDocs[d];
                if (docTokens.Count == 0) continue;

                for (int v = 0; v < vocabSize; v++)
                {
                    string term = vocabulary[v];
                    int termCount = docTokens.Count(t => t == term);
                    if (termCount > 0)
                    {
                        double tf = (double)termCount / docTokens.Count;
                        vectors[d][v] = tf * idf[v];
                    }
                }
            }

            return vectors;
        }

        private double CalculateCosineSimilarity(double[] vecA, double[] vecB)
        {
            if (vecA == null || vecB == null || vecA.Length == 0 || vecB.Length == 0 || vecA.Length != vecB.Length)
                return 0.0;

            double dotProduct = 0.0, normA = 0.0, normB = 0.0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += vecA[i] * vecA[i];
                normB += vecB[i] * vecB[i];
            }

            if (normA == 0.0 || normB == 0.0)
                return 0.0;

            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
