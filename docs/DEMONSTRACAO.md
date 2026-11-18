# Roteiro de demonstração

Pelo app (Locadora.Api + Locadora.Desktop rodando, veja o README). Entre com `admin@locadora.local` / `admin123`.

## 1. Cadastros

| Tela | O que fazer |
|---|---|
| Categorias | Nova categoria "Econômico", diária `150,00`. Tente salvar de novo com o mesmo nome: a mensagem da API aparece no formulário |
| Veículos | Novo veículo com placa `abc1d23` (o campo aceita só letras e números; vira ABC1D23 na placa desenhada), categoria Econômico, km 10000 |
| Clientes | Novo cliente com CPF `12345678901`, CNH de 11 dígitos e telefone `11987654321` (os campos aceitam só números) |

## 2. Retirada

Locações → **Registrar retirada**: escolha o cliente, o veículo e a devolução prevista **daqui a 3 dias**. Adicione uma entrada de R$ 100 em Pix.

O resumo à direita mostra **3 diárias, R$ 450,00 e saldo de R$ 350,00** antes de gravar. **Registrar retirada** abre o contrato, e o veículo some da lista de disponíveis.

## 3. Durante a locação (no contrato)

- **Lançar entrada**: R$ 50 em Dinheiro; o extrato e o saldo atualizam.
- **Prorrogar**: mais 1 dia; a prévia mostra 4 diárias e R$ 600,00 antes de confirmar.
- **Painel**: a locação aparece em "Vencem hoje" quando a prevista é hoje.

## 4. Devolução com atraso (relógio adiantado)

1. Feche o app e a API. Em `src/Locadora.Api/appsettings.Development.json`, coloque `"Relogio": { "DeslocamentoHoras": 122 }` (≈ 26h depois da prevista de 4 dias). Rode de novo.
2. **Painel**: a locação está em "Atrasadas" com **multa estimada de R$ 360,00**, calculada pelo **SQL manual**.
3. No contrato, **Registrar devolução**: a faixa diz "2 dias de atraso. Multa de R$ 360,00." e o saldo bate com o painel.
4. Informe o km (10800) e um pagamento menor que o saldo: o indicador mostra "Falta R$ ..." e o botão fica desabilitado. Ajuste até "Saldo fechado" e **Confirmar devolução**.
5. O contrato fica "Finalizada" e o veículo volta "Disponível" com o km novo (tela Veículos).
6. Volte `DeslocamentoHoras` para `0`.

## 5. Regras que vale mostrar

| Ação | Resultado |
|---|---|
| Inativar um cliente com locação aberta | mensagem da API no formulário |
| Enviar para manutenção um veículo locado | botão desabilitado (e a API também recusa) |
| Inativar o próprio usuário em Funcionários | botão desabilitado, com a explicação |
| Inativar outro funcionário logado em outra máquina | a sessão dele termina na próxima ação |
| Cancelar locação | o veículo volta "Disponível" com o mesmo km; as entradas continuam no extrato |
| Prorrogar uma locação vencida | ação desabilitada: "Registre a devolução." |

## Pelo Swagger (alternativa)

Em `http://localhost:5056/swagger`: `POST /api/auth/login` → **Authorize** com o token → `POST /api/categorias`, `/api/veiculos`, `/api/clientes`, `/api/locacoes` → `GET /api/locacoes/{id}/previa-devolucao` → `POST /api/locacoes/{id}/finalizar`.
