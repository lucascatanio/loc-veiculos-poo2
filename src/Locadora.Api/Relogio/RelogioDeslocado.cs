namespace Locadora.Api.Relogio;

// Só em Development: adianta o relógio para demonstrar multa e tolerância sem esperar dias.
public sealed class RelogioDeslocado(TimeSpan deslocamento) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow() + deslocamento;

    public override TimeZoneInfo LocalTimeZone => TimeProvider.System.LocalTimeZone;
}
