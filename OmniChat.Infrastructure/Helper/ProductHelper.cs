using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Helper
{
    public static class ProductHelper
    {
        public static string GenerateSku(string name, ProductKind kind, double volume)
        {
            if (string.IsNullOrWhiteSpace(name)) return "NA";

            var unsignName = RemoveSign(name);
            var nameAbbr = string.Concat(unsignName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word[0]))
                .ToUpper();

            // 2. Map ProductKind
            var kindAbbr = kind switch
            {
                ProductKind.Sugar => "CD",     
                ProductKind.NoSugar => "KD",    
                ProductKind.Yogurt => "ST",     
                _ => "UN"                       
            };

           
            return $"{nameAbbr}-{kindAbbr}-{volume}";
        }

        private static string RemoveSign(string text)
        {
            var combined = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in combined)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }
    }
}
