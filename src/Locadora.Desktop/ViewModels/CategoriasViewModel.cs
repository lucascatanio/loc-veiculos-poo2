using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Api.Dtos.Categorias;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels.Cadastros;

namespace Locadora.Desktop.ViewModels;

public partial class CategoriasViewModel(IApiClient api, IDialogos dialogos)
    : CadastroViewModel<CategoriaResponse>(dialogos)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public string Titulo => "Categorias";

    [ObservableProperty]
    private string _nome = "";

    [ObservableProperty]
    private string _valorDiariaTexto = "";

    [ObservableProperty]
    private string? _erroValorDiaria;

    protected override string NomeDoCadastro => "categoria";

    protected override Task<List<CategoriaResponse>> ListarAsync(string? busca, bool incluirInativos) =>
        api.ListarCategoriasAsync(busca, incluirInativos);

    protected override int ObterId(CategoriaResponse item) => item.Id;

    protected override bool ObterAtivo(CategoriaResponse item) => item.Ativo;

    protected override void PreencherFormulario(CategoriaResponse? item)
    {
        Nome = item?.Nome ?? "";
        ValorDiariaTexto = item is null ? "" : item.ValorDiaria.ToString("F2", PtBr);
        ErroValorDiaria = null;
    }

    protected override bool FormularioValido()
    {
        ErroValorDiaria = ValidarValorDiaria();
        return ErroValorDiaria is null;
    }

    protected override Task<CategoriaResponse> SalvarNaApiAsync(int? id)
    {
        var valorDiaria = ConverterValorDiaria();
        return id is { } existente
            ? api.AtualizarCategoriaAsync(existente, new AtualizarCategoriaRequest(Nome, valorDiaria))
            : api.CriarCategoriaAsync(new CriarCategoriaRequest(Nome, valorDiaria));
    }

    protected override Task InativarNaApiAsync(int id) => api.InativarCategoriaAsync(id);

    protected override Task ReativarNaApiAsync(int id) => api.ReativarCategoriaAsync(id);

    protected override string MensagemConfirmarInativacao(CategoriaResponse item) =>
        $"A categoria {item.Nome} deixa de aparecer para novas locações. Veículos ativos nela impedem a inativação. Você pode reativá-la depois.";

    protected override string MensagemConfirmarReativacao(CategoriaResponse item) =>
        $"A categoria {item.Nome} volta a ficar disponível para cadastrar veículos.";

    private string? ValidarValorDiaria()
    {
        if (string.IsNullOrWhiteSpace(ValorDiariaTexto))
            return "Informe o valor da diária.";

        return TentarConverterValorDiaria(out _)
            ? null
            : "Valor inválido. Use vírgula para os centavos, como 150,50.";
    }

    private decimal ConverterValorDiaria()
    {
        TentarConverterValorDiaria(out var valor);
        return valor;
    }

    private bool TentarConverterValorDiaria(out decimal valor) =>
        decimal.TryParse(ValorDiariaTexto.Trim(), NumberStyles.AllowDecimalPoint, PtBr, out valor);
}
