# Locadora de Veículos

## Como rodar

**Precisa de:** Visual Studio 2026 (carga *ASP.NET e Web*) com o SQL Server LocalDB.

1. Clone o repositório e abra o `Locadora.sln`.
2. No terminal do VS (*Exibir → Terminal*), crie o banco:
   ```bash
   dotnet tool restore
   dotnet build Locadora.sln
   dotnet ef database update -p src/Locadora.Infra -s src/Locadora.Api
   ```
3. Defina **Locadora.Api** como projeto de inicialização, escolha o perfil **http** e aperte **F5**. O Swagger abre no navegador.
4. Faça login em `POST /api/auth/login` com `admin@locadora.local` / `admin123`. Depois clique em **Authorize** e cole o token.

Se der erro de conexão com o banco, rode `sqllocaldb start MSSQLLocalDB`.

Arquitetura e padrões: [docs/ARQUITETURA.md](docs/ARQUITETURA.md).
