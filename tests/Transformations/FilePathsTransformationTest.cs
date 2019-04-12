using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using System;
using System.Collections.Generic;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class FilePathsTransformationTest : CssTestBase
    {
        public FilePathsTransformationTest() : base(new List<IBundleTransform>() { new FilePathsTransformation() })
        {
        }

        [Fact]
        public void CheckFilePathTransformation_OnValidInput_ShouldReturnValidBundleFileCount()
        {
            BundleResponse bundleResponse = GetBundleResponse();

            int expected = 2;
            int actual = bundleResponse.BundleFiles.Count;

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void CheckFilePathTransformation_OnValidInput_ShouldReturnValidBundleFileContent()
        {
            string workspacewindowPath = @"/" + "bk-workspacewindow.css";
            string cssModuleContentPath = @"/" + "bk-module.css";
            List<string> paths = new List<string>() { workspacewindowPath, cssModuleContentPath };

            BundleResponse bundleResponse = GetBundleResponse();

            for (int i = 0; i < bundleResponse.BundleFiles.Count; i++)
            {
                bool isContains = bundleResponse.BundleFiles.ContainsKey(paths[i]);
                Assert.True(isContains);
            }
        }

        [Fact]
        public void CheckFilePathTransformation_OnNullResponse_ShouldThrowNullReferenceException()
        {
            Assert.Throws<ArgumentNullException>(() => Bundle.Transforms[0].Process(null, null));
        }
    }
}