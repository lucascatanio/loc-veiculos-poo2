# Locadora de Veículos

## Como rodar

**Precisa de:** Visual Studio 2026 (cargas *ASP.NET e Web* e *Desenvolvimento de desktop com .NET*) com o SQL Server LocalDB.

1. Clone o repositório e abra o `Locadora.sln`.
2. No terminal do VS (*Exibir → Terminal*), crie o banco:
   ```bash
   dotnet tool restore
   dotnet build Locadora.sln
   dotnet ef database update -p src/Locadora.Infra -s src/Locadora.Api
   ```
3. Clique com o botão direito na solução → *Configurar Projetos de Inicialização* → *Vários projetos de inicialização*: **Locadora.Api** e **Locadora.Desktop** como *Iniciar* (a Api primeiro). Aperte **F5**.
4. No app, entre com `admin@locadora.local` / `admin123`.

O Swagger da API fica em `http://localhost:5056/swagger` (login em `POST /api/auth/login`, depois **Authorize** com o token).

Se der erro de conexão com o banco, rode `sqllocaldb start MSSQLLocalDB`.

- Roteiro de demonstração: [docs/DEMONSTRACAO.md](docs/DEMONSTRACAO.md)
- Arquitetura, SOLID, Clean Code e SQL manual: [docs/ARQUITETURA.md](docs/ARQUITETURA.md)

## Desenvolvimento

Em Debug, o desktop aceita atalhos de linha de comando (ignorados em Release):
`dotnet run --project src/Locadora.Desktop -- --login admin@locadora.local admin123` entra sozinho,
e `--tela <Painel|Locacoes|NovaLocacao|Clientes|Veiculos|Categorias|Funcionarios>` abre direto nessa tela depois do login; `--locacao <id>` abre o contrato e `--devolucao <id>` a devolução dessa locação. A lista completa está em `src/Locadora.Desktop/Desenvolvimento/AtalhosDeDesenvolvimento.cs`.
