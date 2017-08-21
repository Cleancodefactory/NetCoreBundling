using ccf.CoreKraft.Web.Bundling.Interfaces;
using ccf.CoreKraft.Web.Bundling.Primitives;
using ccf.CoreKraft.Web.Bundling.Resources;
using ccf.CoreKraft.Web.Bundling.Transformations;
using ccf.CoreKraft.Web.Bundling.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;

namespace ccf.CoreKraft.Web.Bundling
{
    public abstract class Bundle
    {
        private string _Route;
        private BundleResponse _BundleResponse;
        private static readonly object _Lock = new object();

        #region Ctors
        internal Bundle(string route) : this(route, BundleCollection.Instance.HostingEnvironment.WebRootFileProvider, new List<IBundleTransform>())
        { }

        internal Bundle(string route, IFileProvider fileProvider) : this(route, fileProvider, new List<IBundleTransform>())
        { }

        internal Bundle(string route, IFileProvider fileProvider, List<IBundleTransform> transformations)
        {
            BundleContext = new BundleContext(route, fileProvider, transformations, this);
            if (BundleContext.Transforms.Count == 0)
            {
                //add the default transformations
                BundleContext.Transforms.Add(new FilePathsTransformation());
                BundleContext.Transforms.Add(new IncludeDirectoryTransformation());
                BundleContext.Transforms.Add(new FileContentReaderTransformation());
                BundleContext.Transforms.Add(new MinificationTransformation());
            }
            _Route = FileUtility.RemoveFirstOccurenceSpecialCharacters(route, FileUtility.EStartPoint.FromStart, new char[] { '/', '\\' });
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

        public virtual Bundle Include(string contentRaw)
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
            _BundleResponse = GetFromCache();
            if (_BundleResponse == null)
            {
                lock (_Lock)
                {
                    _BundleResponse = new BundleResponse(this);
                    foreach (IBundleTransform transformation in BundleContext.Transforms)
                    {
                        if (transformation != null)
                        {
                            transformation.Process(BundleContext, _BundleResponse);
                        }
                    }
                    //make sure that even without transformation the input goes into the output
                    if (_BundleResponse.ContentRaw != null && _BundleResponse.ContentRaw.Length == 0)
                    {
                        _BundleResponse.ContentRaw = BundleContext.ContentRaw;
                    }
                    _BundleResponse.InputCdns = BundleContext.InputCdns;
                    AddToCache(_BundleResponse);
                }
            }
            return _BundleResponse;
        }

        internal string Route
        {
            get
            {
                return _Route;
            }
        }
        #endregion Internal

        #region Private
        private void AddToCache(BundleResponse bundleResponse)
        {
            IMemoryCache memoryCache = BundleContext.ApplicationBuilder.ApplicationServices.GetRequiredService<IMemoryCache>();
            memoryCache?.Set(Route, bundleResponse);
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
        public StyleBundle(string route) : base(route)
        { }

        public StyleBundle(string route, IFileProvider fileProvider) : base(route, fileProvider, new List<IBundleTransform>())
        { }

        public StyleBundle(string route, IFileProvider fileProvider, List<IBundleTransform> transformations) : base(route, fileProvider, transformations)
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

        public ScriptBundle(string route, IFileProvider fileProvider) : base(route, fileProvider, new List<IBundleTransform>())
        { }

        public ScriptBundle(string route, IFileProvider fileProvider, List<IBundleTransform> transformations) : base(route, fileProvider, transformations)
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
