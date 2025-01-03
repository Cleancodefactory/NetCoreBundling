﻿using Ccf.Ck.Libs.Web.Bundling.Interfaces;
using Ccf.Ck.Libs.Web.Bundling.Primitives;
using System;
using System.IO;
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
                foreach (InputFile inputFile in context.InputBundleFiles)
                {
                    BundleFile bundleFile = new BundleFile(context.Parent, context.EnableWatch)
                    {
                        VirtualPath = $"/{RemoveFirstOccurenceSpecialCharacters(inputFile.VirtualPath, EStartPoint.FromStart, new char[] { '/', '\\', '~' })}"
                    };
                    if (!string.IsNullOrEmpty(inputFile.PhysicalPath))
                    {
                        bundleFile.PhysicalPath = inputFile.PhysicalPath;
                    }
                    else
                    {
                        bundleFile.PhysicalPath = context.FileProvider.GetFileInfo(bundleFile.VirtualPath).PhysicalPath;
                        if (bundleFile.PhysicalPath == null)
                        {
                            throw new Exception($"The file: {bundleFile.VirtualPath} was not found, please provide it to the bundler and start again!" );
                        }
                    }
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
