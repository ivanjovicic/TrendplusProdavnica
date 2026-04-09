#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TrendplusProdavnica.Application.Admin.Common;

namespace TrendplusProdavnica.Infrastructure.Admin.Common
{
    internal static class AdminValidationHelper
    {
        private static readonly Regex SlugRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

        public static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();

        public static bool IsValidSlug(string slug) => SlugRegex.IsMatch(slug);

        public static bool IsValidAbsoluteUrl(string value)
            => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        public static bool IsValidEmail(string value)
        {
            var attr = new EmailAddressAttribute();
            return attr.IsValid(value);
        }

        public static bool IsValidLatitude(decimal latitude) => latitude >= -90m && latitude <= 90m;
        public static bool IsValidLongitude(decimal longitude) => longitude >= -180m && longitude <= 180m;

        public static void ThrowIfAny(IDictionary<string, string[]> errors, string message)
        {
            if (errors.Count > 0)
            {
                throw new AdminValidationException(message, errors);
            }
        }

        public static void AddError(IDictionary<string, string[]> errors, string field, string message)
        {
            if (errors.TryGetValue(field, out var existing))
            {
                var merged = new string[existing.Length + 1];
                existing.CopyTo(merged, 0);
                merged[^1] = message;
                errors[field] = merged;
                return;
            }

            errors[field] = new[] { message };
        }
    }
}
