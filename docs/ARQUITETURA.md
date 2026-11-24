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
| `Locadora.Desktop` | nenhum projeto | Interface gráfica WPF (MVVM) que consome a API por HTTP. Usa os DTOs da Api por link de arquivo. Detalhes em [Desktop (WPF)](#desktop-wpf) |
| `Locadora.Domain.Tests` (em `tests/`) | Domain | Testes unitários do Domain. Detalhes em [Testes](#testes) |

`Locadora.Api` referencia `Infra` só para registrar a injeção de dependência em `Program.cs`. Controllers nunca usam tipos da Infra.

## Responsabilidade de cada camada

| Camada | Faz | Não faz |
|---|---|---|
| **Entidade** (`Domain/Entidades`) | Mantém invariantes e cálculos que dependem só do próprio estado. Lança `DomainException` | Acessar banco, ler o relógio, expor setters públicos |
| **Serviço de entidade** (`Domain/Servicos`) | Busca por id que lança `NaoEncontradoException` (`ObterAsync`); regras que precisam consultar dados: unicidade, "existe locação aberta?", veículo disponível, autenticação; operações que alteram o agregado (ex.: `LocacaoService.RegistrarEntradaAsync`); valor padrão de filtro (ex.: `status` ausente → `Aberta`) | Commitar, conhecer HTTP ou DTO, ler o relógio |
| **AppService** (`Application/AppServices`) | Orquestra o caso de uso: obtém "agora" do `TimeProvider`, chama o serviço de entidade, chama `IUnitOfWork.CommitAsync()` e devolve a entidade. Listagens podem ir direto ao repositório | Ter regra de negócio própria; lançar `NaoEncontradoException` |
| **Controller** (`Api/Controllers`) | Recebe o request, valida formato (DataAnnotations e model binding), converte DTO ↔ parâmetros/entidade com Mapster e devolve o status HTTP | Acessar repositório ou serviço de entidade diretamente; try/catch; definir filtro padrão; converter texto em enum de negócio |
| **Infra** | Implementa as interfaces do Domain com EF Core e SQL | Ter regra de negócio, inclusive valor padrão de filtro |

### Fluxo de uma requisição

```
POST /api/locacoes/{id}/pagamentos
  LocacoesController.RegistrarPagamento(id, PagamentoRequest)
    → LocacaoAppService.RegistrarEntradaAsync(id, entrada)      ← lê "agora" do TimeProvider
        → LocacaoService.RegistrarEntradaAsync(id, entrada, agora)
            → LocacaoService.ObterAsync(id)                      ← 404 se não existe
                → ILocacaoRepository.ObterComDetalhesAsync(id)
            → Pagamento.CriarEntrada(valor, forma, agora)        ← forma e valor validados na entidade
            → locacao.RegistrarPagamento(pagamento)              ← status e saldo validados na entidade
        → IUnitOfWork.CommitAsync()                              ← única transação
    ← Locacao
  ← LocacaoResponse (Mapster)
```

## Desktop (WPF)

A interface gráfica é um app WPF em MVVM, o padrão do WPF para a ideia de MVC do enunciado:

| MVC | No Desktop | Onde |
|---|---|---|
| Model | DTOs da API (`Locadora.Api/Dtos`, linkados) + `IApiClient` | `Servicos/ApiClient.cs` |
| View | XAML só com layout e bindings; code-behind no máximo dispara o carregamento no `Loaded` | `Views/` |
| Controller | ViewModel: estado da tela e comandos (`[RelayCommand]`) | `ViewModels/` |

### Organização

| Pasta | Contém |
|---|---|
| `Servicos/` | `IApiClient`/`ApiClient` (um método por endpoint), `AuthHeaderHandler` (Bearer; 401 encerra a sessão), `SessaoUsuario`, `INavegacao`, `IDialogos`, `RegistroDeServicos` (DI) |
| `ViewModels/` | Um por tela; `ViewModelBase.ExecutarAsync` cuida de `Ocupado` e `MensagemErro`; `Cadastros/CadastroViewModel<T>` é a base dos 4 cadastros; `Locacoes/` agrupa lista, contrato, retirada e devolução |
| `Views/` | Um XAML por ViewModel, com a mesma estrutura de pastas |
| `Controles/` | Componentes reutilizáveis: `PlacaMercosul`, `StatusSelo`, `Valor`, `MensagemErro`, `EstadoVazio`, `BarraBusca`, `CampoFormulario` |
| `Temas/` | `Cores`, `Tipografia` (Barlow embutida), `Controles` (estilos), `Contrato` (vocabulário visual do contrato), `Telas` (liga ViewModel → View) |
| `Conversores/` | Formatação para a tela: moeda, CPF, telefone, km, status |

### Fluxo de um erro de regra até a tela

```
Domain lança DomainException("Já existe uma categoria com este nome.")
  → ExcecaoHandler devolve ProblemDetails 422 com o texto em "detail"
    → ApiClient lê o problem+json e lança ApiException
      → ViewModelBase.ExecutarAsync põe o texto em MensagemErro
        → o componente MensagemErro mostra junto do formulário
```

O front não repete regra de negócio. Ele só valida formato antes de enviar (ex.: "12,3,4" não é um valor) e mostra **pré-visualizações** (diárias, saldo, multa) que a API confirma ao gravar.

### Identidade visual

Direção "Pátio": fundo concreto claro, menu em asfalto, azul Mercosul como único acento, títulos em Barlow Condensed e a **placa Mercosul** como elemento de identidade em todas as telas com veículo. Cores de status só onde há informação (atraso em vermelho, vence hoje em amarelo); ações destrutivas como link vermelho (`LinkPerigoso`), nunca competindo com a ação principal.

## Convenções

- **Interfaces de serviço:** todo serviço de entidade e todo AppService tem interface, na mesma pasta da implementação (`Domain/Servicos/IClienteService.cs` ao lado de `ClienteService.cs`; `Application/AppServices/IClienteAppService.cs` ao lado de `ClienteAppService.cs`). Quem consome recebe a interface: AppService recebe `IClienteService`, Controller recebe `IClienteAppService`. Registro sempre `AddScoped<IX, X>()`.
- **Busca por id:** todo serviço de entidade expõe `ObterAsync(id)`, que lança `NaoEncontradoException` quando o registro não existe. AppServices e outros serviços de entidade consomem esse método (ex.: `LocacaoService` usa `IClienteService.ObterAsync` e `IVeiculoService.ObterAsync`; `VeiculoService` usa `ICategoriaService.ObterAsync`) em vez de chamar `ObterPorIdAsync` do repositório e lançar a exceção por conta própria.
- **Enums vindos do HTTP:** chegam como `string` (no corpo e na query) e são convertidos no Domain por `Domain/Enums/ConversorEnum.Converter<T>`, que aceita o nome sem diferenciar maiúsculas e recusa números ("7"). Valor inválido → `DomainException` (422) com os nomes aceitos na mensagem. Controllers e DTOs não referenciam enums do Domain.
- **Parâmetros compostos:** dados de entrada que o Domain recebe agrupados (ex.: `PagamentoInformado`) são `record`s em `Domain/Valores`, nunca tuplas.
- **Erros:** regra violada → `DomainException` (422). Registro inexistente → `NaoEncontradoException` (404). Concorrência → `ConflitoConcorrenciaException` (409). Quem converte é `Api/Middleware/ExcecaoHandler.cs`, que devolve ProblemDetails; o WPF exibe o campo `detail`.
- **Concorrência otimista:** `Veiculos` e `Locacoes` têm coluna `rowversion` (shadow property `RowVersion`, só no mapeamento da Infra). Duas alterações simultâneas do mesmo registro (pagamentos na mesma locação, aberturas do mesmo veículo) fazem a segunda falhar com 409: "O registro foi alterado por outro funcionário...".
- **Data e hora:** só o AppService lê o relógio (`TimeProvider` injetado) e passa a data para a entidade. Em Development, `Relogio:DeslocamentoHoras` no `appsettings.Development.json` adianta o relógio para demonstrar multa e tolerância.
- **Dinheiro:** todo `decimal` vira `decimal(10,2)` por convenção no `LocadoraDbContext`.
- **Exclusão:** não existe DELETE. Registros são inativados (`Ativo = false`) e podem ser reativados.
- **Mapeamento EF:** um `IEntityTypeConfiguration<T>` por entidade em `Infra/Mapeamentos`, carregados automaticamente.
- **DTOs:** `record`s em `Api/Dtos`, sem tipos do ASP.NET (a pasta é compilada também no WPF).
- **Comentários:** só quando o código não explica o porquê.

## Testes

Testes unitários das entidades do Domain, em `tests/Locadora.Domain.Tests`. Rodam com `dotnet test`, sem banco nem API.

- **Ferramentas:** xUnit v3 sobre o Microsoft.Testing.Platform (declarado no `global.json`, exigido pelo `dotnet test` do SDK 10), NSubstitute para as interfaces e o `Assert` do próprio xUnit, sem biblioteca de asserção.
- **Organização:** as pastas espelham o Domain (`Entidades/`, `Servicos/`). Há uma classe de teste por classe testada (`LocacaoTests`), e os métodos seguem `Metodo_Cenario_Resultado` (`Finalizar_ComAtrasoAlemDaTolerancia_CobraMultaPorDia`).
- **Exceções:** o teste confere o tipo e a mensagem exata. Quase toda regra lança `DomainException`, então conferir só o tipo deixaria um teste passar porque outra validação barrou antes. A mensagem também é o texto que o usuário vê na tela.
- **Cálculos:** `[Theory]` nos pontos de virada: 60 e 61 minutos de tolerância, 24h e 24h01 de diária, arredondamento da multa, `VALOR_MAXIMO` e um centavo acima.
- **Dados:** `Suporte/Fabrica` cria entidades válidas com parâmetros opcionais, e cada teste informa só o que importa para ele. As datas partem de `Fabrica.Agora`, que é fixa.
- **Ids:** o `Id` das entidades é `protected set`. Subclasses de teste (`ClienteComId`, `VeiculoComId`...) atribuem o Id sem reflexão e sem `InternalsVisibleTo`.

## Onde está o SOLID

| Princípio | Onde | Como |
|---|---|---|
| S — Responsabilidade única | `Locacao` / `LocacaoService` / `LocacaoAppService` / `LocacoesController` | A entidade guarda invariantes e cálculos (saldo, diárias); o serviço busca dados e aplica regras que dependem deles; o AppService só orquestra relógio e commit; o controller só traduz HTTP ↔ DTO |
| O — Aberto/fechado | `Api/Middleware/ExcecaoHandler.cs` | Novo tipo de exceção entra no dicionário `Mapa`, sem novo `if` |
| O — Aberto/fechado | `Infra/Contexto/LocadoraDbContext.cs` | `ApplyConfigurationsFromAssembly`: entidade nova não altera o DbContext |
| L — Substituição de Liskov | `Api/Relogio/RelogioDeslocado.cs` | Substitui `TimeProvider.System` sem que nenhuma camada perceba |
| I — Segregação de interfaces | `Domain/Interfaces` | Um repositório por agregado; `IPainelConsulta` (só leitura, SQL manual) fica separada de `ILocacaoRepository`, então o painel não depende de métodos de escrita |
| D — Inversão de dependência | `Domain/Interfaces/IUnitOfWork.cs` → `Infra/Repositorios/UnitOfWork.cs` | Domain define, Infra implementa, `Infra/DependencyInjection.cs` liga. O `.csproj` do Domain não referencia nada |
| D — Inversão de dependência | `Application/AppServices/FuncionarioAppService.cs`, `Api/Controllers/FuncionariosController.cs` | Cada camada depende da interface da camada de baixo (`IFuncionarioService`, `IFuncionarioAppService`), nunca da classe concreta |
| S — Responsabilidade única (Desktop) | `Desktop/ViewModels/Locacoes/` | Um ViewModel por tela (lista, contrato, retirada, devolução) e um por formulário embutido (`LancamentoDeEntradaViewModel`, `ProrrogacaoViewModel`); a View só faz layout |
| O — Aberto/fechado (Desktop) | `Desktop/ViewModels/Cadastros/CadastroViewModel.cs` | Um cadastro novo herda a base e implementa os ganchos; lista, busca, salvar, inativar e reativar não mudam |
| L — Substituição de Liskov (Desktop) | `Desktop/Servicos/AuthHeaderHandler.cs` | É um `DelegatingHandler`: entra no pipeline do `HttpClient` no lugar de qualquer handler, sem o `ApiClient` saber |
| I — Segregação de interfaces (Desktop) | `Desktop/Servicos/INavegacao.cs`, `IDialogos.cs` | Cada ViewModel recebe só o que usa: navegar, confirmar ou falar com a API |
| D — Inversão de dependência (Desktop) | `Desktop/Servicos/IApiClient.cs` | Nenhum ViewModel conhece `HttpClient`; todos recebem `IApiClient` pelo DI (`RegistroDeServicos`) |

## Onde está o Clean Code

| Prática | Onde |
|---|---|
| Exceções com mensagem de negócio em vez de códigos de erro | `Domain/Excecoes` |
| Tradução de exceção técnica na fronteira (EF não vaza) | `Infra/Repositorios/UnitOfWork.cs` |
| Configuração em vez de número mágico | `Program.cs` (`CriarRelogio`), `appsettings*.json` |
| Nomes que revelam intenção: `PodeSerLocado()`, `SaldoDevedor()`, `CalcularDiasAtraso()`, `GarantirCpfDisponivelAsync()`, `CancelarAlocacao()` | `Domain/Entidades`, `Domain/Servicos` |
| Constantes nomeadas em vez de números mágicos, reaproveitadas até no SQL manual | `Locacao.TAXA_MULTA` e `Locacao.TOLERANCIA_MINUTOS` → `Infra/Consultas/PainelConsulta.cs` |
| Guard clauses: valida e sai cedo, sem `if` aninhado; validações antes de qualquer mudança de estado | `Locacao.Finalizar`, `Locacao.Prorrogar`, `Veiculo.Alocar` |
| Encapsulamento: `private set` em todas as entidades (só o `Id` é `protected set`, para os testes), coleção exposta como `IReadOnlyCollection` | `Locacao.Pagamentos` (backing field `_pagamentos`) |
| Funções pequenas com uma responsabilidade; AppServices de 3 a 5 linhas por caso de uso | `Application/AppServices/LocacaoAppService.cs` |
| Sem duplicação: conversão de enum e normalização de dados num lugar só | `Domain/Enums/ConversorEnum.cs`, `Cliente.NormalizarCpf`, `Funcionario.NormalizarEmail`, `Veiculo.NormalizarPlaca` |
| Comentários só onde o código não explica o porquê | ex.: `DATEDIFF` em segundos no `PainelConsulta`, relógio real no `JwtTokenService` |
| Desktop: nomes pela ação do usuário e pré-visualização explícita | `SalvarCommand`, `RegistrarRetiradaCommand`, `ResumoDaRetirada.PreverDiarias` |
| Desktop: sem duplicação de XAML ou formatação | estilos em `Temas/`, componentes em `Controles/`, formatação em `Conversores/`, leitura de valores em `ValorMonetario` |
| Desktop: ordem fixa nas classes e métodos curtos | campos → construtor → propriedades → comandos → métodos privados; ex.: `ViewModels/ShellViewModel.cs` |

## SQL manual

Arquivo: `Infra/Consultas/PainelConsulta.cs`, que implementa `Domain/Interfaces/IPainelConsulta.cs`. Usa `context.Database.SqlQuery<T>($"...")` e materializa nos modelos de leitura de `Domain/Consultas` (`LocacaoAtrasadaItem`, `LocacaoVencendoItem`, `ContadorFrotaItem`, agrupados em `Painel`). Endpoint: `GET api/painel`.

| Consulta | O que faz |
|---|---|
| Atrasadas | `Locacoes JOIN Clientes JOIN Veiculos` com `Status = 'Aberta'` e prevista anterior a `agora`. O SQL calcula `MinutosAtraso` (`DATEDIFF`), `DiasAtraso` (0 dentro da tolerância, senão `CEILING(minutos / 1440.0)`) e `MultaEstimada` (`DiasAtraso * ValorDiaria * (1 + taxa)`, 2 casas). Ordena pelo maior atraso |
| Vencendo hoje | Abertas com prevista entre `agora` e o início de amanhã (`agora.Date.AddDays(1)`, calculado em C#), ordenadas pela prevista |
| Frota | `COUNT(*) ... GROUP BY Status` dos veículos ativos |

- **Parâmetros, não concatenação:** `SqlQuery` recebe `FormattableString`. Cada `{valor}` interpolado vira um `DbParameter` (`@p0`, `@p1`...), nunca texto do SQL, então não há injeção. `agora`, `TOLERANCIA_MINUTOS` e `TAXA_MULTA` entram assim; o SQL não tem número mágico.
- **Regra de multa:** a tolerância e a taxa vêm das constantes de `Locacao`, as mesmas da devolução, para a estimativa do painel bater com a multa cobrada.
- **Relógio:** a consulta recebe `agora` por parâmetro; só o `PainelAppService` lê o `TimeProvider`.
- **ORDER BY no LINQ:** o EF embrulha o SQL em subconsulta, onde `ORDER BY` puro é inválido; por isso a ordenação é encadeada depois do `SqlQuery`.
- **Interface separada (ISP):** `IPainelConsulta` só lê e devolve modelos de leitura, não entidades. Quem consome o painel não depende dos métodos de escrita de `ILocacaoRepository`, e a consulta pode mudar (ou virar view) sem tocar no repositório.

## Tradeoffs da arquitetura

| Decisão | Ganha | Custa |
|---|---|---|
| DTOs no projeto Api | Um projeto a menos | AppService devolve entidade; o WPF depende de link de arquivo |
| Entidade chega até o Controller | Sem classes intermediárias na Application | O Controller precisa conhecer a forma da entidade para mapear |
| Repositório por agregado + UnitOfWork | Domain sem EF; commit controlado pelo AppService | Mais interfaces que usar o DbContext direto |
| Relógio injetado | Regras de tempo demonstráveis e determinísticas | Toda chamada com data precisa receber o "agora" |
| Testes unitários só no Domain | Invariantes e cálculos verificados em segundos, sem banco | Application, Infra (mapeamentos, SQL do painel) e Api continuam validados manualmente pelo Swagger |
| Concorrência otimista (`rowversion`) | Sem lock no banco; o caso comum não espera | O segundo funcionário recebe 409 e precisa repetir a operação |
| `ValorPago` guardado na locação | Listagem e saldo sem somar pagamentos | Dado duplicado; só muda em `RegistrarPagamento` e `Finalizar`, protegido pelo `rowversion` |
| Enums de negócio convertidos no Domain | Uma regra só para corpo e query; mensagem em português com os valores aceitos | Valor inválido é 422 (regra), não 400 (formato) |
| Painel em SQL manual | Atende o enunciado; cálculo de atraso numa consulta só | Regra de multa existe em dois lugares (entidade e SQL); as constantes são compartilhadas para não divergir |

## Desvios do enunciado

O modelo implementa só associações 1xN e a composição `Locacao ◆ Pagamento`. Herança, 1x1 e NxN não fazem parte do domínio modelado. Se forem exigidas, as extensões candidatas são `Pessoa → Cliente, Funcionario` (herança), `Cliente ↔ Endereco` (1x1) e `Locacao ↔ Acessorio` (NxN).
