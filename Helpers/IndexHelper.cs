using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Myblog.Helpers
{
    public static class IndexHelper
    {
        public static string GetTitle(IDocument doc)
        {
            var t = doc.GetString("Title");
            if (!string.IsNullOrWhiteSpace(t))
                return t;
            else
                return Path.GetFileNameWithoutExtension(doc.Source.ToString());
        }

        public static (string text, bool isHtml) GetExcerpt(IDocument doc, int maxLength = 50)
        {
            try
            {
                string content;
                using (var reader = doc.GetContentTextReader())
                {
                    content = reader.ReadToEnd();
                }
                var plain = Regex.Replace(content, "<h2.*?/h2>", string.Empty);
                plain = StripHtml(plain);

                if (plain.Length > maxLength)
                    plain = plain.Substring(0, maxLength).TrimEnd() + "…";

                return (plain, false);
            }
            catch
            {
                return (doc.GetString("Excerpt"), true);
            }
        }

        public static string GetUrl(IDocument doc, IExecutionContext ctx)
        {
            if (doc == null) return "#";

            var link = ctx.GetLink(doc);
            if (!string.IsNullOrEmpty(link))
                return link.TrimStart('/') + ".html";
            else
                return "#";
        }

        public static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            string str = Regex.Replace(html, "<.*?>", string.Empty);
            str = Regex.Replace(str, @"\s+", " ").Trim();
            return str;
        }

        public static string Encode(string text) => WebUtility.HtmlEncode(text ?? "");
    }
}
