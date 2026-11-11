# Locadora de Veículos

Arquitetura, responsabilidade de cada camada e convenções: [docs/ARQUITETURA.md](docs/ARQUITETURA.md). Leia antes de implementar qualquer coisa.

## Comandos

```bash
dotnet tool restore                      # instala o dotnet-ef local (dotnet-tools.json)
dotnet build Locadora.sln

dotnet run --project src/Locadora.Api    # Swagger em http://localhost:5056/swagger
dotnet run --project src/Locadora.Desktop

dotnet ef migrations add <Nome> -p src/Locadora.Infra -s src/Locadora.Api
dotnet ef migrations remove     -p src/Locadora.Infra -s src/Locadora.Api
dotnet ef database update       -p src/Locadora.Infra -s src/Locadora.Api

dotnet add src/<Projeto> package <Pacote> -v 10.0.15   # sem -v puxa a 11.x, que não roda em net10.0
```
