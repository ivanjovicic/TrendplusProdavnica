#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Infrastructure.Search.Models
{
    /// <summary>
    /// OpenSearch index mapping configuration for product search
    /// </summary>
    public static class ProductSearchIndexMapping
    {
        public const string IndexName = "products";
        public const int NumberOfShards = 1;
        public const int NumberOfReplicas = 0;

        /// <summary>
        /// OpenSearch index mapping configuration JSON
        /// </summary>
        public static string GetMappingJson() => @"
{
  ""settings"": {
    ""number_of_shards"": 1,
    ""number_of_replicas"": 0,
    ""analysis"": {
      ""analyzer"": {
        ""default"": {
          ""type"": ""standard"",
          ""stopwords"": ""_english_""
        }
      }
    }
  },
  ""mappings"": {
    ""properties"": {
      ""productId"": {
        ""type"": ""keyword""
      },
      ""slug"": {
        ""type"": ""keyword""
      },
      ""name"": {
        ""type"": ""text"",
        ""analyzer"": ""standard"",
        ""fields"": {
          ""keyword"": {
            ""type"": ""keyword""
          }
        }
      },
      ""brandName"": {
        ""type"": ""text"",
        ""analyzer"": ""standard"",
        ""fields"": {
          ""keyword"": {
            ""type"": ""keyword""
          }
        }
      },
      ""shortDescription"": {
        ""type"": ""text"",
        ""analyzer"": ""standard""
      },
      ""primaryCategory"": {
        ""type"": ""text"",
        ""fields"": {
          ""keyword"": {
            ""type"": ""keyword""
          }
        }
      },
      ""secondaryCategories"": {
        ""type"": ""text"",
        ""fields"": {
          ""keyword"": {
            ""type"": ""keyword""
          }
        }
      },
      ""primaryColorName"": {
        ""type"": ""text"",
        ""fields"": {
          ""keyword"": {
            ""type"": ""keyword""
          }
        }
      },
      ""isNew"": {
        ""type"": ""boolean""
      },
      ""isBestseller"": {
        ""type"": ""boolean""
      },
      ""isOnSale"": {
        ""type"": ""boolean""
      },
      ""minPrice"": {
        ""type"": ""double""
      },
      ""maxPrice"": {
        ""type"": ""double""
      },
      ""availableSizes"": {
        ""type"": ""double""
      },
      ""inStock"": {
        ""type"": ""boolean""
      },
      ""searchKeywords"": {
        ""type"": ""text"",
        ""analyzer"": ""standard""
      },
      ""sortRank"": {
        ""type"": ""integer""
      },
      ""publishedAtUtc"": {
        ""type"": ""date"",
        ""format"": ""strict_date_time_no_millis""
      }
    }
  }
}
";
    }
}
