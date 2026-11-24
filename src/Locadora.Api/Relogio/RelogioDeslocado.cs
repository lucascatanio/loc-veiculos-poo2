namespace Locadora.Api.Relogio;

public sealed class RelogioDeslocado(TimeSpan deslocamento) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow() + deslocamento;

    public override TimeZoneInfo LocalTimeZone => TimeProvider.System.LocalTimeZone;
}
