using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Articles;
using WikiRacer.Domain.Languages;

namespace WikiRacer.Infrastructure.Articles;

public sealed class CuratedPlayableArticleSelector(
    IWikipediaArticleClient wikipediaArticleClient,
    IPlayableArticleEligibilityScorer eligibilityScorer) : IPlayableArticleSelector
{
    private static readonly IReadOnlyDictionary<string, PlayableArticleSeed[]> CandidatePools =
        new Dictionary<string, PlayableArticleSeed[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] =
            [
                new("Paris", 10, 9, 10),
                new("New York City", 10, 9, 10),
                new("Albert Einstein", 10, 10, 9),
                new("World War II", 10, 10, 8),
                new("Earth", 10, 10, 10),
                new("Moon", 9, 9, 10),
                new("Lion", 9, 8, 10),
                new("The Beatles", 9, 8, 8),
                new("Basketball", 8, 8, 9),
                new("Pizza", 8, 7, 10),
                new("Internet", 9, 9, 8),
                new("Solar System", 9, 9, 9)
            ],
            ["fr"] =
            [
                new("Paris", 10, 9, 10),
                new("Tour Eiffel", 10, 8, 10),
                new("France", 10, 9, 10),
                new("Londres", 9, 8, 9),
                new("Albert Einstein", 10, 10, 9),
                new("Seconde Guerre mondiale", 10, 10, 8),
                new("Terre", 10, 10, 10),
                new("Lune", 9, 9, 10),
                new("Leonard de Vinci", 9, 8, 8),
                new("Football", 8, 9, 9),
                new("Internet", 9, 9, 8),
                new("Systeme solaire", 9, 9, 9)
            ]
        };

    public async Task<ResolvedArticle?> SelectRandomAsync(
        WikipediaLanguage language,
        IReadOnlyCollection<string> excludedTitles,
        CancellationToken cancellationToken)
    {
        var candidates = BuildWeightedPool(language, excludedTitles);

        foreach (var candidate in candidates)
        {
            var resolved = await wikipediaArticleClient.ResolveAsync(language, candidate.Title, cancellationToken);

            if (resolved is null)
            {
                continue;
            }

            if (excludedTitles.Contains(resolved.Title, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            return resolved;
        }

        return null;
    }

    private IEnumerable<PlayableArticleSeed> BuildWeightedPool(WikipediaLanguage language, IReadOnlyCollection<string> excludedTitles)
    {
        if (!CandidatePools.TryGetValue(language.Value, out var seeds))
        {
            return [];
        }

        var excluded = new HashSet<string>(excludedTitles, StringComparer.OrdinalIgnoreCase);
        var weighted = seeds
            .Where(seed => !excluded.Contains(seed.Title))
            .Select(seed => new WeightedSeed(seed, eligibilityScorer.Score(seed)))
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Seed.Title, StringComparer.Ordinal)
            .ToArray();

        if (weighted.Length == 0)
        {
            return [];
        }

        var shuffled = weighted
            .OrderByDescending(item => Random.Shared.Next(1, item.Score + 1))
            .ThenByDescending(item => item.Score)
            .Select(item => item.Seed)
            .ToArray();

        return shuffled;
    }

    private sealed record WeightedSeed(PlayableArticleSeed Seed, int Score);
}
