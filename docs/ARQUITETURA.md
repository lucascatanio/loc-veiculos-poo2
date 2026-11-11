# Arquitetura — Locadora de Veículos

API REST em .NET 10 + cliente desktop WPF. Banco SQL Server (LocalDB) via EF Core, com uma consulta em SQL manual (painel do balcão).

Modelo de domínio: [diagrama-classes-locadora.png](diagrama-classes-locadora.png).

## Projetos e dependências

```
Locadora.Desktop (WPF) ──HTTP──▶ Locadora.Api ──▶ Locadora.Application ──▶ Locadora.Domain
                                       │                                         ▲
                                       └──────────▶ Locadora.Infra ──────────────┘
```

| Projeto | Referencia | Contém |
|---|---|---|
| `Locadora.Domain` | nada | Entidades, enums, serviços de entidade, interfaces de repositório, exceções |
| `Locadora.Infra` | Domain | `LocadoraDbContext`, mapeamentos EF, repositórios, `UnitOfWork`, SQL manual, hash de senha |
| `Locadora.Application` | Domain | AppServices (um por caso de uso/recurso) |
| `Locadora.Api` | Application, Infra | Controllers, DTOs, Mapster, tratamento de erros, JWT |
| `Locadora.Desktop` | nenhum projeto | Telas WPF (MVVM). Usa os DTOs da Api por link de arquivo |

`Locadora.Api` referencia `Infra` só para registrar a injeção de dependência em `Program.cs`. Controllers nunca usam tipos da Infra.

## Responsabilidade de cada camada

| Camada | Faz | Não faz |
|---|---|---|
| **Entidade** (`Domain/Entidades`) | Mantém invariantes e cálculos que dependem só do próprio estado. Lança `DomainException` | Acessar banco, ler o relógio, expor setters públicos |
| **Serviço de entidade** (`Domain/Servicos`) | Regras que precisam consultar dados: unicidade, "existe locação aberta?", veículo disponível | Commitar, conhecer HTTP ou DTO |
| **AppService** (`Application/AppServices`) | Orquestra o caso de uso: carrega, chama serviço/entidade, obtém "agora" do `TimeProvider`, chama `IUnitOfWork.CommitAsync()` e devolve a entidade | Ter regra de negócio própria |
| **Controller** (`Api/Controllers`) | Recebe o request, valida formato (DataAnnotations), converte DTO ↔ parâmetros/entidade com Mapster e devolve o status HTTP | Acessar repositório ou serviço de entidade diretamente; try/catch |
| **Infra** | Implementa as interfaces do Domain com EF Core e SQL | Ter regra de negócio |

### Fluxo de uma requisição

```
POST /api/locacoes/{id}/finalizar
  LocacoesController.Finalizar(id, FinalizarLocacaoRequest)
    → LocacaoAppService.FinalizarAsync(id, km, obs, saidas)
        → ILocacaoRepository.ObterComVeiculoEPagamentosAsync(id)
        → locacao.Finalizar(agora, km, obs, saidas)      ← regra na entidade
        → IUnitOfWork.CommitAsync()                       ← única transação
    ← Locacao
  ← LocacaoResponse (Mapster)
```

## Convenções

- **Erros:** regra violada → `DomainException` (422). Registro inexistente → `NaoEncontradoException` (404). Concorrência → `ConflitoConcorrenciaException` (409). Quem converte é `Api/Middleware/ExcecaoHandler.cs`, que devolve ProblemDetails; o WPF exibe o campo `detail`.
- **Data e hora:** só o AppService lê o relógio (`TimeProvider` injetado) e passa a data para a entidade. Em Development, `Relogio:DeslocamentoHoras` no `appsettings.Development.json` adianta o relógio para demonstrar multa e tolerância.
- **Dinheiro:** todo `decimal` vira `decimal(10,2)` por convenção no `LocadoraDbContext`.
- **Exclusão:** não existe DELETE. Registros são inativados (`Ativo = false`) e podem ser reativados.
- **Mapeamento EF:** um `IEntityTypeConfiguration<T>` por entidade em `Infra/Mapeamentos`, carregados automaticamente.
- **DTOs:** `record`s em `Api/Dtos`, sem tipos do ASP.NET (a pasta é compilada também no WPF).
- **Comentários:** só quando o código não explica o porquê.

## Onde está o SOLID

| Princípio | Onde | Como |
|---|---|---|
| S — Responsabilidade única | `Locacao` / `LocacaoService` / `LocacaoAppService` / `LocacoesController` | _preencher ao implementar_ |
| O — Aberto/fechado | `Api/Middleware/ExcecaoHandler.cs` | Novo tipo de exceção entra no dicionário `Mapa`, sem novo `if` |
| O — Aberto/fechado | `Infra/Contexto/LocadoraDbContext.cs` | `ApplyConfigurationsFromAssembly`: entidade nova não altera o DbContext |
| L — Substituição de Liskov | `Api/Relogio/RelogioDeslocado.cs` | Substitui `TimeProvider.System` sem que nenhuma camada perceba |
| I — Segregação de interfaces | `Domain/Interfaces` | _preencher: um repositório por agregado; `IPainelConsulta` separado da escrita_ |
| D — Inversão de dependência | `Domain/Interfaces/IUnitOfWork.cs` → `Infra/Repositorios/UnitOfWork.cs` | Domain define, Infra implementa, `Infra/DependencyInjection.cs` liga. O `.csproj` do Domain não referencia nada |

## Onde está o Clean Code

| Prática | Onde |
|---|---|
| Exceções com mensagem de negócio em vez de códigos de erro | `Domain/Excecoes` |
| Tradução de exceção técnica na fronteira (EF não vaza) | `Infra/Repositorios/UnitOfWork.cs` |
| Configuração em vez de número mágico | `Program.cs` (`CriarRelogio`), `appsettings*.json` |
| _preencher ao implementar: nomes que revelam intenção, constantes `TAXA_MULTA`/`TOLERANCIA_MINUTOS`, guard clauses, `private set`_ | |

## SQL manual

_Preencher no card do painel:_ `Infra/Consultas/PainelConsulta.cs`, consultas parametrizadas via `Database.SqlQuery<T>`.

## Tradeoffs da arquitetura

| Decisão | Ganha | Custa |
|---|---|---|
| DTOs no projeto Api | Um projeto a menos | AppService devolve entidade; o WPF depende de link de arquivo |
| Entidade chega até o Controller | Sem classes intermediárias na Application | O Controller precisa conhecer a forma da entidade para mapear |
| Repositório por agregado + UnitOfWork | Domain sem EF; commit controlado pelo AppService | Mais interfaces que usar o DbContext direto |
| Relógio injetado | Regras de tempo demonstráveis e determinísticas | Toda chamada com data precisa receber o "agora" |
| Sem testes automatizados | Menos escopo | Regras de cálculo validadas manualmente pelo Swagger |

## Desvios do enunciado

O modelo implementa só associações 1xN e a composição `Locacao ◆ Pagamento`. Herança, 1x1 e NxN não fazem parte do domínio modelado. Se forem exigidas, as extensões candidatas são `Pessoa → Cliente, Funcionario` (herança), `Cliente ↔ Endereco` (1x1) e `Locacao ↔ Acessorio` (NxN).
