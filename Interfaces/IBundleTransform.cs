using ccf.CoreKraft.Web.Bundling.Primitives;

namespace ccf.CoreKraft.Web.Bundling.Interfaces
{
    public interface IBundleTransform
    {
        void Process(BundleContext context, BundleResponse response);
    }
}
