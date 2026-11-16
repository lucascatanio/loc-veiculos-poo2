using Locadora.Domain.Excecoes;

namespace Locadora.Domain.Enums;

public static class ConversorEnum
{
    public static T Converter<T>(string? valor, string campo) where T : struct, Enum
    {
        // Enum.TryParse sozinho aceita qualquer número ("7"); IsDefined barra o que não é membro do enum.
        if (Enum.TryParse<T>(valor, ignoreCase: true, out var convertido) && Enum.IsDefined(convertido))
            return convertido;

        throw new DomainException(
            $"Valor \"{valor}\" inválido para {campo}. Use: {string.Join(", ", Enum.GetNames<T>())}.");
    }
}
