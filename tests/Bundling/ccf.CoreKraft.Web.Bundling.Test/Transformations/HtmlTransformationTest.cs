using ccf.CoreKraft.Web.Bundling.Primitives;
using ccf.CoreKraft.Web.Bundling.Transformations;
using Microsoft.Extensions.Logging;
using System.Text;
using Xunit;

namespace ccf.CoreKraft.Web.Bundling.Test.Transformations
{
    public class HtmlTransformationTest
    {
        private BundleContext _BundleContext;
        private BundleResponse _Response;

        public HtmlTransformationTest()
        {
            _BundleContext = new BundleContext("kraftcss", null, null, null);
            _Response = new BundleResponse(null);
        }

        [Fact]
        public void CheckHtmlTransformation_OnValidHtmlInput_ShouldReturnNoError()
        {
            HtmlTransformation ht = new HtmlTransformation();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>BindKraft</title></head><body></body></html>");

            StringBuilder result = new StringBuilder();
            result = ht.Process(sb, null, _Response);
            bool isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal("<title>BindKraft</title>", result.ToString());
        }

        [Theory]
        [InlineData("<html><head></head><body></body> <<</html>")]
        [InlineData("<html>Test><<body")]
        [InlineData("<html><head></head><test>><<t></html>")]
        public void CheckHtmlTransformation_OnInvalidHtmlInput_ShouldReturnTransformationError(string input)
        {
             HtmlTransformation ht = new HtmlTransformation();

            StringBuilder sb = new StringBuilder();
            sb.Append(input);
            
            ILoggerFactory loggerFac = new LoggerFactory();
            _BundleContext.Logger = loggerFac.CreateLogger("TestLogger");

            ht.Process(sb, _BundleContext.Logger, _Response);
            bool isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.False(isEmpty);
        }
    }
}
