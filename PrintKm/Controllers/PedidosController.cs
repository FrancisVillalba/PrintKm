using PrintKm.Services;
using Microsoft.AspNetCore.Mvc;

namespace PrintKm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IBackendApiClient _backendApiClient;

    public PedidosController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet]
    public async Task<ActionResult<PagedView<PedidoView>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? estadoVentaId = null,
        [FromQuery] string? estadoPagoId = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("yyyy-MM-dd"))}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={Uri.EscapeDataString(dateTo.Value.ToString("yyyy-MM-dd"))}");
        }

        if (clienteId.HasValue)
        {
            filters.Add($"clienteId={clienteId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(estadoVentaId))
        {
            filters.Add($"estadoVentaId={Uri.EscapeDataString(estadoVentaId)}");
        }

        if (!string.IsNullOrWhiteSpace(estadoPagoId))
        {
            filters.Add($"estadoPagoId={Uri.EscapeDataString(estadoPagoId)}");
        }

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        var pedidosTask = _backendApiClient.GetAsync<PagedResponse<VentaImpresionCabDto>>(
            $"api/VentasImpresion?{string.Join("&", filters)}",
            cancellationToken);
        var opcionesTask = _backendApiClient.GetAsync<PedidoFormOptionsDto>("api/VentasImpresion/opciones", cancellationToken);

        await Task.WhenAll(pedidosTask, opcionesTask);

        var pedidos = await pedidosTask;

        if (pedidos is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo obtener pedidos desde EvaluSystemBack." });
        }

        var vendedores = (await opcionesTask)?.Vendedores
            .ToDictionary(x => x.Id, x => x.Persona ?? x.NombreUsuario ?? $"Usuario {x.Id}")
            ?? new Dictionary<int, string>();

        return Ok(new PagedView<PedidoView>(
            pedidos.Items.Select(pedido => ToView(pedido, vendedores)),
            pedidos.Page,
            pedidos.PageSize,
            pedidos.TotalItems,
            pedidos.TotalPages));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoView>> GetById(int id, CancellationToken cancellationToken)
    {
        var pedido = await _backendApiClient.GetAsync<VentaImpresionCabDto>($"api/VentasImpresion/{id}", cancellationToken);
        return pedido is null ? NotFound() : Ok(ToView(pedido));
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<PedidoFormOptionsDto>> GetOptions(CancellationToken cancellationToken)
    {
        var options = await _backendApiClient.GetAsync<PedidoFormOptionsDto>("api/VentasImpresion/opciones", cancellationToken);
        return options is null
            ? StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudieron obtener opciones de pedidos desde EvaluSystemBack." })
            : Ok(options);
    }

    [HttpGet("mis-ventas")]
    public async Task<ActionResult<VentaUsuarioResumenDto>> GetMySales(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? estadoVentaId = null,
        [FromQuery] string? estadoPagoId = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>();
        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("yyyy-MM-dd"))}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={Uri.EscapeDataString(dateTo.Value.ToString("yyyy-MM-dd"))}");
        }

        if (clienteId.HasValue)
        {
            filters.Add($"clienteId={clienteId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(estadoVentaId))
        {
            filters.Add($"estadoVentaId={Uri.EscapeDataString(estadoVentaId)}");
        }

        if (!string.IsNullOrWhiteSpace(estadoPagoId))
        {
            filters.Add($"estadoPagoId={Uri.EscapeDataString(estadoPagoId)}");
        }

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        var result = await _backendApiClient.GetResultAsync<VentaUsuarioResumenDto>(
            $"api/VentasImpresion/mis-ventas?{string.Join("&", filters)}",
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    [HttpGet("exportar-excel")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? estadoVentaId = null,
        [FromQuery] string? estadoPagoId = null,
        [FromQuery] int? vendedorId = null,
        CancellationToken cancellationToken = default)
    {
        var filters = BuildFilterQuery(dateFrom, dateTo, clienteId, estadoVentaId, estadoPagoId, vendedorId);
        var excel = await _backendApiClient.GetAsync<ExcelFileDto>(
            $"api/VentasImpresion/exportar-excel?{filters}",
            cancellationToken);

        if (excel is null || string.IsNullOrWhiteSpace(excel.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo exportar pedidos desde EvaluSystemBack." });
        }

        var bytes = Convert.FromBase64String(excel.Bytes);
        return File(bytes, excel.ContentType, excel.FileName);
    }

    [HttpGet("{id:int}/presupuesto-pdf")]
    public async Task<IActionResult> DownloadBudgetPdf(int id, CancellationToken cancellationToken)
    {
        var file = await _backendApiClient.GetAsync<ExcelFileDto>(
            $"api/Reportes/presupuesto-pedido/{id}",
            cancellationToken);

        if (file is null || string.IsNullOrWhiteSpace(file.Bytes))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo generar el presupuesto." });
        }

        return File(Convert.FromBase64String(file.Bytes), file.ContentType, file.FileName);
    }
    [HttpPost]
    public async Task<ActionResult<VentaImpresionCabDto>> Create(
        [FromBody] VentaImpresionCompletaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PostResultAsync<VentaImpresionCabDto>(
            "api/VentasImpresion/completa",
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }
    [HttpPut("{id:int}/pago")]
    public async Task<ActionResult<VentaImpresionCabDto>> UpdatePayment(
        int id,
        [FromBody] ActualizarPagoVentaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PutResultAsync<VentaImpresionCabDto>(
            $"api/VentasImpresion/{id}/pago",
            request,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }


    [HttpPost("{id:int}/pagos")]
    public async Task<ActionResult<PagoVentaImpresionDto>> CreatePayment(
        int id,
        [FromBody] PagoVentaImpresionRequest request,
        CancellationToken cancellationToken)
    {
        var backendRequest = request with { VentaImpresionId = id };
        var result = await _backendApiClient.PostResultAsync<PagoVentaImpresionDto>(
            "api/PagosVentasImpresion",
            backendRequest,
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    [HttpGet("{id:int}/pagos")]
    public async Task<ActionResult<IEnumerable<PagoVentaImpresionDto>>> GetPayments(
        int id,
        CancellationToken cancellationToken)
    {
        var pagos = await _backendApiClient.GetAsync<IEnumerable<PagoVentaImpresionDto>>(
            $"api/PagosVentasImpresion/venta/{id}",
            cancellationToken);
        return Ok(pagos ?? []);
    }

    [HttpDelete("{id:int}/pagos/{pagoId:int}")]
    public async Task<IActionResult> DeletePayment(
        int id,
        int pagoId,
        CancellationToken cancellationToken)
    {
        var deleted = await _backendApiClient.DeleteAsync(
            $"api/PagosVentasImpresion/{pagoId}",
            cancellationToken);
        return deleted ? NoContent() : BadRequest(new { message = "No se pudo eliminar el pago." });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VentaImpresionCabDto>> Update(
        int id,
        [FromBody] VentaImpresionCompletaUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var currentOrder = await _backendApiClient.GetAsync<VentaImpresionCabDto>(
            $"api/VentasImpresion/{id}",
            cancellationToken);
        if (currentOrder is null)
        {
            return NotFound();
        }

        if (!IsCargaState(currentOrder))
        {
            request = request with
            {
                Detalles = currentOrder.Detalles.Select(detail => new VentaImpresionDetalleUpdateRequest(
                    detail.Id, detail.ProductoId, detail.TipoMaquinaId, detail.Cantidad,
                    detail.PrecioUnitario, detail.PrecioExtra, detail.ArchivoDisenio,
                    detail.ArchivoDisenioNombre, detail.Observacion, detail.EstadoItem,
                    detail.CheckImpresion)).ToList()
            };
        }

        var result = await _backendApiClient.PutResultAsync<VentaImpresionCabDto>(
            $"api/VentasImpresion/completa/{id}",
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}/detalles/{detalleId:int}/enviar-impresion")]
    public async Task<ActionResult<VentaImpresionDetDto>> SendDetailToPrint(
        int id,
        int detalleId,
        CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.PutResultAsync<VentaImpresionDetDto>(
            $"api/VentasImpresion/{id}/detalles/{detalleId}/enviar-impresion",
            new { },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage });
    }

    [HttpDelete("{id:int}/detalles/{detalleId:int}")]
    public async Task<IActionResult> DeleteDetail(
        int id,
        int detalleId,
        CancellationToken cancellationToken)
    {
        var deleted = await _backendApiClient.DeleteAsync(
            $"api/VentasImpresion/{id}/detalles/{detalleId}",
            cancellationToken);

        return deleted
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo eliminar el item." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromBody] EliminarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Observacion))
        {
            return BadRequest(new { message = "Debe agregar un comentario para eliminar el pedido." });
        }

        var result = await _backendApiClient.PutResultAsync<VentaImpresionCabDto>(
            $"api/VentasImpresion/{id}/marcar-eliminado",
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    private static PedidoView ToView(VentaImpresionCabDto pedido, IReadOnlyDictionary<int, string>? vendedores = null)
    {
        var date = pedido.FechaCreacion?.ToString("yyyy-MM-dd") ?? string.Empty;
        var delivery = pedido.FechaEntrega?.ToString("yyyy-MM-dd") ?? string.Empty;
        var vendedor = !string.IsNullOrWhiteSpace(pedido.Vendedor)
            ? pedido.Vendedor
            : vendedores?.GetValueOrDefault(pedido.VendedorId) ?? $"Usuario {pedido.VendedorId}";
        var details = pedido.Detalles.Select(ToDetailView).ToList();

        return new PedidoView(
            pedido.Id.ToString(),
            pedido.ClienteId,
            pedido.FormaPagoId,
            pedido.VendedorId,
            pedido.EstadoVentaId,
            pedido.EstadoPagadoId,
            date,
            pedido.Cliente ?? string.Empty,
            vendedor,
            pedido.EstadoVenta ?? pedido.EstadoVentaId,
            delivery,
            pedido.MetodoEntrega ?? string.Empty,
            pedido.MetodoEntregaNombre ?? pedido.MetodoEntrega ?? string.Empty,
            pedido.MontoEnvioTransportadora,
            pedido.DeliveryUsuario ?? string.Empty,
            pedido.FechaTomaDelivery?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
            pedido.FormaPago ?? pedido.FormaPagoId,
            pedido.EstadoPagado ?? pedido.EstadoPagadoId ?? "Pendiente",
            pedido.TotalVenta,
            pedido.MontoPagado ?? 0,
            pedido.MontoPagado?.ToString("N0") ?? "0",
            pedido.ComprobantePago ?? string.Empty,
            pedido.ComprobantePagoNombre ?? string.Empty,
            pedido.Observacion ?? string.Empty,
            pedido.Reposicion,
            details,
            pedido.SucursalId,
            pedido.Sucursal);
    }
    private static bool IsCargaState(VentaImpresionCabDto order)
    {
        var detailStates = order.Detalles
            .Select(x => x.EstadoItem?.Trim())
            .OfType<string>()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (detailStates.Count > 0)
        {
            return detailStates.All(x =>
                string.Equals(x, "PC", StringComparison.OrdinalIgnoreCase) ||
                x.Contains("carga", StringComparison.OrdinalIgnoreCase) ||
                x.Contains("cargado", StringComparison.OrdinalIgnoreCase));
        }

        var state = $"{order.EstadoVentaId} {order.EstadoVenta}";
        return state.Contains("carga", StringComparison.OrdinalIgnoreCase) ||
            state.Contains("cargado", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildFilterQuery(
        DateTime? dateFrom,
        DateTime? dateTo,
        int? clienteId,
        string? estadoVentaId,
        string? estadoPagoId,
        int? vendedorId)
    {
        var filters = new List<string>();

        if (dateFrom.HasValue)
        {
            filters.Add($"dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("yyyy-MM-dd"))}");
        }

        if (dateTo.HasValue)
        {
            filters.Add($"dateTo={Uri.EscapeDataString(dateTo.Value.ToString("yyyy-MM-dd"))}");
        }

        if (clienteId.HasValue)
        {
            filters.Add($"clienteId={clienteId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(estadoVentaId))
        {
            filters.Add($"estadoVentaId={Uri.EscapeDataString(estadoVentaId)}");
        }

        if (!string.IsNullOrWhiteSpace(estadoPagoId))
        {
            filters.Add($"estadoPagoId={Uri.EscapeDataString(estadoPagoId)}");
        }

        if (vendedorId.HasValue)
        {
            filters.Add($"vendedorId={vendedorId.Value}");
        }

        return string.Join("&", filters);
    }

    private static PedidoDetalleView ToDetailView(VentaImpresionDetDto detalle)
    {
        return new PedidoDetalleView(
            detalle.Id,
            detalle.ProductoId,
            detalle.TipoMaquinaId,
            detalle.Producto ?? string.Empty,
            detalle.TipoMaquina ?? string.Empty,
            detalle.Cantidad.ToString("N2"),
            detalle.PrecioUnitario.ToString("N0"),
            detalle.PrecioExtra?.ToString("N0") ?? string.Empty,
            detalle.ArchivoDisenio ?? string.Empty,
            detalle.ArchivoDisenioNombre ?? string.Empty,
            detalle.CheckImpresion == true,
            detalle.EstadoItem ?? string.Empty,
            detalle.EstadoItemNombre ?? detalle.EstadoItem ?? string.Empty,
            detalle.Observacion ?? string.Empty);
    }

    [HttpGet("{id:int}/flujo")]
    public async Task<ActionResult<IEnumerable<PedidoFlujoEventoDto>>> GetFlujo(int id, CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<PedidoFlujoEventoDto>>(
            $"api/VentasImpresion/{id}/flujo",
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(result.StatusCode, new { message = result.ErrorMessage ?? "No se pudo cargar el flujo del pedido." });
    }}


