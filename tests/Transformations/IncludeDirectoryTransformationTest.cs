using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.Extensions.FileProviders;
using Moq;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class IncludeDirectoryTransformationTest : IDisposable
    {
        private BundleContext _BundleContext;
        private BundleResponse _Response;
        private string _Dir;

        public IncludeDirectoryTransformationTest()
        {
            _Dir = Path.Combine(Directory.GetCurrentDirectory(), "IncludeDirectoryTransformationTestFolder");
            Directory.CreateDirectory(_Dir);
            File.WriteAllText(Path.Combine(_Dir, "File.css"), "qwerty");
            File.WriteAllText(Path.Combine(_Dir, "Style.css"), "Test");
        }

        public void Dispose()
        {
            if (Directory.Exists(_Dir))
            {
                Directory.Delete(_Dir, true);
            }
        }

        [Fact]
        public void IncludeDirectoryTransformationTest_OnValidResourceFiles_ShouldReturnValidBundledFilesCount()
        {
            var idt = new IncludeDirectoryTransformation();
            IFileProvider provider = new PhysicalFileProvider(_Dir);
            var bundleContextMock = new Mock<BundleContext>("kraft", provider, null, null);

            _BundleContext = new BundleContext("kraft", provider, null, null);

            var type = _BundleContext.GetType();
            var methodInfo = type.GetMethod("IncludeDirectory", BindingFlags.Instance | BindingFlags.NonPublic);

            methodInfo.Invoke(_BundleContext,new object[] { "\\", "*", PatternType.All, true}); 
            _Response = new BundleResponse(null);
            idt.Process(_BundleContext, _Response);

            Assert.Equal(2, _Response.BundleFiles.Count);
        }
    }
}
