#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Content
{
    public class EditorialArticleBrand : EntityBase
    {
        public long EditorialArticleId { get; set; }
        public long BrandId { get; set; }
    }
}
