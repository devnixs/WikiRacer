using System.Reflection;
using WikiRacer.Application.Articles;
using WikiRacer.Infrastructure.Articles;

namespace WikiRacer.ArchitectureTests;

public sealed class CuratedPlayableArticleSelectorTests
{
    [Fact]
    public void French_Candidate_Pool_Should_Contain_2000_Distinct_Entries()
    {
        var candidatePools = GetCandidatePools();

        Assert.True(candidatePools.TryGetValue("fr", out var frenchSeeds));
        Assert.NotNull(frenchSeeds);
        Assert.Equal(2000, frenchSeeds.Length);
        Assert.Equal(2000, frenchSeeds.Select(seed => seed.Title).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(frenchSeeds, seed => Assert.False(string.IsNullOrWhiteSpace(seed.Title)));
    }

    [Fact]
    public void French_Candidate_Pool_Should_Use_High_Recognizability_Scores()
    {
        var frenchSeeds = GetCandidatePools()["fr"];

        Assert.All(frenchSeeds, seed =>
        {
            Assert.True(seed.Recognizability >= 10);
            Assert.True(seed.Breadth >= 9);
            Assert.True(seed.Accessibility >= 10);
        });
    }

    private static IReadOnlyDictionary<string, PlayableArticleSeed[]> GetCandidatePools()
    {
        var field = typeof(CuratedPlayableArticleSelector).GetField(
            "CandidatePools",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);

        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, PlayableArticleSeed[]>>(
            field.GetValue(null));
    }
}
