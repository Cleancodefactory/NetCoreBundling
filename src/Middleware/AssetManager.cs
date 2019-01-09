using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling.Middleware
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
                        BundleResponse bundleResponse = bundle.ExecuteTransformations();
                        if (bundle.BundleContext.HttpContext.Request.Headers.Keys.Contains(HeaderNames.IfNoneMatch) && bundle.BundleContext.HttpContext.Request.Headers[HeaderNames.IfNoneMatch] == bundleResponse.ETag)
                        {
                            bundle.BundleContext.HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                            return;
                        }
                        else
                        {
                            if (routeData.Values.ContainsKey(BundleRouteBuilder.CATCHALL) && routeData.Values[BundleRouteBuilder.CATCHALL] != null)
                            {
                                fileName = routeData.Values[BundleRouteBuilder.CATCHALL].ToString();
                            }
                            
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
                }

                IHeaderDictionary headers = httpContext.Response.Headers;
                if (headers.ContainsKey(HeaderNames.ETag))
                {
                    headers[HeaderNames.ETag] = new[] { eTag };
                }
                else
                {
                    headers.Add(HeaderNames.ETag, new[] { eTag });
                }

                await httpContext.Response.WriteAsync(sb.ToString());
            };
            return requestDelegate;
        }
    }
}
