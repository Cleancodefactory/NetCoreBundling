using ccf.CoreKraft.Web.Bundling.Middleware;
using ccf.CoreKraft.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace ccf.CoreKraft.Web.Bundling
{
    public static class BundleExtensions
    {
        public static BundleCollection UseBundling(this IApplicationBuilder builder, IHostingEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            IHttpContextAccessor contextAccessor = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            if (contextAccessor == null)
            {
                throw new System.Exception("Please call: UseBundling(this IServiceCollection services) in your Startup.ConfigureServices(IServiceCollection services) method!");
            }
            BundleCollection bundleCollection = new BundleCollection(builder, env, contextAccessor, logger, baseBundlingRoute, enableOptimizations);
            if (!enableOptimizations)//Only in debug mode the whole content directory is accessible
            {
                builder.UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = false,
                    FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                    RequestPath = ""
                });
            }
            RouteHandler customRouteHandler = new RouteHandler(AssetManager.ExecutionDelegate(builder));
            builder.UseRouter(BundleRouteBuilder.BuildRoute(builder, customRouteHandler, baseBundlingRoute));
            
            return bundleCollection;
        }

        public static IServiceCollection UseBundling(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }
    }
}
