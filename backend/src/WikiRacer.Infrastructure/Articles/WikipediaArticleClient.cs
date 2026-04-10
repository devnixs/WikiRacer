using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Articles;
using WikiRacer.Domain.Languages;

namespace WikiRacer.Infrastructure.Articles;

public sealed class WikipediaArticleClient(HttpClient httpClient) : IWikipediaArticleClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<ArticleSearchSuggestion>> SearchAsync(WikipediaLanguage language, string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var url =
            $"https://{language.Value}.wikipedia.org/w/api.php?action=opensearch&search={Uri.EscapeDataString(query)}&limit=10&namespace=0&format=json&origin=*";

        var response = await httpClient.GetFromJsonAsync<object[]>(url, SerializerOptions, cancellationToken);

        if (response is null || response.Length < 4)
        {
            return [];
        }

        var titles = ((JsonElement)response[1]).EnumerateArray().Select(element => element.GetString()).OfType<string>().ToArray();
        var descriptions = ((JsonElement)response[2]).EnumerateArray().Select(element => element.GetString()).ToArray();
        var urls = ((JsonElement)response[3]).EnumerateArray().Select(element => element.GetString()).ToArray();

        var suggestions = new List<ArticleSearchSuggestion>(titles.Length);

        for (var index = 0; index < titles.Length; index++)
        {
            var title = titles[index];

            if (!IsPlayableTitle(title))
            {
                continue;
            }

            var urlValue = index < urls.Length ? urls[index] : null;
            var descriptionValue = index < descriptions.Length ? descriptions[index] : null;
            suggestions.Add(new ArticleSearchSuggestion(
                title,
                title,
                ToCanonicalPath(urlValue, title),
                string.IsNullOrWhiteSpace(descriptionValue) ? null : descriptionValue));
        }

        return suggestions;
    }

    public async Task<ResolvedArticle?> ResolveAsync(WikipediaLanguage language, string title, CancellationToken cancellationToken)
    {
        var url =
            $"https://{language.Value}.wikipedia.org/w/api.php?action=query&format=json&formatversion=2&redirects=1&prop=info&inprop=url&titles={Uri.EscapeDataString(title)}&origin=*";

        using var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<QueryResponse>(SerializerOptions, cancellationToken);
        var page = payload?.Query?.Pages?.FirstOrDefault();

        if (page is null || page.Missing || page.Namespace != 0 || !IsPlayableTitle(page.Title))
        {
            return null;
        }

        return new ResolvedArticle(
            page.Title,
            page.Title,
            ToCanonicalPath(page.CanonicalUrl ?? page.FullUrl, page.Title));
    }

    public async Task<string?> GetArticleHtmlAsync(WikipediaLanguage language, string canonicalTitle, CancellationToken cancellationToken)
    {
        var url =
            $"https://{language.Value}.wikipedia.org/w/rest.php/v1/page/{Uri.EscapeDataString(canonicalTitle.Replace(' ', '_'))}/html";

        using var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static bool IsPlayableTitle(string title)
    {
        return !string.IsNullOrWhiteSpace(title) && !title.Contains(':', StringComparison.Ordinal);
    }

    private static string ToCanonicalPath(string? fullUrl, string title)
    {
        if (Uri.TryCreate(fullUrl, UriKind.Absolute, out var uri))
        {
            return string.IsNullOrWhiteSpace(uri.PathAndQuery) ? $"/wiki/{Uri.EscapeDataString(title.Replace(' ', '_'))}" : uri.PathAndQuery;
        }

        return $"/wiki/{Uri.EscapeDataString(title.Replace(' ', '_'))}";
    }

    private sealed class QueryResponse
    {
        [JsonPropertyName("query")]
        public QueryPayload? Query { get; init; }
    }

    private sealed class QueryPayload
    {
        [JsonPropertyName("pages")]
        public IReadOnlyList<PagePayload>? Pages { get; init; }
    }

    private sealed class PagePayload
    {
        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("ns")]
        public int Namespace { get; init; }

        [JsonPropertyName("missing")]
        public bool Missing { get; init; }

        [JsonPropertyName("canonicalurl")]
        public string? CanonicalUrl { get; init; }

        [JsonPropertyName("fullurl")]
        public string? FullUrl { get; init; }
    }
}
