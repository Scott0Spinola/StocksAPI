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
