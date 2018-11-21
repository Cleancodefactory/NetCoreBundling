using Ccf.Ck.Libs.Web.Bundling.Middleware;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Ccf.Ck.Libs.Web.Bundling
{
    public static class BundleExtensions
    {
        public static BundleCollection UseBundling(this IApplicationBuilder builder, IHostingEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            IHttpContextAccessor contextAccessor = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            //try
            //{
            //    builder.UseReact(config =>
            //    {
            //        // If you want to use server-side rendering of React components,
            //        // add all the necessary JavaScript files here. This includes
            //        // your components as well as all of their dependencies.
            //        // See http://reactjs.net/ for more information. Example:
            //        //config
            //        //  .AddScript("~/Scripts/First.jsx")
            //        //  .AddScript("~/Scripts/Second.jsx");

            //        // If you use an external build too (for example, Babel, Webpack,
            //        // Browserify or Gulp), you can improve performance by disabling
            //        // ReactJS.NET's version of Babel and loading the pre-transpiled
            //        // scripts. Example:
            //        //config
            //        //  .SetLoadBabel(false)
            //        //  .AddScriptWithoutTransform("~/Scripts/bundle.server.js");
            //        //config.SetLoadBabel(true);
            //    });
            //}
            //catch{}
            
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddReact();
            return services;
        }
    }
}
