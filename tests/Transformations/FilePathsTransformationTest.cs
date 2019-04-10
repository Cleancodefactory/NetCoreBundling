using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Web.Bundling.Test.Transformations.Setup;
using System;
using System.Collections.Generic;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class FilePathsTransformationTest : CssTestBase
    {
        [Fact]
        public void CheckFilePathTransformation_OnValidInput_ShouldReturnValidBundelFileCountAndContent()
        {
            int expectedResult = 2;
            int bundleFilesCount = Response.BundleFiles.Count;

            Assert.Equal(bundleFilesCount, expectedResult);


            string workspacewindowPath = DirectoryName + @"\" + "bk-workspacewindow.css";
            string cssModuleContentPath = DirectoryName + @"\" + "bk-module.css";
            List<string> paths = new List<string>() { workspacewindowPath, workspacewindowPath };

            for (int i = 0; i < Response.BundleFiles.Count; i++)
            {
                bool isContains = Response.BundleFiles.ContainsKey(paths[i]);
                Assert.True(isContains);
            }
        }

        [Fact]
        public void CheckFilePathTransformation_OnNullResponse_ShouldThrowNullReferenceException()
        {
            FilePathsTransformation fileTransformation = new FilePathsTransformation();

            Assert.Throws<ArgumentNullException>(() => fileTransformation.Process(null, null));
        }
    }
}