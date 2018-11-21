using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using System;
using static Ccf.Ck.Libs.Web.Bundling.Utils.FileUtility;

namespace Ccf.Ck.Libs.Web.Bundling.Transformations
{
    public class FilePathsTransformation : IBundleTransform
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
                    BundleFile bundleFile = new BundleFile(context.Parent);
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
