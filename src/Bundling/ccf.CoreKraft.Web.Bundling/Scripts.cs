using ccf.CoreKraft.Web.Bundling.Primitives;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ccf.CoreKraft.Web.Bundling
{
    public class Scripts : Dictionary<string, Bundle>
    {
        internal Dictionary<string, Bundle> ScriptBundles { get; private set; }

        public Scripts()
        {
            ScriptBundles = new Dictionary<string, Bundle>();
        }

        public Scripts Exclude(string bundleKey)
        {
            return (Scripts)ScriptBundles.Where(p => p.Key == bundleKey).ToDictionary(p => p.Key, p => p.Value);
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
            if (BundleCollection.Instance.EnableOptimizations)
            {
                bundleResponse = bundle.ExecuteTransformations();
                foreach (CdnObject cdn in bundleResponse.InputCdns)
                {
                    string additional = !string.IsNullOrEmpty(cdn.Integrity) ? $"integrity='{cdn.Integrity}'" : string.Empty;
                    additional += !string.IsNullOrEmpty(cdn.Crossorigin) ? $" crossorigin='{cdn.Crossorigin}'" : string.Empty;
                    sb.Append($"<script src='{cdn.CdnPath}' {additional}></script>");
                }
                sb.Append($"<script src='/{bundle.BundleContext.BaseBundlingRoute}/{bundle.Route}?{bundleResponse.Version}' type='text/javascript'></script>");
            }
            else
            {
                bundleResponse = bundle.ExecuteTransformations();
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
                    sb.Append($"<script type='text/javascript' src='{file.VirtualPath}?{DateTime.Now.Millisecond}'></script>");
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
