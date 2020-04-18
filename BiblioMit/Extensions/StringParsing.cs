using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NCalc;

namespace BiblioMit.Extensions
{
    public static class StringParsing
    {
        public static string ParseAcronym(this string text) =>
            Regex.Replace(text?.ToUpperInvariant().RemoveDiacritics(), @"[^A-Z]", "");
        public static object Evaluate(object num, string operation)
        {
            Expression e = new Expression(($"{num}{operation}"));
            return e.Evaluate();
        }
        public static double? ParseDouble(
            this string text, 
            int? decimalPlaces = null, 
            char? decimalSeparator = null, 
            bool? deleteAfter2ndNegative = null,
            string operation = null)
        {
            if (string.IsNullOrEmpty(text)) return null;
            text = Regex.Replace(text, @"[^\d-\.,]", "");
            if (deleteAfter2ndNegative.HasValue && deleteAfter2ndNegative.Value) 
                text = Regex.Replace(text, @"([^^])\-.*", "$1");
            double? num = null;
            if (decimalSeparator.HasValue)
            {
                text = Regex.Replace(text, @$"[^\d]{decimalSeparator.Value}", "");
                if (string.IsNullOrEmpty(text)) return null;
                var orders = text.Split(decimalSeparator.Value);
                if(orders.Length > 1)
                    num = double.Parse($"{string.Join("", orders.SkipLast(1))}.{orders.Last()}", CultureInfo.InvariantCulture);
                else
                    num = double.Parse($"{string.Join("", orders.First())}", CultureInfo.InvariantCulture);
            }
            else if (decimalPlaces.HasValue)
            {
                text = Regex.Replace(text, @"\D", "");
                if (string.IsNullOrEmpty(text)) return null;
                text = $"{text.Substring(0, text.Length - decimalPlaces.Value)}.{text.Substring(text.Length - decimalPlaces.Value)}";
                num = double.Parse(text, CultureInfo.InvariantCulture);
            }
            else
            {
                //text = Regex.Replace(text, @"([^^])[^\.,].*", "$1");
                text = Regex.Replace(text, @"\.+", ".");
                text = Regex.Replace(text, @",+", ",");
                if (string.IsNullOrEmpty(text)) return null;
                var separators = Regex.Matches(text, @"\D").Select(m => m.Value);
                var orders = Regex.Matches(text, @"[0-9]+").Select(m => m.Value);
                var negative = text.First() == '-';
                num = orders.Count() switch
                {
                    1 => double.Parse(orders.First(), CultureInfo.InvariantCulture),
                    _ => double.Parse($"{orders.First()}.{orders.Last()}", CultureInfo.InvariantCulture)
                };
            }
            if (operation != null)
            {
                return (double)Evaluate(num, operation);
            }
            return num;
        }
        public static int? ParseInt(this string text, bool? deleteAfter2ndNegative = null, string operation = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = Regex.Replace(text, @"[^\d-\.,]", "");
            if (deleteAfter2ndNegative.HasValue && deleteAfter2ndNegative.Value)
                text = Regex.Replace(text, @"([^^])\-.*", "$1");
            else
                text = Regex.Replace(text, @"([^^])\-+", "$1");
            text = Regex.Replace(text, @"[\.,].*", "");
            if (string.IsNullOrEmpty(text)) return null;
            if (operation != null)
            {
                return (int)Evaluate(text, operation);
            }
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
        public static DateTime? ParseDateTime(this string text)
        {
            var formats = new string[] { "yyyyMMdd", "yyyy-MM-dd", "dd-MM-yyyy HH:mm", "dd-MM-yyyy", "dd-MM-yyyy HH:mm'&nbsp;'" };
            var parsed = DateTime.TryParseExact(text, formats, new CultureInfo("es-CL"), DateTimeStyles.None, out DateTime date);
            if (parsed) return date;
            return null;
        }
        public static ProductionType? ParseProductionType(this string text)
        {
            foreach (var tipo in Enum.GetNames(typeof(ProductionType)))
            {
                if (text.Contains(tipo.ToString(CultureInfo.InvariantCulture)
                    .ToUpperInvariant(),
                    StringComparison.Ordinal))
                {
                    var parsed = Enum.TryParse(tipo, out ProductionType production);
                    if (parsed)
                        return production;
                }
            }
            return ProductionType.Unknown;
        }
        public static Item? ParseItem(this string text)
        {
            if (text == null) return null;
            foreach (var tipo in Enum.GetNames(typeof(Item)))
            {
                if (text[0].Equals(
                    tipo.ToString(CultureInfo.InvariantCulture)[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant()))
                {
                    var parsed = Enum.TryParse(tipo, out Item production);
                    if (parsed)
                        return production;
                }
            }
            return null;
        }
        public static DeclarationType? ParseTipo(this string text)
        {
            if (text == null) return null;
            return text[0].Equals("M") ? DeclarationType.RawMaterial : DeclarationType.Production;
        }
    }
}
