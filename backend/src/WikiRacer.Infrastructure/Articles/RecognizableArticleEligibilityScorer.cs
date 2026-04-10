using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Articles;

namespace WikiRacer.Infrastructure.Articles;

public sealed class RecognizableArticleEligibilityScorer : IPlayableArticleEligibilityScorer
{
    public int Score(PlayableArticleSeed seed)
    {
        return (seed.Recognizability * 4) + (seed.Breadth * 2) + seed.Accessibility;
    }
}
