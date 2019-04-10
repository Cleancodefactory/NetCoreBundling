using System.Collections.Generic;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;

namespace Ccf.Ck.Web.Bundling.Test.Transformations.Setup
{
    public class JsTestBase : TestBase
    {
        private const string MODULE_JS_CONTENT = "/*\r\n    #using \"./ bindkraft -public-profile.js\"\r\n*/\r\n";
        private const string WORKSPACE_WINDOW_FILENAME = "bk-workspacewindow.js";
        private const string WORKSPACE_WINDOW_CONTENT = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\nconsole.log();\r\n}\r\n";
        private const string FILENAME = "bk-module.js";

        private Bundle _Bundle;

        public JsTestBase() : base(MODULE_JS_CONTENT, WORKSPACE_WINDOW_FILENAME, WORKSPACE_WINDOW_CONTENT, FILENAME)
        {
        }

        protected override Bundle Bundle
        {
            get { return _Bundle; }
            set
            {
                _Bundle = new ScriptBundle(DirectoryName, null, null, new List<IBundleTransform>());
            }
        }
    }
}
