using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Collections.Concurrent;
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
            Bundle bundle;
            bool found = ScriptBundles.TryGetValue(bundleKey, out bundle);
            if (found)
            {
                return ScriptBundles.Values.Remove(bundle);
            }
            return false;
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
                
                if (bundle.BundleContext.HttpContext.Request.Headers.Keys.Contains(HeaderNames.IfNoneMatch) && bundle.BundleContext.HttpContext.Request.Headers[HeaderNames.IfNoneMatch] == bundleResponse.ETag)
                {
                    bundle.BundleContext.HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                }
                else
                {
                    sb.Append($"<script src='{bundle.BundleContext.HttpContext?.Request.PathBase}/{bundle.BundleContext.BaseBundlingRoute}/{bundle.Route}?{bundleResponse.ETag}' type='text/javascript'></script>");
                    bundle.BundleContext.HttpContext.Response.Headers.Add(HeaderNames.ETag, new[] { bundleResponse.ETag });
                }                
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
