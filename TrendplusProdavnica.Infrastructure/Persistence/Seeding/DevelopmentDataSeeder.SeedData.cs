#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Infrastructure.Persistence.Seeding
{
    public sealed partial class DevelopmentDataSeeder
    {
        private sealed record BrandSeed(
            string Name,
            string Slug,
            string ShortDescription,
            string LongDescription,
            string LogoUrl,
            string CoverImageUrl,
            string? WebsiteUrl,
            bool IsFeatured,
            int SortOrder);

        private sealed record CollectionSeed(
            string Name,
            string Slug,
            CollectionType CollectionType,
            string ShortDescription,
            string LongDescription,
            string CoverImageUrl,
            string ThumbnailImageUrl,
            string? BadgeText,
            bool IsFeatured,
            int SortOrder);

        private sealed record ProductSeed
        {
            public string Name { get; init; } = string.Empty;
            public string Slug { get; init; } = string.Empty;
            public string? Subtitle { get; init; }
            public string ShortDescription { get; init; } = string.Empty;
            public string LongDescription { get; init; } = string.Empty;
            public string PrimaryColorName { get; init; } = string.Empty;
            public string BrandSlug { get; init; } = string.Empty;
            public string CategorySlug { get; init; } = string.Empty;
            public string[] AdditionalCategorySlugs { get; init; } = Array.Empty<string>();
            public string StyleTag { get; init; } = string.Empty;
            public string OccasionTag { get; init; } = string.Empty;
            public string SeasonTag { get; init; } = string.Empty;
            public bool IsNew { get; init; }
            public bool IsBestseller { get; init; }
            public int SortRank { get; init; }
            public int PublishedDaysAgo { get; init; }
            public int VariantCount { get; init; } = 4;
            public int MediaCount { get; init; } = 4;
            public string SearchKeywords { get; init; } = string.Empty;
            public string[] CollectionSlugs { get; init; } = Array.Empty<string>();
            public string[] PinnedCollectionSlugs { get; init; } = Array.Empty<string>();
        }

        private sealed record StoreSeed(
            string Name,
            string Slug,
            string City,
            string AddressLine1,
            string? AddressLine2,
            string? PostalCode,
            string? MallName,
            string? Phone,
            string? Email,
            decimal? Latitude,
            decimal? Longitude,
            string WorkingHoursText,
            string ShortDescription,
            string CoverImageUrl,
            string DirectionsUrl,
            int SortOrder);

        private sealed record EditorialSeed
        {
            public string Title { get; init; } = string.Empty;
            public string Slug { get; init; } = string.Empty;
            public string Excerpt { get; init; } = string.Empty;
            public string CoverImageUrl { get; init; } = string.Empty;
            public string Topic { get; init; } = string.Empty;
            public string AuthorName { get; init; } = string.Empty;
            public int ReadingTimeMinutes { get; init; }
            public int PublishedDaysAgo { get; init; }
            public string SeoTitle { get; init; } = string.Empty;
            public string SeoDescription { get; init; } = string.Empty;
            public string[] BodyParagraphs { get; init; } = Array.Empty<string>();
            public string[] ProductSlugs { get; init; } = Array.Empty<string>();
            public string[] CategorySlugs { get; init; } = Array.Empty<string>();
            public string[] CollectionSlugs { get; init; } = Array.Empty<string>();
        }

        private sealed record TrustPageSeed(
            TrustPageKind Kind,
            string Title,
            string Slug,
            string Summary,
            string[] Items,
            string SeoTitle,
            string SeoDescription);

        private static IReadOnlyList<BrandSeed> GetBrandSeeds()
        {
            return new[]
            {
                new BrandSeed(
                    "Tamaris",
                    "tamaris",
                    "Tamaris donosi urbane modele koji spajaju moderan izgled i svakodnevnu udobnost.",
                    "Tamaris je izbor za zene koje zele elegantnu i nosivu obucu za posao, grad i vecernji izlazak. Kolekcije su prepoznatljive po stabilnim djonom i diskretnim detaljima.",
                    "https://cdn.trendplus.demo/brands/tamaris/logo.svg",
                    "https://cdn.trendplus.demo/brands/tamaris/cover.jpg",
                    "https://www.tamaris.com",
                    true,
                    10),
                new BrandSeed(
                    "ECCO",
                    "ecco",
                    "ECCO je sinonim za premium udobnost i dugotrajne materijale u svakom koraku.",
                    "ECCO modeli su namenjeni kupcima koji traze odlicnu ergonomiju i stabilnost, bez odricanja od minimalistickog dizajna. Idealni su za celodnevno nosenje.",
                    "https://cdn.trendplus.demo/brands/ecco/logo.svg",
                    "https://cdn.trendplus.demo/brands/ecco/cover.jpg",
                    "https://global.ecco.com",
                    true,
                    20),
                new BrandSeed(
                    "Skechers",
                    "skechers",
                    "Skechers nudi lagane i fleksibilne modele za aktivan gradski ritam.",
                    "Od lifestyle patika do opustenih modela za vikend, Skechers je prepoznatljiv po mekim uloscima i sportskom duhu. Kolekcije su fokusirane na lako nosenje.",
                    "https://cdn.trendplus.demo/brands/skechers/logo.svg",
                    "https://cdn.trendplus.demo/brands/skechers/cover.jpg",
                    "https://www.skechers.com",
                    true,
                    30),
                new BrandSeed(
                    "Steve Madden",
                    "steve-madden",
                    "Steve Madden donosi statement siluete i trend detalje za odlucan stil.",
                    "Brend je poznat po modernim formama i efektnoj obuci za izlazak, posao i street kombinacije. Dizajn je hrabar, a udobnost prilagodjena celodnevnom nosenju.",
                    "https://cdn.trendplus.demo/brands/steve-madden/logo.svg",
                    "https://cdn.trendplus.demo/brands/steve-madden/cover.jpg",
                    "https://www.stevemadden.com",
                    true,
                    40),
                new BrandSeed(
                    "Liu Jo",
                    "liu-jo",
                    "Liu Jo kombinuje italijansku estetiku i sofisticirane detalje za zenski look.",
                    "Od elegantnih salonki do chic papuca, Liu Jo kolekcije su kreirane za kupce koji zele prepoznatljiv stil sa dozom glamura.",
                    "https://cdn.trendplus.demo/brands/liu-jo/logo.svg",
                    "https://cdn.trendplus.demo/brands/liu-jo/cover.jpg",
                    "https://www.liujo.com",
                    false,
                    50)
            };
        }

        private static IReadOnlyList<CollectionSeed> GetCollectionSeeds()
        {
            return new[]
            {
                new CollectionSeed(
                    "Novo",
                    "novo",
                    CollectionType.Manual,
                    "Najnoviji modeli koji su upravo stigli u Trendplus webshop.",
                    "Izdvojili smo aktuelne novitete kroz razlicite stilove, od elegantnih modela do opustenih vikend kombinacija.",
                    "https://cdn.trendplus.demo/collections/novo/cover.jpg",
                    "https://cdn.trendplus.demo/collections/novo/thumb.jpg",
                    "Novo",
                    true,
                    10),
                new CollectionSeed(
                    "Bestseleri",
                    "bestseleri",
                    CollectionType.Manual,
                    "Najtrazeniji modeli koje kupci najcesce biraju.",
                    "Bestseleri su proizvodi sa proverenom nosivoscu, dobrim fitom i stilom koji se lako uklapa u dnevne i poslovne kombinacije.",
                    "https://cdn.trendplus.demo/collections/bestseleri/cover.jpg",
                    "https://cdn.trendplus.demo/collections/bestseleri/thumb.jpg",
                    "Top izbor",
                    true,
                    20),
                new CollectionSeed(
                    "Office Edit",
                    "office-edit",
                    CollectionType.RuleBased,
                    "Selekcija modela za posao, sastanke i uredske outfite.",
                    "Office Edit spaja udobnost i uredan izgled kroz salonke, mokasine i odabrane baletanke koje lako prate radni dan.",
                    "https://cdn.trendplus.demo/collections/office-edit/cover.jpg",
                    "https://cdn.trendplus.demo/collections/office-edit/thumb.jpg",
                    "Za posao",
                    true,
                    30),
                new CollectionSeed(
                    "Vikend Stil",
                    "vikend-stil",
                    CollectionType.RuleBased,
                    "Lezerna i funkcionalna obuca za opustene dane.",
                    "Za setnju gradom, putovanje ili kafu sa prijateljima, Vikend Stil donosi lagane patike, sandale i papuce u modernim bojama.",
                    "https://cdn.trendplus.demo/collections/vikend-stil/cover.jpg",
                    "https://cdn.trendplus.demo/collections/vikend-stil/thumb.jpg",
                    "Casual",
                    true,
                    40),
                new CollectionSeed(
                    "Prolecna Kolekcija",
                    "prolecna-kolekcija",
                    CollectionType.Seasonal,
                    "Lagani modeli i sveze boje za prelaznu sezonu.",
                    "Prolecna kolekcija okuplja modele koji prate promenljivo vreme: od baletanki i patika do sandala za toplije dane.",
                    "https://cdn.trendplus.demo/collections/prolecna-kolekcija/cover.jpg",
                    "https://cdn.trendplus.demo/collections/prolecna-kolekcija/thumb.jpg",
                    "Prolece",
                    true,
                    50),
                new CollectionSeed(
                    "Minimalisticki Izbor",
                    "minimalisticki-izbor",
                    CollectionType.Manual,
                    "Ciste linije i neutralne nijanse za bezvremenski stil.",
                    "Minimalisticki izbor je namenjen kupcima koji vole jednostavne forme, kvalitetnu izradu i modele koje mogu nositi vise sezona.",
                    "https://cdn.trendplus.demo/collections/minimalisticki-izbor/cover.jpg",
                    "https://cdn.trendplus.demo/collections/minimalisticki-izbor/thumb.jpg",
                    null,
                    false,
                    60)
            };
        }

        private static IReadOnlyList<StoreSeed> GetStoreSeeds()
        {
            return new[]
            {
                new StoreSeed(
                    "Beograd Usce",
                    "beograd-usce",
                    "Beograd",
                    "Bulevar Mihajla Pupina 4",
                    "Lokal A12",
                    "11070",
                    "SC Usce",
                    "+381 11 4000 221",
                    "usce@trendplus.demo",
                    44.819540m,
                    20.441980m,
                    "Pon-Ned 10:00-22:00",
                    "Najveci izbor lifestyle i office modela uz strucan tim za pomoc pri odabiru velicine.",
                    "https://cdn.trendplus.demo/stores/beograd-usce/cover.jpg",
                    "https://maps.google.com/?q=44.819540,20.441980",
                    10),
                new StoreSeed(
                    "Beograd Knez",
                    "beograd-knez",
                    "Beograd",
                    "Knez Mihailova 23",
                    null,
                    "11000",
                    null,
                    "+381 11 3001 155",
                    "knez@trendplus.demo",
                    44.816930m,
                    20.460080m,
                    "Pon-Sub 09:00-21:00, Ned 10:00-18:00",
                    "Centralna gradska radnja sa akcentom na elegantne i premium modele.",
                    "https://cdn.trendplus.demo/stores/beograd-knez/cover.jpg",
                    "https://maps.google.com/?q=44.816930,20.460080",
                    20),
                new StoreSeed(
                    "Novi Sad Promenada",
                    "novi-sad-promenada",
                    "Novi Sad",
                    "Bulevar oslobodjenja 119",
                    "Lokal 1.32",
                    "21000",
                    "Promenada",
                    "+381 21 4800 330",
                    "promenada@trendplus.demo",
                    45.246940m,
                    19.842450m,
                    "Pon-Ned 10:00-22:00",
                    "Savremen izbor sezonskih modela i vikend kolekcija za Novi Sad i okolinu.",
                    "https://cdn.trendplus.demo/stores/novi-sad-promenada/cover.jpg",
                    "https://maps.google.com/?q=45.246940,19.842450",
                    30)
            };
        }

        private static IReadOnlyList<ProductSeed> GetProductSeeds()
        {
            return new[]
            {
                new ProductSeed
                {
                    Name = "Tamaris City Grace Salonke",
                    Slug = "tamaris-city-grace-salonke",
                    Subtitle = "Klasicna salonka sa stabilnom petom",
                    ShortDescription = "Elegantne salonke za posao i vecernje prilike.",
                    LongDescription = "Model sa zatvorenim vrhom i udobnim uloskom, pogodan za celodnevno nosenje u kancelariji i posle posla.",
                    PrimaryColorName = "Crna",
                    BrandSlug = "tamaris",
                    CategorySlug = "salonke",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "minimalisticki",
                    OccasionTag = "posao",
                    SeasonTag = "prolece",
                    IsNew = true,
                    IsBestseller = true,
                    SortRank = 990,
                    PublishedDaysAgo = 6,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "salonke crne posao elegantne",
                    CollectionSlugs = new[] { "office-edit", "minimalisticki-izbor", "prolecna-kolekcija" },
                    PinnedCollectionSlugs = new[] { "office-edit" }
                },
                new ProductSeed
                {
                    Name = "Steve Madden Bold Point Salonke",
                    Slug = "steve-madden-bold-point-salonke",
                    Subtitle = "Silueta koja izduzuje nogu",
                    ShortDescription = "Naglasene salonke za samouverene poslovne i vecernje kombinacije.",
                    LongDescription = "Model sa modernim spicom i laganim djonom koji unosi dozu statement stila, bez gubitka stabilnosti pri hodu.",
                    PrimaryColorName = "Bez",
                    BrandSlug = "steve-madden",
                    CategorySlug = "salonke",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "statement",
                    OccasionTag = "izlazak",
                    SeasonTag = "prolece",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 975,
                    PublishedDaysAgo = 34,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "salonke bez steve madden spic",
                    CollectionSlugs = new[] { "office-edit", "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "Liu Jo Sculpt Chic Salonke",
                    Slug = "liu-jo-sculpt-chic-salonke",
                    Subtitle = "Sofisticiran izgled za posebne prilike",
                    ShortDescription = "Salonke sa finim detaljima i elegantnom linijom.",
                    LongDescription = "Liu Jo model koji spaja glam detalje i ciste linije, idealan za kombinacije uz haljinu, suknju ili klasicne pantalone.",
                    PrimaryColorName = "Bordo",
                    BrandSlug = "liu-jo",
                    CategorySlug = "salonke",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "elegantno",
                    OccasionTag = "svecano",
                    SeasonTag = "prolece",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 960,
                    PublishedDaysAgo = 11,
                    VariantCount = 4,
                    MediaCount = 4,
                    SearchKeywords = "liu jo salonke bordo elegantne",
                    CollectionSlugs = new[] { "office-edit", "prolecna-kolekcija" }
                },
                new ProductSeed
                {
                    Name = "ECCO Comfort Line Baletanke",
                    Slug = "ecco-comfort-line-baletanke",
                    Subtitle = "Mekano gaziste i cista forma",
                    ShortDescription = "Baletanke koje prate stopalo i smanjuju zamor tokom dana.",
                    LongDescription = "Model sa fleksibilnim djonom i ergonomskim uloskom namenjen je svakodnevnim gradskim obavezama i laganim poslovnim kombinacijama.",
                    PrimaryColorName = "Tamno plava",
                    BrandSlug = "ecco",
                    CategorySlug = "baletanke",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "udobnost",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 945,
                    PublishedDaysAgo = 45,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "ecco baletanke udobne plave",
                    CollectionSlugs = new[] { "office-edit", "minimalisticki-izbor", "prolecna-kolekcija" }
                },
                new ProductSeed
                {
                    Name = "Tamaris Soft Bow Baletanke",
                    Slug = "tamaris-soft-bow-baletanke",
                    Subtitle = "Lagani model sa diskretnim detaljem",
                    ShortDescription = "Baletanke koje lako prelaze iz poslovnog u casual stil.",
                    LongDescription = "Klasicna Tamaris silueta sa mekim uloskom i malim dekorativnim detaljem, pogodna za duze setnje po gradu.",
                    PrimaryColorName = "Puder roze",
                    BrandSlug = "tamaris",
                    CategorySlug = "baletanke",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "romanticno",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = true,
                    SortRank = 940,
                    PublishedDaysAgo = 5,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "baletanke puder roze tamaris",
                    CollectionSlugs = new[] { "prolecna-kolekcija", "vikend-stil" },
                    PinnedCollectionSlugs = new[] { "prolecna-kolekcija" }
                },
                new ProductSeed
                {
                    Name = "Skechers Flexi Ballet Baletanke",
                    Slug = "skechers-flexi-ballet-baletanke",
                    Subtitle = "Sporty feel u baletanka formi",
                    ShortDescription = "Baletanke inspirisane patikama za maksimalnu fleksibilnost.",
                    LongDescription = "Model kombinuje laganu konstrukciju i mekano gaziste, pa je odlican izbor za uzurban tempo i dugo hodanje.",
                    PrimaryColorName = "Siva",
                    BrandSlug = "skechers",
                    CategorySlug = "baletanke",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "casual",
                    OccasionTag = "vikend",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 930,
                    PublishedDaysAgo = 8,
                    VariantCount = 4,
                    MediaCount = 3,
                    SearchKeywords = "skechers baletanke sive udobne",
                    CollectionSlugs = new[] { "vikend-stil", "prolecna-kolekcija" }
                },
                new ProductSeed
                {
                    Name = "ECCO Milano Mokasine",
                    Slug = "ecco-milano-mokasine",
                    Subtitle = "Mokasine za pouzdan dnevni ritam",
                    ShortDescription = "Premium mokasine sa fokusom na ergonomiju i stabilnost.",
                    LongDescription = "Model odlikuje meka konstrukcija i cvrst oslonac, pa je idealan za kupce koji traze poslovni model za celodnevno nosenje.",
                    PrimaryColorName = "Tamno braon",
                    BrandSlug = "ecco",
                    CategorySlug = "mokasine",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "klasicno",
                    OccasionTag = "posao",
                    SeasonTag = "jesen-prolece",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 920,
                    PublishedDaysAgo = 52,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "mokasine ecco braon posao",
                    CollectionSlugs = new[] { "office-edit", "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "Tamaris Office Link Mokasine",
                    Slug = "tamaris-office-link-mokasine",
                    Subtitle = "Diskretan detalj za poslovne kombinacije",
                    ShortDescription = "Mokasine koje spajaju uredan izgled i udobno gaziste.",
                    LongDescription = "Lagani model za kancelariju i grad, sa finim gornjim detaljem koji ostaje nenametljiv i lako uklopiv.",
                    PrimaryColorName = "Crna",
                    BrandSlug = "tamaris",
                    CategorySlug = "mokasine",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "office",
                    OccasionTag = "posao",
                    SeasonTag = "prolece",
                    IsNew = false,
                    IsBestseller = false,
                    SortRank = 905,
                    PublishedDaysAgo = 63,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "tamaris mokasine crne office",
                    CollectionSlugs = new[] { "office-edit" }
                },
                new ProductSeed
                {
                    Name = "Liu Jo Chain Detail Mokasine",
                    Slug = "liu-jo-chain-detail-mokasine",
                    Subtitle = "Elegantne mokasine sa modernim akcentom",
                    ShortDescription = "Model za posao i grad sa prepoznatljivim Liu Jo detaljem.",
                    LongDescription = "Mokasine sa suptilnim metalnim akcentom, osmisljene za kombinacije koje traze balans elegancije i prakticnosti.",
                    PrimaryColorName = "Pesak",
                    BrandSlug = "liu-jo",
                    CategorySlug = "mokasine",
                    AdditionalCategorySlugs = new[] { "cipele" },
                    StyleTag = "chic",
                    OccasionTag = "posao",
                    SeasonTag = "prolece",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 900,
                    PublishedDaysAgo = 17,
                    VariantCount = 4,
                    MediaCount = 4,
                    SearchKeywords = "liu jo mokasine pesak elegantne",
                    CollectionSlugs = new[] { "office-edit", "minimalisticki-izbor" },
                    PinnedCollectionSlugs = new[] { "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "Skechers Metro Glide Lifestyle",
                    Slug = "skechers-metro-glide-lifestyle",
                    Subtitle = "Lagana lifestyle patika",
                    ShortDescription = "Patike za gradske obaveze i vikend setnje.",
                    LongDescription = "Skechers model sa mekim uloskom i prozracnim gornjistem nudi udobnost za svaki dan uz moderan sportski izgled.",
                    PrimaryColorName = "Bela",
                    BrandSlug = "skechers",
                    CategorySlug = "lifestyle",
                    AdditionalCategorySlugs = new[] { "patike" },
                    StyleTag = "casual-sport",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece-leto",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 890,
                    PublishedDaysAgo = 27,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "skechers lifestyle bele patike",
                    CollectionSlugs = new[] { "vikend-stil", "prolecna-kolekcija" },
                    PinnedCollectionSlugs = new[] { "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "ECCO Street Motion Lifestyle",
                    Slug = "ecco-street-motion-lifestyle",
                    Subtitle = "Premium lifestyle model",
                    ShortDescription = "Udobne patike za aktivan gradski tempo.",
                    LongDescription = "Street Motion kombinuje stabilnost i elegantnu minimalisticku estetiku, idealnu za kombinacije od farmerki do smart casual izgleda.",
                    PrimaryColorName = "Bela",
                    BrandSlug = "ecco",
                    CategorySlug = "lifestyle",
                    AdditionalCategorySlugs = new[] { "patike" },
                    StyleTag = "minimalisticki",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 880,
                    PublishedDaysAgo = 12,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "ecco lifestyle patike bele",
                    CollectionSlugs = new[] { "vikend-stil", "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "Steve Madden Urban Pulse Lifestyle",
                    Slug = "steve-madden-urban-pulse-lifestyle",
                    Subtitle = "Street inspiracija za svaki dan",
                    ShortDescription = "Lifestyle patike sa modernim linijama i laganim djonom.",
                    LongDescription = "Urban Pulse je model koji daje dinamiku jednostavnim outfitima, uz udobnost potrebnu za duze gradske relacije.",
                    PrimaryColorName = "Bela-crna",
                    BrandSlug = "steve-madden",
                    CategorySlug = "lifestyle",
                    AdditionalCategorySlugs = new[] { "patike" },
                    StyleTag = "street",
                    OccasionTag = "vikend",
                    SeasonTag = "prolece-leto",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 870,
                    PublishedDaysAgo = 39,
                    VariantCount = 4,
                    MediaCount = 3,
                    SearchKeywords = "steve madden lifestyle patike",
                    CollectionSlugs = new[] { "vikend-stil", "prolecna-kolekcija" }
                },
                new ProductSeed
                {
                    Name = "Tamaris Air Lite Lifestyle",
                    Slug = "tamaris-air-lite-lifestyle",
                    Subtitle = "Prozracna patika za duge dane",
                    ShortDescription = "Lagani model koji smanjuje zamor stopala.",
                    LongDescription = "Air Lite je namenjen kupcima koji traze prakticnu patiku za ceo dan, sa diskretnim detaljima i stabilnim osloncem.",
                    PrimaryColorName = "Svetlo siva",
                    BrandSlug = "tamaris",
                    CategorySlug = "lifestyle",
                    AdditionalCategorySlugs = new[] { "patike" },
                    StyleTag = "casual",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = true,
                    SortRank = 860,
                    PublishedDaysAgo = 3,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "tamaris lifestyle patike sive",
                    CollectionSlugs = new[] { "vikend-stil", "prolecna-kolekcija", "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "Steve Madden Chelsea Edge Gleznjace",
                    Slug = "steve-madden-chelsea-edge-gleznjace",
                    Subtitle = "Moderni chelsea model",
                    ShortDescription = "Gleznjace koje daju cvrst karakter svakodnevnim kombinacijama.",
                    LongDescription = "Model sa elasticnim umetkom i stabilnim djonom, pogodan za prelaznu sezonu i gradske setnje.",
                    PrimaryColorName = "Crna",
                    BrandSlug = "steve-madden",
                    CategorySlug = "gleznjace",
                    AdditionalCategorySlugs = new[] { "cizme" },
                    StyleTag = "urban",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "jesen-zima",
                    IsNew = false,
                    IsBestseller = false,
                    SortRank = 850,
                    PublishedDaysAgo = 66,
                    VariantCount = 4,
                    MediaCount = 4,
                    SearchKeywords = "gleznjace crne chelsea steve madden",
                    CollectionSlugs = new[] { "minimalisticki-izbor", "vikend-stil" },
                    PinnedCollectionSlugs = new[] { "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "Tamaris Cozy Block Gleznjace",
                    Slug = "tamaris-cozy-block-gleznjace",
                    Subtitle = "Stabilna peta za svakodnevno nosenje",
                    ShortDescription = "Gleznjace sa blok petom i mekanim uloskom.",
                    LongDescription = "Model koji pruza sigurnost pri hodu i lako se uklapa uz dzins, pantalone i midi suknje tokom hladnijih dana.",
                    PrimaryColorName = "Tamno braon",
                    BrandSlug = "tamaris",
                    CategorySlug = "gleznjace",
                    AdditionalCategorySlugs = new[] { "cizme" },
                    StyleTag = "casual-elegant",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "jesen-zima",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 840,
                    PublishedDaysAgo = 58,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "tamaris gleznjace braon blok peta",
                    CollectionSlugs = new[] { "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "ECCO Nordic Zip Gleznjace",
                    Slug = "ecco-nordic-zip-gleznjace",
                    Subtitle = "Komfor i topla podrska",
                    ShortDescription = "Gleznjace za duze hodanje tokom hladnije sezone.",
                    LongDescription = "Nordic Zip model kombinuje anatomski oblik i stabilan djon, pa je odlican izbor za svakodnevni gradski tempo.",
                    PrimaryColorName = "Tamno siva",
                    BrandSlug = "ecco",
                    CategorySlug = "gleznjace",
                    AdditionalCategorySlugs = new[] { "cizme" },
                    StyleTag = "functional",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "jesen-zima",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 830,
                    PublishedDaysAgo = 19,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "ecco gleznjace sive udobne",
                    CollectionSlugs = new[] { "minimalisticki-izbor", "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "Liu Jo Shimmer Strap Sandale",
                    Slug = "liu-jo-shimmer-strap-sandale",
                    Subtitle = "Elegantne sandale sa sjajnim detaljima",
                    ShortDescription = "Sandale koje podizu vecernji i dnevni look.",
                    LongDescription = "Model sa tankim trakama i stabilnim osloncem donosi sofisticiran izgled uz dovoljno udobnosti za duze nosenje.",
                    PrimaryColorName = "Zlatna",
                    BrandSlug = "liu-jo",
                    CategorySlug = "sandale",
                    StyleTag = "glam",
                    OccasionTag = "izlazak",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = true,
                    SortRank = 820,
                    PublishedDaysAgo = 7,
                    VariantCount = 4,
                    MediaCount = 4,
                    SearchKeywords = "liu jo sandale zlatne",
                    CollectionSlugs = new[] { "prolecna-kolekcija", "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "Tamaris Daylight Sandale",
                    Slug = "tamaris-daylight-sandale",
                    Subtitle = "Lagane sandale za tople dane",
                    ShortDescription = "Model sa mekim uloskom za svakodnevno letnje nosenje.",
                    LongDescription = "Daylight sandale donose udobnost i jednostavan dizajn koji se lako uklapa uz haljine, suknje i lanene pantalone.",
                    PrimaryColorName = "Bez",
                    BrandSlug = "tamaris",
                    CategorySlug = "sandale",
                    StyleTag = "minimalisticki",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 810,
                    PublishedDaysAgo = 10,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "tamaris sandale bez udobne",
                    CollectionSlugs = new[] { "prolecna-kolekcija", "vikend-stil", "minimalisticki-izbor" },
                    PinnedCollectionSlugs = new[] { "prolecna-kolekcija" }
                },
                new ProductSeed
                {
                    Name = "Skechers Breeze Step Sandale",
                    Slug = "skechers-breeze-step-sandale",
                    Subtitle = "Sportski lagan letnji model",
                    ShortDescription = "Sandale za aktivan vikend i gradske setnje.",
                    LongDescription = "Breeze Step kombinuje fleksibilan djon i podesive trake za stabilno i prijatno nosenje tokom toplijih dana.",
                    PrimaryColorName = "Maslinasta",
                    BrandSlug = "skechers",
                    CategorySlug = "sandale",
                    StyleTag = "sport-casual",
                    OccasionTag = "vikend",
                    SeasonTag = "prolece-leto",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 800,
                    PublishedDaysAgo = 31,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "skechers sandale maslinaste",
                    CollectionSlugs = new[] { "prolecna-kolekcija", "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "Steve Madden Resort Sandale",
                    Slug = "steve-madden-resort-sandale",
                    Subtitle = "Trend model za gradski odmor",
                    ShortDescription = "Sandale sa modernim dizajnom i laganom platformom.",
                    LongDescription = "Resort model donosi savremenu siluetu i udobnu osnovu, pa je pogodan i za dnevne i za vecernje letnje kombinacije.",
                    PrimaryColorName = "Crna",
                    BrandSlug = "steve-madden",
                    CategorySlug = "sandale",
                    StyleTag = "trend",
                    OccasionTag = "vikend",
                    SeasonTag = "prolece-leto",
                    IsNew = false,
                    IsBestseller = false,
                    SortRank = 790,
                    PublishedDaysAgo = 48,
                    VariantCount = 4,
                    MediaCount = 3,
                    SearchKeywords = "steve madden sandale crne platforma",
                    CollectionSlugs = new[] { "prolecna-kolekcija", "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "Tamaris Soft Home Papuce",
                    Slug = "tamaris-soft-home-papuce",
                    Subtitle = "Mekane papuce za svaki dan",
                    ShortDescription = "Opustene papuce sa udobnim uloskom za kucu i grad.",
                    LongDescription = "Soft Home model je praktican izbor za toplije dane kada su vam potrebni lagan korak i lako uklapanje uz casual stil.",
                    PrimaryColorName = "Krem",
                    BrandSlug = "tamaris",
                    CategorySlug = "papuce",
                    StyleTag = "casual",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece-leto",
                    IsNew = false,
                    IsBestseller = true,
                    SortRank = 780,
                    PublishedDaysAgo = 56,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "tamaris papuce krem udobne",
                    CollectionSlugs = new[] { "vikend-stil", "minimalisticki-izbor" }
                },
                new ProductSeed
                {
                    Name = "ECCO Pure Comfort Papuce",
                    Slug = "ecco-pure-comfort-papuce",
                    Subtitle = "Premium papuce za dugotrajnu udobnost",
                    ShortDescription = "Ergonomski model za opusten korak tokom celog dana.",
                    LongDescription = "Pure Comfort je dizajniran da pruzi dodatnu podrsku svodu stopala i smanji pritisak pri duzem hodanju.",
                    PrimaryColorName = "Svetlo braon",
                    BrandSlug = "ecco",
                    CategorySlug = "papuce",
                    StyleTag = "comfort",
                    OccasionTag = "svakodnevno",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = false,
                    SortRank = 770,
                    PublishedDaysAgo = 15,
                    VariantCount = 6,
                    MediaCount = 5,
                    SearchKeywords = "ecco papuce udobne svetlo braon",
                    CollectionSlugs = new[] { "vikend-stil", "minimalisticki-izbor" },
                    PinnedCollectionSlugs = new[] { "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "Skechers Cozy Foam Papuce",
                    Slug = "skechers-cozy-foam-papuce",
                    Subtitle = "Mekano gaziste za opustene dane",
                    ShortDescription = "Lagane papuce sa sporty casual izgledom.",
                    LongDescription = "Cozy Foam model je namenjen svakodnevnoj upotrebi i donosi stabilnost, lako odrzavanje i prijatan osecaj pri hodu.",
                    PrimaryColorName = "Siva",
                    BrandSlug = "skechers",
                    CategorySlug = "papuce",
                    StyleTag = "sport-casual",
                    OccasionTag = "vikend",
                    SeasonTag = "prolece-leto",
                    IsNew = false,
                    IsBestseller = false,
                    SortRank = 760,
                    PublishedDaysAgo = 40,
                    VariantCount = 4,
                    MediaCount = 3,
                    SearchKeywords = "skechers papuce sive cozy",
                    CollectionSlugs = new[] { "vikend-stil" }
                },
                new ProductSeed
                {
                    Name = "Liu Jo Lounge Chic Papuce",
                    Slug = "liu-jo-lounge-chic-papuce",
                    Subtitle = "Elegantna papuca sa chic detaljima",
                    ShortDescription = "Model koji spaja opustenost i prepoznatljiv fashion karakter.",
                    LongDescription = "Lounge Chic papuce imaju finu obradu i moderan izgled, pa lako prelaze iz dnevne u vecernju letnju kombinaciju.",
                    PrimaryColorName = "Puder bez",
                    BrandSlug = "liu-jo",
                    CategorySlug = "papuce",
                    StyleTag = "chic",
                    OccasionTag = "vikend",
                    SeasonTag = "prolece-leto",
                    IsNew = true,
                    IsBestseller = true,
                    SortRank = 750,
                    PublishedDaysAgo = 13,
                    VariantCount = 5,
                    MediaCount = 4,
                    SearchKeywords = "liu jo papuce bez chic",
                    CollectionSlugs = new[] { "vikend-stil", "minimalisticki-izbor" }
                }
            };
        }

        private static IReadOnlyList<EditorialSeed> GetEditorialSeeds()
        {
            return new[]
            {
                new EditorialSeed
                {
                    Title = "Kako odabrati udobne patike",
                    Slug = "kako-odabrati-udobne-patike",
                    Excerpt = "Praktican vodic za izbor patika koje ostaju udobne od jutra do veceri.",
                    CoverImageUrl = "https://cdn.trendplus.demo/editorial/kako-odabrati-udobne-patike/cover.jpg",
                    Topic = "Udobnost",
                    AuthorName = "Trendplus redakcija",
                    ReadingTimeMinutes = 5,
                    PublishedDaysAgo = 4,
                    SeoTitle = "Kako odabrati udobne patike - Trendplus editorial",
                    SeoDescription = "Saznajte kako da birate udobne lifestyle patike prema djonu, sirini i nameni.",
                    BodyParagraphs = new[]
                    {
                        "Dobar izbor patika pocinje od osecanja stabilnosti u peti i dovoljno prostora u prednjem delu stopala.",
                        "Za svakodnevno nosenje prednost dajte modelima sa mekim uloskom i fleksibilnim djonom koji prati prirodan korak.",
                        "Ako ste izmedju dve velicine, proverite tabelu velicina i uzmite veci broj kada planirate celodnevno kretanje."
                    },
                    ProductSlugs = new[] { "skechers-metro-glide-lifestyle", "ecco-street-motion-lifestyle", "tamaris-air-lite-lifestyle" },
                    CategorySlugs = new[] { "lifestyle", "patike" },
                    CollectionSlugs = new[] { "vikend-stil", "prolecna-kolekcija" }
                },
                new EditorialSeed
                {
                    Title = "Trendovi za prolece",
                    Slug = "trendovi-za-prolece",
                    Excerpt = "Najvazniji stilovi za prolecnu sezonu i kako da ih uklopite u svoju garderobu.",
                    CoverImageUrl = "https://cdn.trendplus.demo/editorial/trendovi-za-prolece/cover.jpg",
                    Topic = "Trendovi",
                    AuthorName = "Milica Djukic",
                    ReadingTimeMinutes = 6,
                    PublishedDaysAgo = 9,
                    SeoTitle = "Trendovi za prolece - trendplus inspiracija",
                    SeoDescription = "Otkrijte koje sandale, baletanke i lagane patike dominiraju ovog proleca.",
                    BodyParagraphs = new[]
                    {
                        "Prolecni trendovi ove sezone favorizuju svetle tonove, tanke trake i udobne platforme srednje visine.",
                        "Baletanke se vracaju u fokus kroz minimalisticke forme i neutralne boje koje lako prate poslovne i casual kombinacije.",
                        "Ako zelite osvezenje garderobe jednim modelom, birajte obucu koja radi i uz farmerke i uz midi suknju."
                    },
                    ProductSlugs = new[] { "liu-jo-shimmer-strap-sandale", "tamaris-soft-bow-baletanke", "skechers-breeze-step-sandale" },
                    CategorySlugs = new[] { "sandale", "baletanke" },
                    CollectionSlugs = new[] { "prolecna-kolekcija", "novo" }
                },
                new EditorialSeed
                {
                    Title = "Kako nositi baletanke",
                    Slug = "kako-nositi-baletanke",
                    Excerpt = "Tri jednostavna nacina da baletanke izgledaju moderno i van kancelarije.",
                    CoverImageUrl = "https://cdn.trendplus.demo/editorial/kako-nositi-baletanke/cover.jpg",
                    Topic = "Styling",
                    AuthorName = "Ana Stankovic",
                    ReadingTimeMinutes = 4,
                    PublishedDaysAgo = 14,
                    SeoTitle = "Kako nositi baletanke - style vodic",
                    SeoDescription = "Saveti za kombinovanje baletanki uz poslovne, gradske i vikend outfite.",
                    BodyParagraphs = new[]
                    {
                        "Baletanke su najprakticnije kada ih kombinujete sa cropped pantalonama koje otkrivaju clanak i produzuju siluetu.",
                        "Za kancelarijski look dovoljno je dodati strukturirani sako i torbu cvrste forme kako bi kombinacija izgledala uredno.",
                        "U vikend varijanti odaberite model sa diskretnim detaljem i nosite ga uz opusten denim i laganu kosulju."
                    },
                    ProductSlugs = new[] { "ecco-comfort-line-baletanke", "tamaris-soft-bow-baletanke", "skechers-flexi-ballet-baletanke" },
                    CategorySlugs = new[] { "baletanke", "cipele" },
                    CollectionSlugs = new[] { "minimalisticki-izbor", "office-edit" }
                },
                new EditorialSeed
                {
                    Title = "Modeli za posao",
                    Slug = "modeli-za-posao",
                    Excerpt = "Selekcija modela koji izgledaju profesionalno i ostaju udobni tokom celog dana.",
                    CoverImageUrl = "https://cdn.trendplus.demo/editorial/modeli-za-posao/cover.jpg",
                    Topic = "Office",
                    AuthorName = "Trendplus redakcija",
                    ReadingTimeMinutes = 5,
                    PublishedDaysAgo = 18,
                    SeoTitle = "Modeli za posao - obuca za kancelariju",
                    SeoDescription = "Salonke, mokasine i baletanke koje lako prate poslovni dress code.",
                    BodyParagraphs = new[]
                    {
                        "Kada birate obucu za posao, prioritet je stabilan djon i visina pete koja ne opterecuje stopalo tokom smene.",
                        "Mokasine su odlican izbor za dane sa vise kretanja, dok salonke daju formalniji ton sastancima.",
                        "Drzite se neutralnih boja i minimalistickih detalja kako biste modele lako uklopili kroz celu sezonu."
                    },
                    ProductSlugs = new[] { "tamaris-city-grace-salonke", "ecco-milano-mokasine", "liu-jo-chain-detail-mokasine" },
                    CategorySlugs = new[] { "salonke", "mokasine" },
                    CollectionSlugs = new[] { "office-edit", "bestseleri" }
                },
                new EditorialSeed
                {
                    Title = "Kako izabrati obucu za ceo dan",
                    Slug = "kako-izabrati-obucu-za-ceo-dan",
                    Excerpt = "Na sta obratiti paznju kada vam je potrebna pouzdana obuca od jutra do veceri.",
                    CoverImageUrl = "https://cdn.trendplus.demo/editorial/kako-izabrati-obucu-za-ceo-dan/cover.jpg",
                    Topic = "Udobnost",
                    AuthorName = "Jovana Kostic",
                    ReadingTimeMinutes = 7,
                    PublishedDaysAgo = 24,
                    SeoTitle = "Kako izabrati obucu za ceo dan - trendplus saveti",
                    SeoDescription = "Prakticni kriterijumi za izbor obuce kada ste dugo na nogama.",
                    BodyParagraphs = new[]
                    {
                        "Obuca za ceo dan treba da ima dovoljno amortizacije, stabilnu petu i materijal koji dozvoljava stopalu da dise.",
                        "Preporuka je da model probate krajem dana, kada je stopalo prirodno malo sire i tada najrealnije procenjujete fit.",
                        "Ako kombinujete gradsku voznju i duze setnje, birajte modele sa gumiranim djonom i srednjim profilom."
                    },
                    ProductSlugs = new[] { "ecco-pure-comfort-papuce", "skechers-metro-glide-lifestyle", "tamaris-office-link-mokasine" },
                    CategorySlugs = new[] { "lifestyle", "mokasine", "papuce" },
                    CollectionSlugs = new[] { "vikend-stil", "minimalisticki-izbor" }
                }
            };
        }

        private static IReadOnlyList<TrustPageSeed> GetTrustPageSeeds()
        {
            return new[]
            {
                new TrustPageSeed(
                    TrustPageKind.Delivery,
                    "Dostava i isporuka",
                    "dostava-i-isporuka",
                    "Porudzbine saljemo svakog radnog dana uz pracenje posiljke.",
                    new[]
                    {
                        "Standardna dostava je 2 do 5 radnih dana.",
                        "Isporuka je besplatna za porudzbine preko 9.000 RSD.",
                        "SMS i email notifikacije stizu pri svakoj promeni statusa."
                    },
                    "Dostava i isporuka - Trendplus",
                    "Informacije o rokovima, troskovima dostave i pracenju posiljke."),
                new TrustPageSeed(
                    TrustPageKind.Returns,
                    "Povrat i zamena",
                    "povrat-i-zamena",
                    "Ako model ne odgovara, povrat i zamenu mozete pokrenuti brzo i jednostavno.",
                    new[]
                    {
                        "Povrat je moguc u roku od 14 dana od prijema porudzbine.",
                        "Zamena velicine je besplatna jednom po porudzbini.",
                        "Proizvod treba vratiti nekoriscen, u originalnom pakovanju."
                    },
                    "Povrat i zamena - Trendplus",
                    "Saznajte uslove i korake za povrat i zamenu porucenih artikala."),
                new TrustPageSeed(
                    TrustPageKind.SizeGuide,
                    "Vodic za velicine",
                    "vodic-za-velicine",
                    "Koristite tabelu velicina i nase smernice da izaberete odgovarajuci broj.",
                    new[]
                    {
                        "Merenje stopala uradite uvece, na kraju dana.",
                        "Ako ste izmedju dva broja, preporuka je veci broj.",
                        "Za dodatnu pomoc kontaktirajte nas tim za podrsku."
                    },
                    "Vodic za velicine - Trendplus",
                    "Prakticne smernice kako da odaberete idealnu velicinu obuce."),
                new TrustPageSeed(
                    TrustPageKind.Payments,
                    "Nacini placanja",
                    "nacini-placanja",
                    "Omogucili smo vise nacina placanja za sigurnu i jednostavnu kupovinu.",
                    new[]
                    {
                        "Placanje platnim karticama online.",
                        "Placanje pouzecem pri preuzimanju posiljke.",
                        "Placanje preko e-banking opcija podrzanih od strane banke."
                    },
                    "Nacini placanja - Trendplus",
                    "Pregled podrzanih nacina placanja u Trendplus webshop-u."),
                new TrustPageSeed(
                    TrustPageKind.About,
                    "O nama",
                    "o-nama",
                    "Trendplus je specijalizovani webshop za zensku obucu sa fokusom na stil i udobnost.",
                    new[]
                    {
                        "Biramo modele koji odgovaraju svakodnevnom zivotu i poslovnim obavezama.",
                        "Saradujemo sa brendovima koji nude provereni kvalitet.",
                        "Kupcima nudimo jasne informacije i podrsku pre i posle kupovine."
                    },
                    "O nama - Trendplus",
                    "Upoznajte Trendplus i nas pristup odabiru zenske obuce."),
                new TrustPageSeed(
                    TrustPageKind.Contact,
                    "Kontakt",
                    "kontakt",
                    "Tu smo da pomognemo oko izbora modela, velicine i statusa porudzbine.",
                    new[]
                    {
                        "Email: podrska@trendplus.demo",
                        "Telefon: +381 11 7700 880",
                        "Radno vreme podrske: Pon-Pet 09:00-17:00"
                    },
                    "Kontakt - Trendplus",
                    "Kontakt informacije i radno vreme korisnicke podrske Trendplus webshop-a.")
            };
        }
    }
}

