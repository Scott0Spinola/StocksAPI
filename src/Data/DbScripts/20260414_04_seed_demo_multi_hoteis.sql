-- Seed de dados de exemplo (multi-hotéis) para testar os endpoints:
--  - /api/movimentos/pesquisa (tipo=0 entradas, tipo=1 saidas, tipo=2 ambos)
--  - /api/movimentos/evolucao
--  - /api/movimentos/proximasentregas (usa apenas movimentos com Datetime >= agora)
--
-- Convenções usadas no código:
--  Entradas: Para = <Hotel>
--  Saídas:   Para = 'Lavandaria' e De = <Hotel>
--
-- Este script é idempotente: usa IF NOT EXISTS por MovementRID.

SET NOCOUNT ON;

DECLARE @Now datetime2(0) = SYSDATETIME();
DECLARE @Yesterday datetime2(0) = DATEADD(day, -1, @Now);
DECLARE @TwoDaysAgo datetime2(0) = DATEADD(day, -2, @Now);
DECLARE @Tomorrow datetime2(0) = DATEADD(day, 1, @Now);
DECLARE @InTwoDays datetime2(0) = DATEADD(day, 2, @Now);
DECLARE @InThreeDays datetime2(0) = DATEADD(day, 3, @Now);

DECLARE @Hotels TABLE (Hotel nvarchar(80) NOT NULL);
INSERT INTO @Hotels (Hotel) VALUES
(N'HotelA'),
(N'HotelB'),
(N'HotelC'),
(N'HotelD');

DECLARE @Hotel nvarchar(80);

DECLARE hotel_cursor CURSOR FAST_FORWARD FOR
SELECT Hotel FROM @Hotels;

OPEN hotel_cursor;
FETCH NEXT FROM hotel_cursor INTO @Hotel;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Entradas históricas (para testar pesquisa/evolução)
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-ENT-001'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-ENT-001'), N'Rececao', @Hotel, @Hotel, N'LENÇOL',  DATEADD(hour, 10, CAST(@TwoDaysAgo AS datetime2(0))), CONVERT(nvarchar(10), CAST(@TwoDaysAgo AS date), 103), 12);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-ENT-002'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-ENT-002'), N'Armazem', @Hotel, @Hotel, N'TOALHA',  DATEADD(hour, 12, CAST(@Yesterday AS datetime2(0))), CONVERT(nvarchar(10), CAST(@Yesterday AS date), 103), 8);
    END;

    -- Saídas históricas (para testar pesquisa/evolução)
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-SAI-001'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-SAI-001'), @Hotel, N'Lavandaria', @Hotel, N'LENÇOL', DATEADD(hour, 8, CAST(@Yesterday AS datetime2(0))), CONVERT(nvarchar(10), CAST(@Yesterday AS date), 103), 10);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-SAI-002'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-SAI-002'), @Hotel, N'Lavandaria', @Hotel, N'TOALHA', DATEADD(hour, 9, CAST(@Yesterday AS datetime2(0))), CONVERT(nvarchar(10), CAST(@Yesterday AS date), 103), 6);
    END;

    -- Movimentos futuros (para testar /proximasentregas)
    -- Entrada futura: Para = <Hotel>
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-FUT-ENT-001'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-FUT-ENT-001'), N'Central', @Hotel, @Hotel, N'FRONHA', DATEADD(hour, 11, CAST(@Tomorrow AS datetime2(0))), CONVERT(nvarchar(10), CAST(@Tomorrow AS date), 103), 20);
    END;

    -- Saída futura: Para = Lavandaria e De = <Hotel>
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-FUT-SAI-001'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-FUT-SAI-001'), @Hotel, N'Lavandaria', @Hotel, N'COLCHA', DATEADD(hour, 7, CAST(@InTwoDays AS datetime2(0))), CONVERT(nvarchar(10), CAST(@InTwoDays AS date), 103), 4);
    END;

    -- Um movimento adicional futuro, para variar o "próxima entrega" por unidade
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = CONCAT('DEMO-', @Hotel, '-FUT-ENT-002'))
    BEGIN
        INSERT INTO dbo.Cliente_Movimentos
            (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
        VALUES
            (CONCAT('DEMO-', @Hotel, '-FUT-ENT-002'), N'Central', @Hotel, @Hotel, N'TAPETE', DATEADD(hour, 15, CAST(@InThreeDays AS datetime2(0))), CONVERT(nvarchar(10), CAST(@InThreeDays AS date), 103), 2);
    END;

    FETCH NEXT FROM hotel_cursor INTO @Hotel;
END

CLOSE hotel_cursor;
DEALLOCATE hotel_cursor;

PRINT 'Seed multi-hotéis aplicado (ou já existente).';
