using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using System;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using NUglify;
using NUglify.Css;
using NUglify.JavaScript;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling.Transformations
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
            if (context.Parent is StyleBundle)
            {
                if (context.EnableOptimizations)
                {
                    MinifyCss(context, response);
                }
            }
            else if (context.Parent is ScriptBundle)
            {
                if (context.EnableOptimizations)
                {
                    MinifyJs(context, response);
                }
            }
        }

        private void MinifyCss(BundleContext context, BundleResponse response)
        {
            UglifyResult uglifyResult = new UglifyResult();
            response.Content = new StringBuilder(10000);
            foreach (BundleFile bundleFile in response.BundleFiles.Values)
            {
                try
                {
                    uglifyResult = Uglify.Css(bundleFile.Content.ToString(), ConfigureSettings(new CssSettings()));
                    if (uglifyResult.HasErrors)
                    {
                        AddErrors(uglifyResult, context, response, bundleFile.PhysicalPath);
                    }
                    response.Content.Append(uglifyResult.Code);
                }
                catch (Exception ex)
                {
                    AddErrors(ex.Message, ex.StackTrace, context, response, bundleFile.PhysicalPath);
                    response.Content.Append(bundleFile.Content);//in case of an error append the non minified
                }
            }
        }

        private void MinifyJs(BundleContext context, BundleResponse response)
        {
            UglifyResult uglifyResult = new UglifyResult();
            response.Content = new StringBuilder(10000);
            foreach (BundleFile bundleFile in response.BundleFiles.Values)
            {
                try
                {
                    uglifyResult = Uglify.Js(bundleFile.Content.ToString(), ConfigureSettings(new CodeSettings()));
                    if (uglifyResult.HasErrors)
                    {
                        AddErrors(uglifyResult, context, response, bundleFile.PhysicalPath);
                    }
                    response.Content.Append(uglifyResult.Code);
                    if (!string.IsNullOrEmpty(uglifyResult.Code) && uglifyResult.Code.Length > 1)
                    {
                        char last = uglifyResult.Code[uglifyResult.Code.Length - 1];
                        if (!last.Equals('}') && !last.Equals(';'))
                        {
                            response.Content.Append(";");
                            AddErrors($"Javascript file is missing trailing ';'", $"Control and fix the missing ';'!", context, response, bundleFile.PhysicalPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddErrors(ex.Message, ex.StackTrace, context, response, bundleFile.PhysicalPath);
                    response.Content.Append(bundleFile.Content);//in case of an error append the non minified
                }
            }
            response.Content.Append(response.ContentRaw);
        }

        private void AddErrors(UglifyResult uglifyResult, BundleContext context, BundleResponse response, string file)
        {
            if (uglifyResult.HasErrors)
            {
                foreach (UglifyError error in uglifyResult.Errors)
                {
                    AddErrors(error.Message, error.ToString(), context, response, file);
                }
            }
        }

        private void AddErrors(string errorShort, string errorLong, BundleContext context, BundleResponse response, string file)
        {
            if (context.EnableInstrumentation)
            {
                response.TransformationErrors.Append($"Error in {file}:<br />{errorShort}<br />Error details: {errorLong}").Append("<br />");
            }
            context.Logger.LogCritical($"Error in {file}: {errorShort} {Environment.NewLine} Error details: {errorLong}");
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
