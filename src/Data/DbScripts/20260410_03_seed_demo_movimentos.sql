-- Seed de dados de exemplo para testar o endpoint /api/movimentos/pesquisa
-- Cria movimentos que geram resultados para:
--  - tipo=0 (entradas): Para = 'HotelA'
--  - tipo=1 (saidas):   Para = 'Lavandaria'
--  - tipo=2 (ambos):    entradas + saidas

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = 'DEMO-ENT-001')
BEGIN
    INSERT INTO dbo.Cliente_Movimentos
        (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
    VALUES
        ('DEMO-ENT-001', 'Rececao', 'HotelA', 'HotelA', 'LENÇOL',  '2026-04-09T10:15:00', '09/04/2026', 12);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = 'DEMO-ENT-002')
BEGIN
    INSERT INTO dbo.Cliente_Movimentos
        (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
    VALUES
        ('DEMO-ENT-002', 'Armazem', 'HotelA', 'HotelA', 'TOALHA',  '2026-04-09T12:30:00', '09/04/2026', 8);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = 'DEMO-SAI-001')
BEGIN
    INSERT INTO dbo.Cliente_Movimentos
        (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
    VALUES
        ('DEMO-SAI-001', 'HotelA', 'Lavandaria', 'HotelA', 'LENÇOL', '2026-04-10T08:05:00', '10/04/2026', 10);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente_Movimentos WHERE MovementRID = 'DEMO-SAI-002')
BEGIN
    INSERT INTO dbo.Cliente_Movimentos
        (MovementRID, De, Para, Cliente, Descricao, Datetime, DataFormatada, Quantidade)
    VALUES
        ('DEMO-SAI-002', 'HotelA', 'Lavandaria', 'HotelA', 'TOALHA', '2026-04-10T09:45:00', '10/04/2026', 6);
END;

GO
