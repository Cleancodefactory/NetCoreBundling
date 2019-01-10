using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling
{
    public class Styles : ConcurrentDictionary<string, Bundle>
    {
        internal ConcurrentDictionary<string, Bundle> StyleBundles { get; private set; }

        public Styles()
        {
            StyleBundles = new ConcurrentDictionary<string, Bundle>();
        }

        public bool HasBundle(string bundleKey)
        {
            return StyleBundles.ContainsKey(bundleKey);
        }

        public bool RemoveBundle(string bundleKey)
        {
            Bundle bundle;
            bool found = StyleBundles.TryGetValue(bundleKey, out bundle);
            if (found)
            {
                return StyleBundles.Values.Remove(bundle);
            }
            return false;
        }

        public HtmlString Render(string bundleKey)
        {
            if (StyleBundles.ContainsKey(bundleKey))
            {
                return Render(StyleBundles[bundleKey]);
            }
            return new HtmlString(string.Empty);
        }

        public HtmlString Render(Bundle bundle)
        {
            StringBuilder sb = new StringBuilder();
            BundleResponse bundleResponse;
            if (BundleCollection.Instance.EnableOptimizations)
            {
                bundleResponse = bundle.ExecuteTransformations();
                foreach (CdnObject cdn in bundleResponse.InputCdns)
                {
                    string additional = !string.IsNullOrEmpty(cdn.Integrity) ? $"integrity='{cdn.Integrity}'" : string.Empty;
                    additional += !string.IsNullOrEmpty(cdn.Crossorigin) ? $" crossorigin='{cdn.Crossorigin}'" : string.Empty;
                    string rel = !string.IsNullOrEmpty(cdn.Rel) ? $"rel='{cdn.Rel}'" : "rel='stylesheet'";
                    sb.Append($"<link href='{cdn.CdnPath}' {additional} {rel}/>");
                }
                sb.Append($"<link href='{bundle.BundleContext.HttpContext?.Request.PathBase}/{bundle.BundleContext.BaseBundlingRoute}/{bundle.Route}' rel='stylesheet'/>");
            }
            else
            {
                bundleResponse = bundle.ExecuteTransformations();
                if (bundleResponse.TransformationErrors.Length > 0)
                {
                    sb.Append("<div>");
                    sb.Append("Transformation(Styles)-Errors:").Append(Environment.NewLine);
                    sb.Append(bundleResponse.TransformationErrors);
                    sb.Append("</div>");
                }
                foreach (CdnObject cdn in bundleResponse.InputCdns)
                {
                    string additional = !string.IsNullOrEmpty(cdn.Integrity) ? $"integrity='{cdn.Integrity}'" : string.Empty;
                    additional += !string.IsNullOrEmpty(cdn.Crossorigin) ? $" crossorigin='{cdn.Crossorigin}'" : string.Empty;
                    string rel = !string.IsNullOrEmpty(cdn.Rel) ? $"rel='{cdn.Rel}'" : "rel='stylesheet'";
                    sb.Append($"<link href='{cdn.CdnPath}' {additional} {rel}/>");
                }
                foreach (BundleFile file in bundleResponse.BundleFiles.Values)
                {
                    sb.Append($"<link href='{bundle.BundleContext.HttpContext?.Request.PathBase}{file.VirtualPath}?{file.ETag}' rel='stylesheet'/>");
                }
            }
            return new HtmlString(sb.ToString());
        }

        public HtmlString Render()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Bundle bundle in StyleBundles.Values)
            {
                sb.Append(Render(bundle));
            }
            return new HtmlString(sb.ToString());
        }
    }
}
