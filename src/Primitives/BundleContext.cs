using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling.Primitives
{
    public class BundleContext
    {
        private HashSet<string> _InputBundleFiles;
        ///// <summary>
        ///// Initializes a new instance of the <see cref="BundleContext"/> class.
        ///// </summary>
        ///// <param name="route">The bundle virtual path.</param>
        ///// <param name="fileProvider">The configured fileprovider.</param>
        ///// <param name="transformations">Transformations which will carry the real bundling.</param>
        public BundleContext(string route, IFileProvider fileProvider, List<IBundleTransform> transformations, Bundle bundle, bool enableWatch)
        {
            BundleRoute = route;
            FileProvider = fileProvider;
            Transforms = transformations;
            BundleCollection = BundleCollection.Instance;
            _InputBundleFiles = new HashSet<string>();
            Parent = bundle;
            ContentRaw = new StringBuilder(10000);
            IncludeDirectoryDataStructures = new List<IncludeDirectoryData>(3);
            InputCdns = new List<CdnObject>();
            EnableWatch = enableWatch;
        }

        internal void Init(IApplicationBuilder app, IWebHostEnvironment env, ILogger logger, string baseBundlingRoute, bool enableOptimizations, bool enableInstrumentations)
        {
            ApplicationBuilder = app;
            HostingEnvironment = env;
            Logger = logger;
            BaseBundlingRoute = baseBundlingRoute;
            EnableOptimizations = enableOptimizations;
            EnableInstrumentation = enableInstrumentations;
        }

        public string BundleRoute { get; internal set; }

        public Func<string> CustomPropertiesCallback { get; set; }

        public StringBuilder ContentRaw { internal get; set; }

        public IFileProvider FileProvider { get; internal set; }

        public IList<IBundleTransform> Transforms { get; internal set; }

        public IApplicationBuilder ApplicationBuilder { get; internal set; }

        internal void AddInputFiles(string[] virtualPaths)
        {
            foreach (string fileName in virtualPaths)
            {
                _InputBundleFiles.Add(fileName);
            }
        }

        internal List<CdnObject> InputCdns { get; set; }

        public IWebHostEnvironment HostingEnvironment { get; internal set; }

        public ILogger Logger { get; internal set; }

        public string BaseBundlingRoute { get; internal set; }

        /// <summary>
        /// Gets or sets whether instrumentation mode is enabled.
        /// </summary>
        public bool EnableInstrumentation { get; internal set; }

        /// <summary>
        /// Gets or sets whether the processed files should be watched for changes
        /// </summary>
        public bool EnableWatch { get; internal set; }

        /// <summary>
        /// Gets or sets whether optimizations are enabled via <see cref="BundleCollection.EnableOptimizations"/>
        /// </summary>
        public bool EnableOptimizations { get; internal set; }

        /// <summary>
        /// Gets the Http context for the request.
        /// </summary>
        /// <remarks>
        /// The value for HttpContext will generally be the current instance of <see cref="HttpContext"/>. However, 
        /// using the base wrapper class enables HttpContext to be mocked for unit testing.
        /// </remarks>
        public HttpContext HttpContext
        {
            get
            {
                return BundleCollection.Instance.HttpContextAccessor.HttpContext;
            }
        }

        public List<string> InputBundleFiles
        {
            get
            {
                return _InputBundleFiles.ToList();
            }
        }

        public Bundle Parent { get; private set; }

        /// <summary>
        /// Gets the bundle collection associated with the request.
        /// </summary>
        public BundleCollection BundleCollection { get; internal set; }

        internal void IncludeDirectory(string directoryVirtualPath, string searchPattern, PatternType patternType, bool searchSubdirectories)
        {
            IncludeDirectoryData includeDirectoryData = new IncludeDirectoryData
            {
                DirectoryVirtualPath = directoryVirtualPath,
                SearchPattern = searchPattern,
                SearchSubdirectories = searchSubdirectories,
                PatternType = patternType
            };
            IncludeDirectoryDataStructures.Add(includeDirectoryData);
        }

        public List<IncludeDirectoryData> IncludeDirectoryDataStructures { get; private set; }
        public string CdnPath { get; private set; }
        public string CdnFallbackExpression { get; private set; }

        public struct IncludeDirectoryData
        {
            public string DirectoryVirtualPath { get; internal set; }
            public string SearchPattern { get; internal set; }
            public bool SearchSubdirectories { get; internal set; }
            public PatternType PatternType { get; internal set; }
        }
    }
}