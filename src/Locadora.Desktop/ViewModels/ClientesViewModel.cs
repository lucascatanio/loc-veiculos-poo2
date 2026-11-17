using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Api.Dtos.Clientes;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels.Cadastros;

namespace Locadora.Desktop.ViewModels;

public partial class ClientesViewModel(IApiClient api, IDialogos dialogos)
    : CadastroViewModel<ClienteResponse>(dialogos)
{
    public string Titulo => "Clientes";

    [ObservableProperty]
    private string _nome = "";

    [ObservableProperty]
    private string _cpf = "";

    [ObservableProperty]
    private string _cnh = "";

    [ObservableProperty]
    private string _telefone = "";

    [ObservableProperty]
    private string _email = "";

    protected override string NomeDoCadastro => "cliente";

    protected override Task<List<ClienteResponse>> ListarAsync(string? busca, bool incluirInativos) =>
        api.ListarClientesAsync(busca, incluirInativos);

    protected override int ObterId(ClienteResponse item) => item.Id;

    protected override bool ObterAtivo(ClienteResponse item) => item.Ativo;

    protected override void PreencherFormulario(ClienteResponse? item)
    {
        Nome = item?.Nome ?? "";
        Cpf = item?.Cpf ?? "";
        Cnh = item?.Cnh ?? "";
        Telefone = item?.Telefone ?? "";
        Email = item?.Email ?? "";
    }

    protected override Task<ClienteResponse> SalvarNaApiAsync(int? id)
    {
        var emailInformado = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();
        return id is { } existente
            ? api.AtualizarClienteAsync(existente, new AtualizarClienteRequest(Nome, Cnh, Telefone, emailInformado))
            : api.CriarClienteAsync(new CriarClienteRequest(Nome, Cpf, Cnh, Telefone, emailInformado));
    }

    protected override Task InativarNaApiAsync(int id) => api.InativarClienteAsync(id);

    protected override Task ReativarNaApiAsync(int id) => api.ReativarClienteAsync(id);

    protected override string MensagemConfirmarInativacao(ClienteResponse item) =>
        $"{item.Nome} não poderá abrir novas locações enquanto estiver inativo. Cliente com locação aberta não pode ser inativado. Você pode reativá-lo depois.";

    protected override string MensagemConfirmarReativacao(ClienteResponse item) =>
        $"{item.Nome} volta a poder abrir locações.";
}
