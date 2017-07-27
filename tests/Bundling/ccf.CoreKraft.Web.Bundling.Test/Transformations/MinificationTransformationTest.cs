using ccf.CoreKraft.Web.Bundling.Interfaces;
using ccf.CoreKraft.Web.Bundling.Primitives;
using ccf.CoreKraft.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ccf.CoreKraft.Web.Bundling.Test.Transformations
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
            _Response = new BundleResponse();
        }

        [Theory]
        [InlineData(_CSSInput, _ExpectedCssResult, false)]
        [InlineData(_JSInput, _ExpectedJsResult, true)]
        public void MinificationTransformation_OnValidInput_ShouldReturnNoErrorOutputAndValidResult(string input, string expectedResult ,bool isScriptBundle)
        {
            MinificationTransformation m = new MinificationTransformation();
            List<IBundleTransform> transformation = new List<IBundleTransform>();
            IFileProvider fileProvider = null;
            
            if (isScriptBundle)
            {
                _Bundle = new ScriptBundle("TestScript", fileProvider, transformation);
            }
            else
            {
                _Bundle = new StyleBundle("TestStyle", fileProvider, transformation);
            }
          
            _BundleContext = new BundleContext("kraftcss", null, null, _Bundle);
            _BundleContext.EnableOptimizations = true;

            _InputContent.Append(input);
            _Response.Content = _InputContent;
            
            m.Process(_BundleContext, _Response);

            string result = _Response.Content.ToString();
            bool isContainsError = !string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.Equal(expectedResult, result);
            Assert.False(isContainsError);
        }
    }
}
