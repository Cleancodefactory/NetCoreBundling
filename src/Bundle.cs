﻿using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.Web.Bundling.Enumerations;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Resources;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ccf.Ck.Libs.Web.Bundling
{
    public abstract class Bundle
    {
        private static readonly object _Lock = new object();

        #region Ctors
        internal Bundle(string route) : this(route, BundleCollection.Instance.HostingEnvironment.WebRootFileProvider, null, new List<IBundleTransform>(), true)
        { }

        internal Bundle(string route, string versionFile) : this(route, BundleCollection.Instance.HostingEnvironment.WebRootFileProvider, null, new List<IBundleTransform>(), true)
        {
            try
            {
                string fileInfoExisting = BundleCollection.Instance.HostingEnvironment.WebRootFileProvider.GetFileInfo(versionFile).PhysicalPath;
                using (FileStream fs = new FileStream(fileInfoExisting, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        ExternalVersion = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                KraftLogger.LogError($"Version file: {versionFile} throws an exception: {ex.Message}", ex);
            }
        }

        internal Bundle(string route, IFileProvider fileProvider) : this(route, fileProvider, null, new List<IBundleTransform>(), true)
        { }

        internal Bundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback) : this(route, fileProvider, customPropertiesCallback, new List<IBundleTransform>(), true)
        { }

        internal Bundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback, List<IBundleTransform> transformations, bool enableWatch)
        {
            BundleContext = new BundleContext(route, fileProvider, transformations, this, enableWatch)
            {
                CustomPropertiesCallback = customPropertiesCallback
            };
            if (BundleContext.Transforms.Count == 0)
            {
                //add the default transformations
                BundleContext.Transforms.Add(new FilePathsTransformation());
                BundleContext.Transforms.Add(new IncludeDirectoryTransformation());
                BundleContext.Transforms.Add(new FileContentReaderTransformation());
                //BundleContext.Transforms.Add(new LessTransformation());
                //BundleContext.Transforms.Add(new BabelTransformation());
                BundleContext.Transforms.Add(new MinificationTransformation());
            }
            Route = FileUtility.RemoveFirstOccurenceSpecialCharacters(route, FileUtility.EStartPoint.FromStart, new char[] { '/', '\\' });
            HttpCacheability = HttpCacheability.Public;
            MaxAge = TimeSpan.FromDays(365);
        }
        #endregion Ctors

        #region Public
        /// <summary>
        /// Add your own transformations in the order you want them to be executed
        /// </summary>
        public IList<IBundleTransform> Transforms
        {
            get
            {
                return BundleContext.Transforms;
            }
        }

        /// <summary>
        /// Include only virtual paths. The lib will calculate the physical path with help of the virtual provider
        /// </summary>
        /// <param name="virtualPaths"></param>
        /// <returns></returns>
        public virtual Bundle Include(params string[] virtualPaths)
        {
            InputFile[] inputFiles = new InputFile[virtualPaths.Length];
            for (int i = 0; i < virtualPaths.Length; i++)
            {
                InputFile inputFile = new InputFile { VirtualPath = virtualPaths[i] };
                inputFiles[i] = inputFile;
            }
            BundleContext.AddInputFiles(inputFiles);
            return this;
        }

        /// <summary>
        /// Include files with physical path or virtual path definded
        /// </summary>
        /// <param name="inputFiles"></param>
        /// <returns></returns>
        public virtual Bundle Include(params InputFile[] inputFiles)
        {
            BundleContext.AddInputFiles(inputFiles);
            return this;
        }

        /// <summary>
        /// Add raw content which will be added at the end of the produced output
        /// </summary>
        /// <param name="contentRaw"></param>
        /// <returns></returns>
        public virtual Bundle IncludeContent(string contentRaw)
        {
            BundleContext.ContentRaw.Append(contentRaw);
            return this;
        }

        /// <summary>
        /// Support for CDNs
        /// </summary>
        /// <param name="cdn"></param>
        /// <returns></returns>
        public virtual Bundle IncludeCdn(CdnObject cdn)
        {
            BundleContext.InputCdns.Add(cdn);
            return this;
        }

        /// <summary>
        /// Defaults:
        /// HttpCacheability = HttpCacheability.Public;
        /// MaxAge = TimeSpan.FromDays(365);
        /// </summary>
        /// <param name="httpCacheability">public, private</param>
        /// <param name="maxAge">Cache-Control: max-age=<seconds></param>
        /// <returns></returns>
        public virtual Bundle Caching(HttpCacheability httpCacheability, TimeSpan maxAge)
        {
            HttpCacheability = httpCacheability;
            MaxAge = maxAge;
            return this;
        }

        public void RemoveTransformationType(Type transformationType)
        {
            IBundleTransform transformation = Transforms.First(i => i.GetType().IsAssignableFrom(transformationType));
            Transforms?.Remove(transformation);
        }

        /// <summary>
        /// Includes all files in a directory that match a search pattern.
        /// </summary>
        /// <param name="directoryVirtualPath">The virtual path to the directory from which to search for files.</param>
        /// <param name="searchPattern">The search pattern to use in selecting files to add to the bundle.</param>
        /// <returns>The <see cref="Bundle"/> object itself for use in subsequent method chaining.</returns>
        public virtual Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern)
        {
            return IncludeDirectory(directoryVirtualPath, searchPattern, searchSubdirectories: false);
        }

        /// <summary>
        /// Includes all files in a directory that match a search pattern.
        /// </summary>
        /// <param name="directoryVirtualPath">The virtual path to the directory from which to search for files.</param>
        /// <param name="searchPattern">The search pattern to use in selecting files to add to the bundle.</param>
        /// <param name="searchSubdirectories">Specifies whether to recursively search subdirectories of <paramref name="directoryVirtualPath"/>.</param>
        /// <returns>The <see cref="Bundle"/> object itself for use in subsequent method chaining.</returns>
        public virtual Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern, bool searchSubdirectories)
        {
            if (ExceptionUtil.IsPureWildcardSearchPattern(searchPattern))
            {
                throw new ArgumentException(BundlingResources.InvalidWildcardSearchPattern, nameof(searchPattern));
            }
            PatternType patternType = PatternHelper.GetPatternType(searchPattern);
            Exception error = PatternHelper.ValidatePattern(patternType, searchPattern, "virtualPaths");
            if (error != null)
            {
                throw error;
            }
            BundleContext.IncludeDirectory(directoryVirtualPath, searchPattern, patternType, searchSubdirectories);
            return this;
        }

        public abstract string ContentType { get; }
        public string ExternalVersion { get; internal set; }
        public static string VERSION_INTERNAL_REPLACEMENT
        {
            get
            {
                return "@@@version@@@";
            }
        }
        #endregion Public

        #region Internal 
        internal BundleContext BundleContext { get; private set; }

        internal BundleResponse ExecuteTransformations()
        {
            BundleResponse bundleResponse = GetFromCache();

            if (bundleResponse == null)
            {
                lock (_Lock)
                {
                    bundleResponse = new BundleResponse(this);
                    try
                    {
                        foreach (IBundleTransform transformation in BundleContext.Transforms)
                        {
                            if (transformation != null)
                            {
                                transformation.Process(BundleContext, bundleResponse);
                            }
                        }
                        //make sure that even without transformation the input goes into the output
                        if (bundleResponse.ContentRaw != null && bundleResponse.ContentRaw.Length == 0)
                        {
                            bundleResponse.ContentRaw = BundleContext.ContentRaw;
                        }
                        bundleResponse.InputCdns = BundleContext.InputCdns;
                        AddToCache(bundleResponse);
                    }
                    catch (Exception ex)
                    {
                        bundleResponse.TransformationErrors.Append($"Error in ExecuteTransformations(): {ex.Message}<br />Error details: {ex.StackTrace}").Append("<br />");
                        throw;
                    }

                }
            }
            return bundleResponse;
        }

        internal string Route { get; }
        internal HttpCacheability HttpCacheability { get; private set; }
        internal TimeSpan MaxAge { get; private set; }
        #endregion Internal

        #region Private
        private void AddToCache(BundleResponse bundleResponse)
        {
            IMemoryCache memoryCache = BundleContext.ApplicationBuilder.ApplicationServices.GetRequiredService<IMemoryCache>();
            memoryCache?.Set(Route, bundleResponse);
        }

        internal void RemoveFromCache()
        {
            try
            {
                IMemoryCache memoryCache = BundleContext.ApplicationBuilder.ApplicationServices.GetService<IMemoryCache>();

                BundleResponse bundleResponse = GetFromCache();
                if (bundleResponse != null)
                {
                    foreach (var bundleFile in bundleResponse.BundleFiles)
                    {
                        //KraftLogger.LogError("PhysicalPath: " + bundleFile.Value.PhysicalPath);
                        //KraftLogger.LogError("VirtualPath: " + bundleFile.Value.VirtualPath);
                        bundleFile.Value.CleanUpEvents();
                    }
                }
                memoryCache?.Remove(Route);
            }
            catch (Exception)
            {
                //do nothing probably services already disposed
            }            
        }

        private BundleResponse GetFromCache()
        {
            IMemoryCache memoryCache = BundleContext.ApplicationBuilder.ApplicationServices.GetRequiredService<IMemoryCache>();
            if (memoryCache != null && memoryCache.TryGetValue(Route, out BundleResponse bundleResponse))
            {
                return bundleResponse;
            }
            return null;
        }
        #endregion Private
    }

    public class StyleBundle : Bundle
    {
        public StyleBundle(string route) :
            base(route)
        { }

        public StyleBundle(string route, string versionFile) :
           base(route, versionFile)
        { }

        public StyleBundle(string route, IFileProvider fileProvider) :
            base(route, fileProvider, null, new List<IBundleTransform>(), true)
        { }

        public StyleBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback) :
            base(route, fileProvider, customPropertiesCallback, new List<IBundleTransform>(), true)
        { }

        public StyleBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback, List<IBundleTransform> transformations, bool enableWatch) :
            base(route, fileProvider, customPropertiesCallback, transformations, enableWatch)
        { }

        public override string ContentType
        {
            get
            {
                return "text/css";
            }
        }
    }

    public class ScriptBundle : Bundle
    {
        public ScriptBundle(string route) : base(route)
        { }

        public ScriptBundle(string route, string versionFile) :
            base(route, versionFile)
        { }

        public ScriptBundle(string route, IFileProvider fileProvider) :
            base(route, fileProvider, null, new List<IBundleTransform>(), true)
        { }

        public ScriptBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback) :
            base(route, fileProvider, customPropertiesCallback, new List<IBundleTransform>(), true)
        { }

        public ScriptBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback, List<IBundleTransform> transformations, bool enableWatch) :
            base(route, fileProvider, customPropertiesCallback, transformations, enableWatch)
        { }

        public override string ContentType
        {
            get
            {
                return "text/javascript";
            }
        }
    }
}
