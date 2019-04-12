using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Microsoft.Extensions.FileProviders;

namespace Ccf.Ck.Web.Bundling.Test.Transformations.Setup
{
    public abstract class TestBase : IDisposable
    {
        private const string BUNDLE_ROUTE = "testbundle";

        private string _ModuleFilename;
        private string _ModuleContent;
        private string _WorkspaceWindowFileName;
        private string _WorkspaceWindowContent;
        private readonly string _DirectoryName = Guid.NewGuid().ToString();

        protected TestBase(string moduleFilename, string moduleContent, string workspaceWindowFileName, string workspaceWindowContent)
        {
            _ModuleFilename = moduleFilename;
            _ModuleContent = moduleContent;
            _WorkspaceWindowFileName = workspaceWindowFileName;
            _WorkspaceWindowContent = workspaceWindowContent;
            Transformations = new List<IBundleTransform>();
            Initialize();
        }

        protected PhysicalFileProvider Provider { get; private set; }
        protected string BundleRoute => BUNDLE_ROUTE;
        protected List<IBundleTransform> Transformations { get; set; }
        protected string[] VirtualPaths { get; private set; }

        protected abstract Bundle Bundle { get; set; }

        private void Initialize()
        {
            Directory.CreateDirectory(_DirectoryName);            
            File.WriteAllText(Path.Combine(_DirectoryName, _ModuleFilename), _ModuleContent);
            File.WriteAllText(Path.Combine(_DirectoryName, _WorkspaceWindowFileName), _WorkspaceWindowContent);

            Provider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),_DirectoryName));

            var dirInfo = new DirectoryInfo(_DirectoryName);
            VirtualPaths = dirInfo.EnumerateFiles().Select(x => x.Name).ToArray();
        }

        protected BundleResponse GetBundleResponse ()
        {
            BundleResponse bundleResponse = new BundleResponse(Bundle);

            foreach (IBundleTransform transform in Bundle.Transforms)
            {
                transform.Process(Bundle.BundleContext, bundleResponse);
            }

            return bundleResponse;
        }

        public void Dispose()
        {
            if (Directory.Exists(_DirectoryName))
            {
                Directory.Delete(_DirectoryName, true);
            }
        }
    }
}
