﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.IO;

namespace BiblioMit.Extensions
{
    public static class ViewPageExtensions
    {
        private const string BLOCK_BUILDER = "BlockBuilder";
        public static HtmlString Blocks(this RazorPageBase webPage, string name, params Func<dynamic, HelperResult>[] templates)
        {
            var sb = new StringBuilder();
            foreach (var t in templates)
            {
                sb.Append(webPage.Block(name, t));
            }
            return new HtmlString(sb.ToString());
        }
        public static HtmlString Block(this RazorPageBase webPage, string name, Func<dynamic, HelperResult> template)
        {
            var sb = new StringBuilder();
            using TextWriter tw = new StringWriter(sb);
            var encoder = (HtmlEncoder)webPage?.ViewContext.HttpContext.RequestServices.GetService(typeof(HtmlEncoder));

            if (webPage.ViewContext.HttpContext.Request.Headers["x-requested-with"] != "XMLHttpRequest")
            {
                var scriptBuilder = webPage.ViewContext.HttpContext.Items[name + BLOCK_BUILDER] as StringBuilder ?? new StringBuilder();

                template?.Invoke(null).WriteTo(tw, encoder);
                scriptBuilder.Append(sb);
                webPage.ViewContext.HttpContext.Items[name + BLOCK_BUILDER] = scriptBuilder;

                return new HtmlString(string.Empty);
            }

            template?.Invoke(null).WriteTo(tw, encoder);

            return new HtmlString(sb.ToString());
        }

        public static HtmlString WriteBlocks(this RazorPageBase webPage, string name)
        {
            var scriptBuilder = webPage?.ViewContext.HttpContext.Items[name + BLOCK_BUILDER] as StringBuilder ?? new StringBuilder();

            return new HtmlString(scriptBuilder.ToString());
        }
    }
}
