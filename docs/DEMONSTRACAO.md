# Roteiro de demonstração

Tudo pelo Swagger (`http://localhost:5056/swagger`), depois de fazer login e clicar em **Authorize**. Use os `id`s que cada resposta devolver.

## 1. Cadastros

| Endpoint | Corpo |
|---|---|
| `POST /api/categorias` | `{ "nome": "Econômico", "valorDiaria": 150 }` |
| `POST /api/veiculos` | `{ "placa": "ABC1D23", "marca": "Fiat", "modelo": "Argo", "ano": 2023, "cor": "Prata", "kmAtual": 10000, "categoriaId": 1 }` |
| `POST /api/clientes` | `{ "nome": "Maria Silva", "cpf": "123.456.789-01", "cnh": "12345678901", "telefone": "(11) 98765-4321" }` |

## 2. Retirada

`POST /api/locacoes` com a data prevista **daqui a 3 dias** e entrada de R$ 100:

```json
{ "clienteId": 1, "veiculoId": 1, "dataPrevistaDevolucao": "2026-11-20T10:00:00",
  "entradas": [ { "valor": 100, "forma": "Pix" } ] }
```

Esperado: 3 diárias, `valorPrevisto` 450, `saldoDevedor` 350. O veículo fica "Locado".

## 3. Durante a locação

- `POST /api/locacoes/{id}/pagamentos`: `{ "valor": 50, "forma": "Dinheiro" }`
- `POST /api/locacoes/{id}/prorrogar`: `{ "novaDataPrevista": "<1 dia depois da prevista>" }` → o previsto vai para 600
- `GET /api/painel`: a locação aparece em `vencendoHoje` quando a prevista é hoje

## 4. Devolução com atraso (relógio adiantado)

1. Pare a API e, em `src/Locadora.Api/appsettings.Development.json`, coloque `"Relogio": { "DeslocamentoHoras": 122 }` (≈ 26h depois da prevista de 4 dias). Rode de novo.
2. `GET /api/locacoes/{id}/previa-devolucao` → 2 dias de atraso, **multa 360**.
3. `GET /api/painel` → a mesma multa em `atrasadas`, calculada pelo **SQL manual**.
4. `POST /api/locacoes/{id}/finalizar` com saídas que **não** fecham o saldo → 422 dizendo quanto falta.
5. `POST /api/locacoes/{id}/finalizar` com `{ "kmDevolucao": 10800, "saidas": [ { "valor": <saldo da prévia>, "forma": "CartaoCredito" } ] }` → "Finalizada". O veículo volta "Disponivel" com o km novo.
6. Volte `DeslocamentoHoras` para `0`.

## 5. Regras que vale mostrar

| Ação | Resultado |
|---|---|
| Abrir locação com veículo já locado | 422 |
| Inativar cliente com locação aberta / veículo locado | 422 |
| Entrada maior que o saldo | 422, e nada é gravado |
| Devolver até 60 min depois da prevista | sem multa (tolerância) |
| `POST /api/locacoes/{id}/cancelar` | veículo volta "Disponivel" com o mesmo km; as entradas continuam registradas |
| Inativar um funcionário que está logado | o token dele para de valer na hora (401) |
