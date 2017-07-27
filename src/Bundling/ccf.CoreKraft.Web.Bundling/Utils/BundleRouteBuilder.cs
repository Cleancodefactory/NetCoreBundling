using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ccf.CoreKraft.Web.Bundling.Utils
{
    internal class BundleRouteBuilder
    {
        internal const string BUNDLE = "bundle";
        internal const string CATCHALL = "catchall";
        internal static IRouter BuildRoute(IApplicationBuilder builder, RouteHandler routeHandler, string baseBundlingRoute)
        {
            // create route builder with route handler
            RouteBuilder customRouteBuilder = new RouteBuilder(builder, routeHandler);

            //we expect the routing to be like this:
            //domain.com/<baseBundlingRoute>/<css|js>?version
            customRouteBuilder.MapRoute(
                    name: "bundling_" + baseBundlingRoute,
                    template: baseBundlingRoute + "/{bundle}/{*catchall}",
                    defaults: null,
                    constraints: null,
                    dataTokens: new { key = "bundling_" + baseBundlingRoute }
                );
            IRouter customRouter = customRouteBuilder.Build();
            return customRouter;
        }
    }
}
