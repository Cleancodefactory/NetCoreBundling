using System.Collections.Generic;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;

namespace Ccf.Ck.Web.Bundling.Test.Transformations.Setup
{
    public class CssTestBase : TestBase
    {
        private const string CSS_MODULE_FILENAME = "bk-module.css";
        private const string CSS_MODULE_CONTENT = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n";
        private const string WORKSPACE_WINDOW_FILENAME = "bk-workspacewindow.css";
        private const string WORKSPACE_WINDOW_CONTENT = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n";

        public CssTestBase(List<IBundleTransform> transformations, string moduleContent = CSS_MODULE_CONTENT, string workspaceContent = WORKSPACE_WINDOW_CONTENT) 
            : base(CSS_MODULE_FILENAME, moduleContent, WORKSPACE_WINDOW_FILENAME, workspaceContent)
        {
            Bundle = new StyleBundle(BundleRoute, Provider, null, transformations);
            Bundle.Include(VirtualPaths);
        }

        protected override Bundle Bundle { get; set; }
    }
}
