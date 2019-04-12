using System.Collections.Generic;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class JsMinificationTransformationTest : JsTestBase
    {
        private const string _JS_Content = "function WorkspaceDesktop() {\r\n    WorkspaceWindow.apply(this, arguments);\r\n};";
        private const string _JS_WorkspaceContent= "var a = 12;";

        public JsMinificationTransformationTest() : base(new List<IBundleTransform>() { new FileContentReaderTransformation(),
                                                                                        new MinificationTransformation() },
                                                                                        _JS_Content,
                                                                                        _JS_WorkspaceContent)
        {
        }

        [Fact]
        public void JsMinificationTransformation_OnValidInput_ShouldReturnNoErrorOutputAndValidResult()
        {
            Bundle.BundleContext.EnableOptimizations = true;
            BundleResponse bundleResponse = GetBundleResponse();

            string result = bundleResponse.Content.ToString();

            bool hasErrors = bundleResponse.TransformationErrors.Length > 0 ? true : false;

            // \r\n in the end comes from the empty ContentRaw StringBuilder
            string expected = "function WorkspaceDesktop(){WorkspaceWindow.apply(this,arguments)}var a=12;\r\n";
            Assert.Equal(expected, result);
            Assert.False(hasErrors);
        }
    }
}
