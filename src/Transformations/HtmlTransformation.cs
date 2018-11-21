using System;
using System.Text;
using Microsoft.Extensions.Logging;
using NUglify.Html;
using NUglify;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;

namespace Ccf.Ck.Libs.Web.Bundling.Transformations
{
    public class HtmlTransformation : IBundleTransform
    {
        private HtmlSettings _HtmlSettings;
        public HtmlTransformation()
        {

        }
        public HtmlTransformation(HtmlSettings htmlSettings)
        {
            _HtmlSettings = htmlSettings;
        }
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
            if (context.EnableOptimizations)
            {
                response.Content = Process(response.Content, context.Logger, response);
            }
            else
            {
                response.ContentRaw = context.ContentRaw;
            }         
        }

        public StringBuilder Process(StringBuilder inputHtml, ILogger logger)
        {
            return Process(inputHtml, logger, null);
        }

        public StringBuilder Process(StringBuilder inputHtml, ILogger logger, BundleResponse response)
        {
            StringBuilder sb = new StringBuilder(1000);
            UglifyResult uglifyResult = new UglifyResult();
            try
            {
                uglifyResult = Uglify.Html(inputHtml.ToString(), ConfigureSettings(new HtmlSettings()));
                if (uglifyResult.HasErrors)
                {
                    foreach (UglifyError error in uglifyResult.Errors)
                    {
                        if (response != null)
                        {
                            response.TransformationErrors.Append($"Error message: {error.Message}<br />Error details: {error.ToString()}").Append("<br />");
                        }
                        logger.LogCritical($"Error message: {error.Message} {Environment.NewLine} Error details: {error.ToString()}", error);
                    }
                }
                sb.Append(uglifyResult.Code);
            }
            catch (Exception ex)
            {
                response.TransformationErrors.Append($"Error: {ex.Message}<br />Error details: {ex.StackTrace}").Append("<br />");
                logger.LogCritical($"Error: {ex.Message} {Environment.NewLine} Error details: {ex.StackTrace}", ex);
                throw;
            }
            return sb;
        }

        HtmlSettings ConfigureSettings(HtmlSettings htmlSettings)
        {
            if (_HtmlSettings != null)
            {
                return _HtmlSettings;
            }
            htmlSettings.RemoveEmptyAttributes = false;
            htmlSettings.KeepOneSpaceWhenCollapsing = true;
            htmlSettings.RemoveComments = true;
            htmlSettings.AttributeQuoteChar = '\'';
            return htmlSettings;
        }
    }
}
