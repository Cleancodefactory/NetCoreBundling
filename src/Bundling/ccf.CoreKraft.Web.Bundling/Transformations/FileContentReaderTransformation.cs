using ccf.CoreKraft.Web.Bundling.Interfaces;
using ccf.CoreKraft.Web.Bundling.Primitives;
using System;
using System.IO;
using System.Text;

namespace ccf.CoreKraft.Web.Bundling.Transformations
{
    public class FileContentReaderTransformation : IBundleTransform
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
                if (context.EnableOptimizations)
                {
                    foreach (BundleFile bundleFile in response.BundleFiles.Values)
                    {
                        if (bundleFile.Content == null || bundleFile.Content.Length == 0)
                        {
                            StringBuilder content = new StringBuilder(10000);
                            if (bundleFile.PhysicalPath == null)
                            {
                                throw new ArgumentNullException(nameof(bundleFile.PhysicalPath));
                            }
                            using (FileStream fs = new FileStream(bundleFile.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    content.Append(sr.ReadToEnd());
                                }
                            }
                            bundleFile.Content = content;
                            response.Content.AppendLine(content.ToString());
                        }
                    }
                    response.ContentRaw.AppendLine(context.ContentRaw.ToString());
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
