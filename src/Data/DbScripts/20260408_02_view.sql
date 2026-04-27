CREATE OR ALTER VIEW dbo.vw_ClienteMovimentos
AS
SELECT
	[Id],
	[MovementRID],
	[De],
	[Para],
	[Cliente],
	[Descricao],
	[Datetime],
	[DataFormatada],
	[Quantidade]
FROM dbo.Cliente_Movimentos;
GO

-- View dedicada ao feed/seleção de movimentos (IDSP).
-- Mantem a mesma estrutura base de dbo.Cliente_Movimentos.
CREATE OR ALTER VIEW dbo.vw_Movimentos_IDSP
AS
SELECT
	[Id],
	[MovementRID],
	[De],
	[Para],
	[Cliente],
	[Descricao],
	[Datetime],
	[DataFormatada],
	[Quantidade]
FROM dbo.Cliente_Movimentos;
GO

CREATE OR ALTER VIEW dbo.vw_ProximasEntregas
AS
SELECT
	[Para] AS [UnidadeHotel],
	MIN([Datetime]) AS [Datetime]
FROM dbo.Cliente_Movimentos
WHERE [Para] IS NOT NULL
	AND [Para] <> N'Lavandaria'
	AND [Datetime] >= SYSDATETIME()
GROUP BY [Para];
GO

-- Count interventions with alerta per hotel (Hotel == Cliente.CliCampo3).
-- Requires the login used by this DB to have SELECT permission on Intervencoe.*.
CREATE OR ALTER VIEW dbo.vw_IntervencoesAlertasPorHotel
AS
SELECT
	UPPER(LTRIM(RTRIM(c.CliCampo3))) AS Hotel,
	MAX(i.Alerta) AS Alerta
FROM Intervencoe.dbo.Clientes AS c
JOIN Intervencoe.dbo.ProcessoProjectos AS p
	ON p.ClienteId = c.Id
JOIN Intervencoe.dbo.Intervencaos AS i
	ON i.ProcessoId = p.Id
WHERE
	c.CliCampo3 IS NOT NULL
	AND LTRIM(RTRIM(c.CliCampo3)) <> ''
	AND i.Alerta <> 0
GROUP BY
	UPPER(LTRIM(RTRIM(c.CliCampo3)));
GO
