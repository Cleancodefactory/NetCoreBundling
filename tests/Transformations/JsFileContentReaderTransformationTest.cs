using System.Collections.Generic;
using System.Linq;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class JsFileContentReaderTransformationTest : JsTestBase
    {
        public JsFileContentReaderTransformationTest() : base(new List<IBundleTransform>() { new FileContentReaderTransformation() })
        {
        }

        [Fact]
        public void CheckFileContentReaderTransformation_OnValidScriptInput_ShouldReturnValidOutput()
        {
            string expectedResult = "/*\r\n    #using \"./ bindkraft -public-profile.js\"\r\n*/\r\n{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\nconsole.log();\r\n}\r\n";

            BundleResponse bundleResponse = GetBundleResponse();

            List<string> results = bundleResponse.BundleFiles.Values.Select(x => x.Content.ToString()).ToList();
            string result = string.Join("", results);
            bool isEmpty = string.IsNullOrEmpty(bundleResponse.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal(expectedResult, result);
        }
    }
}
