namespace Ccf.Ck.Libs.Web.Bundling
{
    public class Profile
    {
        public Profile(string key)
        {
            Key = key;
            Styles = new Styles();
            Scripts = new Scripts();
        }

        public string  Key { get; private set; }
        public Scripts Scripts { get; private set; }
        public Styles Styles { get; private set; }

        public void Add(Bundle bundle)
        {
            bundle.BundleContext.Init(BundleCollection.Instance.ApplicationBuilder, BundleCollection.Instance.HostingEnvironment, BundleCollection.Instance.Logger, BundleCollection.Instance.BaseBundlingRoute, BundleCollection.Instance.EnableOptimizations, BundleCollection.Instance.EnableInstrumentations);
            if (bundle is StyleBundle)
            {
                Styles.StyleBundles.TryAdd(bundle.Route, bundle);
            }
            else if (bundle is ScriptBundle)
            {
                Scripts.ScriptBundles.TryAdd(bundle.Route, bundle);
            }
        }

        public bool HasScriptBundle(string bundleKey)
        {
            return Scripts != null && Scripts.HasBundle(bundleKey);
        }

        public bool HasStyleBundle(string bundleKey)
        {
            return Styles != null && Styles.HasBundle(bundleKey);
        }
    }
}
