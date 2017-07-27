using ccf.CoreKraft.Web.Bundling.Interfaces;
using ccf.CoreKraft.Web.Bundling.Primitives;
using ccf.CoreKraft.Web.Bundling.Transformations;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ccf.CoreKraft.Web.Bundling.Test.Transformations
{
    public class FileContentReaderTransformationTest : IDisposable
    {
        private string _ModuleCSSContent = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n";
        private string _WorkspacewindowCSSContent = "{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n";

        private string _ModuleCSS = "bk-module.css";
        private string _WorkspacewindowCSS = "bk-workspacewindow.css";
        private string _DirName = "TestFolder\\";
        
        private BundleContext _BundleContext;
        private BundleResponse _Response;
        FileContentReaderTransformation _ContentReaderTransformation;

        public FileContentReaderTransformationTest()
        {
            _Response = new BundleResponse();
            _ContentReaderTransformation = new FileContentReaderTransformation();

            string moduleFullPath = Path.Combine(_DirName, _ModuleCSS);
            string workspacewindowFullPath = Path.Combine(_DirName, _WorkspacewindowCSS);

            BundleFile bf = new BundleFile();
            bf.PhysicalPath = moduleFullPath;

            BundleFile bf2 = new BundleFile();
            bf2.PhysicalPath = workspacewindowFullPath;

            _Response.BundleFiles.Add("bundle-BK", bf);
            _Response.BundleFiles.Add("bundle-BK2", bf2);
            
            Directory.CreateDirectory(_DirName);
           
            File.WriteAllText(moduleFullPath, _ModuleCSSContent);
            File.WriteAllText(workspacewindowFullPath, _WorkspacewindowCSSContent);
        }
        
        [Fact]
        public void CheckFileContentReaderTransformation_OnValidCSSInput_ShouldReturnValidOutput()
        {
            string expectedResult = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n\r\n{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n\r\n";
            _BundleContext = new BundleContext("kraftcss", null, null, null);
            _BundleContext.EnableOptimizations = true;

            _ContentReaderTransformation.Process(_BundleContext, _Response);

            string result = _Response.Content.ToString();
            bool isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal(expectedResult, result);
        }
        
        [Fact]
        public void CheckFileContentReaderTransformation_OnValidScriptInput_ShouldReturnValidOutput()
        {
            string expectedResult = "/*\r\n    #using \"./ bindkraft -public-profile.css\"\r\n*/\r\n\r\n{/* GENERIC */\r\n\r\n/* desktop */\r\n.bk-desktop {\r\noverflow: auto;\r\nheight: 100%;\r\nmin-width:30em;\r\n}\r\n\r\n";

            List<IBundleTransform> transformation = new List<IBundleTransform>();
            IFileProvider fileProvider = null;
            Bundle sb = new ScriptBundle(_DirName, fileProvider, transformation);
            _BundleContext = new BundleContext("kraftcss", null, null, sb);
            _BundleContext.EnableOptimizations = true;

            _ContentReaderTransformation.Process(_BundleContext, _Response);

            string result = _Response.Content.ToString();
            bool isEmpty = string.IsNullOrEmpty(_Response.TransformationErrors.ToString());

            Assert.True(isEmpty);
            Assert.Equal(expectedResult, result);
        }
        
        public void Dispose()
        {
            if (Directory.Exists(_DirName))
            {
                Directory.Delete(_DirName, true);
            }
        }

    }
}
