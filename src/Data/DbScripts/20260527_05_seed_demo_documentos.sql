-- Seed de dados de exemplo para testar os endpoints:
--  - POST /documentos/faturas/pesquisa
--  - POST /documentos/guias/pesquisa
--
-- Este script é idempotente: usa IF NOT EXISTS por NumeroDoc / NumeroGuia.
--
-- NOTA:
--  - Por omissão, o serviço devolve "faturas do mês atual" se não enviar datas.
--  - E devolve "guias de hoje" se não enviar datas.
--  - Este seed cria precisamente esses dados para o hotel @Hotel.

SET NOCOUNT ON;

DECLARE @Hotel nvarchar(80) = N'HOTEL_DEMO';
DECLARE @HotelSlug nvarchar(80) = UPPER(REPLACE(@Hotel, N' ', N''));

DECLARE @Now datetime2(0) = SYSDATETIME();
DECLARE @Today date = CAST(@Now AS date);
DECLARE @MonthStart date = DATEFROMPARTS(YEAR(@Now), MONTH(@Now), 1);
DECLARE @MonthKey nvarchar(6) = LEFT(CONVERT(nvarchar(8), @MonthStart, 112), 6); -- yyyyMM

-- ========== FATURAS (mês atual + 1 exemplo mês anterior) ==========

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Faturas WHERE NumeroDoc = CONCAT(N'FT-', @HotelSlug, N'-', @MonthKey, N'-0001'))
BEGIN
    INSERT INTO dbo.Cliente_Faturas
        (Id, Cliente, NumeroDoc, Data, Valor, Estado, UrlDocument)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'FT-', @HotelSlug, N'-', @MonthKey, N'-0001'),
         DATEADD(hour, 10, DATEADD(day, 1, CAST(@MonthStart AS datetime2(0)))),
         125.30, N'PAGO', N'https://example.local/docs/fatura/0001');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Faturas WHERE NumeroDoc = CONCAT(N'FT-', @HotelSlug, N'-', @MonthKey, N'-0002'))
BEGIN
    INSERT INTO dbo.Cliente_Faturas
        (Id, Cliente, NumeroDoc, Data, Valor, Estado, UrlDocument)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'FT-', @HotelSlug, N'-', @MonthKey, N'-0002'),
         DATEADD(hour, 15, DATEADD(day, 10, CAST(@MonthStart AS datetime2(0)))),
         89.99, N'PENDENTE', N'https://example.local/docs/fatura/0002');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Faturas WHERE NumeroDoc = CONCAT(N'FT-', @HotelSlug, N'-', @MonthKey, N'-0003'))
BEGIN
    INSERT INTO dbo.Cliente_Faturas
        (Id, Cliente, NumeroDoc, Data, Valor, Estado, UrlDocument)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'FT-', @HotelSlug, N'-', @MonthKey, N'-0003'),
         DATEADD(hour, 9, DATEADD(day, 20, CAST(@MonthStart AS datetime2(0)))),
         342.10, N'PAGO', N'https://example.local/docs/fatura/0003');
END;

DECLARE @PrevMonthStart date = DATEADD(month, -1, @MonthStart);
DECLARE @PrevMonthKey nvarchar(6) = LEFT(CONVERT(nvarchar(8), @PrevMonthStart, 112), 6); -- yyyyMM

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Faturas WHERE NumeroDoc = CONCAT(N'FT-', @HotelSlug, N'-', @PrevMonthKey, N'-0099'))
BEGIN
    INSERT INTO dbo.Cliente_Faturas
        (Id, Cliente, NumeroDoc, Data, Valor, Estado, UrlDocument)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'FT-', @HotelSlug, N'-', @PrevMonthKey, N'-0099'),
         DATEADD(hour, 12, DATEADD(day, 15, CAST(@PrevMonthStart AS datetime2(0)))),
         55.00, N'PAGO', N'https://example.local/docs/fatura/0099');
END;

-- 1 fatura para outro hotel (para testar o filtro por hotel)
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Faturas WHERE NumeroDoc = CONCAT(N'FT-HOTELOUTRO-', @MonthKey, N'-0001'))
BEGIN
    INSERT INTO dbo.Cliente_Faturas
        (Id, Cliente, NumeroDoc, Data, Valor, Estado, UrlDocument)
    VALUES
        (NEWID(), N'HOTEL_OUTRO', CONCAT(N'FT-HOTELOUTRO-', @MonthKey, N'-0001'),
         DATEADD(hour, 11, DATEADD(day, 5, CAST(@MonthStart AS datetime2(0)))),
         999.00, N'PAGO', N'https://example.local/docs/fatura/outro');
END;


-- ========== GUIAS (hoje + 1 exemplo ontem) ==========

DECLARE @TodayKey nvarchar(8) = CONVERT(nvarchar(8), @Today, 112); -- yyyyMMdd

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Guias WHERE NumeroGuia = CONCAT(N'G-', @HotelSlug, N'-', @TodayKey, N'-0001'))
BEGIN
    INSERT INTO dbo.Cliente_Guias
        (Id, Cliente, NumeroGuia, Data, TotalPecas, UrlDocumento)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'G-', @HotelSlug, N'-', @TodayKey, N'-0001'),
         DATEADD(hour, 8, CAST(@Today AS datetime2(0))),
         12, N'https://example.local/docs/guia/0001');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Guias WHERE NumeroGuia = CONCAT(N'G-', @HotelSlug, N'-', @TodayKey, N'-0002'))
BEGIN
    INSERT INTO dbo.Cliente_Guias
        (Id, Cliente, NumeroGuia, Data, TotalPecas, UrlDocumento)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'G-', @HotelSlug, N'-', @TodayKey, N'-0002'),
         DATEADD(hour, 14, CAST(@Today AS datetime2(0))),
         7, N'https://example.local/docs/guia/0002');
END;

DECLARE @Yesterday date = DATEADD(day, -1, @Today);
DECLARE @YesterdayKey nvarchar(8) = CONVERT(nvarchar(8), @Yesterday, 112);

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Guias WHERE NumeroGuia = CONCAT(N'G-', @HotelSlug, N'-', @YesterdayKey, N'-0009'))
BEGIN
    INSERT INTO dbo.Cliente_Guias
        (Id, Cliente, NumeroGuia, Data, TotalPecas, UrlDocumento)
    VALUES
        (NEWID(), @Hotel, CONCAT(N'G-', @HotelSlug, N'-', @YesterdayKey, N'-0009'),
         DATEADD(hour, 16, CAST(@Yesterday AS datetime2(0))),
         20, N'https://example.local/docs/guia/0009');
END;

-- 1 guia para outro hotel
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Guias WHERE NumeroGuia = CONCAT(N'G-HOTELOUTRO-', @TodayKey, N'-0001'))
BEGIN
    INSERT INTO dbo.Cliente_Guias
        (Id, Cliente, NumeroGuia, Data, TotalPecas, UrlDocumento)
    VALUES
        (NEWID(), N'HOTEL_OUTRO', CONCAT(N'G-HOTELOUTRO-', @TodayKey, N'-0001'),
         DATEADD(hour, 10, CAST(@Today AS datetime2(0))),
         1, N'https://example.local/docs/guia/outro');
END;

PRINT CONCAT(N'Seed documentos aplicado (ou já existente) para hotel=', @Hotel, N' (', @HotelSlug, N').');

GO
