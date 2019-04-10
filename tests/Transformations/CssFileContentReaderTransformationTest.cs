using System.Collections.Generic;
using System.Linq;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class CssFileContentReaderTransformationTest : CssTestBase
    {
        [Fact]
        public void CheckFileContentReaderTransformation_OnValidCSSInput_ShouldReturnValidOutput()
        {
            string expectedResult = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n";

            ContentReaderTransformation.Process(BundleContext, Response);

            List<string> results = Response.BundleFiles.Values.Select(x => x.Content.ToString()).ToList();
            string result = string.Join("", results);
            bool isEmpty = string.IsNullOrEmpty(Response.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal(expectedResult, result);
        }
    }
}
