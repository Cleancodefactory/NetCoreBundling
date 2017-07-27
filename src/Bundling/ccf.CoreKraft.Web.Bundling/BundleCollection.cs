using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;

namespace ccf.CoreKraft.Web.Bundling
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/performance/bundling-and-minification
    public class BundleCollection
    {
        static BundleCollection _BundleCollection;
        IApplicationBuilder _App;
        IHostingEnvironment _Env;
        ILogger _Logger;
        string _BaseBundlingRoute;
        internal BundleCollection(IApplicationBuilder app, IHostingEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            _BundleCollection = this;
            _App = app;
            _Env = env;
            _Logger = logger;
            _BaseBundlingRoute = baseBundlingRoute;
            Scripts = new Scripts();
            Styles = new Styles();
            EnableOptimizations = enableOptimizations;
        }

        internal IFileProvider WebRootFileProvider
        {
            get
            {
                return _Env.WebRootFileProvider;
            }
        }

        internal Bundle GetBundle(string bundleKey)
        {
            if (Styles.StyleBundles.ContainsKey(bundleKey))
            {
                return Styles.StyleBundles[bundleKey];
            }
            if (Scripts.ScriptBundles.ContainsKey(bundleKey))
            {
                return Scripts.ScriptBundles[bundleKey];
            }
            throw new Exception($"The requested bundle {bundleKey} doesn't exist.");
        }

        public static BundleCollection Instance
        {
            get
            {
                return _BundleCollection;
            }
        }

        public Scripts Scripts { get; private set; }
        public Styles Styles { get; private set; }
        public bool EnableOptimizations { get; private set; }
        public bool EnableInstrumentations { get; set; }

        public void Add(Bundle bundle)
        {
            bundle.BundleContext.Init(_App, _Env, _Logger, _BaseBundlingRoute, EnableOptimizations, EnableInstrumentations);
            if (bundle is StyleBundle)
            { 
                Styles.StyleBundles.Add(bundle.Route, bundle);
            }
            else if (bundle is ScriptBundle)
            {
                Scripts.ScriptBundles.Add(bundle.Route, bundle);
            }
        }
    }
}