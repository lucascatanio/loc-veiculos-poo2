using Mapster;

namespace Locadora.Api.Mapeamento;

public static class MapsterConfig
{
    public static void Registrar()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MapsterConfig).Assembly);
    }
}
