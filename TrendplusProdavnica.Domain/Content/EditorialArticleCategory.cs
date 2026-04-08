#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Content
{
    public class EditorialArticleCategory : EntityBase
    {
        public long EditorialArticleId { get; set; }
        public long CategoryId { get; set; }
    }
}
