using System.Text;

namespace ccf.CoreKraft.Web.Bundling.Primitives
{
    public class BundleFile
    {
        public string VirtualPath { get; set; }
        public string PhysicalPath { get; set; }
        public StringBuilder Content { get; set; }
    }
}
