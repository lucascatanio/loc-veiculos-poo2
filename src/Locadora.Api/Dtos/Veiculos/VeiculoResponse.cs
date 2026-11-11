namespace Locadora.Api.Dtos.Veiculos;

public record VeiculoResponse(
    int Id, string Placa, string Marca, string Modelo, int Ano, string Cor, int KmAtual,
    string Status, bool Ativo, int CategoriaId, string CategoriaNome);
