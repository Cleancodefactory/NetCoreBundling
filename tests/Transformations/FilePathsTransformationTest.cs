using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class FilePathsTransformationTest
    {
        private string _ModuleCSS = "bk-module.css";
        private string _WorkspacewindowCSS = "bk-workspacewindow.css";
        private string _DirName = "TestFolder\\";

        private BundleContext _BundleContext;
        private BundleResponse _Response;

        public FilePathsTransformationTest()
        {
            _BundleContext = new BundleContext("kraftcss", null, null, null);
            _Response = new BundleResponse(null);
        }

        [Fact]
        public void CheckFilePathTransformation_OnValidInput_ShouldReturnValidBundelFileCountAndContent()
        {
            int expectedResult = 2;
            string path = Directory.GetCurrentDirectory();
            IFileProvider fileProvider = new PhysicalFileProvider(path);

            string[] virtualPath = { $"{_DirName}{_ModuleCSS}", $"{_DirName}{_WorkspacewindowCSS}" };

            //_BundleContext.AddInputFiles(virtualPath);
            //_BundleContext.FileProvider = fileProvider;

            FilePathsTransformation f = new FilePathsTransformation();
            f.Process(_BundleContext, _Response);

            int result = _Response.BundleFiles.Count;

            Assert.Equal(result, expectedResult);

            for (int i = 0; i < _Response.BundleFiles.Count; i++)
            {
                bool isContains = _Response.BundleFiles.ContainsKey($"/{virtualPath[i]}");
                Assert.True(isContains);
            }
        }

        [Fact]
        public void CheckFilePathTransformation_OnNullResponse_ShouldThrowNullReferenceException()
        {
            FilePathsTransformation f = new FilePathsTransformation();
            string[] virtualPath = { $"{_DirName}{_ModuleCSS}", $"{_DirName}{_WorkspacewindowCSS}" };
            //_BundleContext.AddInputFiles(virtualPath);

            Assert.Throws<NullReferenceException>(() => f.Process(_BundleContext, _Response));
            Assert.True(!string.IsNullOrEmpty(_Response.TransformationErrors.ToString()));

        }
    }
}
