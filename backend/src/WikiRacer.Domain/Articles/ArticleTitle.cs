using WikiRacer.Domain.Common;

namespace WikiRacer.Domain.Articles;

public readonly record struct ArticleTitle
{
    public ArticleTitle(string value)
    {
        Value = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(ArticleTitle title) => title.Value;
}
