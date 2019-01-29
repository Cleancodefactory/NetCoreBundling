using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using System;
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
                        if (httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etag) && WithQuotes(bundleResponse.ETag) == etag)
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
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
                                eTag = WithQuotes(bundleResponse.ETag);
                            }
                        }
                        CacheControlHeaderValue cacheControlHeaderValue = new CacheControlHeaderValue();
                        cacheControlHeaderValue.MaxAge = bundle.MaxAge;
                        if (bundle.HttpCacheability == Enumerations.HttpCacheability.Public)
                        {
                            cacheControlHeaderValue.Public = true;
                        }
                        else if (bundle.HttpCacheability == Enumerations.HttpCacheability.Private)
                        {
                            cacheControlHeaderValue.Private = true;
                        }

                        httpContext.Response.GetTypedHeaders().CacheControl = cacheControlHeaderValue;
                        httpContext.Response.Headers.Add(HeaderNames.ETag, new[] { eTag });
                        await httpContext.Response.WriteAsync(sb.ToString());
                    }
                }
            };
            return requestDelegate;
        }

        private static string WithQuotes(string value)
        {
            return $"\"{value}\"";
        }
    }
}
