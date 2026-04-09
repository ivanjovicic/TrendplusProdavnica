#nullable enable
using System;
using System.Linq;
using System.Text.Json;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Admin.Common
{
    internal static class AdminMappingHelper
    {
        public static SeoAdminDto? ToSeoDto(SeoMetadata? seo)
        {
            if (seo is null)
            {
                return null;
            }

            return new SeoAdminDto(
                seo.SeoTitle,
                seo.SeoDescription,
                seo.CanonicalUrl,
                seo.RobotsDirective,
                seo.OgTitle,
                seo.OgDescription,
                seo.OgImageUrl,
                seo.StructuredDataOverrideJson);
        }

        public static SeoMetadata? ToSeoModel(SeoAdminDto? seo)
        {
            if (seo is null)
            {
                return null;
            }

            return new SeoMetadata
            {
                SeoTitle = seo.SeoTitle,
                SeoDescription = seo.SeoDescription,
                CanonicalUrl = seo.CanonicalUrl,
                RobotsDirective = seo.RobotsDirective,
                OgTitle = seo.OgTitle,
                OgDescription = seo.OgDescription,
                OgImageUrl = seo.OgImageUrl,
                StructuredDataOverrideJson = seo.StructuredDataOverrideJson
            };
        }

        public static FaqItemAdminDto[]? ToFaqDtos(System.Collections.Generic.IEnumerable<FaqItem>? items)
            => items?.Select(item => new FaqItemAdminDto(item.Question, item.Answer)).ToArray();

        public static FeaturedLinkAdminDto[]? ToFeaturedLinkDtos(System.Collections.Generic.IEnumerable<FeaturedLink>? items)
            => items?.Select(item => new FeaturedLinkAdminDto(item.Title, item.Url, item.ImageUrl)).ToArray();

        public static MerchBlockAdminDto[]? ToMerchBlockDtos(System.Collections.Generic.IEnumerable<MerchBlock>? items)
            => items?.Select(item => new MerchBlockAdminDto(item.Title, item.Html, item.ProductSlugs?.ToArray())).ToArray();

        public static FaqItem[]? ToFaqModels(FaqItemAdminDto[]? items)
            => items?.Select(item => new FaqItem
            {
                Question = item.Question,
                Answer = item.Answer
            }).ToArray();

        public static FeaturedLink[]? ToFeaturedLinkModels(FeaturedLinkAdminDto[]? items)
            => items?.Select(item => new FeaturedLink
            {
                Title = item.Title,
                Url = item.Url,
                ImageUrl = item.ImageUrl
            }).ToArray();

        public static MerchBlock[]? ToMerchBlockModels(MerchBlockAdminDto[]? items)
            => items?.Select(item => new MerchBlock
            {
                Title = item.Title,
                Html = item.Html,
                ProductSlugs = item.ProductSlugs
            }).ToArray();

        public static HomeModuleAdminDto[] ToHomeModuleDtos(System.Collections.Generic.IEnumerable<HomeModule>? modules)
        {
            if (modules is null)
            {
                return Array.Empty<HomeModuleAdminDto>();
            }

            return modules
                .Select(module => new HomeModuleAdminDto(
                    module.Type,
                    module.Payload is null
                        ? null
                        : JsonSerializer.SerializeToElement(module.Payload)))
                .ToArray();
        }

        public static HomeModule[] ToHomeModuleModels(HomeModuleAdminDto[] modules)
        {
            return modules
                .Select(module => new HomeModule
                {
                    Type = module.Type,
                    Payload = module.Payload.HasValue
                        ? JsonSerializer.Deserialize<object>(module.Payload.Value.GetRawText())
                        : null
                })
                .ToArray();
        }
    }
}
