-- Seed de linhas (Cliente_DetalheFaturas) a partir dos cabeçalhos (Cliente_Faturas)
--
-- Objetivo:
--  - Para cada documento em dbo.Cliente_Faturas que ainda não tem linhas em dbo.Cliente_DetalheFaturas,
--    criar 1 linha de detalhe com DocId = Cliente_Faturas.Id e NumeroDoc igual.
--
-- Este script é idempotente: só insere quando não existe nenhuma linha para o DocId.
--
-- Nota sobre IVA:
--  - Assume que Cliente_Faturas.Valor representa o total (bruto) do documento.
--  - Divide o total em Valor (base) + Iva, garantindo que (Valor + Iva) = total.

SET NOCOUNT ON;

DECLARE @IvaRate decimal(9,6) = 0.23; -- 23%

;WITH MissingDetails AS
(
    SELECT
        f.Id AS DocId,
        f.Cliente,
        f.NumeroDoc,
        f.Valor AS Total
    FROM dbo.Cliente_Faturas f
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Cliente_DetalheFaturas d
        WHERE d.DocId = f.Id
    )
)
INSERT INTO dbo.Cliente_DetalheFaturas
(
    Id,
    DocId,
    Cliente,
    NumeroDoc,
    Servico,
    Produto,
    Codigo,
    Quantidade,
    Valor,
    Iva
)
SELECT
    NEWID() AS Id,
    m.DocId,
    m.Cliente,
    m.NumeroDoc,
    N'SEED_LINHA' AS Servico,
    N'SEED_PRODUTO' AS Produto,
    N'SEED' AS Codigo,
    1 AS Quantidade,
    ROUND(m.Total / (1 + @IvaRate), 2) AS Valor,
    m.Total - ROUND(m.Total / (1 + @IvaRate), 2) AS Iva
FROM MissingDetails m;

DECLARE @Inserted int = @@ROWCOUNT;
PRINT CONCAT(N'Seed Cliente_DetalheFaturas aplicado. Linhas inseridas=', @Inserted, N'.');

GO
