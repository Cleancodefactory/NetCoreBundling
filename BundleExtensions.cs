using ccf.CoreKraft.Web.Bundling.Middleware;
using ccf.CoreKraft.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace ccf.CoreKraft.Web.Bundling
{
    public static class BundleExtensions
    {
        public static BundleCollection UseBundling(this IApplicationBuilder builder, IHostingEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            BundleCollection bundleCollection = new BundleCollection(builder, env, logger, baseBundlingRoute, enableOptimizations);
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
    }
}
