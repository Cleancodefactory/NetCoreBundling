using ccf.CoreKraft.Web.Bundling.Interfaces;
using System;
using ccf.CoreKraft.Web.Bundling.Primitives;
using NUglify;
using NUglify.Css;
using NUglify.JavaScript;
using Microsoft.Extensions.Logging;
using System.Text;
using System.IO;

namespace ccf.CoreKraft.Web.Bundling.Transformations
{
    public class MinificationTransformation : IBundleTransform
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
            UglifyResult uglifyResult = new UglifyResult();
            try
            {
                if (context.EnableOptimizations)
                {
                    if (context.Parent is StyleBundle)
                    {
                        uglifyResult = Uglify.Css(response.Content.ToString(), ConfigureSettings(new CssSettings()));
                    }
                    else if (context.Parent is ScriptBundle)
                    {

                        uglifyResult = Uglify.Js(response.Content.ToString(), ConfigureSettings(new CodeSettings()));
                    }
                    if (uglifyResult.HasErrors)
                    {
                        foreach (UglifyError error in uglifyResult.Errors)
                        {
                            response.TransformationErrors.Append($"Errors: {error.Message}<br />Error details: {error.ToString()}").Append("<br />");
                            context.Logger.LogCritical($"Error message: {error.Message} {Environment.NewLine} Error details: {error.ToString()}", error);
                        }
                    }
                    response.Content = new StringBuilder(uglifyResult.Code.Length);
                    response.Content.Append(uglifyResult.Code + response.ContentRaw);
                }
                else if (context.EnableInstrumentation)
                {
                    if (context.Parent is StyleBundle)
                    {
                        foreach (BundleFile bundleFile in response.BundleFiles.Values)
                        {
                            uglifyResult = Uglify.Css(GetContent(bundleFile.PhysicalPath).ToString(), ConfigureSettings(new CssSettings()));
                            AddErrors(uglifyResult, context, response, bundleFile.PhysicalPath);
                        }
                    }
                    else if (context.Parent is ScriptBundle)
                    {
                        foreach (BundleFile bundleFile in response.BundleFiles.Values)
                        {
                            uglifyResult = Uglify.Js(GetContent(bundleFile.PhysicalPath).ToString(), ConfigureSettings(new CodeSettings()));
                            AddErrors(uglifyResult, context, response, bundleFile.PhysicalPath);
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

        private StringBuilder GetContent(string physicalPath)
        {
            StringBuilder content = new StringBuilder(1000);
            using (FileStream fs = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    content.Append(sr.ReadToEnd());
                }
            }
            return content;
        }

        private void AddErrors(UglifyResult uglifyResult, BundleContext context, BundleResponse response, string file)
        {
            if (uglifyResult.HasErrors)
            {
                foreach (UglifyError error in uglifyResult.Errors)
                {
                    response.TransformationErrors.Append($"Error in {file}:<br />{error.Message}<br />Error details: {error.ToString()}").Append("<br />");
                    context.Logger.LogCritical($"Error in {file}: {error.Message} {Environment.NewLine} Error details: {error.ToString()}", error);
                }
            }
        }

        CssSettings ConfigureSettings(CssSettings cssSettings)
        {
            cssSettings.CommentMode = CssComment.None;
            cssSettings.OutputMode = OutputMode.SingleLine;
            cssSettings.TermSemicolons = true;
            return cssSettings;
        }

        CodeSettings ConfigureSettings(CodeSettings codeSettings)
        {
            codeSettings.AlwaysEscapeNonAscii = true;
            codeSettings.EvalTreatment = EvalTreatment.Ignore;
            codeSettings.LocalRenaming = LocalRenaming.CrunchAll;
            codeSettings.MinifyCode = true;
            codeSettings.OutputMode = OutputMode.SingleLine;
            codeSettings.PreserveImportantComments = true;
            codeSettings.TermSemicolons = true;
            codeSettings.InlineSafeStrings = true;

            return codeSettings;
        }
    }
}
