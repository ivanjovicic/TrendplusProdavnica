#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Seeding
{
    public sealed partial class DevelopmentDataSeeder
    {
        private sealed record BrandPageContentSeed(
            string BrandSlug,
            string HeroTitle,
            string HeroSubtitle,
            string IntroTitle,
            string IntroText,
            string HeroImageUrl,
            string SeoTitle,
            string SeoDescription,
            IReadOnlyList<FaqItem> Faq,
            IReadOnlyList<MerchBlock> MerchBlocks);

        private sealed record CollectionPageContentSeed(
            string CollectionSlug,
            string HeroTitle,
            string HeroSubtitle,
            string IntroTitle,
            string IntroText,
            string HeroImageUrl,
            string SeoTitle,
            string SeoDescription,
            IReadOnlyList<FaqItem> Faq,
            IReadOnlyList<MerchBlock> MerchBlocks);

        private sealed record StorePageContentSeed(
            string StoreSlug,
            string HeroTitle,
            string HeroSubtitle,
            string IntroTitle,
            string IntroText,
            string HeroImageUrl,
            string SeoTitle,
            string SeoDescription,
            IReadOnlyList<FaqItem> Faq);

        private async Task UpsertHomePageAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var page = await _db.HomePages
                .FirstOrDefaultAsync(entity => entity.Slug == "/", cancellationToken);

            if (page is null)
            {
                page = new HomePage
                {
                    CreatedAtUtc = now
                };
                _db.HomePages.Add(page);
            }

            page.Title = "Trendplus - zenska obuca";
            page.Slug = "/";
            page.IsPublished = true;
            page.PublishedAtUtc = now.AddDays(-1);
            page.Seo = new SeoMetadata
            {
                SeoTitle = "Trendplus - zenska obuca za posao, grad i vikend",
                SeoDescription = "Otkrivajte nove modele: salonke, baletanke, mokasine, patike, cizme, sandale i papuce.",
                CanonicalUrl = "/"
            };
            page.Modules = BuildHomeModules();
            page.UpdatedAtUtc = now;

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertBrandPageContentAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var seeds = GetBrandPageContentSeeds();
            var slugs = seeds.Select(seed => seed.BrandSlug).Distinct().ToArray();
            var brands = await _db.Brands.AsNoTracking()
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);
            var brandIds = brands.Values.ToArray();

            var existing = await _db.BrandPageContents
                .Where(entity => brandIds.Contains(entity.BrandId))
                .ToListAsync(cancellationToken);

            foreach (var seed in seeds)
            {
                if (!brands.TryGetValue(seed.BrandSlug, out var brandId))
                {
                    continue;
                }

                var page = existing.FirstOrDefault(entity => entity.BrandId == brandId);

                if (page is null)
                {
                    page = new BrandPageContent
                    {
                        BrandId = brandId,
                        CreatedAtUtc = now
                    };
                    _db.BrandPageContents.Add(page);
                    existing.Add(page);
                }

                page.IsPublished = true;
                page.HeroTitle = seed.HeroTitle;
                page.HeroSubtitle = seed.HeroSubtitle;
                page.IntroTitle = seed.IntroTitle;
                page.IntroText = seed.IntroText;
                page.SeoText = seed.IntroText;
                page.HeroImageUrl = seed.HeroImageUrl;
                page.Faq = seed.Faq;
                page.MerchBlocks = seed.MerchBlocks;
                page.FeaturedLinks = Array.Empty<FeaturedLink>();
                page.Seo = new SeoMetadata
                {
                    SeoTitle = seed.SeoTitle,
                    SeoDescription = seed.SeoDescription,
                    CanonicalUrl = $"/brend/{seed.BrandSlug}"
                };
                page.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertCollectionPageContentAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var seeds = GetCollectionPageContentSeeds();
            var slugs = seeds.Select(seed => seed.CollectionSlug).Distinct().ToArray();
            var collections = await _db.Collections.AsNoTracking()
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);
            var collectionIds = collections.Values.ToArray();

            var existing = await _db.CollectionPageContents
                .Where(entity => collectionIds.Contains(entity.CollectionId))
                .ToListAsync(cancellationToken);

            foreach (var seed in seeds)
            {
                if (!collections.TryGetValue(seed.CollectionSlug, out var collectionId))
                {
                    continue;
                }

                var page = existing.FirstOrDefault(entity => entity.CollectionId == collectionId);

                if (page is null)
                {
                    page = new CollectionPageContent
                    {
                        CollectionId = collectionId,
                        CreatedAtUtc = now
                    };
                    _db.CollectionPageContents.Add(page);
                    existing.Add(page);
                }

                page.IsPublished = true;
                page.HeroTitle = seed.HeroTitle;
                page.HeroSubtitle = seed.HeroSubtitle;
                page.IntroTitle = seed.IntroTitle;
                page.IntroText = seed.IntroText;
                page.SeoText = seed.IntroText;
                page.HeroImageUrl = seed.HeroImageUrl;
                page.Faq = seed.Faq;
                page.MerchBlocks = seed.MerchBlocks;
                page.FeaturedLinks = Array.Empty<FeaturedLink>();
                page.Seo = new SeoMetadata
                {
                    SeoTitle = seed.SeoTitle,
                    SeoDescription = seed.SeoDescription,
                    CanonicalUrl = $"/kolekcija/{seed.CollectionSlug}"
                };
                page.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertStorePageContentAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var seeds = GetStorePageContentSeeds();
            var slugs = seeds.Select(seed => seed.StoreSlug).Distinct().ToArray();
            var stores = await _db.Stores.AsNoTracking()
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);
            var storeIds = stores.Values.ToArray();

            var existing = await _db.StorePageContents
                .Where(entity => storeIds.Contains(entity.StoreId))
                .ToListAsync(cancellationToken);

            foreach (var seed in seeds)
            {
                if (!stores.TryGetValue(seed.StoreSlug, out var storeId))
                {
                    continue;
                }

                var page = existing.FirstOrDefault(entity => entity.StoreId == storeId);

                if (page is null)
                {
                    page = new StorePageContent
                    {
                        StoreId = storeId,
                        CreatedAtUtc = now
                    };
                    _db.StorePageContents.Add(page);
                    existing.Add(page);
                }

                page.IsPublished = true;
                page.HeroTitle = seed.HeroTitle;
                page.HeroSubtitle = seed.HeroSubtitle;
                page.IntroTitle = seed.IntroTitle;
                page.IntroText = seed.IntroText;
                page.SeoText = seed.IntroText;
                page.HeroImageUrl = seed.HeroImageUrl;
                page.Faq = seed.Faq;
                page.FeaturedLinks = Array.Empty<FeaturedLink>();
                page.MerchBlocks = Array.Empty<MerchBlock>();
                page.Seo = new SeoMetadata
                {
                    SeoTitle = seed.SeoTitle,
                    SeoDescription = seed.SeoDescription,
                    CanonicalUrl = $"/prodavnica/{seed.StoreSlug}"
                };
                page.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertSalePageAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var page = await _db.SalePages
                .FirstOrDefaultAsync(entity => entity.Slug == "akcija", cancellationToken);

            if (page is null)
            {
                page = new SalePage
                {
                    CreatedAtUtc = now
                };
                _db.SalePages.Add(page);
            }

            page.Slug = "akcija";
            page.Title = "Akcija";
            page.Subtitle = "Izdvojeni modeli sa snizenim cenama";
            page.IntroText = "Pronasli smo modele sa odlicnim odnosom cene i kvaliteta. Ponuda se redovno osvezava.";
            page.SeoText = page.IntroText;
            page.HeroImageUrl = "https://cdn.trendplus.demo/pages/sale/hero.jpg";
            page.Faq = new[]
            {
                new FaqItem
                {
                    Question = "Da li su na akciji dostupne sve velicine?",
                    Answer = "Dostupnost zavisi od modela i zaliha. U listingu mozete ukljuciti filter 'Na stanju'."
                },
                new FaqItem
                {
                    Question = "Koliko dugo traje akcija?",
                    Answer = "Trajanje zavisi od dostupnosti modela i planskih kampanja."
                }
            };
            page.IsPublished = true;
            page.Seo = new SeoMetadata
            {
                SeoTitle = "Akcija - Trendplus zenska obuca",
                SeoDescription = "Pogledajte aktuelne modele na popustu: salonke, patike, mokasine, cizme i sandale.",
                CanonicalUrl = "/akcija"
            };
            page.UpdatedAtUtc = now;

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertEditorialLinksAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var seeds = GetEditorialSeeds();
            var articleSlugs = seeds.Select(seed => seed.Slug).Distinct().ToArray();
            var articles = await _db.EditorialArticles.AsNoTracking()
                .Where(entity => articleSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);
            var articleIds = articles.Values.ToArray();

            if (articleIds.Length == 0)
            {
                return;
            }

            var productSlugs = seeds.SelectMany(seed => seed.ProductSlugs).Distinct().ToArray();
            var categorySlugs = seeds.SelectMany(seed => seed.CategorySlugs).Distinct().ToArray();
            var collectionSlugs = seeds.SelectMany(seed => seed.CollectionSlugs).Distinct().ToArray();

            var products = await _db.Products.AsNoTracking()
                .Where(entity => productSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);
            var categories = await _db.Categories.AsNoTracking()
                .Where(entity => categorySlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);
            var collections = await _db.Collections.AsNoTracking()
                .Where(entity => collectionSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, entity => entity.Id, cancellationToken);

            var existingProductLinks = await _db.Set<EditorialArticleProduct>()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .ToListAsync(cancellationToken);
            var existingCategoryLinks = await _db.Set<EditorialArticleCategory>()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .ToListAsync(cancellationToken);
            var existingCollectionLinks = await _db.Set<EditorialArticleCollection>()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .ToListAsync(cancellationToken);

            _db.RemoveRange(existingProductLinks);
            _db.RemoveRange(existingCategoryLinks);
            _db.RemoveRange(existingCollectionLinks);

            foreach (var seed in seeds)
            {
                if (!articles.TryGetValue(seed.Slug, out var articleId))
                {
                    continue;
                }

                var productSortOrder = 1;

                foreach (var productSlug in seed.ProductSlugs.Distinct())
                {
                    if (!products.TryGetValue(productSlug, out var productId))
                    {
                        continue;
                    }

                    _db.Set<EditorialArticleProduct>().Add(new EditorialArticleProduct
                    {
                        EditorialArticleId = articleId,
                        ProductId = productId,
                        SortOrder = productSortOrder++,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }

                foreach (var categorySlug in seed.CategorySlugs.Distinct())
                {
                    if (!categories.TryGetValue(categorySlug, out var categoryId))
                    {
                        continue;
                    }

                    _db.Set<EditorialArticleCategory>().Add(new EditorialArticleCategory
                    {
                        EditorialArticleId = articleId,
                        CategoryId = categoryId,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }

                foreach (var collectionSlug in seed.CollectionSlugs.Distinct())
                {
                    if (!collections.TryGetValue(collectionSlug, out var collectionId))
                    {
                        continue;
                    }

                    _db.Set<EditorialArticleCollection>().Add(new EditorialArticleCollection
                    {
                        EditorialArticleId = articleId,
                        CollectionId = collectionId,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private static HomeModule[] BuildHomeModules()
        {
            return new[]
            {
                new HomeModule
                {
                    Type = "announcementBar",
                    Payload = new
                    {
                        text = "Besplatna dostava za porudzbine preko 9.000 RSD",
                        backgroundColor = "#111111",
                        textColor = "#FFFFFF",
                        callToActionUrl = "/dostava-i-isporuka"
                    }
                },
                new HomeModule
                {
                    Type = "heroSection",
                    Payload = new
                    {
                        title = "Nova sezona, novi modeli",
                        subtitle = "Zenska obuca za posao, grad i vikend u jednom mestu.",
                        imageUrl = "https://cdn.trendplus.demo/home/hero.jpg"
                    }
                },
                new HomeModule
                {
                    Type = "categoryCards",
                    Payload = new
                    {
                        items = new[]
                        {
                            new { name = "Salonke", slug = "salonke", imageUrl = "https://cdn.trendplus.demo/home/categories/salonke.jpg" },
                            new { name = "Baletanke", slug = "baletanke", imageUrl = "https://cdn.trendplus.demo/home/categories/baletanke.jpg" },
                            new { name = "Mokasine", slug = "mokasine", imageUrl = "https://cdn.trendplus.demo/home/categories/mokasine.jpg" },
                            new { name = "Lifestyle patike", slug = "lifestyle", imageUrl = "https://cdn.trendplus.demo/home/categories/lifestyle.jpg" },
                            new { name = "Gleznjace", slug = "gleznjace", imageUrl = "https://cdn.trendplus.demo/home/categories/gleznjace.jpg" },
                            new { name = "Sandale", slug = "sandale", imageUrl = "https://cdn.trendplus.demo/home/categories/sandale.jpg" }
                        }
                    }
                },
                new HomeModule { Type = "newArrivals", Payload = new { title = "Novo u ponudi", limit = 12 } },
                new HomeModule { Type = "featuredCollections", Payload = new { title = "Izdvojene kolekcije", limit = 6 } },
                new HomeModule { Type = "bestsellers", Payload = new { title = "Bestseleri", limit = 12 } },
                new HomeModule { Type = "brandWall", Payload = new { title = "Brendovi koje volite", limit = 8 } },
                new HomeModule
                {
                    Type = "editorialStatement",
                    Payload = new
                    {
                        title = "Trendplus izbor",
                        text = "Biramo modele koji izgledaju odlicno, ali i dalje ostaju udobni kroz ceo dan."
                    }
                },
                new HomeModule { Type = "storeTeaser", Payload = new { title = "Posetite nas u radnjama", storeSlug = "beograd-usce" } },
                new HomeModule
                {
                    Type = "trustItems",
                    Payload = new
                    {
                        items = new[]
                        {
                            new { title = "Brza isporuka", description = "Dostava u roku od 2 do 5 radnih dana." },
                            new { title = "Povrat 14 dana", description = "Jednostavan povrat i zamena velicine." },
                            new { title = "Sigurno placanje", description = "Pouzdani nacini placanja i zastita podataka." }
                        }
                    }
                },
                new HomeModule { Type = "newsletter", Payload = new { title = "Prijavite se na Trendplus novosti", placeholder = "Unesite email adresu" } }
            };
        }

        private static IReadOnlyList<BrandPageContentSeed> GetBrandPageContentSeeds()
        {
            return new[]
            {
                new BrandPageContentSeed(
                    "tamaris",
                    "Tamaris modeli za svaki dan",
                    "Od kancelarije do vecernjeg izlaska u jednom koraku.",
                    "Zasto Tamaris",
                    "Tamaris nudi balans modernog dizajna i udobnosti, pa je idealan izbor za svakodnevne i poslovne kombinacije.",
                    "https://cdn.trendplus.demo/brands/tamaris/hero.jpg",
                    "Tamaris zenska obuca - Trendplus",
                    "Pregled Tamaris modela: salonke, baletanke, mokasine i patike.",
                    new[]
                    {
                        new FaqItem { Question = "Kako odgovaraju Tamaris modeli?", Answer = "Vecina modela odgovara standardnom kalupu." },
                        new FaqItem { Question = "Da li imate Tamaris modele za posao?", Answer = "Da, office selekcija je dostupna kroz salonke i mokasine." }
                    },
                    new[]
                    {
                        new MerchBlock
                        {
                            Title = "Office favorites",
                            Html = "Najprodavaniji Tamaris modeli za poslovni stil.",
                            ProductSlugs = new[] { "tamaris-city-grace-salonke", "tamaris-office-link-mokasine" }
                        }
                    }),
                new BrandPageContentSeed(
                    "ecco",
                    "ECCO premium udobnost",
                    "Modeli za zene koje su dugo na nogama.",
                    "Zasto ECCO",
                    "ECCO kolekcije su poznate po ergonomiji i stabilnosti, uz minimalan i lako nosiv dizajn.",
                    "https://cdn.trendplus.demo/brands/ecco/hero.jpg",
                    "ECCO zenska obuca - Trendplus",
                    "ECCO modeli za udobnost tokom celog dana.",
                    new[]
                    {
                        new FaqItem { Question = "Koji ECCO modeli su najbolji za celodnevno nosenje?", Answer = "Lifestyle patike i mokasine imaju najbolji odnos amortizacije i stabilnosti." },
                        new FaqItem { Question = "Da li ECCO modeli imaju standardan broj?", Answer = "Preporuka je standardan broj, uz proveru vodica za velicine." }
                    },
                    new[]
                    {
                        new MerchBlock
                        {
                            Title = "Comfort selection",
                            Html = "Izdvojeni ECCO modeli za dugacke dane.",
                            ProductSlugs = new[] { "ecco-street-motion-lifestyle", "ecco-milano-mokasine", "ecco-pure-comfort-papuce" }
                        }
                    }),
                new BrandPageContentSeed(
                    "steve-madden",
                    "Steve Madden trend edit",
                    "Hrabar dizajn za moderan gradski stil.",
                    "Zasto Steve Madden",
                    "Ako volite modele sa karakterom, Steve Madden kolekcije donose prepoznatljive linije i statement detalje.",
                    "https://cdn.trendplus.demo/brands/steve-madden/hero.jpg",
                    "Steve Madden zenska obuca - Trendplus",
                    "Steve Madden salonke, patike, gleznjace i sandale.",
                    new[]
                    {
                        new FaqItem { Question = "Da li Steve Madden modeli odgovaraju standardnom kalupu?", Answer = "Uglavnom da, a za modele uskog vrha preporucujemo proveru tabele velicina." },
                        new FaqItem { Question = "Koji modeli su najtrazeniji?", Answer = "Salonke i lifestyle patike su najprodavaniji u tekucoj sezoni." }
                    },
                    new[]
                    {
                        new MerchBlock
                        {
                            Title = "Trend picks",
                            Html = "Najtrazeniji Steve Madden modeli ove sezone.",
                            ProductSlugs = new[] { "steve-madden-bold-point-salonke", "steve-madden-urban-pulse-lifestyle" }
                        }
                    })
            };
        }

        private static IReadOnlyList<CollectionPageContentSeed> GetCollectionPageContentSeeds()
        {
            return new[]
            {
                new CollectionPageContentSeed(
                    "office-edit",
                    "Office Edit",
                    "Modeli koji prate tempo radnog dana.",
                    "Poslovni izbor",
                    "U Office Edit kolekciji nalaze se salonke, mokasine i baletanke koje lako uklapate uz poslovne kombinacije.",
                    "https://cdn.trendplus.demo/collections/office-edit/hero.jpg",
                    "Office Edit kolekcija - Trendplus",
                    "Poslovni modeli zenske obuce: salonke, baletanke i mokasine.",
                    new[]
                    {
                        new FaqItem { Question = "Koji modeli su najbolji za kancelariju?", Answer = "Mokasine i salonke srednje visine su najprakticniji izbor." },
                        new FaqItem { Question = "Da li imate office modele na akciji?", Answer = "Da, deo modela redovno ulazi u akcijsku ponudu." }
                    },
                    new[]
                    {
                        new MerchBlock
                        {
                            Title = "Top office modeli",
                            Html = "Najprodavaniji modeli za poslovni stil.",
                            ProductSlugs = new[] { "tamaris-city-grace-salonke", "ecco-milano-mokasine", "liu-jo-chain-detail-mokasine" }
                        }
                    }),
                new CollectionPageContentSeed(
                    "vikend-stil",
                    "Vikend Stil",
                    "Lezerni modeli za opustene dane.",
                    "Vikend inspiracija",
                    "Od lifestyle patika do laganih sandala i papuca, ova kolekcija je kreirana za udobnost i jednostavno kombinovanje.",
                    "https://cdn.trendplus.demo/collections/vikend-stil/hero.jpg",
                    "Vikend stil kolekcija - Trendplus",
                    "Casual modeli za setnju, putovanje i slobodno vreme.",
                    new[]
                    {
                        new FaqItem { Question = "Da li su modeli iz vikend kolekcije lagani?", Answer = "Da, selekcija je fokusirana na lagane i fleksibilne modele." },
                        new FaqItem { Question = "Koji je najprodavaniji vikend model?", Answer = "Lifestyle patike i sandale su trenutno najtrazenije." }
                    },
                    new[]
                    {
                        new MerchBlock
                        {
                            Title = "Vikend best picks",
                            Html = "Najprakticniji modeli za slobodne dane.",
                            ProductSlugs = new[] { "skechers-metro-glide-lifestyle", "tamaris-daylight-sandale", "ecco-pure-comfort-papuce" }
                        }
                    }),
                new CollectionPageContentSeed(
                    "prolecna-kolekcija",
                    "Prolecna Kolekcija",
                    "Lagan korak za novu sezonu.",
                    "Prolecni modeli",
                    "Izdvojili smo modele koji najbolje rade u prelaznom periodu - od baletanki do sandala i lifestyle patika.",
                    "https://cdn.trendplus.demo/collections/prolecna-kolekcija/hero.jpg",
                    "Prolecna kolekcija - Trendplus",
                    "Sezonska selekcija zenske obuce za prolece.",
                    new[]
                    {
                        new FaqItem { Question = "Da li je prolecna kolekcija dostupna i u radnjama?", Answer = "Da, deo asortimana je dostupan i u nasim radnjama." },
                        new FaqItem { Question = "Koliko cesto se osvezava kolekcija?", Answer = "Kolekcija se dopunjava tokom cele sezone." }
                    },
                    new[]
                    {
                        new MerchBlock
                        {
                            Title = "Prolecni favoriti",
                            Html = "Modeli koji su najtrazeniji u prelaznoj sezoni.",
                            ProductSlugs = new[] { "tamaris-soft-bow-baletanke", "liu-jo-shimmer-strap-sandale", "tamaris-air-lite-lifestyle" }
                        }
                    })
            };
        }

        private static IReadOnlyList<StorePageContentSeed> GetStorePageContentSeeds()
        {
            return new[]
            {
                new StorePageContentSeed(
                    "beograd-usce",
                    "Trendplus Beograd Usce",
                    "Najveci izbor modela na jednoj lokaciji.",
                    "Dobrodosli u Usce",
                    "U radnji Beograd Usce mozete isprobati izdvojene modele iz novih kolekcija i dobiti pomoc pri izboru velicine.",
                    "https://cdn.trendplus.demo/stores/beograd-usce/hero.jpg",
                    "Trendplus Beograd Usce - prodavnica",
                    "Informacije o Trendplus radnji u SC Usce.",
                    new[]
                    {
                        new FaqItem { Question = "Da li je moguce preuzimanje online porudzbine?", Answer = "Da, preuzimanje je dostupno za odabrane modele." }
                    }),
                new StorePageContentSeed(
                    "beograd-knez",
                    "Trendplus Beograd Knez",
                    "Centralna gradska lokacija za premium modele.",
                    "Dobrodosli u Knez",
                    "Radnja u Knez Mihailovoj je fokusirana na elegantne i office modele sa najnovijim sezonskim izborom.",
                    "https://cdn.trendplus.demo/stores/beograd-knez/hero.jpg",
                    "Trendplus Beograd Knez - prodavnica",
                    "Informacije o Trendplus radnji u centru Beograda.",
                    new[]
                    {
                        new FaqItem { Question = "Da li je moguce naruciti model koji nije na stanju u radnji?", Answer = "Da, nas tim moze proveriti dostupnost u drugim radnjama i online." }
                    }),
                new StorePageContentSeed(
                    "novi-sad-promenada",
                    "Trendplus Novi Sad Promenada",
                    "Sezonski i vikend modeli za Novi Sad.",
                    "Dobrodosli u Promenadu",
                    "U Promenadi mozete pronaci lagane lifestyle i sezonske modele, uz pomoc tima pri odabiru odgovarajuce velicine.",
                    "https://cdn.trendplus.demo/stores/novi-sad-promenada/hero.jpg",
                    "Trendplus Novi Sad Promenada - prodavnica",
                    "Informacije o Trendplus radnji u Promenada centru.",
                    new[]
                    {
                        new FaqItem { Question = "Da li u radnji postoje modeli iz online akcije?", Answer = "Deo akcijskih modela je dostupan i u ovoj radnji, u zavisnosti od zaliha." }
                    })
            };
        }
    }
}
