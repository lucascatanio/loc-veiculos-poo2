# Relatório inicial de testes unitários — Locadora de Veículos

Execução: 23/11/2026 · .NET 10 · xUnit 2.9.3 · NSubstitute 6.2.0

| Total | Passaram | Falharam | Inconsistências distintas |
|---:|---:|---:|---:|
| 113 | 103 | **10** | **4** |

Resultado bruto: [relatorio-inicial.trx](relatorio-inicial.trx) (abre no Visual Studio) e [saida-console-inicial.txt](saida-console-inicial.txt).

## Escopo

Três requisitos do sistema de POO2, testados na camada de domínio (`src/Locadora.Domain`): as entidades e os serviços de entidade. Os repositórios dos serviços são substituídos por dublês do NSubstitute, então nenhum teste acessa banco ou rede.

| Requisito | Classes sob teste | Técnica | Testes | Falhas |
|---|---|---|---:|---:|
| R1 Devolução com multa | `Locacao.Finalizar`, `CalcularMulta`, `CalcularPrevia`, `LocacaoService.FinalizarAsync`/`ObterPreviaAsync` | Tabela de decisão | 29 | 5 |
| R2 Ciclo de vida da locação | construtor de `Locacao`, `RegistrarPagamento`, `Prorrogar`, `Cancelar`, `LocacaoService.AbrirAsync`/`CancelarAsync`/`ProrrogarAsync`/`ListarAsync` | Transição de estados | 41 | 2 |
| R3 Status do veículo | `Veiculo` (Alocar, Liberar, Manutenção, Inativar...), `VeiculoService` | Transição de estados + tabela de decisão | 43 | 3 |

### Oráculo

Cada teste descreve o comportamento que a **regra de negócio** de uma locadora exige, e não o que o código faz hoje. Uma falha, portanto, indica que o código diverge da regra. Os casos em que a regra era ambígua ficaram de fora para não inventar defeito. Exemplo: com atraso de 24h01, a tolerância de 60 minutos deveria valer de novo no segundo dia? Esse caso não foi testado.

### Como as técnicas aparecem no código

- **Tabela de decisão**: a tabela fica em comentário no topo da classe de teste e cada coluna (regra R1–R7) vira um teste com o mesmo prefixo. Ver [FinalizarTabelaDecisaoTests.cs](../../tests/Locadora.Domain.Tests/R1_DevolucaoComMulta/FinalizarTabelaDecisaoTests.cs).
- **Transição de estados**: a tabela estado × evento também está em comentário e vira dois `[Theory]`. Um percorre as transições válidas (estado final esperado) e o outro as proibidas (exceção e estado mantido). Ver [LocacaoTransicaoEstadosTests.cs](../../tests/Locadora.Domain.Tests/R2_CicloDeVidaDaLocacao/LocacaoTransicaoEstadosTests.cs) e [VeiculoTransicaoEstadosTests.cs](../../tests/Locadora.Domain.Tests/R3_StatusDoVeiculo/VeiculoTransicaoEstadosTests.cs).

Os estados são alcançados pelos métodos públicos da própria entidade (por exemplo, `Alocar()` para chegar em Locado), nunca setando o status direto. Assim cada estado testado é um estado que o sistema consegue produzir de verdade.

Todo teste tem os traits `Requisito` e `Tecnica`, então dá para filtrar por eles: `dotnet test --filter "Tecnica=Tabela de decisão"`.

## Inconsistências encontradas

### I1 — Devolução com data anterior à retirada é aceita (R1)

| | |
|---|---|
| Testes | `FinalizarTabelaDecisaoTests.R6_DevolucaoAntesDaRetirada_LancaExcecao`, `CalculoMultaTests.CalcularPrevia_DevolucaoAntesDaRetirada_LancaExcecao` |
| Esperado | `DomainException`: o carro não pode ser devolvido antes de ter sido retirado |
| Obtido | A locação é finalizada com `DataDevolucao` uma hora antes da `DataRetirada`, sem multa. A prévia também é calculada normalmente |
| Onde | `Locacao.Finalizar` e `Locacao.CalcularPrevia` só validam status, km e pagamentos. Nenhum compara `dataDevolucao` com `DataRetirada` |
| Impacto | Médio. Hoje a API usa o relógio do servidor como data de devolução, o que esconde o problema, mas a entidade não protege a própria invariante |

