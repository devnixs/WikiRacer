using WikiRacer.Application.Articles;

namespace WikiRacer.Application.Abstractions.Articles;

public interface IPlayableArticleEligibilityScorer
{
    int Score(PlayableArticleSeed seed);
}
