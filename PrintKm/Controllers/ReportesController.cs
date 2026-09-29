using PrintKm.Services;
using Microsoft.AspNetCore.Mvc;

namespace PrintKm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public ReportesController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<object>> GetOptions(CancellationToken cancellationToken)
    {
        var usuarios = await _backendApiClient.GetAsync<IEnumerable<UsuarioDto>>("api/Usuarios", cancellationToken);
        var formasPago = await _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/FormasPago", cancellationToken)
            ?? Enumerable.Empty<CatalogStringDto>();
        var perfiles = await _backendApiClient.GetAsync<IEnumerable<PerfilDto>>("api/Perfiles", cancellationToken)
            ?? Enumerable.Empty<PerfilDto>();
        var productoComisiones = await _backendApiClient.GetAsync<IEnumerable<ProductoComisionDto>>("api/ProductoComisiones", cancellationToken)
            ?? Enumerable.Empty<ProductoComisionDto>();
        if (usuarios is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudieron obtener opciones de reportes." });
        }

        var gruposVenta = await _backendApiClient.GetAsync<IEnumerable<GrupoVentaDto>>("api/GruposVenta", cancellationToken)
            ?? Enumerable.Empty<GrupoVentaDto>();
        var vendedoresExternosIds = gruposVenta
            .Where(grupo => grupo.Estado)
            .SelectMany(grupo => grupo.Vendedores.Where(vendedor => vendedor.Estado))
            .Select(vendedor => vendedor.VendedorUsuarioId)
            .ToHashSet();

        var vendedoresTodos = usuarios
            .Where(usuario => usuario.Estado != false)
            .OrderBy(usuario => usuario.Persona ?? usuario.NombreUsuario)
            .ToList();
        var vendedores = vendedoresTodos
            .Where(vendedor => !vendedoresExternosIds.Contains(vendedor.Id))
            .ToList();
        var vendedoresExternos = usuarios
            .Where(vendedor => vendedoresExternosIds.Contains(vendedor.Id))
            .ToList();
        var teamLeaders = usuarios
            .Where(IsPerfilTeamLeader)
            .ToList();

        var perfilesVentaIds = productoComisiones
            .Where(comision => comision.Estado)
            .Select(comision => comision.PerfilId)
            .ToHashSet();
        var perfilesVenta = perfiles
            .Where(perfil => perfil.Estado && perfilesVentaIds.Contains(perfil.Id))
            .OrderBy(perfil => perfil.Nombre)
            .ToList();
        var usuariosVenta = usuarios
            .Where(usuario => usuario.Estado != false && usuario.PerfilIds.Any(perfilesVentaIds.Contains))
            .OrderBy(usuario => usuario.Persona ?? usuario.NombreUsuario)
            .ToList();

        return Ok(new { perfilesVenta, usuariosVenta, vendedores, vendedoresTodos, vendedoresExternos, teamLeaders, formasPago = formasPago.Where(x => x.Estado != false).OrderBy(x => x.Nombre) });
    }

    [HttpGet("comisiones-vendedores")]
    public async Task<ActionResult<ReporteComisionesDto>> GetComisionesVendedores(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        [FromQuery] string? scope = null,
        [FromQuery] int? perfilId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildFilterQuery(dateFrom, dateTo, vendedorId, NormalizeCommissionsScope(scope), perfilId);
        var result = await _backendApiClient.GetResultAsync<ReporteComisionesDto>(
            $"api/Reportes/comisiones-vendedores?{filters}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo obtener el reporte." });
    }

    [HttpGet("comisiones-vendedores/{format}")]
    public async Task<IActionResult> DownloadComisionesVendedores(
        string format,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        [FromQuery] string? scope = null,
        [FromQuery] int? perfilId = null,
        [FromQuery] int? vendedorExternoId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedFormat = format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? "pdf"
            : format.Equals("txt", StringComparison.OrdinalIgnoreCase)
                ? "txt"
                : "excel";
        var filters = BuildFilterQuery(dateFrom, dateTo, vendedorId, NormalizeCommissionsScope(scope), perfilId, vendedorExternoId);
        var result = await _backendApiClient.GetResultAsync<ExcelFileDto>(
            $"api/Reportes/comisiones-vendedores/{normalizedFormat}?{filters}",
            cancellationToken);

        if (!result.IsSuccess || result.Value is null || string.IsNullOrWhiteSpace(result.Value.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo preparar el archivo." });
        }

        var file = result.Value;
        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    [HttpGet("lotes-pago")]
    public async Task<ActionResult<PagedResponse<LotePagoDto>>> GetLotesPago(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? estado = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildPaymentLotsFilterQuery(dateFrom, dateTo, estado, page, pageSize);
        var result = await _backendApiClient.GetResultAsync<PagedResponse<LotePagoDto>>(
            $"api/Reportes/lotes-pago?{filters}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudieron cargar los lotes de pago." });
    }

    [HttpGet("clientes-deuda")]
    public async Task<ActionResult<ReporteClientesDeudaDto>> GetClientesDeuda(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? cliente = null,
        [FromQuery] string? estadoPago = null,
        [FromQuery] string? formaPagoId = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>();
        if (dateFrom.HasValue) filters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        if (dateTo.HasValue) filters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(cliente)) filters.Add($"cliente={Uri.EscapeDataString(cliente)}");
        if (!string.IsNullOrWhiteSpace(estadoPago)) filters.Add($"estadoPago={Uri.EscapeDataString(estadoPago)}");
        if (!string.IsNullOrWhiteSpace(formaPagoId)) filters.Add($"formaPagoId={Uri.EscapeDataString(formaPagoId)}");
        if (vendedorId.HasValue) filters.Add($"vendedorId={vendedorId.Value}");

        var result = await _backendApiClient.GetResultAsync<ReporteClientesDeudaDto>(
            $"api/Reportes/clientes-deuda?{string.Join("&", filters)}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = result.ErrorMessage ?? "No se pudo cargar el reporte de ventas de clientes."
            });
    }

    [HttpGet("envios")]
    public async Task<ActionResult<ReporteEnviosDto>> GetReporteEnvios(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? cliente = null,
        [FromQuery] string? metodoEntrega = null,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildEnviosFilterQuery(dateFrom, dateTo, cliente, metodoEntrega);
        var result = await _backendApiClient.GetResultAsync<ReporteEnviosDto>(
            $"api/Reportes/envios?{filters}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo cargar el reporte de envios." });
    }

    [HttpGet("resumen-gerencial")]
    public async Task<ActionResult<ReporteResumenGerencialDto>> GetResumenGerencial(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildFilterQuery(dateFrom, dateTo, vendedorId);
        var result = await _backendApiClient.GetResultAsync<ReporteResumenGerencialDto>(
            $"api/Reportes/resumen-gerencial?{filters}",
            cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo cargar el resumen gerencial." });
    }

    [HttpGet("lotes-pago/{id:int}/txt")]
    public async Task<IActionResult> DownloadLotePagoTxt(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<ExcelFileDto>(
            $"api/Reportes/lotes-pago/{id}/txt",
            cancellationToken);

        if (!result.IsSuccess || result.Value is null || string.IsNullOrWhiteSpace(result.Value.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo descargar el lote." });
        }

        var file = result.Value;
        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }

    [HttpPut("lotes-pago/{id:int}/estado")]
    public async Task<IActionResult> UpdateLotePagoEstado(int id, [FromBody] object request, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PutResultAsync<object>(
            $"api/Reportes/lotes-pago/{id}/estado",
            request,
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage ?? "No se pudo actualizar el lote." });
    }

    private static string BuildFilterQuery(DateTime? dateFrom, DateTime? dateTo, int? vendedorId, string? scope = null, int? perfilId = null, int? vendedorExternoId = null)
    {
        var filters = new List<string>();

        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            filters.Add($"scope={Uri.EscapeDataString(scope)}");
        }

        if (perfilId.HasValue)
        {
            filters.Add($"perfilId={perfilId.Value}");
        }

        if (vendedorExternoId.HasValue)
        {
            filters.Add($"vendedorExternoId={vendedorExternoId.Value}");
        }

        return string.Join("&", filters);
    }

    private static string BuildPaymentLotsFilterQuery(DateTime? dateFrom, DateTime? dateTo, string? estado, int page, int pageSize)
    {
        var filters = new List<string>
        {
            "tipoPago=COMISIONES",
            $"page={Math.Max(page, 1)}",
            $"pageSize={Math.Clamp(pageSize, 1, 100)}"
        };

        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            filters.Add($"estado={Uri.EscapeDataString(estado)}");
        }

        return string.Join("&", filters);
    }

    private static bool IsPerfilVendedor(UsuarioDto usuario)
    {
        var perfil = $"{usuario.Perfil} {usuario.Perfiles}";
        return perfil.Contains("vendedor", StringComparison.OrdinalIgnoreCase) ||
            perfil.Contains("ventas", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPerfilTeamLeader(UsuarioDto usuario)
    {
        var perfil = $"{usuario.Perfil} {usuario.Perfiles}";
        return perfil.Contains("team leader", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExternalScope(string? scope)
    {
        return scope is not null &&
            (scope.Equals("externos", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("external", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("vendedores-externos", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("team-leaders", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("teamleader", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("team-leader", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeCommissionsScope(string? scope)
    {
        if (scope is not null &&
            (scope.Equals("team-leaders", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("teamleader", StringComparison.OrdinalIgnoreCase) ||
             scope.Equals("team-leader", StringComparison.OrdinalIgnoreCase)))
        {
            return "team-leaders";
        }

        return IsExternalScope(scope) ? "externos" : "vendedores";
    }

    private static string BuildEnviosFilterQuery(DateTime? dateFrom, DateTime? dateTo, string? cliente, string? metodoEntrega)
    {
        var filters = new List<string>();

        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            filters.Add($"cliente={Uri.EscapeDataString(cliente)}");
        }

        if (!string.IsNullOrWhiteSpace(metodoEntrega))
        {
            filters.Add($"metodoEntrega={Uri.EscapeDataString(metodoEntrega)}");
        }

        return string.Join("&", filters);
    }
}