### I2 — Pagamento de saída registrado com a locação aberta deixa o saldo negativo (R2)

| | |
|---|---|
| Teste | `LocacaoTransicaoEstadosTests.RegistrarPagamento_SaidaComLocacaoAberta_LancaExcecao` |
| Esperado | `DomainException`. Saídas só existem no fechamento (`Finalizar` exige que sejam do tipo saída e que fechem o saldo), e o saldo devedor nunca fica negativo |
| Obtido | Uma saída de R$ 500 numa locação de R$ 300 é aceita: `ValorPago` = 500 e `SaldoDevedor()` = −200 |
| Onde | `Locacao.RegistrarPagamento` só verifica o saldo quando `Tipo == Entrada` |
| Impacto | Baixo hoje, porque `LocacaoService` sempre cria entradas nesse método. O risco é de regressão: qualquer novo chamador quebra a invariante do saldo |

### I3 — Forma de pagamento e status aceitam número e combinação de nomes (R1 e R2)

| | |
|---|---|
| Testes | `LocacaoServiceDevolucaoTests.FinalizarAsync_FormaInformadaPorNumeroOuCombinacao_LancaExcecao` com `"0"`, `"3"` e `"CartaoCredito,CartaoDebito"`; `LocacaoServiceCicloTests.ListarAsync_StatusQueNaoEhNomeDeEstado_LancaExcecao` com `"1"` |
| Esperado | `DomainException`. A própria mensagem de erro diz "Use: Dinheiro, CartaoCredito, CartaoDebito, Pix", ou seja, só nomes valem |
| Obtido | `"0"` vira Dinheiro, `"3"` vira Pix, `"CartaoCredito,CartaoDebito"` vira **Pix** (1 \| 2 = 3) e o status `"1"` vira Finalizada |
| Onde | `ConversorEnum.Converter`. O comentário ali diz que `IsDefined` barra números, mas ele só barra números **fora** do enum. `Enum.TryParse` também aceita listas separadas por vírgula e combina os valores bit a bit |
| Impacto | Médio. Um pagamento informado como "cartão de crédito e débito" é gravado como Pix, e números entram por qualquer endpoint que receba forma ou status |

### I4 — Verificação de placa duplicada não normaliza a placa (R3)

| | |
|---|---|
| Testes | `VeiculoServiceTests.GarantirPlacaDisponivelAsync_PlacaCadastradaEmOutroFormato_LancaExcecao` com `"abc1d23"`, `"ABC-1D23"` e `" abc 1d23 "` |
| Esperado | `DomainException`, porque `ABC1D23` já está cadastrada e as três grafias são a mesma placa |
| Obtido | O serviço consulta o repositório com o texto cru e conclui que a placa está livre |
| Onde | `VeiculoService.GarantirPlacaDisponivelAsync`. Os serviços equivalentes normalizam antes de consultar: `ClienteService` usa `Cliente.NormalizarCpf` e `FuncionarioService` usa `Funcionario.NormalizarEmail` |
| Impacto | Baixo, defeito **latente**. O único chamador atual (`VeiculoAppService`) passa `veiculo.Placa`, que a entidade já normalizou. O teste unitário isola o serviço e expõe a inconsistência de contrato que o teste pela API não mostraria |

## Mudança no código de produção para testabilidade

O `Id` das entidades era `private set` e só o banco o preenchia. Para que `Locacao` copie `ClienteId`/`VeiculoId` com valores conhecidos, `Categoria`, `Cliente` e `Veiculo` passaram a ter `Id { get; protected set; }` e construtor vazio `protected`. Os testes usam subclasses (`ClienteComId`, etc., em [EntidadesComId.cs](../../tests/Locadora.Domain.Tests/Apoio/EntidadesComId.cs)) que definem o Id. O EF Core continua materializando as entidades normalmente, e nenhuma outra propriedade foi aberta.

## Como reproduzir

```bash
dotnet test tests/Locadora.Domain.Tests --logger "trx;LogFileName=relatorio-inicial.trx" --results-directory docs/testes
```
