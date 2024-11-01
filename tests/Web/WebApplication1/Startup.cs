using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.UseBundling();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //Bundling
            BundleCollection bundleCollection = app.UseBundling(env, loggerFactory.CreateLogger("Bundling"), "res", true);
            bundleCollection.EnableInstrumentations = true;// env.IsDevelopment(); //Logging enabled
                //Configure bundles
                //bundleCollection.Profile("mvc").Add(new StyleBundle("cssbundle")
                //    .Include(@"~/css/site.css")
                //    .IncludeCdn(new CdnObject { CdnPath = "//maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css", Integrity = "sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u", Crossorigin = "anonymous" })
                //    .IncludeCdn(new CdnObject { CdnPath = "https://cdnjs.cloudflare.com/ajax/libs/typicons/2.0.9/typicons.css" })
                //    .IncludeCdn(new CdnObject { CdnPath = "https://cdnjs.cloudflare.com/ajax/libs/typicons/2.0.9/typicons.woff" }));
            bundleCollection.Profile("mvc").Add(new ScriptBundle("jsbundle")
                .Include(@"~/js/site.js"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
