#nullable enable

namespace TrendplusProdavnica.Application.Content.Queries
{
    public record GetBrandPageQuery(string Slug);
    public record GetCollectionPageQuery(string Slug);
    public record GetEditorialArticleQuery(string Slug);
}
