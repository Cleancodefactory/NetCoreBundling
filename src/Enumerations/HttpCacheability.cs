namespace Ccf.Ck.Libs.Web.Bundling.Enumerations
{
    public enum HttpCacheability
    {
        //
        // Summary:
        //     Sets the Cache-Control: no-cache header. Without a field name, the directive
        //     applies to the entire request and a shared (proxy server) cache must force a
        //     successful revalidation with the origin Web server before satisfying the request.
        //     With a field name, the directive applies only to the named field; the rest of
        //     the response may be supplied from a shared cache.
        NoCache = 1,
        //
        // Summary:
        //     Default value. Sets Cache-Control: private to specify that the response is cacheable
        //     only on the client and not by shared (proxy server) caches.
        Private = 2,
        //
        // Summary:
        //     Specifies that the response is cached only at the origin server. Similar to the
        //     System.Web.HttpCacheability.NoCache option. Clients receive a Cache-Control:
        //     no-cache directive but the document is cached on the origin server. Equivalent
        //     to System.Web.HttpCacheability.ServerAndNoCache.
        Server = 3,
        //
        // Summary:
        //     Applies the settings of both System.Web.HttpCacheability.Server and System.Web.HttpCacheability.NoCache
        //     to indicate that the content is cached at the server but all others are explicitly
        //     denied the ability to cache the response.
        ServerAndNoCache = 3,
        //
        // Summary:
        //     Sets Cache-Control: public to specify that the response is cacheable by clients
        //     and shared (proxy) caches.
        Public = 4,
        //
        // Summary:
        //     Indicates that the response is cached at the server and at the client but nowhere
        //     else. Proxy servers are not allowed to cache the response.
        ServerAndPrivate = 5
    }
}
