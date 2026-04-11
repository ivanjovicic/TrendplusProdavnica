#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using TrendplusProdavnica.Application.Merchandising.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Merchandising;

namespace TrendplusProdavnica.Infrastructure.Merchandising
{
    /// <summary>
    /// Evaluator za primenu merchandising pravila na proizvode
    /// </summary>
    public class MerchandisingRuleEvaluator
    {
        /// <summary>
        /// Evaluira primjenjiva pravila i vraća boost score za svaki proizvod
        /// </summary>
        public Dictionary<long, decimal> Evaluate(
            IEnumerable<RuleEvaluationInput> products,
            IEnumerable<MerchandisingRule> rules,
            DateTimeOffset evaluationTimeUtc)
        {
            var result = new Dictionary<long, decimal>();
            var productList = products.ToList();
            var rulesList = rules.Where(r => r.IsValidAtTime(evaluationTimeUtc)).ToList();

            if (!rulesList.Any())
                return result;

            // Grupiraj pravila po tipu za lakšu obradu
            var pinRules = rulesList
                .Where(r => r.RuleType == MerchandisingRuleType.Pin)
                .OrderByDescending(r => r.Priority)
                .ToList();

            var boostRules = rulesList
                .Where(r => r.RuleType == MerchandisingRuleType.Boost)
                .OrderByDescending(r => r.Priority)
                .ToList();

            var demoteRules = rulesList
                .Where(r => r.RuleType == MerchandisingRuleType.Demote)
                .OrderByDescending(r => r.Priority)
                .ToList();

            // Primeni Pin pravila (najviši prioritet)
            foreach (var product in productList)
            {
                var pinRule = pinRules.FirstOrDefault(r => 
                    r.AppliesToProduct(product.ProductId) &&
                    r.AppliesToCategory(product.CategoryId) &&
                    r.AppliesToBrand(product.BrandId));

                if (pinRule != null)
                {
                    // Pin pravila daju fiksnu, veoma visoku vrednost
                    result[product.ProductId] = 10000 + pinRule.Priority;
                    continue;
                }

                // Primeni Boost/Demote pravila ako nema Pin-a
                decimal totalBoost = 0;

                foreach (var rule in boostRules)
                {
                    if (AppliesToProduct(rule, product))
                    {
                        totalBoost += rule.BoostScore;
                    }
                }

                foreach (var rule in demoteRules)
                {
                    if (AppliesToProduct(rule, product))
                    {
                        totalBoost -= rule.BoostScore;
                    }
                }

                if (totalBoost != 0)
                {
                    // Primeni boost kao procenta originalne vrednosti (gornja granica +/- 100%)
                    var adjustedScore = product.CurrentScore * (1 + (totalBoost / 100m));
                    result[product.ProductId] = Math.Max(0, adjustedScore);
                }
            }

            return result;
        }

        /// <summary>
        /// Provjerava da li se pravilo primjenjuje na proizvod prema svim filterima
        /// </summary>
        private bool AppliesToProduct(MerchandisingRule rule, RuleEvaluationInput product)
        {
            // Ako je pravilo specifično za proizvod
            if (rule.ProductId.HasValue)
            {
                return rule.ProductId == product.ProductId;
            }

            // Ako ima filter na kategoriju
            if (rule.CategoryId.HasValue && rule.CategoryId != product.CategoryId)
            {
                return false;
            }

            // Ako ima filter na marku
            if (rule.BrandId.HasValue && rule.BrandId != product.BrandId)
            {
                return false;
            }

            // Ako nije filtriran po proizvodu, primenjuje se na sve koje zadovoljavaju kategoriju/marku filtere
            return !rule.ProductId.HasValue || rule.ProductId == product.ProductId;
        }
    }
}
