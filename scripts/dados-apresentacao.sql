/*
    Dados da apresentação: apaga tudo, menos o admin, e cria uma locadora em funcionamento.

    Pré-requisito: banco criado com
        dotnet ef database update -p src/Locadora.Infra -s src/Locadora.Api

    Rodar (ou abrir no SSMS / extensão SQL Server do VS Code e executar):
        sqlcmd -S "(localdb)\MSSQLLocalDB" -d Locadora -f 65001 -i scripts/dados-apresentacao.sql

    Login: admin@locadora.local / admin123

    As datas partem do horário em que o script roda, então rode no dia da gravação.
    O painel fica com 1 locação atrasada e 1 vencendo hoje. Para a retirada do roteiro,
    o Chevrolet Onix (Intermediário, R$ 150) e a cliente Mariana Costa Ribeiro estão livres.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DELETE FROM Pagamentos;
DELETE FROM Locacoes;
DELETE FROM Veiculos;
DELETE FROM Clientes;
DELETE FROM Categorias;
DELETE FROM Funcionarios WHERE Email <> N'admin@locadora.local';
UPDATE Funcionarios SET Ativo = 1 WHERE Email = N'admin@locadora.local';

-- Faz os Ids recomeçarem em 1. Tabela que nunca teve linha já começa em 1 e não pode ser ressemeada.
IF (SELECT last_value FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'Pagamentos')) IS NOT NULL DBCC CHECKIDENT (N'Pagamentos', RESEED, 0) WITH NO_INFOMSGS;
IF (SELECT last_value FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'Locacoes')) IS NOT NULL DBCC CHECKIDENT (N'Locacoes', RESEED, 0) WITH NO_INFOMSGS;
IF (SELECT last_value FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'Veiculos')) IS NOT NULL DBCC CHECKIDENT (N'Veiculos', RESEED, 0) WITH NO_INFOMSGS;
IF (SELECT last_value FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'Clientes')) IS NOT NULL DBCC CHECKIDENT (N'Clientes', RESEED, 0) WITH NO_INFOMSGS;
IF (SELECT last_value FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'Categorias')) IS NOT NULL DBCC CHECKIDENT (N'Categorias', RESEED, 0) WITH NO_INFOMSGS;

INSERT INTO Categorias (Nome, ValorDiaria, Ativo) VALUES
    (N'Econômico',     120.00, 1),
    (N'Intermediário', 150.00, 1),
    (N'SUV',           230.00, 1),
    (N'Utilitário',    190.00, 1),
    (N'Conversível',   450.00, 0);

INSERT INTO Veiculos (Placa, Marca, Modelo, Ano, Cor, KmAtual, Status, Ativo, CategoriaId)
SELECT v.Placa, v.Marca, v.Modelo, v.Ano, v.Cor, v.KmAtual, v.Status, v.Ativo, c.Id
FROM (VALUES
    (N'RIO2A18', N'Fiat',      N'Mobi',        2023, N'Branco',   18750, N'Disponivel', 1, N'Econômico'),
    (N'SBK3C47', N'Fiat',      N'Argo',        2024, N'Vermelho',  9400, N'Disponivel', 1, N'Econômico'),
    (N'QTP5F21', N'Renault',   N'Kwid',        2022, N'Prata',    41230, N'Locado',     1, N'Econômico'),
    (N'LMK3921', N'Chevrolet', N'Prisma',      2017, N'Preto',    98210, N'Disponivel', 0, N'Econômico'),
    (N'RTA4B92', N'Chevrolet', N'Onix Plus',   2024, N'Preto',    12480, N'Disponivel', 1, N'Intermediário'),
    (N'PXD7E35', N'Hyundai',   N'HB20S',       2023, N'Cinza',    27315, N'Locado',     1, N'Intermediário'),
    (N'SAF1G63', N'Toyota',    N'Yaris Sedan', 2023, N'Branco',   35060, N'Manutencao', 1, N'Intermediário'),
    (N'RNV8H14', N'Jeep',      N'Compass',     2024, N'Cinza',    30610, N'Disponivel', 1, N'SUV'),
    (N'SCJ2D58', N'Volkswagen',N'T-Cross',     2023, N'Azul',     22740, N'Locado',     1, N'SUV'),
    (N'QZR6J07', N'Fiat',      N'Strada',      2022, N'Branco',   52890, N'Disponivel', 1, N'Utilitário'),
    (N'FTN4K29', N'Fiat',      N'Fiorino',     2021, N'Branco',   67420, N'Disponivel', 1, N'Utilitário')
) AS v (Placa, Marca, Modelo, Ano, Cor, KmAtual, Status, Ativo, Categoria)
JOIN Categorias c ON c.Nome = v.Categoria;

INSERT INTO Clientes (Nome, Cpf, Cnh, Telefone, Email, Ativo) VALUES
    (N'Mariana Costa Ribeiro',    N'31846275009', N'04718263590', N'11987452163', N'mariana.ribeiro@example.com', 1),
    (N'Carlos Eduardo Lima',      N'47290183650', N'05827364918', N'11976325418', N'carlos.lima@example.com',     1),
    (N'Fernanda Alves Moreira',   N'26583910472', N'03619482755', N'11965184732', N'fernanda.moreira@example.com',1),
    (N'Rafael Souza Martins',     N'58017342961', N'06473918204', N'1132548719',  NULL,                           1),
    (N'Juliana Pereira Santos',   N'73950264118', N'02958173640', N'11991827364', N'juliana.santos@example.com',  1),
    (N'Bruno Henrique Oliveira',  N'19476038255', N'07384625193', N'11958473621', N'bruno.oliveira@example.com',  1),
    (N'Patrícia Gomes Ferreira',  N'84621759307', N'01847293658', N'11947362518', N'patricia.ferreira@example.com',1),
    (N'Thiago Nascimento Rocha',  N'62538147092', N'08592736415', N'11936251847', NULL,                           1),
    (N'Aline Barbosa Teixeira',   N'95184276033', N'09273846152', N'11925147386', N'aline.teixeira@example.com',  1),
    (N'Roberto Dias Carvalho',    N'38705926144', N'05163829470', N'11914736258', NULL,                           0);

-- Horário de agora sem os segundos, base de todas as datas abaixo.
DECLARE @agora datetime2(0) = DATEADD(MINUTE, DATEDIFF(MINUTE, '2000-01-01', SYSDATETIME()), CAST('2000-01-01' AS datetime2(0)));
DECLARE @hoje datetime2(0) = CAST(CAST(@agora AS date) AS datetime2(0));
DECLARE @locacao int, @retirada datetime2(0), @prevista datetime2(0), @devolucao datetime2(0);

-- 1. Finalizada no prazo: 3 diárias de R$ 120, devolvida 40 min depois (dentro da tolerância), sem multa.
SET @retirada  = DATEADD(HOUR, -288, @agora);
SET @prevista  = DATEADD(HOUR, 72, @retirada);
SET @devolucao = DATEADD(MINUTE, 40, @prevista);
INSERT INTO Locacoes (ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, KmRetirada, KmDevolucao,
                      ObservacaoRetirada, ObservacaoDevolucao, ValorDiaria, ValorPrevisto, ValorMulta, ValorTotal, ValorPago, Status)
SELECT c.Id, v.Id, @retirada, @prevista, @devolucao, 18200, 18750,
       NULL, N'Sem avarias.', 120.00, 360.00, 0.00, 360.00, 360.00, N'Finalizada'
FROM Clientes c, Veiculos v WHERE c.Cpf = N'47290183650' AND v.Placa = N'RIO2A18';
SET @locacao = SCOPE_IDENTITY();
INSERT INTO Pagamentos (LocacaoId, Valor, DataPagamento, Tipo, Forma) VALUES
    (@locacao, 100.00, @retirada,  N'Entrada', N'Pix'),
    (@locacao, 260.00, @devolucao, N'Saida',   N'CartaoCredito');

-- 2. Finalizada com atraso: 2 diárias de R$ 230, 26 h de atraso = 2 dias de multa (2 x 230 x 1,2 = R$ 552).
SET @retirada  = DATEADD(HOUR, -216, @agora);
SET @prevista  = DATEADD(HOUR, 48, @retirada);
SET @devolucao = DATEADD(HOUR, 26, @prevista);
INSERT INTO Locacoes (ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, KmRetirada, KmDevolucao,
                      ObservacaoRetirada, ObservacaoDevolucao, ValorDiaria, ValorPrevisto, ValorMulta, ValorTotal, ValorPago, Status)
SELECT c.Id, v.Id, @retirada, @prevista, @devolucao, 30120, 30610,
       N'Viagem para o litoral.', N'Devolvido com um dia de atraso.', 230.00, 460.00, 552.00, 1012.00, 1012.00, N'Finalizada'
FROM Clientes c, Veiculos v WHERE c.Cpf = N'26583910472' AND v.Placa = N'RNV8H14';
SET @locacao = SCOPE_IDENTITY();
INSERT INTO Pagamentos (LocacaoId, Valor, DataPagamento, Tipo, Forma) VALUES
    (@locacao, 200.00, @retirada,  N'Entrada', N'Dinheiro'),
    (@locacao, 812.00, @devolucao, N'Saida',   N'Pix');

-- 3. Cancelada: 2 diárias de R$ 120, sem pagamento.
SET @retirada = DATEADD(HOUR, -120, @agora);
SET @prevista = DATEADD(HOUR, 48, @retirada);
INSERT INTO Locacoes (ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, KmRetirada, KmDevolucao,
                      ObservacaoRetirada, ObservacaoDevolucao, ValorDiaria, ValorPrevisto, ValorMulta, ValorTotal, ValorPago, Status)
SELECT c.Id, v.Id, @retirada, @prevista, NULL, 9400, NULL,
       NULL, NULL, 120.00, 240.00, 0.00, 240.00, 0.00, N'Cancelada'
FROM Clientes c, Veiculos v WHERE c.Cpf = N'58017342961' AND v.Placa = N'SBK3C47';

-- 4. Aberta e atrasada: venceu há 28 h, aparece no painel com multa estimada de 2 x 120 x 1,2 = R$ 288.
SET @retirada = DATEADD(HOUR, -100, @agora);
SET @prevista = DATEADD(HOUR, 72, @retirada);
INSERT INTO Locacoes (ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, KmRetirada, KmDevolucao,
                      ObservacaoRetirada, ObservacaoDevolucao, ValorDiaria, ValorPrevisto, ValorMulta, ValorTotal, ValorPago, Status)
SELECT c.Id, v.Id, @retirada, @prevista, NULL, 41230, NULL,
       NULL, NULL, 120.00, 360.00, 0.00, 360.00, 150.00, N'Aberta'
FROM Clientes c, Veiculos v WHERE c.Cpf = N'73950264118' AND v.Placa = N'QTP5F21';
SET @locacao = SCOPE_IDENTITY();
INSERT INTO Pagamentos (LocacaoId, Valor, DataPagamento, Tipo, Forma) VALUES
    (@locacao, 150.00, @retirada, N'Entrada', N'CartaoDebito');

-- 5. Aberta e vencendo hoje (18:00, ou 23:30 se o script rodar depois das 18:00): 2 diárias de R$ 150, já quitada.
SET @prevista = CASE WHEN @agora < DATEADD(HOUR, 18, @hoje) THEN DATEADD(HOUR, 18, @hoje) ELSE DATEADD(MINUTE, 1410, @hoje) END;
SET @retirada = DATEADD(HOUR, -48, @prevista);
INSERT INTO Locacoes (ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, KmRetirada, KmDevolucao,
                      ObservacaoRetirada, ObservacaoDevolucao, ValorDiaria, ValorPrevisto, ValorMulta, ValorTotal, ValorPago, Status)
SELECT c.Id, v.Id, @retirada, @prevista, NULL, 27315, NULL,
       NULL, NULL, 150.00, 300.00, 0.00, 300.00, 300.00, N'Aberta'
FROM Clientes c, Veiculos v WHERE c.Cpf = N'19476038255' AND v.Placa = N'PXD7E35';
SET @locacao = SCOPE_IDENTITY();
INSERT INTO Pagamentos (LocacaoId, Valor, DataPagamento, Tipo, Forma) VALUES
    (@locacao, 300.00, @retirada, N'Entrada', N'Pix');

-- 6. Aberta no prazo: 4 diárias de R$ 230, vence daqui a 3 dias e 4 h.
SET @retirada = DATEADD(HOUR, -20, @agora);
SET @prevista = DATEADD(HOUR, 96, @retirada);
INSERT INTO Locacoes (ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, KmRetirada, KmDevolucao,
                      ObservacaoRetirada, ObservacaoDevolucao, ValorDiaria, ValorPrevisto, ValorMulta, ValorTotal, ValorPago, Status)
SELECT c.Id, v.Id, @retirada, @prevista, NULL, 22740, NULL,
       N'Cadeirinha infantil instalada.', NULL, 230.00, 920.00, 0.00, 920.00, 400.00, N'Aberta'
FROM Clientes c, Veiculos v WHERE c.Cpf = N'84621759307' AND v.Placa = N'SCJ2D58';
SET @locacao = SCOPE_IDENTITY();
INSERT INTO Pagamentos (LocacaoId, Valor, DataPagamento, Tipo, Forma) VALUES
    (@locacao, 400.00, @retirada, N'Entrada', N'CartaoCredito');

COMMIT TRANSACTION;

SELECT
    (SELECT COUNT(*) FROM Categorias) AS Categorias,
    (SELECT COUNT(*) FROM Veiculos)   AS Veiculos,
    (SELECT COUNT(*) FROM Clientes)   AS Clientes,
    (SELECT COUNT(*) FROM Locacoes)   AS Locacoes,
    (SELECT COUNT(*) FROM Pagamentos) AS Pagamentos;
