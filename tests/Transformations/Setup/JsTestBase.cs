using System.Collections.Generic;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;

namespace Ccf.Ck.Web.Bundling.Test.Transformations.Setup
{
    public class JsTestBase : TestBase
    {
        private const string JS_MODULE_FILENAME = "bk-module.js";
        private const string JS_MODULE_CONTENT = "/*\r\n    #using \"./ bindkraft -public-profile.js\"\r\n*/\r\n";
        private const string WORKSPACE_WINDOW_FILENAME = "bk-workspacewindow.js";
        private const string WORKSPACE_WINDOW_CONTENT = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\nconsole.log();\r\n}\r\n";

        private Bundle _Bundle;

        public JsTestBase(List<IBundleTransform> transformations, string moduleContent = JS_MODULE_CONTENT, string workspaceContent = WORKSPACE_WINDOW_CONTENT) 
            : base(JS_MODULE_FILENAME, moduleContent, WORKSPACE_WINDOW_FILENAME, workspaceContent)
        {
            Bundle = new ScriptBundle(BundleRoute, Provider, null, transformations);
            Bundle.Include(VirtualPaths);
        }

        protected override Bundle Bundle { get; set; }
    }
}
