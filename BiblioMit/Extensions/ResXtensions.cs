using AngleSharp.Xml.Parser;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Extensions
{
    public static class ResXtensions
    {
        public static async Task<string> LocalizeStringAsync<T>(this T t, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException($"{t} text is null");
            }
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (lang == "en") return text;
            try
            {
                var file = string.Join('.', typeof(T).Namespace.Split('.').Skip(1)
                    .Append(typeof(T).Name).Append(lang).Append("resx"));
                var parser = new XmlParser();
                string resx = File.ReadAllText($"Resources/{file}");
                var document = await parser.ParseDocumentAsync(resx).ConfigureAwait(false);
                var element = document.QuerySelector($"data[name='{text}'] value");
                return element == null ? text : element.TextContent;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                return text;
            }
        }
    }
}
