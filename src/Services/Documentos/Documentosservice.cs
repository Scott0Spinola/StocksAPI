using src.Data;
using src.Dtos.Dashboard_Dtos;
using Microsoft.EntityFrameworkCore;
using src.Services.TimeSeries;

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
}