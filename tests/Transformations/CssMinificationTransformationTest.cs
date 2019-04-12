using System.Collections.Generic;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class CssMinificationTransformationTest : CssTestBase
    {
        private const string _CSS_CONTENT = "\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r";
        private const string _CSS_CONTENT2 = "\r\n.bk-desktop2 {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r";

        public CssMinificationTransformationTest() 
            : base(new List<IBundleTransform>() { new FileContentReaderTransformation(), new MinificationTransformation() }, 
                  _CSS_CONTENT, 
                  _CSS_CONTENT2)
        {
        }

        [Fact]
        public void CssMinificationTransformation_OnValidInput_ShouldReturnNoErrorOutputAndValidResult()
        {
            Bundle.BundleContext.EnableOptimizations = true;
            BundleResponse bundleResponse = GetBundleResponse();

            string result = bundleResponse.Content.ToString();

            bool hasErrors = bundleResponse.TransformationErrors.Length > 0 ? true : false;

            // \r\n in the end comes from the empty ContentRaw StringBuilder
            string expected = ".bk-desktop{overflow:auto;height:100%;min-width:30em;}.bk-desktop2{overflow:auto;height:100%;min-width:30em;}";
            Assert.Equal(expected, result);
            Assert.False(hasErrors);
        }
    }
}
