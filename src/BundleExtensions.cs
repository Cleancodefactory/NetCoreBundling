using Ccf.Ck.Libs.Web.Bundling.Middleware;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Ccf.Ck.Libs.Web.Bundling
{
    public static class BundleExtensions
    {
        public static BundleCollection UseBundling(this IApplicationBuilder builder, IWebHostEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            return builder.UseBundling(env, null, null, logger, baseBundlingRoute, enableOptimizations);
        }

        public static BundleCollection UseBundling(this IApplicationBuilder builder, IWebHostEnvironment env, IEnumerable<string> rootPaths, string rootVirtualPath, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            IHttpContextAccessor contextAccessor = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            if (contextAccessor == null)
            {
                throw new System.Exception("Please call: UseBundling(this IServiceCollection services) in your Startup.ConfigureServices(IServiceCollection services) method!");
            }
            BundleCollection bundleCollection = new BundleCollection(builder, env, contextAccessor, logger, baseBundlingRoute, enableOptimizations);
            if (!enableOptimizations)//Only in debug mode the whole content directory is accessible
            {
                if (rootPaths != null)
                {
                    foreach (string rootPath in rootPaths)
                    {
                        builder.UseStaticFiles(new StaticFileOptions
                        {
                            ServeUnknownFileTypes = false,
                            FileProvider = new PhysicalFileProvider(rootPath),
                            RequestPath = rootVirtualPath ?? string.Empty
                        });
                    }
                }
                else
                {
                    builder.UseStaticFiles(new StaticFileOptions
                    {
                        ServeUnknownFileTypes = false,
                        FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                        RequestPath = string.Empty
                    }); ;
                }
            }
            RouteHandler customRouteHandler = new RouteHandler(AssetManager.ExecutionDelegate(builder));
            builder.UseRouter(BundleRouteBuilder.BuildRoute(builder, customRouteHandler, baseBundlingRoute));

            return bundleCollection;
        }

        public static IServiceCollection UseBundling(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }
    }
}
