using Ccf.Ck.Libs.Logging;
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
using System.Linq;

namespace Ccf.Ck.Libs.Web.Bundling
{
    public abstract class Bundle
    {
        private static readonly object _Lock = new object();

        #region Ctors
        internal Bundle(string route) : this(route, BundleCollection.Instance.HostingEnvironment.WebRootFileProvider, null, new List<IBundleTransform>())
        { }

        internal Bundle(string route, IFileProvider fileProvider) : this(route, fileProvider, null, new List<IBundleTransform>())
        { }

        internal Bundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback) : this(route, fileProvider, customPropertiesCallback, new List<IBundleTransform>())
        { }

        internal Bundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback, List<IBundleTransform> transformations)
        {
            BundleContext = new BundleContext(route, fileProvider, transformations, this);
            BundleContext.CustomPropertiesCallback = customPropertiesCallback;
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
        public IList<IBundleTransform> Transforms
        {
            get
            {
                return BundleContext.Transforms;
            }
        }

        public virtual Bundle Include(params string[] virtualPaths)
        {
            BundleContext.AddInputFiles(virtualPaths);
            return this;
        }

        public virtual Bundle IncludeContent(string contentRaw)
        {
            BundleContext.ContentRaw.Append(contentRaw);
            return this;
        }

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
                throw new ArgumentException(BundlingResources.InvalidWildcardSearchPattern, "searchPattern");
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
            IMemoryCache memoryCache = BundleContext.ApplicationBuilder.ApplicationServices.GetRequiredService<IMemoryCache>();
            
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

        private BundleResponse GetFromCache()
        {
            IMemoryCache memoryCache = BundleContext.ApplicationBuilder.ApplicationServices.GetRequiredService<IMemoryCache>();
            BundleResponse bundleResponse;
            if (memoryCache != null && memoryCache.TryGetValue(Route, out bundleResponse))
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

        public StyleBundle(string route, IFileProvider fileProvider) :
            base(route, fileProvider, null, new List<IBundleTransform>())
        { }

        public StyleBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback) :
            base(route, fileProvider, customPropertiesCallback, new List<IBundleTransform>())
        { }

        public StyleBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback, List<IBundleTransform> transformations) :
            base(route, fileProvider, customPropertiesCallback, transformations)
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

        public ScriptBundle(string route, IFileProvider fileProvider) :
            base(route, fileProvider, null, new List<IBundleTransform>())
        { }

        public ScriptBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback) :
            base(route, fileProvider, customPropertiesCallback, new List<IBundleTransform>())
        { }

        public ScriptBundle(string route, IFileProvider fileProvider, Func<string> customPropertiesCallback, List<IBundleTransform> transformations) :
            base(route, fileProvider, customPropertiesCallback, transformations)
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
