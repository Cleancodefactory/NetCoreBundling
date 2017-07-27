using ccf.CoreKraft.Web.Bundling.Enumerations;
using ccf.CoreKraft.Web.Bundling.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ccf.CoreKraft.Web.Bundling.Primitives
{
    public class BundleResponse
    {
        private string _Version;
        private string _ETag;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        public BundleResponse()
        {
            CreationDate = DateTimeOffset.UtcNow;
            BundleFiles = new Dictionary<string, BundleFile>();
            Cacheability = HttpCacheability.Public;
            Content = new StringBuilder(10000);
            ContentRaw = new StringBuilder(10000);
            TransformationErrors = new StringBuilder(1000);
        }

        public Dictionary<string, BundleFile> BundleFiles { get; internal set; }

        /// <summary>
        /// The content of the bundle which is sent as the response body.
        /// </summary>
        public StringBuilder Content { get; set; }

        public List<CdnObject> InputCdns { get; set; }

        public StringBuilder TransformationErrors { get; set; }

        public StringBuilder ContentRaw { get; set; }
        /// <summary>
        /// The response content-type header.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The time when this response was created
        /// </summary>
        public DateTimeOffset CreationDate { get; private set; }

        /// <summary>
        /// Enables control over the cache headers that are spent in the bundle response.
        /// </summary>
        public HttpCacheability Cacheability { get; set; }

        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_Version))
                {
                    _Version = GeneralUtility.GenerateETag(Encoding.UTF8.GetBytes(Content.ToString()));
                }
                return _Version;
            }
            set
            {
                _Version = value;
            }
        }

        public string ETag {
            get
            {
                if (string.IsNullOrEmpty(_ETag))
                {
                    _ETag = GeneralUtility.GenerateETag(Encoding.UTF8.GetBytes(Content.ToString()));
                }
                return _ETag;
            }
            set
            {
                _ETag = value;
            }
        }
    }
}