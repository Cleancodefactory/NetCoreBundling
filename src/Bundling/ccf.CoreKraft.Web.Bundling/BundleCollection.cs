using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ccf.CoreKraft.Web.Bundling
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/performance/bundling-and-minification
    public class BundleCollection
    {
        static BundleCollection _BundleCollection;

        public static BundleCollection Instance
        {
            get
            {
                return _BundleCollection;
            }
        }
        public bool EnableOptimizations { get; private set; }
        public bool EnableInstrumentations { get; set; }
        public ConcurrentDictionary<string, Profile> Profiles { get; private set; }

        public Profile Profile(string key = "Generic")
        {
            return Profiles.GetOrAdd(key, new Profile(key));
        }

        internal BundleCollection(IApplicationBuilder app, IHostingEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations)
        {
            _BundleCollection = this;
            ApplicationBuilder = app;
            HostingEnvironment = env;
            Logger = logger;
            BaseBundlingRoute = baseBundlingRoute;
            EnableOptimizations = enableOptimizations;
            Profiles = new ConcurrentDictionary<string, Profile>();
        }

        internal IApplicationBuilder ApplicationBuilder { get; private set; }
        internal IHostingEnvironment HostingEnvironment { get; private set; }
        internal ILogger Logger { get; private set; }
        internal string BaseBundlingRoute { get; private set; }

        internal Bundle GetBundle(string bundleKey)
        {
            foreach (var profile in Profiles)
            {
                if (profile.Value.Styles != null)
                {
                    if (profile.Value.Styles.StyleBundles.ContainsKey(bundleKey))
                    {
                        return profile.Value.Styles.StyleBundles[bundleKey];
                    }
                    else if (profile.Value.Styles.StyleBundles.ContainsKey(bundleKey + "-css"))
                    {
                        return profile.Value.Styles.StyleBundles[bundleKey + "-css"];
                    }
                }
                if (profile.Value.Scripts != null)
                {
                    if (profile.Value.Scripts.ScriptBundles.ContainsKey(bundleKey))
                    {
                        return profile.Value.Scripts.ScriptBundles[bundleKey];
                    }
                    else if (profile.Value.Scripts.ScriptBundles.ContainsKey(bundleKey + "-scripts"))
                    {
                        return profile.Value.Scripts.ScriptBundles[bundleKey + "-scripts"];
                    }
                }
            }
            throw new Exception($"The requested bundle {bundleKey} doesn't exist.");
        }
    }
}