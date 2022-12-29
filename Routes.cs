using System.Runtime.CompilerServices;
using X_Pace_Backend.Middleware;

namespace X_Pace_Backend;

public static class RoutesBuilder
{
    public static IRouteBuilder BuildAuthMiddlewareRoute(this IRouteBuilder routes, string pattern)
    {
        var pipeline = routes.ApplicationBuilder
            .UseMiddleware<AuthMiddleware>()
            .Build();

        return routes.MapGet(pattern, pipeline);
    }
}