using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Ccf.Ck.Web.Bundling.Test.Transformations
{
    public class FileContentReaderTransformationTest
    {
        private string _DirName = "TestFolder\\";
        private BundleContext _BundleContext;
        private BundleResponse _Response;
        private FileContentReaderTransformation _ContentReaderTransformation;

        public FileContentReaderTransformationTest()
        {
            _Response = new BundleResponse(null);
            _ContentReaderTransformation = new FileContentReaderTransformation();
        }

        [Fact]
        public void CheckFileContentReaderTransformation_OnValidCSSInput_ShouldReturnValidOutput()
        {
            var expectedResult = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n";
            var moduleCssContent = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n";
            var workspacewindowContent = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n";
            var fileName = "bk-module.css";
            var workspacewindow = "bk-workspacewindow.css";

            InitializeDirectoryAndFiles(moduleCssContent, workspacewindowContent, fileName, workspacewindow);

            var bundleFiles = CreateBundleFiles(fileName, workspacewindow);

            AddBundleFilesToResponse(bundleFiles, "bundle-BK", "bundle-BK2");

            CheckFileContentReaderTransformation("kraftcss", expectedResult);

            DeleteDirectoryIfExists();
        }

        [Fact]
        public void CheckFileContentReaderTransformation_OnValidScriptInput_ShouldReturnValidOutput()
        {
            var expectedResult = "/*\r\n    #using \"./ bindkraft -public-profile.js\"\r\n*/\r\n{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\nconsole.log();\r\n}\r\n";
            var moduleJsContent = "/*\r\n    #using \"./ bindkraft -public-profile.js\"\r\n*/\r\n";
            var workspacewindowContent = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\nconsole.log();\r\n}\r\n";
            var fileName = "bk-module.js";
            var workspacewindow = "bk-workspacewindow.css";

            InitializeDirectoryAndFiles(moduleJsContent, workspacewindowContent, fileName, workspacewindow);

            var bundleFiles = CreateBundleFiles(fileName, workspacewindow);

            AddBundleFilesToResponse(bundleFiles, "bundle-BK", "bundle-BK2");

            CheckFileContentReaderTransformation("kraftjs", expectedResult);

            DeleteDirectoryIfExists();
        }

        private void CheckFileContentReaderTransformation(string route, string expectedResult)
        {
            var transformation = new List<IBundleTransform>();
            IFileProvider fileProvider = null;
            var sb = new ScriptBundle(_DirName, fileProvider, null, transformation);

            _BundleContext = new BundleContext(route, null, null, sb);
            //_BundleContext.EnableOptimizations = true;

            _ContentReaderTransformation.Process(_BundleContext, _Response);

            var results = _Response.BundleFiles.Values.Select(x => x.Content);
            var result = string.Join("", results);
            var isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal(expectedResult, result);
        }

        private void DeleteDirectoryIfExists()
        {
            if (Directory.Exists(_DirName))
            {
                Directory.Delete(_DirName, true);
            }
        }

        private void CreateFilesIfNotExists(string moduleFullPath, string workspacewindowFullPath, string moduleContent, string workspacewindowContent)
        {
            File.WriteAllText(moduleFullPath, moduleContent);
            File.WriteAllText(workspacewindowFullPath, workspacewindowContent);
        }

        private void InitializeDirectoryAndFiles(string moduleContext, string workspacewindowContent, string fileName, string workspacewindow)
        {
            var moduleFullPath = Path.Combine(_DirName, fileName);
            var workspacewindowFullPath = Path.Combine(_DirName, workspacewindow);

            Directory.CreateDirectory(_DirName);

            CreateFilesIfNotExists(moduleFullPath, workspacewindowFullPath, moduleContext, workspacewindowContent);
        }

        private List<BundleFile> CreateBundleFiles(string fileName, string workspacewindow)
        {
            var moduleFullPath = Path.Combine(_DirName, fileName);
            var workspacewindowFullPath = Path.Combine(_DirName, workspacewindow);
            var bundleFiles = new List<BundleFile>();
            var bf1 = new BundleFile(null);

            bf1.PhysicalPath = moduleFullPath;

            var bf2 = new BundleFile(null);
            bf2.PhysicalPath = workspacewindowFullPath;

            bundleFiles.Add(bf1);
            bundleFiles.Add(bf2);

            return bundleFiles;
        }

        private void AddBundleFilesToResponse(List<BundleFile> bundleFiles, params string[] keys)
        {
            for (int i = 0; i < bundleFiles.Count; i++)
            {
                _Response.BundleFiles.Add(keys[i], bundleFiles[i]);
            }
        }
    }
}
