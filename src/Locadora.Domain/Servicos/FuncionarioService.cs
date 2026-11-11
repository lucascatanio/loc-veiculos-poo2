using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class FuncionarioService(IFuncionarioRepository funcionarios)
{
    public async Task GarantirEmailDisponivelAsync(string email, int? ignorarId, CancellationToken ct)
    {
        if (await funcionarios.ExisteEmailAsync(Funcionario.NormalizarEmail(email), ignorarId, ct))
            throw new DomainException("Já existe um funcionário com este e-mail.");
    }
}
