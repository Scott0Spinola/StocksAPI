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