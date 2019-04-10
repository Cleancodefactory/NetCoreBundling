using System;
using System.Collections.Generic;
using System.IO;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public abstract class TestBase : IDisposable
    {
        private const string DIRECTORY_NAME = "TestFolder\\";
        private const string BUNDLE_ROUTE = "kraftjs";

        private string _ModuleContent;
        private string _WorkspaceWindowFileName;
        private string _WorkspaceWindowContent;
        private string _Filename;
        private string _DirectoryName;
        private string _BundleRoute;

        protected TestBase(string ModuleContent, string WorkspaceWindowFileName, string WorkspaceWindowContent, string Filename, string DirectoryName = DIRECTORY_NAME, string BundleRoute = BUNDLE_ROUTE)
        {
            _ModuleContent = ModuleContent;
            _WorkspaceWindowFileName = WorkspaceWindowFileName;
            _WorkspaceWindowContent = WorkspaceWindowContent;
            _Filename = Filename;
            _DirectoryName = DirectoryName;
            _BundleRoute = BundleRoute;
            Initialize();
        }

        protected abstract Bundle Bundle { get; set; }
        protected BundleResponse Response { get; set; }
        protected FileContentReaderTransformation ContentReaderTransformation { get; set; }
        protected string DirectoryName => _DirectoryName;
        protected BundleContext BundleContext { get; set; }

        private void Initialize()
        {
            //Add Bundle Files to Response
            Directory.CreateDirectory(_DirectoryName);

            File.WriteAllText(Path.Combine(_DirectoryName, _Filename), _ModuleContent);
            File.WriteAllText(Path.Combine(_DirectoryName, _WorkspaceWindowFileName), _WorkspaceWindowContent);

            string[] filePaths = Directory.GetFiles(_DirectoryName);

            Response = new BundleResponse(null);
            for (int i = 0; i < filePaths.Length; i++)
            {
                BundleFile bFile = new BundleFile(null);
                bFile.PhysicalPath = filePaths[i];

                Response.BundleFiles.Add(filePaths[i], bFile);
            }

            ContentReaderTransformation = new FileContentReaderTransformation();

            ScriptBundle sb = new ScriptBundle(_DirectoryName, null, null, new List<IBundleTransform>());
            BundleContext = new BundleContext(_BundleRoute, null, null, Bundle);
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
