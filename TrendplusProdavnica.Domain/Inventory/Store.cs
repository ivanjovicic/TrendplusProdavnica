#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Inventory
{
    public class Store : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string? PostalCode { get; set; }
        public string? MallName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? WorkingHoursText { get; set; }
        public string? ShortDescription { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? DirectionsUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoMetadata? Seo { get; set; }

        public IList<StoreInventory> Inventory { get; } = new List<StoreInventory>();
    }
}
