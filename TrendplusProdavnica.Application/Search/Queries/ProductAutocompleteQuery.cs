#nullable enable

namespace TrendplusProdavnica.Application.Search.Queries
{
    public record ProductAutocompleteQuery(
        string? QueryText,
        int Limit = 10);
}
