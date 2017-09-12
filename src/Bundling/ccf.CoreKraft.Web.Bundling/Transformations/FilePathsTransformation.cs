using ccf.CoreKraft.Web.Bundling.Interfaces;
using ccf.CoreKraft.Web.Bundling.Primitives;
using System;
using static ccf.CoreKraft.Web.Bundling.Utils.FileUtility;

namespace ccf.CoreKraft.Web.Bundling.Transformations
{
    class FilePathsTransformation : IBundleTransform
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
                foreach (string inputFile in context.InputBundleFiles)
                {
                    BundleFile bundleFile = new BundleFile();
                    bundleFile.VirtualPath = $"/{RemoveFirstOccurenceSpecialCharacters(inputFile, EStartPoint.FromStart, new char[] { '/', '\\', '~' })}";
                    bundleFile.PhysicalPath = context.FileProvider.GetFileInfo(bundleFile.VirtualPath).PhysicalPath;
                    response.BundleFiles.Add(bundleFile.VirtualPath, bundleFile);
                }
            }
            catch (Exception ex)
            {
                response.TransformationErrors.Append($"Error: {ex.Message}<br />Error details: {ex.StackTrace}").Append("<br />");
                throw;
            }            
        }
    }
}
