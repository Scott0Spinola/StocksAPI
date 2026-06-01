using src.Data;
using Microsoft.EntityFrameworkCore;
using src.Dtos.Documentos_Dtos;
using System.Globalization;
using System.Net.Mime;

namespace src.Services.Documentos;

public class DocumentosService
{
    private readonly StocksContext _context;
    private readonly ILogger<DocumentosService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;


    public record DocumentDownloadResult(Stream Stream, string ContentType, string FileName, IDisposable? Cleanup);

    public DocumentosService(StocksContext context, ILogger<DocumentosService> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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
                    MesAno: x.Data.ToString("MM/yyyy", CultureInfo.InvariantCulture),
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
                    Data: x.Data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
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

    public async Task<DocumentDownloadResult?> DownloadAsync(string tipo, Guid docId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ArgumentException("'tipo' is required.");
        }

        if (docId == Guid.Empty)
        {
            throw new ArgumentException("'docid' is required.");
        }

        tipo = tipo.Trim();

        string? url = null;
        string? numeroDocumento = null;

        // TODO: substituir por pesquisa na VIEW de integração do cliente (por docId) quando estiver disponível.
        if (tipo.Equals("fatura", StringComparison.OrdinalIgnoreCase))
        {
            var doc = await _context.Cliente_Faturas
                .AsNoTracking()
                .Where(x => x.Id == docId)
                .Select(x => new { x.NumeroDoc, x.UrlDocument })
                .SingleOrDefaultAsync(cancellationToken);

            if (doc == null)
            {
                return null;
            }

            numeroDocumento = doc.NumeroDoc;
            url = doc.UrlDocument;
        }
        else if (tipo.Equals("guia", StringComparison.OrdinalIgnoreCase))
        {
            var doc = await _context.Cliente_Guias
                .AsNoTracking()
                .Where(x => x.Id == docId)
                .Select(x => new { x.NumeroGuia, x.UrlDocumento })
                .SingleOrDefaultAsync(cancellationToken);

            if (doc == null)
            {
                return null;
            }

            numeroDocumento = doc.NumeroGuia;
            url = doc.UrlDocumento;
        }
        else
        {
            throw new ArgumentException("'tipo' must be 'fatura' or 'guia'.");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            // TODO: trocar pelo URL real quando a integração (VIEW) estiver pronta.
            // Placeholder content so the endpoint remains testable without external dependencies.
            var placeholderText = $"Documento '{tipo}' (docid={docId}) ainda sem URL associado. TODO: ligar à VIEW de integração.";
            var placeholderBytes = System.Text.Encoding.UTF8.GetBytes(placeholderText);
            var placeholderStream = new MemoryStream(placeholderBytes);

            var fileName = $"{tipo}_{numeroDocumento ?? docId.ToString()}_placeholder.txt";
            return new DocumentDownloadResult(placeholderStream, MediaTypeNames.Text.Plain, fileName, Cleanup: null);
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || !(uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            // TODO: quando a VIEW de integração estiver pronta, remover este fallback e garantir URL válido.
            var placeholderText = $"Documento '{tipo}' (docid={docId}) com URL inválido: '{url}'. TODO: ligar à VIEW de integração.";
            var placeholderBytes = System.Text.Encoding.UTF8.GetBytes(placeholderText);
            var placeholderStream = new MemoryStream(placeholderBytes);

            var fileName = $"{tipo}_{numeroDocumento ?? docId.ToString()}_placeholder.txt";
            return new DocumentDownloadResult(placeholderStream, MediaTypeNames.Text.Plain, fileName, Cleanup: null);
        }

        // If the DB currently contains a placeholder URL, keep returning placeholder content.
        // TODO: remover quando estiveres a gravar o URL real.
        if (uri.Host.Equals("example.local", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("example.com", StringComparison.OrdinalIgnoreCase))
        {
            var placeholderText = $"Documento '{tipo}' (docid={docId}) ainda com URL placeholder: '{url}'. TODO: substituir pelo URL real (VIEW de integração).";
            var placeholderBytes = System.Text.Encoding.UTF8.GetBytes(placeholderText);
            var placeholderStream = new MemoryStream(placeholderBytes);

            var fileName = $"{tipo}_{numeroDocumento ?? docId.ToString()}_placeholder.txt";
            return new DocumentDownloadResult(placeholderStream, MediaTypeNames.Text.Plain, fileName, Cleanup: null);
        }

        var http = _httpClientFactory.CreateClient(nameof(DocumentosService));

        // Stream the remote content without buffering the entire response in memory.
        var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? MediaTypeNames.Application.Octet;
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var fileName2 = BuildFileName(tipo, numeroDocumento, docId, contentType, uri);

        _logger.LogInformation(
            "Documentos download ok tipo={Tipo} docid={DocId} url={Url} contentType={ContentType} fileName={FileName}",
            tipo,
            docId,
            url,
            contentType,
            fileName2);

        return new DocumentDownloadResult(stream, contentType, fileName2, Cleanup: response);
    }

    private static string BuildFileName(string tipo, string? numeroDocumento, Guid docId, string contentType, Uri uri)
    {
        var safeNumero = string.IsNullOrWhiteSpace(numeroDocumento) ? docId.ToString() : numeroDocumento.Trim();

        // Try to keep a reasonable file extension.
        var extension = Path.GetExtension(uri.AbsolutePath) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ? ".pdf" : ".bin";
        }

        return $"{tipo}_{safeNumero}{extension}";
    }
    
}