using ccf.CoreKraft.Web.Bundling.Interfaces;
using ccf.CoreKraft.Web.Bundling.Primitives;
using ccf.CoreKraft.Web.Bundling.Utils;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using static ccf.CoreKraft.Web.Bundling.Primitives.BundleContext;

namespace ccf.CoreKraft.Web.Bundling.Transformations
{
    class IncludeDirectoryTransformation : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            try
            {
                Dictionary<string, BundleFile> bundleFiles = new Dictionary<string, BundleFile>();
                foreach (IncludeDirectoryData includeDirectoryData in context.IncludeDirectoryDataStructures)
                {
                    if (!string.IsNullOrEmpty(includeDirectoryData.DirectoryVirtualPath) && !string.IsNullOrEmpty(includeDirectoryData.SearchPattern))
                    {
                        ProcessDirectory(context, includeDirectoryData, includeDirectoryData.DirectoryVirtualPath.Replace(@"\", "/"), ref bundleFiles);
                        foreach (KeyValuePair<string, BundleFile> item in bundleFiles)
                        {
                            response.BundleFiles.Add(item.Key, item.Value);
                        }
                        bundleFiles = new Dictionary<string, BundleFile>(10);
                    }
                }
            }
            catch (Exception ex)
            {
                response.TransformationErrors.Append($"Error: {ex.Message}<br />Error details: {ex.StackTrace}").Append("<br />");
                throw;
            }            
        }

        public void ProcessDirectory(BundleContext context, IncludeDirectoryData includeDirectoryData, string directoryPath, ref Dictionary<string, BundleFile> files)
        {
            IEnumerable<IFileInfo> directoryFiles;
            IDirectoryContents directoryContent = context.FileProvider.GetDirectoryContents(directoryPath);
            if (directoryContent != null)
            {
                Regex regEx;
                switch (includeDirectoryData.PatternType)
                {
                    // We used to be able to just call DirectoryInfo.GetFiles,
                    // now we have to add support for * and {version} syntax on top of VPP
                    case PatternType.Version:
                        regEx = PatternHelper.BuildRegex(includeDirectoryData.SearchPattern);
                        directoryFiles = directoryContent.Where(file => regEx.IsMatch(file.Name) && !file.IsDirectory);
                        break;
                    case PatternType.All:
                        directoryFiles = directoryContent.Where(file => !file.IsDirectory);
                        break;
                    case PatternType.Exact:
                        directoryFiles = directoryContent.Where(file => String.Equals(file.Name, includeDirectoryData.SearchPattern, StringComparison.OrdinalIgnoreCase) && !file.IsDirectory);
                        break;
                    case PatternType.Suffix:
                    case PatternType.Prefix:
                    default:
                        regEx = PatternHelper.BuildWildcardRegex(includeDirectoryData.SearchPattern);
                        directoryFiles = directoryContent.Where(file => regEx.IsMatch(file.Name) && !file.IsDirectory);
                        break;
                }

                // Sort the directory files so we get deterministic order
                directoryFiles = directoryFiles.OrderBy(file => file, PhysicalFileComparer.Instance);

                //List<BundleFile> filterList = new List<BundleFile>();
                //foreach (IFileInfo file in directoryFiles)
                //{
                //    filterList.Add(new BundleFile());
                //}
                //files.AddRange(context.BundleCollection.DirectoryFilter.FilterIgnoredFiles(context, filterList));

                foreach (IFileInfo file in directoryFiles)
                {
                    files.Add(directoryPath + "/" + file.Name, new BundleFile { PhysicalPath = file.PhysicalPath, VirtualPath = directoryPath + "/" + file.Name });
                }

                // Need to recurse on subdirectories if requested
                if (includeDirectoryData.SearchSubdirectories)
                {
                    foreach (IFileInfo subDir in directoryContent.Where(file => file.IsDirectory))
                    {
                        ProcessDirectory(context, includeDirectoryData, directoryPath + "/" + subDir.Name, ref files);
                    }
                }
            }
        }
    }

    internal sealed class PhysicalFileComparer : IEqualityComparer<IFileInfo>, IComparer<IFileInfo>
    {
        internal static readonly PhysicalFileComparer Instance = new PhysicalFileComparer();

        // Should always use the static instance
        private PhysicalFileComparer()
        {
        }

        public bool Equals(IFileInfo x, IFileInfo y)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            return string.Equals(x.PhysicalPath, y.PhysicalPath, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IFileInfo obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return obj.PhysicalPath.GetHashCode();
        }

        public int Compare(IFileInfo x, IFileInfo y)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            return String.Compare(x.PhysicalPath, y.PhysicalPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
