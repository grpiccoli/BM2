using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BiblioMit.Models.VM
{
    public static class CSPTag
    {
        public static HashSet<string> BaseUri { get; } = new HashSet<string>();
        public static bool BlockAllMixedContent { get; set; } = true;
        public static HashSet<string> DefaultSrc { get; } = new HashSet<string>();
        public static HashSet<string> ConnectSrc { get; } = new HashSet<string>();
        public static HashSet<string> ImgSrc { get; } = new HashSet<string>();
        public static HashSet<string> FontSrc { get; } = new HashSet<string>();
        public static HashSet<string> ObjectSrc { get; } = new HashSet<string>();
        public static HashSet<string> ScriptSrc { get; } = new HashSet<string>();
        public static HashSet<string> ScriptSrcElem { get; } = new HashSet<string>();
        public static HashSet<string> StyleSrc { get; } = new HashSet<string>();
        public static HashSet<string> StyleSrcElem { get; } = new HashSet<string>();
        public static HashSet<string> FrameSrc { get; } = new HashSet<string>();
        public static Dictionary<string, string> Files { get; } = new Dictionary<string, string>();
        public static bool UpgradeInsecureRequests { get; set; } = true;
        public static HashSet<string> AccessControlUrls { get; } = new HashSet<string>();
        public static string GetString(HostString baseUrl)
        {
            StyleSrcElem.Add("'unsafe-inline'");
            StyleSrcElem.Add("https://fonts.googleapis.com/");
            FontSrc.Add("https://fonts.gstatic.com/");
            FontSrc.Add("https://fonts.googleapis.com/");
            ScriptSrcElem.Add("https://connect.facebook.net/");
            ScriptSrcElem.Add("https://fonts.googleapis.com/");
            ConnectSrc.Add("https://fonts.googleapis.com/");
            ConnectSrc.Add("https://fonts.gstatic.com/");
            ConnectSrc.Add("https://www.google-analytics.com/");
            ConnectSrc.Add("https://www.facebook.com/");
            FrameSrc.Add("https://www.facebook.com/");
            FrameSrc.Add("https://web.facebook.com/");
            if (StyleSrcElem.Contains("'unsafe-inline'"))
                StyleSrcElem.RemoveWhere(s => s.StartsWith("'nonce-", System.StringComparison.Ordinal) || s.StartsWith("'sha", System.StringComparison.Ordinal));
            if (ScriptSrcElem.Contains("'unsafe-inline'"))
                ScriptSrcElem.RemoveWhere(s => s.StartsWith("'nonce-", System.StringComparison.Ordinal) || s.StartsWith("'sha", System.StringComparison.Ordinal));
            var blockmixed = BlockAllMixedContent ? "block-all-mixed-content;" : string.Empty;
            var upgradeinsecure = UpgradeInsecureRequests ? "upgrade-insecure-requests;" : string.Empty;
            return $"base-uri 'self' {string.Join(" ", BaseUri)} ; " +
                $"{blockmixed}" +
                $"default-src 'self' {string.Join(" ", DefaultSrc)} ; " +
                $"connect-src 'self' ws://{baseUrl} {string.Join(" ", ConnectSrc)} ; " +
                $"frame-src 'self' {string.Join(" ", FrameSrc)} ; " +
                $"img-src data: blob: 'self' {string.Join(" ", ImgSrc)} ; " +
                $"object-src 'none' {string.Join(" ", ObjectSrc)} ; " +
                $"script-src 'self' {string.Join(" ", ScriptSrc)} ; " +
                $"script-src-elem 'self' {string.Join(" ", ScriptSrcElem)} ; " +
                $"style-src 'self' {string.Join(" ", StyleSrc)} ; " +
                $"style-src-elem 'self' {string.Join(" ", StyleSrcElem)} ; " +
                $"font-src 'self' data: {string.Join(" ", FontSrc)} ; " +
                $"{upgradeinsecure}";
        }
        public static string GetAccessControlString()
        {
            return string.Join(" ", AccessControlUrls);
        }
        public static void Clear()
        {
            BaseUri.Clear();
            DefaultSrc.Clear();
            ConnectSrc.Clear();
            FrameSrc.Clear();
            ImgSrc.Clear();
            ObjectSrc.Clear();
            ScriptSrc.Clear();
            ScriptSrcElem.Clear();
            StyleSrc.Clear();
            StyleSrcElem.Clear();
            FontSrc.Clear();
        }
    }
}
