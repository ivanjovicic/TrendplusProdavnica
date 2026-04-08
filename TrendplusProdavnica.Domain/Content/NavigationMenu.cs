#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Content
{
    public class NavigationMenu : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public MenuLocation Location { get; set; }
        public bool IsActive { get; set; } = true;

        public IList<NavigationMenuItem> Items { get; } = new List<NavigationMenuItem>();
    }
}
