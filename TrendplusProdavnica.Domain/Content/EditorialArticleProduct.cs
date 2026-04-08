#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Content
{
    public class EditorialArticleProduct : EntityBase
    {
        public long EditorialArticleId { get; set; }
        public long ProductId { get; set; }
        public int SortOrder { get; set; }
    }
}
