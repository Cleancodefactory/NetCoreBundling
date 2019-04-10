using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class MinificationTransformationTest
    {
        private const string _CSSInput = "\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r";
        private const string _ExpectedCssResult = ".bk-desktop{overflow:auto;height:100%;min-width:30em;}";
        private const string _JSInput = "function WorkspaceDesktop() {\r\n    WorkspaceWindow.apply(this, arguments);\r\n};";
        private const string _ExpectedJsResult = "function WorkspaceDesktop(){WorkspaceWindow.apply(this,arguments)}";
        
        private BundleContext _BundleContext;
        private BundleResponse _Response;
        private Bundle _Bundle;
        private StringBuilder _InputContent = new StringBuilder();

        public MinificationTransformationTest()
        {
            _Response = new BundleResponse(null);
        }

        [Theory]
        [InlineData(_CSSInput, _ExpectedCssResult, false)]
        [InlineData(_JSInput, _ExpectedJsResult, true)]
        public void MinificationTransformation_OnValidInput_ShouldReturnNoErrorOutputAndValidResult(string input, string expectedResult ,bool isScriptBundle)
        {
            var m = new MinificationTransformation();
            var transformation = new List<IBundleTransform>();
            
            if (isScriptBundle)
            {
                _Bundle = new ScriptBundle("TestScript", null, null, transformation);
            }
            else
            {
                _Bundle = new StyleBundle("TestStyle", null, null, transformation);
            }
          
            _BundleContext = new BundleContext("kraftcss", null, null, _Bundle);

            var bundleFileValue = new BundleFile(_Bundle);

            _InputContent.Append(input);
            bundleFileValue.Content = _InputContent;

            var kvp = new Dictionary<string, BundleFile>();

            kvp["input"] = bundleFileValue;
            _BundleContext.GetType().GetProperty("EnableOptimizations").SetValue(_BundleContext, true);

            var type = _Response.GetType();

            type.GetProperty("BundleFiles").SetValue(_Response, kvp);
            m.Process(_BundleContext, _Response);

            var result = _Response.Content.ToString();
            var isContainsError = !string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.Equal(expectedResult, result);
            Assert.False(isContainsError);
        }
    }
}
