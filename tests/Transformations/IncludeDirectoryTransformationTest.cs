using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using System.Collections.Generic;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class IncludeDirectoryTransformationTest : CssTestBase
    {
        public IncludeDirectoryTransformationTest() : base(new List<IBundleTransform>() { new IncludeDirectoryTransformation() })
        {
        }

        [Fact]
        public void IncludeDirectoryTransformationTest_OnValidResourceFiles_ShouldReturnValidBundledFilesCount()
        {
            BundleResponse bundleResponse = GetBundleResponse();

            int expected = 2;
            int actual = bundleResponse.BundleFiles.Count;

            Assert.Equal(expected, actual);
        }
    }
}
