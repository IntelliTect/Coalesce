using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

/// <summary>
/// Service for testing XML doc comment processing.
/// References <see cref="System.Threading.CancellationToken"/> and <see cref="WeatherService"/>.
/// </summary>
[Coalesce, Service]
public class XmlDocTestService
{
    /// <summary>
    /// This mirrors <see cref="DeleteAsync(int, CancellationToken)"/>, but for a single user
    /// rather than a whole org. See AGENTS.md ("Database changes - required updates"): when a
    /// new entity gains a direct FK to <see cref="SimpleModelTarget"/>, it must be handled here.
    /// </summary>
    [Coalesce]
    public Task<ItemResult> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ItemResult(true));
    }

    /// <summary>
    /// Deletes an organization. This method references <see cref="WeatherService.GetWeather"/>.
    /// Also references <see cref="System.DateTime.Now"/>.
    /// </summary>
    [Coalesce]
    public Task<ItemResult> DeleteAsync(int orgId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ItemResult(true));
    }
}
