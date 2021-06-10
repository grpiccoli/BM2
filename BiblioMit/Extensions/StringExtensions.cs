using HtmlAgilityPack;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BiblioMit.Extensions
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToLower(propertyName);
        }
    }
    public static class StringExtensions
    {
        private readonly static List<string> romanNumerals = 
            new()
            { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        private readonly static List<int> numerals = 
            new()
            { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        private const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string CheckSRI(this string local, Uri url)
        {
            var path = Directory.GetCurrentDirectory();
            var file = Path.Combine(path, "wwwroot", local);
            using FileStream fileStream = File.OpenRead(file);
            using var sha = SHA384.Create();
            var localHash = sha.ComputeHash(fileStream);
            var req = WebRequest.Create(url);
            Stream urlStream = req.GetResponse().GetResponseStream();
            var urlHash = sha.ComputeHash(urlStream);
            if (urlHash == localHash) return Convert.ToBase64String(localHash);
            //Console.WriteLine("Error: {0}", $"local and source hash differ in file {local}");
            return null;
        }
        public static string HtmlToPlainText(this string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);
            var text = html;
            //Decode html specific characters
            text = WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);
            text = Regex.Replace(text, @"<[^>]+>|&nbsp;", "").Trim();
            text = Regex.Replace(text, @"\s{2,}", " ");
            text = Regex.Replace(text, @">", "");
            return text;
        }
        public static string GetColumn(this int index)
        {
            var value = string.Empty;
            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];
            value += letters[index % letters.Length];
            return value;
        }
        public static int GetColumn(this string col) =>
            col.Select((c, i) => 
            (letters.IndexOf(c, StringComparison.Ordinal) + 1) * letters.Length ^ (col.Length - i - 1) )
                .Sum();
        public static int Cell2Row(this string cell) => cell.ParseInt() ?? 0;
        public static string AddColumnRow(this string cell, int columns = 0, int rows = 0)
        {
            if (cell == null) return null;
            int startIndex = cell.IndexOfAny("0123456789".ToCharArray());
            string column = cell.Substring(0, startIndex);
            var rowParsed = int.TryParse(cell[startIndex..], out int row);
            return rowParsed ? $"{(column.GetColumn() + columns).GetColumn()}{row + rows}" : null;
        }
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(str))
                //throw new ArgumentException("El texto a buscar no puede estar vacío", nameof(value));
                return null;
            List<int> indexes = new();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        public static string ToRomanNumeral(this int number)
        {
            var romanNumeral = string.Empty;
            while (number > 0)
            {
                // find biggest numeral that is less than equal to number
                var index = numerals.FindIndex(x => x <= number);
                // subtract it's value from your number
                number -= numerals[index];
                // tack it onto the end of your roman numeral
                romanNumeral += romanNumerals[index];
            }
            return romanNumeral;
        }
        public static string GetSeason(this DateTime date) =>
            ((int)(date.Month + date.Day / 100 - 0.21) / 3) switch
            {
                1 => "Autumn",
                2 => "Winter",
                3 => "Spring",
                _ => "Summer"
            };
        public static string TranslateText(
            this string input,
            string targetLanguage,
            string languagePair)
        {
            //return Task.Run(() =>  
            //{
            //    string url = String.Format("http://www.google.com/translate_t?hl={0}&ie=UTF8&text={1}&langpair={2}", targetLanguage, input, languagePair);
            //    HttpClient hc = new HttpClient();
            //    HttpResponseMessage result = hc.GetAsync(url).Result;
            //    HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
            //    doc.Load(result.Content.ReadAsStreamAsync().Result);
            //    string resultado = "bla";
            //    try
            //    {
            //        resultado = HtmlToPlainText(doc.DocumentNode.SelectSingleNode("//span[@id='result_box']/span").InnerHtml);
            //    }
            //    catch { }
            //    return resultado;
            //}).Result;

            var url = new Uri(string.Format(CultureInfo.InvariantCulture,
                "http://www.google.com/translate_t?hl={0}&ie=UTF8&text={1}&langpair={2}", targetLanguage, input, languagePair));
            using HttpClient hc = new();
            HttpResponseMessage result = hc.GetAsync(url).Result;
            HtmlDocument doc = new() { OptionReadEncoding = true };
            doc.Load(result.Content.ReadAsStreamAsync().Result);
            string resultado = "bla";
            var node = doc.DocumentNode.SelectSingleNode("//span[@id='result_box']/span");
            if (node != null && !string.IsNullOrWhiteSpace(node.InnerHtml))
            {
                resultado = HtmlToPlainText(node.InnerHtml);
            }
            return resultado;
        }
        public static string RemoveDiacritics(this string text)
        {
            if (text == null) return text;
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public static string FirstCharToUpper(this string input)
        {
            var textInfo = new CultureInfo("en-GB").TextInfo;
            return string.IsNullOrWhiteSpace(input) ? null :
            input[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant() + textInfo.ToLower(input[1..]);
        }
        public static string CleanCell(this string text) =>
            //first toupper to gain speed
            //< ( ) needed for species - , and . needed for numbers spaces needed for everything else
            Regex.Replace(Regex.Replace(text?.ToUpperInvariant().RemoveDiacritics().Trim(), @"\s{2,}", " "), @"[^A-Z0-9 _\.\-,\/\<\(\)\@;\:]", "");
        public static string CleanScientificName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return Regex.Replace(Regex.Replace(name, @"\d|\.|\s*<.*|\s*\(.*\)|\s*ESTADO.*|PSEUDO\-|\b[\w']{1,3}\b", "").Trim(), @" {2,}", " ");
            //name = Regex.Replace(name, @"\s{2,}", " ");
            //name = Regex.Replace(name, @"^\s|\s$", "");
        }
    }
}
