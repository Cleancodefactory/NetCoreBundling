using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using System;

namespace Ccf.Ck.Libs.Web.Bundling.Transformations
{
    public class BabelTransformation : IBundleTransform
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
            //IReactEnvironment environment = ReactEnvironment.Current;
            //try
            //{
            //    if (context.EnableOptimizations)
            //    {
            //        if (context.Parent is ScriptBundle)
            //        {
            //            response.Content = new StringBuilder(environment.Babel.Transform(response.Content.ToString()));
            //        }
            //    }
            //    else if (context.EnableInstrumentation)
            //    {
            //        if (context.Parent is ScriptBundle)
            //        {
            //            response.Content = new StringBuilder(environment.Babel.Transform(response.Content.ToString()));
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response.TransformationErrors.Append($"Error: {ex.Message} {Environment.NewLine} Error details: {ex.StackTrace}").Append(Environment.NewLine);
            //    context.Logger.LogCritical($"Error: {ex.Message} {Environment.NewLine} Error details: {ex.StackTrace}", ex);
            //    throw;
            //}
        }
    }
}
