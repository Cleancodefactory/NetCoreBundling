using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using dotless.Core;
using dotless.Core.configuration;
using dotless.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling.Transformations
{
    public class LessTransformation : IBundleTransform
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
                ILessEngine lessEngine = new EngineFactory(ConfigureSettings(new DotlessConfiguration())).GetEngine();
                if (context.CustomPropertiesCallback != null)
                {
                    lessEngine.CurrentDirectory = context.CustomPropertiesCallback();// ".//Modules";
                }
                if (context.Parent is StyleBundle)
                {
                    if (context.EnableOptimizations)
                    {
                        StringBuilder sb = new StringBuilder(response.Content.Length);
                        sb.Append(lessEngine.TransformToCss(response.Content.ToString(), response.ETag));
                        response.Content = sb;
                        if (!lessEngine.LastTransformationSuccessful)
                        {
                            ParameterDecorator p = lessEngine as ParameterDecorator;
                            CacheDecorator d = p.Underlying as CacheDecorator;
                            LessEngine l = d.Underlying as LessEngine;
                            string errMessage = l.LastTransformationError.Message;
                            response.TransformationErrors.Append($"Errors: {errMessage}<br />Error details: {errMessage}").Append("<br />");
                            context.Logger.LogCritical($"Error message: {errMessage} {Environment.NewLine} Error details: {errMessage}");
                        }
                    }
                    else if (context.EnableInstrumentation)
                    {
                        StringBuilder sb = new StringBuilder(10000);
                        foreach (BundleFile bundleFile in response.BundleFiles.Values)
                        {
                            if (bundleFile.PhysicalPath.LastIndexOf(".less") > 0)
                            {
                                sb.Append(lessEngine.TransformToCss(bundleFile.Content.ToString(), bundleFile.PhysicalPath));
                                if (!lessEngine.LastTransformationSuccessful)
                                {
                                    AddErrors(((LessEngine)lessEngine).LastTransformationError, context, response, bundleFile.PhysicalPath);
                                }
                                bundleFile.Content = sb;
                                FileInfo fileInfo = new FileInfo(bundleFile.PhysicalPath);
                                string cssFileName = Path.Combine(fileInfo.Directory.FullName, fileInfo.Name.Replace(".less", ".css"));
                                //Create the css file
                                CreateFile(sb, cssFileName);
                                bundleFile.PhysicalPath = cssFileName;
                                bundleFile.VirtualPath = bundleFile.VirtualPath.Replace(".less", ".css");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.TransformationErrors.Append($"Error: {ex.Message} {Environment.NewLine} Error details: {ex.StackTrace}").Append(Environment.NewLine);
                context.Logger.LogCritical($"Error: {ex.Message} {Environment.NewLine} Error details: {ex.StackTrace}", ex);
                throw;
            }
        }

        private void CreateFile(StringBuilder sb, string cssFileName)
        {
            using (StreamWriter file = new System.IO.StreamWriter(File.Create(cssFileName)))
            {
                file.WriteLine(sb.ToString());
            }
        }

        private void AddErrors(ParserException parserException, BundleContext context, BundleResponse response, string file)
        {
            response.TransformationErrors.Append($"Error in {file}:<br />{parserException.Message}<br />Error details: {parserException}").Append("<br />");
            context.Logger.LogCritical($"Error in {file}: {parserException.Message} {Environment.NewLine} Error details: {parserException}", parserException);
        }

        DotlessConfiguration ConfigureSettings(DotlessConfiguration lessSettings)
        {
            return lessSettings;
        }
    }
}
