using Ccf.Ck.Libs.Web.Bundling.Primitives;

namespace Ccf.Ck.Libs.Web.Bundling.Interfaces
{
    public interface IBundleTransform
    {
        void Process(BundleContext context, BundleResponse response);
    }
}
