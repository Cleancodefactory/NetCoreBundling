using System.Collections.Generic;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;

namespace Ccf.Ck.Web.Bundling.Test.Transformations.Setup
{
    public class CssTestBase : TestBase
    {
        private const string CSS_MODULE_CONTENT = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n";
        private const string WORKSPACE_WINDOW_FILENAME = "bk-workspacewindow.css";
        private const string WORKSPACE_WINDOW_CONTENT = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n";
        private const string FILENAME = "bk-module.css";

        private Bundle _Bundle;

        public CssTestBase() : base(CSS_MODULE_CONTENT, WORKSPACE_WINDOW_FILENAME, WORKSPACE_WINDOW_CONTENT, FILENAME)
        {
        }

        protected override Bundle Bundle
        {
            get { return _Bundle; }
            set
            {
                _Bundle = new StyleBundle(DirectoryName, null, null, new List<IBundleTransform>());
            }
        }
    }
}
