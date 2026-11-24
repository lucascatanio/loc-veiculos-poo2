using Locadora.Domain.Consultas;
using Locadora.Domain.Entidades;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Consultas;

public class PainelConsulta(LocadoraDbContext context) : IPainelConsulta
{
    public async Task<List<LocacaoAtrasadaItem>> ListarAtrasadasAsync(DateTime agora, CancellationToken ct)
    {
        var toleranciaSegundos = Locacao.TOLERANCIA_MINUTOS * 60;
        var taxaMulta = Locacao.TAXA_MULTA;

        return await context.Database
            .SqlQuery<LocacaoAtrasadaItem>($"""
                SELECT
                    atraso.LocacaoId,
                    atraso.ClienteNome,
                    atraso.ClienteTelefone,
                    atraso.VeiculoPlaca,
                    atraso.VeiculoModelo,
                    atraso.DataPrevistaDevolucao,
                    atraso.SegundosAtraso / 60 AS MinutosAtraso,
                    dias.DiasAtraso,
                    CAST(ROUND(dias.DiasAtraso * atraso.ValorDiaria * (1 + {taxaMulta}), 2) AS decimal(18,2)) AS MultaEstimada
                FROM (
                    SELECT
                        l.Id AS LocacaoId,
                        c.Nome AS ClienteNome,
                        c.Telefone AS ClienteTelefone,
                        v.Placa AS VeiculoPlaca,
                        v.Modelo AS VeiculoModelo,
                        l.DataPrevistaDevolucao,
                        l.ValorDiaria,
                        DATEDIFF(SECOND, l.DataPrevistaDevolucao, {agora}) AS SegundosAtraso
                    FROM Locacoes l
                    JOIN Clientes c ON c.Id = l.ClienteId
                    JOIN Veiculos v ON v.Id = l.VeiculoId
                    WHERE l.Status = 'Aberta' AND l.DataPrevistaDevolucao < {agora}
                ) AS atraso
                CROSS APPLY (
                    SELECT CASE
                        WHEN atraso.SegundosAtraso <= {toleranciaSegundos} THEN 0
                        ELSE CAST(CEILING(atraso.SegundosAtraso / 86400.0) AS int)
                    END AS DiasAtraso
                ) AS dias
                """)
            .OrderByDescending(a => a.MinutosAtraso)
            .ToListAsync(ct);
    }

    public async Task<List<LocacaoVencendoItem>> ListarVencendoHojeAsync(DateTime agora, CancellationToken ct)
    {
        var inicioDeAmanha = agora.Date.AddDays(1);

        return await context.Database
            .SqlQuery<LocacaoVencendoItem>($"""
                SELECT
                    l.Id AS LocacaoId,
                    c.Nome AS ClienteNome,
                    c.Telefone AS ClienteTelefone,
                    v.Placa AS VeiculoPlaca,
                    v.Modelo AS VeiculoModelo,
                    l.DataPrevistaDevolucao
                FROM Locacoes l
                JOIN Clientes c ON c.Id = l.ClienteId
                JOIN Veiculos v ON v.Id = l.VeiculoId
                WHERE l.Status = 'Aberta'
                  AND l.DataPrevistaDevolucao >= {agora}
                  AND l.DataPrevistaDevolucao < {inicioDeAmanha}
                """)
            .OrderBy(l => l.DataPrevistaDevolucao)
            .ToListAsync(ct);
    }

    public Task<List<ContadorFrotaItem>> ContarFrotaPorStatusAsync(CancellationToken ct) =>
        context.Database
            .SqlQuery<ContadorFrotaItem>($"""
                SELECT Status, COUNT(*) AS Quantidade
                FROM Veiculos
                WHERE Ativo = 1
                GROUP BY Status
                """)
            .ToListAsync(ct);
}
