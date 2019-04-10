using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class FilePathsTransformationTest
    {
        private string _ModuleCSS = "bk-module.css";
        private string _WorkspacewindowCSS = "bk-workspacewindow.css";
        private string _DirName = "TestFolder\\";

        [Fact]
        public void CheckFilePathTransformation_OnValidInput_ShouldReturnValidBundelFileCountAndContent()
        {
            var path = "some path";
            var virtualPath = new HashSet<string>() { $"{_DirName}{_ModuleCSS}", $"{_DirName}{_WorkspacewindowCSS}" };
            var fileInfoMock = new Mock<IFileInfo>();

            fileInfoMock.Setup(x => x.PhysicalPath).Returns(path);

            var fileInfo = fileInfoMock.Object;
            var fileProviderMock = new Mock<IFileProvider>();

            fileProviderMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(fileInfo);

            var fileProvider = fileProviderMock.Object;
            var bundle = new StyleBundle(path, fileProvider);
            var response = new BundleResponse(bundle);
            var bundleContextMock = new Mock<BundleContext>("test", fileProvider, null, bundle);
            var bundleContext = Activator.CreateInstance(typeof(BundleContext),new object[] { "test", fileProvider, null, bundle });
            var type = bundleContext.GetType();
            var prop = type.GetField("_InputBundleFiles", BindingFlags.Instance | BindingFlags.NonPublic);

            prop.SetValue(bundleContext, virtualPath);
            
            var filePatshTreansformation = new FilePathsTransformation();

            filePatshTreansformation.Process((BundleContext)bundleContext, response);

            var expectedResult = 2;
            var bundleFilesCount = response.BundleFiles.Count;

            Assert.Equal(bundleFilesCount, expectedResult);

            var virtualPathList = virtualPath.ToList();

            for (int i = 0; i < response.BundleFiles.Count; i++)
            {
                bool isContains = response.BundleFiles.ContainsKey($"/{virtualPathList[i]}");
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