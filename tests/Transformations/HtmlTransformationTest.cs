using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
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
            var ht = new HtmlTransformation();

            var sb = new StringBuilder();
            sb.Append("<html><head><title>BindKraft</title></head><body></body></html>");

            var result = new StringBuilder();

            result = ht.Process(sb, null, _Response);

            var isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal("<title>BindKraft</title>", result.ToString());
        }

        [Theory]
        [InlineData("<html><head></head><body></body> <<</html>")]
        [InlineData("<html>Test><<body")]
        [InlineData("<html><head></head><test>><<t></html>")]
        public void CheckHtmlTransformation_OnInvalidHtmlInput_ShouldReturnTransformationError(string input)
        {
            var ht = new HtmlTransformation();

            var sb = new StringBuilder();

            sb.Append(input);
            
            ILoggerFactory loggerFac = new LoggerFactory();
            var logger = loggerFac.CreateLogger("TestLogger");

            ht.Process(sb, logger, _Response);

            var isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.False(isEmpty);
        }
    }
}
