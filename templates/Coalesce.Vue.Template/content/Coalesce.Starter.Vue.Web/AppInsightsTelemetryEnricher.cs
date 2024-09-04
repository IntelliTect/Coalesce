using IntelliTect.Coalesce.Utilities;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Coalesce.Starter.Vue.Web;

public class AppInsightsTelemetryEnricher(IHttpContextAccessor httpContextAccessor) : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is not null)
        {
            telemetry.Context.User.AuthenticatedUserId = user.GetUserId();
        }

        if (telemetry is ExceptionTelemetry ex)
        {
            // Always log exceptions
            ex.ProactiveSamplingDecision = SamplingDecision.SampledIn;
        }
        else if (telemetry is RequestTelemetry req)
        {
            if (req.Success == false)
            {
                // Always log failed requests.
                req.ProactiveSamplingDecision = SamplingDecision.SampledIn;
                return;
            }
        }
        else if (telemetry is DependencyTelemetry dep)
        {
            if (dep.Success == false)
            {
                // Always log failed dependencies.
                dep.ProactiveSamplingDecision = SamplingDecision.SampledIn;
                return;
            }
        }
    }
}