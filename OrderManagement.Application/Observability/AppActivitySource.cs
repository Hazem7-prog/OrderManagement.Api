using System.Diagnostics;

namespace OrderManagement.Application.Observability;

// One ActivitySource (= "tracer") for the whole app. A span started
// here becomes a child of whatever span is already active - usually
// the automatic ASP.NET Core span for the incoming HTTP request - so
// in Jaeger you will see, e.g.:
//   POST /api/orders
//     └── CreateOrder
//           └── INSERT ... (from OpenTelemetry.Instrumentation.SqlClient)
public static class AppActivitySource
{
    public const string Name = "OrderManagement";

    public static readonly ActivitySource Source = new(Name, "1.0.0");
}
