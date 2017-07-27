using ccf.CoreKraft.Web.Bundling.Primitives;
using ccf.CoreKraft.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace ccf.CoreKraft.Web.Bundling.Middleware
{
    internal class AssetManager
    {
        internal static RequestDelegate ExecutionDelegate(IApplicationBuilder builder)
        {
            RequestDelegate requestDelegate = async httpContext =>
            {
                StringBuilder sb = new StringBuilder(10000);
                string eTag = null;
                RouteData routeData = httpContext.GetRouteData();
                if (routeData.Values != null)
                {
                    string bundleName = null;
                    string fileName = null;
                    if (routeData.Values.ContainsKey(BundleRouteBuilder.BUNDLE))
                    {
                        bundleName = routeData.Values[BundleRouteBuilder.BUNDLE].ToString();
                    }
                    Bundle bundle = BundleCollection.Instance.GetBundle(bundleName);
                    if (bundle != null)
                    {
                        if (routeData.Values.ContainsKey(BundleRouteBuilder.CATCHALL) && routeData.Values[BundleRouteBuilder.CATCHALL] != null)
                        {
                            fileName = routeData.Values[BundleRouteBuilder.CATCHALL].ToString();
                        }
                        BundleResponse bundleResponse = bundle.ExecuteTransformations();
                        if (!string.IsNullOrEmpty(fileName))    //a single file has been requested
                        {
                            if (bundleResponse.BundleFiles.ContainsKey(fileName))
                            {
                                BundleFile bundleFile = bundleResponse.BundleFiles[fileName];
                                sb.Append(bundleFile.Content);
                                httpContext.Response.ContentType = bundle.ContentType;
                            }                            
                        }
                        else                                    //the whole bundle is requested
                        {
                            sb.Append(bundleResponse.Content);
                            httpContext.Response.ContentType = bundle.ContentType;
                            eTag = bundleResponse.ETag;
                        }
                    }
                }

                httpContext.Response.Headers.Add("ETag", new[] { eTag });
                await httpContext.Response.WriteAsync(sb.ToString());
            };
            return requestDelegate;
        }
    }
}
