#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Content
{
    public class NavigationMenuItem : EntityBase
    {
        public long MenuId { get; set; }
        public NavigationMenu? Menu { get; set; }

        public long? ParentId { get; set; }
        public NavigationMenuItem? Parent { get; set; }
        public IList<NavigationMenuItem> Children { get; } = new List<NavigationMenuItem>();

        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Badge { get; set; }
        public bool OpensInNewTab { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
