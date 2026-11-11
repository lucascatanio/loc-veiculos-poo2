# Locadora de Veículos

API REST em .NET 10 com camadas (Domain, Infra, Application, Api), SQL Server LocalDB via EF Core e cliente desktop WPF.
Arquitetura, padrões e onde estão SOLID e Clean Code: [docs/ARQUITETURA.md](docs/ARQUITETURA.md).

## Rodando num PC novo (Visual Studio)

### 1. Pré-requisitos

| O quê | Como conferir |
|---|---|
| **Visual Studio 2026** com as cargas de trabalho **Desenvolvimento ASP.NET e Web** e **Desenvolvimento de desktop com .NET** | No Visual Studio Installer, botão *Modificar* |
| **SQL Server Express LocalDB** (componente individual; costuma vir com a carga ASP.NET) | `sqllocaldb info` no terminal deve listar `MSSQLLocalDB` |
| **.NET 10 SDK** (vem com o VS 2026) | `dotnet --list-sdks` deve mostrar `10.0.x` |
| Internet (para baixar os pacotes NuGet na primeira compilação) | — |

> O Visual Studio 2022 não abre projetos .NET 10. Se o laboratório só tiver o VS 2022, instale o [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) e use a seção [Sem Visual Studio](#sem-visual-studio-terminal).

### 2. Clonar

No Visual Studio, use *Clonar um repositório* com a URL abaixo. Pelo terminal:

```bash
git clone https://github.com/lucascatanio/loc-veiculos-poo2.git
cd loc-veiculos-poo2
```

> Enquanto os PRs dos cards 03 a 06 não forem mergeados, o código completo está na branch `feat/card-06-abertura-locacao` (`git checkout feat/card-06-abertura-locacao`). Depois do merge, use a `main`.

Abra o arquivo **`Locadora.sln`**.

### 3. Criar o banco

Abra o terminal do Visual Studio (*Exibir → Terminal*). Ele já abre na pasta da solution. Rode os três comandos **nesta ordem**:

```bash
dotnet tool restore
dotnet build Locadora.sln
dotnet ef database update -p src/Locadora.Infra -s src/Locadora.Api
```

- `dotnet tool restore` instala o `dotnet-ef` na versão do projeto.
- `dotnet build` baixa os pacotes NuGet. **Sem ele, o comando seguinte falha** com `NETSDK1004 ... project.assets.json not found`.
- `dotnet ef database update` cria o banco `Locadora` no LocalDB, aplica todas as migrations e cadastra o usuário admin.

Se terminar com `Done.`, o banco está pronto.

### 4. Rodar a API

1. No Gerenciador de Soluções, clique com o botão direito em **Locadora.Api** → *Definir como Projeto de Inicialização*.
2. No seletor ao lado do botão ▶, escolha o perfil **http**.
3. Aperte **F5**. O navegador abre o Swagger em `http://localhost:5056/swagger`.

### 5. Fazer login no Swagger

Todos os endpoints exigem token, exceto o login.

1. Abra `POST /api/auth/login` → *Try it out* e envie:
   ```json
   { "email": "admin@locadora.local", "senha": "admin123" }
   ```
2. Copie o valor de `token` da resposta.
3. Clique em **Authorize** (cadeado no topo), cole **só o token**, sem a palavra `Bearer`, e confirme.

### 6. Roteiro rápido de teste

Execute nesta ordem pelo Swagger. Cada passo usa o `id` que o anterior devolveu.

| Passo | Endpoint | Corpo |
|---|---|---|
| Categoria | `POST /api/categorias` | `{ "nome": "Econômico", "valorDiaria": 150 }` |
| Veículo | `POST /api/veiculos` | `{ "placa": "ABC1D23", "marca": "Fiat", "modelo": "Argo", "ano": 2023, "cor": "Prata", "kmAtual": 10000, "categoriaId": 1 }` |
| Cliente | `POST /api/clientes` | `{ "nome": "Maria Silva", "cpf": "123.456.789-01", "cnh": "12345678901", "telefone": "(11) 98765-4321" }` |
| Locação | `POST /api/locacoes` | `{ "clienteId": 1, "veiculoId": 1, "dataPrevistaDevolucao": "<daqui a 3 dias, ex.: 2026-11-14T09:00:00>", "entradas": [ { "valor": 100, "forma": "Pix" } ] }` |

A locação deve voltar com 3 diárias: `valorPrevisto` 450, `valorPago` 100 e `saldoDevedor` 350. O veículo passa a "Locado" e some de `GET /api/veiculos/disponiveis`.

## Demonstrar multa por atraso sem esperar dias

> A prévia e a devolução com multa chegam no card 08. Até lá, este ajuste não tem efeito visível.

Em `src/Locadora.Api/appsettings.Development.json`, aumente o relógio da API:

```json
"Relogio": { "DeslocamentoHoras": 98 }
```

Pare e rode de novo (F5). A API passa a achar que está 98 horas no futuro. Volte para `0` ao terminar. Isso só vale em Development.

## Sem Visual Studio (terminal)

Mesmos passos 2 e 3, e depois:

```bash
dotnet run --project src/Locadora.Api      # Swagger em http://localhost:5056/swagger
```

## Problemas comuns

| Sintoma | Solução |
|---|---|
| `NETSDK1004 ... project.assets.json not found` no `dotnet ef` | Rode `dotnet build Locadora.sln` antes |
| `dotnet ef` não é reconhecido / `Could not execute because the specified command or file was not found` | Rode `dotnet tool restore` na pasta da solution |
| `A network-related or instance-specific error ... (localdb)\MSSQLLocalDB` | `sqllocaldb start MSSQLLocalDB`. Se o `sqllocaldb info` não listar nada: `sqllocaldb create MSSQLLocalDB -s` |
| `NETSDK1045: The current .NET SDK does not support targeting .NET 10.0` | Instale o .NET 10 SDK ou use o VS 2026 |
| Navegador pede para confiar em certificado HTTPS | Use o perfil **http** (passo 4) |
| `Failed to bind to address http://127.0.0.1:5056: address already in use` | Já existe uma API rodando: feche a outra janela ou pare a depuração anterior |
| Erro 401 em tudo | Faça login de novo (o token vale 8h) e cole em **Authorize** só o token |
| Quer começar com o banco vazio | `dotnet ef database drop -f -p src/Locadora.Infra -s src/Locadora.Api` e depois o `database update` do passo 3 |

## Comandos úteis

```bash
dotnet build Locadora.sln
dotnet ef migrations add <Nome> -p src/Locadora.Infra -s src/Locadora.Api
dotnet ef database update     -p src/Locadora.Infra -s src/Locadora.Api
```

> O projeto **Locadora.Desktop** (WPF) ainda é só o esqueleto: ele compila, mas não tem telas. Toda a demonstração por enquanto é pelo Swagger.
