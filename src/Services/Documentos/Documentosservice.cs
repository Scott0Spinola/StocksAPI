using src.Data;
using Microsoft.EntityFrameworkCore;
using src.Dtos.Documentos_Dtos;

namespace src.Services.Documentos;

public class DocumentosService
{
    private readonly StocksContext _context;
    private readonly ILogger<DocumentosService> _logger;

    

    public DocumentosService(StocksContext context, ILogger<DocumentosService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<DocumentoFaturaDTO>> PesquisaFaturasAsync(FiltroFatura filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (filtro.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (filtro.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        if (filtro.DataInicio.HasValue && filtro.DataFim.HasValue && filtro.DataInicio.Value > filtro.DataFim.Value)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        var hotel = filtro.Hotel.Trim();
        var numDocumento = filtro.NumDocumento?.Trim();

        DateTime? dataInicio = filtro.DataInicio;
        DateTime? dataFim = filtro.DataFim;

        // "Documentos mensais": if no dates are provided, default to the current month.
        if (!dataInicio.HasValue && !dataFim.HasValue)
        {
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local);
            dataInicio = start;
            dataFim = start.AddMonths(1).AddTicks(-1);
        }

        try
        {
            IQueryable<Models.Cliente_Faturas> query = _context.Cliente_Faturas
                .AsNoTracking()
                .Where(x => x.Cliente == hotel);

            if (!string.IsNullOrWhiteSpace(numDocumento))
            {
                query = query.Where(x => x.NumeroDoc.Contains(numDocumento));
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(x => x.Data >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(x => x.Data <= dataFim.Value);
            }

            var skip = (filtro.Pagina - 1) * filtro.NumRegistos;

            var result = await query
                .OrderByDescending(x => x.Data)
                .ThenByDescending(x => x.NumeroDoc)
                .Skip(skip)
                .Take(filtro.NumRegistos)
                .Select(x => new DocumentoFaturaDTO(
                    NumDocumento: x.NumeroDoc,
                    MesAno: x.Data.ToString("MM/yyyy"),
                    Valor: x.Valor,
                    Estado: x.Estado,
                    IdDoc: x.Id,
                    Pagina: filtro.Pagina,
                    NumRegistos: filtro.NumRegistos))
                .ToListAsync();

            _logger.LogInformation(
                "Documentos faturas pesquisa hotel={Hotel} numDocumento={NumDocumento} dataInicio={DataInicio} dataFim={DataFim} pagina={Pagina} numRegistos={NumRegistos} result={Count}",
                hotel,
                numDocumento,
                dataInicio,
                dataFim,
                filtro.Pagina,
                filtro.NumRegistos,
                result.Count);

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to execute faturas pesquisa for hotel='{hotel}' numDocumento='{numDocumento}' dataInicio='{dataInicio:O}' dataFim='{dataFim:O}'.",
                ex);
        }
    }

    public async Task<List<DocumentoGuiaDTO>> PesquisaGuiasAsync(FiltroGuia filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (filtro.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (filtro.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        if (filtro.DataInicio.HasValue && filtro.DataFim.HasValue && filtro.DataInicio.Value > filtro.DataFim.Value)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        var hotel = filtro.Hotel.Trim();
        var numDocumento = filtro.NumDocumento?.Trim();

        DateTime? dataInicio = filtro.DataInicio;
        DateTime? dataFim = filtro.DataFim;

        // "Guias diárias": if no dates are provided, default to today.
        if (!dataInicio.HasValue && !dataFim.HasValue)
        {
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
            dataInicio = start;
            dataFim = start.AddDays(1).AddTicks(-1);
        }

        try
        {
            IQueryable<Models.Cliente_Guias> query = _context.Cliente_Guias
                .AsNoTracking()
                .Where(x => x.Cliente == hotel);

            if (!string.IsNullOrWhiteSpace(numDocumento))
            {
                query = query.Where(x => x.NumeroGuia.Contains(numDocumento));
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(x => x.Data >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(x => x.Data <= dataFim.Value);
            }

            var skip = (filtro.Pagina - 1) * filtro.NumRegistos;

            var result = await query
                .OrderByDescending(x => x.Data)
                .ThenByDescending(x => x.NumeroGuia)
                .Skip(skip)
                .Take(filtro.NumRegistos)
                .Select(x => new DocumentoGuiaDTO(
                    NumDocumento: x.NumeroGuia,
                    Data: x.Data.ToString("dd/MM/yyyy"),
                    TotalPecas: x.TotalPecas,
                    IdDoc: x.Id,
                    Pagina: filtro.Pagina,
                    NumRegistos: filtro.NumRegistos))
                .ToListAsync();

            _logger.LogInformation(
                "Documentos guias pesquisa hotel={Hotel} numDocumento={NumDocumento} dataInicio={DataInicio} dataFim={DataFim} pagina={Pagina} numRegistos={NumRegistos} result={Count}",
                hotel,
                numDocumento,
                dataInicio,
                dataFim,
                filtro.Pagina,
                filtro.NumRegistos,
                result.Count);

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to execute guias pesquisa for hotel='{hotel}' numDocumento='{numDocumento}' dataInicio='{dataInicio:O}' dataFim='{dataFim:O}'.",
                ex);
        }
    }
}