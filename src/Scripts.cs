using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Microsoft.AspNetCore.Html;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling
{
    public class Scripts : ConcurrentDictionary<string, Bundle>
    {
        internal ConcurrentDictionary<string, Bundle> ScriptBundles { get; private set; }

        public Scripts()
        {
            ScriptBundles = new ConcurrentDictionary<string, Bundle>();
        }

        public bool HasBundle(string bundleKey)
        {
            return ScriptBundles.ContainsKey(bundleKey);
        }

        public bool RemoveBundle(string bundleKey)
        {
            bool result = ScriptBundles.TryRemove(bundleKey, out Bundle bundle);
            if (bundle != null)
            {
                bundle.RemoveFromCache();
            }
            return result;
        }

        public void RemoveAllBundles()
        {
            if (ScriptBundles.Values != null && ScriptBundles.Values.Count > 0)
            {
                List<string> keys = new List<string>(ScriptBundles.Values.Count);
                foreach (Bundle bundle in ScriptBundles.Values)
                {
                    keys.Add(bundle.Route);
                }
                foreach (string key in keys)
                {
                    RemoveBundle(key);
                }
            }
        }

        public HtmlString Render(string bundleKey)
        {
            if (ScriptBundles.ContainsKey(bundleKey))
            {
                return Render(ScriptBundles[bundleKey]);
            }
            return new HtmlString(string.Empty);
        }

        public HtmlString Render(Bundle bundle)
        {
            StringBuilder sb = new StringBuilder();
            BundleResponse bundleResponse;
            bundleResponse = bundle.ExecuteTransformations();
            if (BundleCollection.Instance.EnableOptimizations)
            {
                foreach (CdnObject cdn in bundleResponse.InputCdns)
                {
                    string additional = !string.IsNullOrEmpty(cdn.Integrity) ? $"integrity='{cdn.Integrity}'" : string.Empty;
                    additional += !string.IsNullOrEmpty(cdn.Crossorigin) ? $" crossorigin='{cdn.Crossorigin}'" : string.Empty;
                    sb.Append($"<script src='{cdn.CdnPath}' {additional}></script>");
                }
                string eTag = bundleResponse.ETag;
                if (!string.IsNullOrEmpty(bundle.ExternalVersion))
                {
                    bundleResponse.Content = bundleResponse.Content.Replace(bundle.VERSION_INTERNAL_REPLACEMENT, bundle.ExternalVersion + "-" + bundleResponse.ETag);
                    eTag = bundle.ExternalVersion + "-" + bundleResponse.ETag;
                }
                sb.Append($"<script src='{bundle.BundleContext.HttpContext?.Request.PathBase}/{bundle.BundleContext.BaseBundlingRoute}/{bundle.Route}?{eTag}' type='text/javascript'></script>");
            }
            else
            {
                if (bundleResponse.TransformationErrors.Length > 0)
                {
                    sb.Append("<div>");
                    sb.Append("Transformation(Scripts)-Errors:").Append("<br />");
                    sb.Append(bundleResponse.TransformationErrors);
                    sb.Append("</div>");
                }
                foreach (CdnObject cdn in bundleResponse.InputCdns)
                {
                    string additional = !string.IsNullOrEmpty(cdn.Integrity) ? $"integrity='{cdn.Integrity}'" : string.Empty;
                    additional += !string.IsNullOrEmpty(cdn.Crossorigin) ? $" crossorigin='{cdn.Crossorigin}'" : string.Empty;
                    sb.Append($"<script src='{cdn.CdnPath}' {additional}></script>");
                }
                foreach (BundleFile file in bundleResponse.BundleFiles.Values)
                {
                    sb.Append($"<script type='text/javascript' src='{bundle.BundleContext.HttpContext?.Request.PathBase}{file.VirtualPath}?{file.ETag}'></script>");
                }
                if (bundleResponse.ContentRaw.Length > 0)
                {
                    sb.Append("<script type='text/javascript'>").Append(bundleResponse.ContentRaw).Append("</script>");
                }
            }
            return new HtmlString(sb.ToString());
        }

        public HtmlString Render()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Bundle bundle in ScriptBundles.Values)
            {
                sb.Append(Render(bundle));
            }
            return new HtmlString(sb.ToString());
        }
    }
}
