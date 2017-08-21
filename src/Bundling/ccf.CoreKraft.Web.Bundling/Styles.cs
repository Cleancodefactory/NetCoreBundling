using ccf.CoreKraft.Web.Bundling.Primitives;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ccf.CoreKraft.Web.Bundling
{
    public class Styles : Dictionary<string, Bundle>
    {
        internal Dictionary<string, Bundle> StyleBundles { get; private set; }

        public Styles()
        {
            StyleBundles = new Dictionary<string, Bundle>();
        }

        public bool HasBundle(string bundleKey)
        {
            return StyleBundles.ContainsKey(bundleKey);
        }

        public Styles Exclude(string bundleKey)
        {
            return (Styles)StyleBundles.Where(p => p.Key == bundleKey).ToDictionary(p => p.Key, p => p.Value);
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
                    sb.Append($"<link href='{cdn.CdnPath}' {additional} rel='stylesheet'/>");
                }
                sb.Append($"<link href='/{bundle.BundleContext.BaseBundlingRoute}/{bundle.Route}?{bundleResponse.Version}' rel='stylesheet'/>");
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
                    sb.Append($"<link href='{cdn.CdnPath}' {additional} rel='stylesheet'/>");
                }
                foreach (BundleFile file in bundleResponse.BundleFiles.Values)
                {
                    sb.Append($"<link href='{file.VirtualPath}?{DateTime.Now.Millisecond}' rel='stylesheet'/>");
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
