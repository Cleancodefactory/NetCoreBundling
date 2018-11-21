using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class IncludeDirectoryTransformationTest : IDisposable
    {
        private BundleContext _BundleContext;
        private BundleResponse _Response;

        private string _Dir = string.Empty;

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
            IncludeDirectoryTransformation idt = new IncludeDirectoryTransformation();
            IFileProvider provider = new PhysicalFileProvider(_Dir);

            _BundleContext = new BundleContext("kraft", provider, null, null);
            //_BundleContext.IncludeDirectory("\\", "*", Utils.PatternType.All, true);

            _Response = new BundleResponse(null);

            idt.Process(_BundleContext, _Response);

            Assert.Equal(2, _Response.BundleFiles.Count);
        }
    }
}
