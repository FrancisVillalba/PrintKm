namespace PrintKm.Services;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record DashboardSummaryDto(
    int TotalPedidos,
    int PedidosCargados,
    int PedidosImpresos,
    int PedidosPendientesImpresion,
    int PedidosControl,
    int PedidosEntregados,
    decimal TotalPedidosMensuales,
    DashboardGoalDto MetaMensualTotal,
    IEnumerable<DashboardMachineDto> PedidosPorMaquina,
    IEnumerable<DashboardMachineDto> PedidosMensualesPorMaquina,
    IEnumerable<DashboardGoalDto> MetasMensualesPorMaquina,
    IEnumerable<DashboardMoneyDto> PendientesPago,
    IEnumerable<DashboardSellerDto> MejoresVendedores,
    DashboardStageDto Carga,
    DashboardStageDto Impresion,
    DashboardStageDto Control,
    DashboardStageDto PendienteEnvio,
    DashboardStageDto EnviadosHoy,
    DashboardStageDto Incidencias);

public record DashboardStageDto(int Total, int MovimientosHoy, int Demorados24, int Demorados48);

public record DashboardMachineDto(string Nombre, decimal Cantidad);

public record DashboardGoalDto(string Nombre, decimal Cantidad, decimal Meta, decimal Faltante, decimal Porcentaje, bool Cumplido);

public record DashboardMoneyDto(string Nombre, decimal Monto);

public record DashboardSellerDto(string Nombre, decimal Monto);

public record DashboardPedidoDto(
    int Id,
    DateTime Fecha,
    string Cliente,
    string Vendedor,
    string Estado,
    string FormaPago,
    string MetodoEntrega,
    decimal TotalVenta,
    decimal MontoPagado,
    decimal SaldoPendiente);

public record LoginRequest(string Usuario, string Pass);

public record RefreshTokenRequest(string RefreshToken);

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt,
    int UsuarioId,
    string Usuario,
    int? PersonaId,
    string? Persona,
    IEnumerable<PerfilFormularioPermisoDto> Permisos);

public record MensajePendienteDto(string Clave, string Titulo, string Mensaje, string Tipo);

public record ClienteDto(
    int Id,
    string? Nombre,
    string? Documento,
    string TipoDocumentoId,
    string TipoClienteId,
    string? Email,
    string? NroTelefono,
    string? Direccion,
    int? DepartamentoId,
    string? Departamento,
    int? CiudadId,
    string? Ciudad,
    bool? Estado,
    ClienteDatosEnvioDto? DatosEnvio);

public record ClienteDatosEnvioDto(
    int Id,
    int ClienteId,
    int TransportadoraId,
    string? Transportadora,
    decimal MontoTransportadora,
    string NombreReceptor,
    string DocumentoReceptor,
    string TelefonoReceptor,
    int DepartamentoId,
    string? Departamento,
    int CiudadId,
    string? Ciudad,
    string Direccion,
    string? Observacion,
    bool Estado);

public record ClienteRequest(
    string? Nombre,
    string? Documento,
    string TipoDocumentoId,
    string TipoClienteId,
    string? Email,
    string? NroTelefono,
    string? Direccion,
    int? DepartamentoId,
    int? CiudadId,
    bool? Estado);

public record ClienteDatosEnvioRequest(
    int ClienteId,
    int TransportadoraId,
    string NombreReceptor,
    string DocumentoReceptor,
    string TelefonoReceptor,
    int DepartamentoId,
    int CiudadId,
    string Direccion,
    string? Observacion,
    bool Estado);

public record CatalogStringDto(string Id, string? Nombre, bool? Estado);

public record DepartamentoDto(int Id, string Nombre, bool Estado);

public record CiudadDto(int Id, int DepartamentoId, string? Departamento, int CodigoDistrito, string Nombre, bool Estado);

public record SucursalDto(int Id, string Nombre, string? Direccion, bool Estado);

public record TransportadoraDto(int Id, string Nombre, string? Telefono, string? Direccion, string? Observacion, decimal Monto, bool Estado);

public record ClienteOptionsDto(
    IEnumerable<CatalogStringDto> TiposDocumento,
    IEnumerable<CatalogStringDto> TiposCliente,
    IEnumerable<TransportadoraDto> Transportadoras,
    IEnumerable<DepartamentoDto> Departamentos,
    IEnumerable<CiudadDto> Ciudades);

public record PerfilDto(int Id, string Nombre, string? Descripcion, bool Estado);

public record PersonaDto(
    int Id,
    string? PrimerNombre,
    string? SegundoNombre,
    string? PrimerApellido,
    string? SegundoApellido,
    DateTime? FechaCumpleanios,
    string? TipoDocumentoId,
    string? Documento,
    string? Telefono,
    bool? Estado);

public record FormularioDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? Ruta,
    string? Icono,
    int Orden,
    bool Estado,
    int? FormularioPadreId,
    string? FormularioPadre);

public record PerfilFormularioPermisoDto(
    int Id,
    int PerfilId,
    string? Perfil,
    int FormularioId,
    string Formulario,
    string? Descripcion,
    string? Ruta,
    string? Icono,
    int Orden,
    int? FormularioPadreId,
    string? FormularioPadre,
    bool PuedeVer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeEliminar);

public record PerfilFormularioPermisoRequest(
    int PerfilId,
    int FormularioId,
    bool PuedeVer,
    bool PuedeCrear,
    bool PuedeEditar,
    bool PuedeEliminar);

public record AdminOptionsDto(
    IEnumerable<PerfilDto> Perfiles,
    IEnumerable<PersonaDto> Personas,
    IEnumerable<TipoMaquinaDto> Maquinas,
    IEnumerable<DepartamentoDto> Departamentos,
    IEnumerable<CatalogStringDto> TiposDocumento,
    IEnumerable<ProductoDto> Productos,
    IEnumerable<UsuarioDto> Usuarios,
    IEnumerable<SucursalDto> Sucursales);

public record EstadoVentaOptionDto(string Id, string? Nombre, string? Estado, int? NumeroFlujo);

public record UsuarioDto(
    int Id,
    string? NombreUsuario,
    int? PersonaId,
    string? Persona,
    int? PerfilId,
    string? Perfil,
    IEnumerable<int> PerfilIds,
    string? Perfiles,
    bool? Estado,
    int? SucursalId = null,
    string? Sucursal = null);

public record ProductoDto(int Id, string Nombre, decimal PrecioBase, decimal PrecioMenor, decimal CompraMinimaCm, int? MaquinaId, string? Maquina, bool Estado);

public record ProductoComisionDto(
    int Id,
    int ProductoId,
    string? Producto,
    int PerfilId,
    string? Perfil,
    decimal Porcentaje,
    bool Estado);

public record GrupoVentaDto(
    int Id,
    string Nombre,
    int TeamLeaderUsuarioId,
    string? TeamLeader,
    bool Estado,
    IEnumerable<GrupoVentaVendedorDto> Vendedores);

public record GrupoVentaVendedorDto(int Id, int VendedorUsuarioId, string? Vendedor, bool Estado);

public record GrupoVentaEquipoDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    IEnumerable<GrupoVentaResumenVendedorDto> Resumen,
    IEnumerable<GrupoVentaVentaDto> Ventas,
    IEnumerable<GrupoVentaFiltroVendedorDto> Vendedores,
    bool PuedeFiltrarVendedores);

public record GrupoVentaFiltroVendedorDto(int VendedorId, string Vendedor);

public record GrupoVentaResumenVendedorDto(
    int VendedorId,
    string Vendedor,
    int CantidadPedidos,
    decimal TotalVenta,
    decimal TotalMetros,
    decimal TotalComision);

public record GrupoVentaVentaDto(
    int PedidoId,
    DateTime Fecha,
    int VendedorId,
    string Vendedor,
    string Cliente,
    string Estado,
    decimal TotalVenta,
    decimal TotalPagado,
    decimal TotalMetros,
    decimal TotalComision,
    string? UsuarioEntrega,
    bool Reposicion,
    IEnumerable<GrupoVentaDetalleEstadoDto> Detalles);

public record GrupoVentaDetalleEstadoDto(
    int DetalleId,
    string Producto,
    string EstadoItem,
    string EstadoItemNombre);

public record TipoMaquinaDto(int Id, string Nombre, decimal MetaMensual, bool Estado);

public record PedidoFormOptionsDto(
    IEnumerable<ClienteDto> Clientes,
    IEnumerable<CatalogStringDto> FormasPago,
    IEnumerable<UsuarioDto> Vendedores,
    IEnumerable<CatalogStringDto> EstadosPago,
    IEnumerable<EstadoVentaOptionDto> EstadosVenta,
    IEnumerable<ProductoDto> Productos,
    IEnumerable<TipoMaquinaDto> Maquinas,
    int? UsuarioActualId,
    bool PuedeVerTodosPedidos,
    bool PuedeVerVentasUsuario,
    decimal MontoEnvioTransportadora,
    decimal CmPrecioMayorOMenor,
    decimal CompraMinimaCm);

public record VentaUsuarioResumenDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    bool PuedeVerVentasUsuario,
    VentaUsuarioTotalesDto Totales,
    IEnumerable<VentaUsuarioItemDto> Ventas);

public record VentaUsuarioTotalesDto(
    int CantidadPedidos,
    decimal TotalVenta,
    decimal TotalMetros,
    decimal TotalComision);

public record VentaUsuarioItemDto(
    int PedidoId,
    DateTime Fecha,
    string Cliente,
    string Estado,
    decimal TotalVenta,
    decimal TotalMetros,
    decimal TotalComision);

public record ExcelFileDto(string FileName, string ContentType, string Bytes);

public record ArchivoUploadResponse(
    bool Guardado,
    string NombreOriginal,
    string NombreDescarga,
    string NombreGuardado,
    string Ruta,
    long TamanioBytes);

public record ArchivoBase64Response(
    string NombreArchivo,
    string ContentType,
    string Base64,
    long TamanioBytes);

public record ControlPedidoDto(
    int Id,
    DateTime FechaCreacion,
    DateTime? FechaEntrega,
    string Cliente,
    string Vendedor,
    string EstadoVentaId,
    string? EstadoVenta,
    string? MetodoEntregaId,
    string? MetodoEntrega,
    decimal TotalVenta,
    IEnumerable<ControlPedidoDetalleDto> Detalles);

public record ControlPedidoDetalleDto(
    int Id,
    int PedidoId,
    int TipoMaquinaId,
    string TipoMaquina,
    string Producto,
    decimal Cantidad,
    string? ArchivoDisenioNombre,
    string? Observacion,
    string EstadoItem,
    bool Impreso);
public record DeliveryPedidoDto(
    int Id,
    DateTime FechaCreacion,
    DateTime? FechaEntrega,
    string Cliente,
    string? Telefono,
    string? Direccion,
    string? Departamento,
    string? Ciudad,
    string EstadoVentaId,
    string? EstadoVenta,
    string Vendedor,
    decimal TotalVenta,
    string MetodoEntregaId,
    string MetodoEntrega,
    int? DeliveryUsuarioId,
    string? DeliveryUsuario,
    DateTime? FechaTomaDelivery,
    string Productos,
    string? Transportadora = null);

public record DeliveryResumenDto(
    int UsuarioId,
    string Delivery,
    int CantidadPedidos,
    decimal TotalPedidos,
    string Ciudades,
    string MetodosEnvio);

public record DeliveryRutaDto(
    int Id,
    string NumeroLote,
    int UsuarioDeliveryId,
    string UsuarioDelivery,
    DateTime FechaGeneracion,
    string Estado,
    int CantidadPedidos,
    string Ciudades,
    string MetodosEnvio,
    IEnumerable<DeliveryPedidoDto> Pedidos);

public record ReporteComisionesDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    IEnumerable<ReporteComisionVendedorDto> Vendedores);

public record ReporteComisionVendedorDto(
    int VendedorId,
    string Vendedor,
    int CantidadPedidos,
    decimal TotalVenta,
    decimal TotalComision,
    IEnumerable<ReporteComisionDetalleDto> Detalles);

public record ReporteComisionDetalleDto(
    int PedidoId,
    DateTime Fecha,
    string Cliente,
    string Producto,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal PrecioExtra,
    decimal TotalDetalle,
    decimal ComisionUnitario,
    decimal ComisionTotal,
    string? VendedorOrigen = null,
    int? VendedorOrigenId = null);

public record LotePagoDto(
    int Id,
    string TipoPago,
    DateTime FechaGeneracion,
    string UsuarioGenero,
    DateTime FechaDesde,
    DateTime FechaHasta,
    DateTime FechaPago,
    string? Vendedor,
    int? PerfilId,
    string? Perfil,
    decimal MontoTotal,
    int CantidadPersonas,
    string NombreArchivo,
    string Estado);

public record ReporteEnviosDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    IEnumerable<ReporteEnvioResumenDto> Resumen,
    IEnumerable<ReporteEnvioDetalleDto> Detalles);

public record ReporteEnvioResumenDto(
    int? UsuarioEntregaId,
    string UsuarioEntrega,
    int CantidadPedidos,
    int CantidadTransportadora,
    decimal TotalPedidos,
    decimal TotalComisionDelivery);

public record ReporteEnvioDetalleDto(
    int PedidoId,
    DateTime Fecha,
    string Cliente,
    string MetodoEntregaId,
    string MetodoEntrega,
    string Estado,
    string? UsuarioEntrega,
    string? Ciudad,
    decimal TotalPedido,
    decimal ComisionDelivery);
public record ReporteClientesDeudaDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    decimal DeudaTotal,
    int CantidadClientes,
    int CantidadPedidos,
    decimal TotalVendido,
    decimal TotalPagado,
    IEnumerable<ReporteClienteDeudaDto> Clientes);

public record ReporteClienteDeudaDto(
    int ClienteId,
    string Cliente,
    string? Telefono,
    int CantidadPedidos,
    decimal TotalVendido,
    decimal TotalPagado,
    decimal SaldoPendiente,
    DateTime UltimoPedido,
    string Estado,
    IEnumerable<ReporteClienteDeudaPedidoDto> Pedidos);

public record ReporteClienteDeudaPedidoDto(
    int PedidoId,
    DateTime Fecha,
    string Vendedor,
    decimal TotalVenta,
    decimal MontoPagado,
    decimal SaldoPendiente,
    string EstadoPago,
    IEnumerable<ReporteClienteComprobanteDto> ComprobantesTransferencia);

public record ReporteClienteComprobanteDto(
    DateTime Fecha,
    decimal Monto,
    string Ruta,
    string Nombre);

public record ReporteResumenGerencialDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    decimal TotalVendido,
    int CantidadPedidos,
    decimal PromedioPedido,
    decimal TotalVendidoComisionPagada,
    decimal TotalComisionPagada,
    IEnumerable<ReporteResumenProductoDto> VentasPorProducto,
    IEnumerable<ReporteResumenMaquinaDto> VentasPorMaquina,
    IEnumerable<ReporteResumenPerfilComisionDto> ComisionesPorPerfil);

public record ReporteResumenProductoDto(
    string Producto,
    int CantidadPedidos,
    decimal Cantidad,
    decimal TotalVenta);
public record ReporteResumenMaquinaDto(
    string Maquina,
    int CantidadPedidos,
    decimal Cantidad,
    decimal TotalVenta);

public record ReporteResumenPerfilComisionDto(
    int PerfilId,
    string Perfil,
    int CantidadPedidos,
    decimal TotalVendido,
    decimal TotalPagado);

public record ReporteResumenEstadoDto(
    string Estado,
    int NumeroFlujo,
    int CantidadPedidos,
    decimal TotalVenta);

public record ReporteResumenVendedorDto(
    int VendedorId,
    string Vendedor,
    int CantidadPedidos,
    decimal TotalVenta,
    decimal TotalComision);

public record ReporteResumenDeudaDto(
    int PedidoId,
    DateTime Fecha,
    string Cliente,
    string Vendedor,
    decimal TotalVenta,
    decimal MontoPagado,
    decimal Saldo,
    int DiasAtraso);

public record ReporteResumenEntregaDto(
    int? UsuarioEntregaId,
    string UsuarioEntrega,
    int PedidosTomados,
    int PedidosEntregados,
    decimal TotalMovido);

public record VentaImpresionCompletaRequest(
    int ClienteId,
    string FormaPagoId,
    int VendedorId,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    DateTime? FechaEntrega,
    string? ComprobantePago,
    string? ComprobantePagoNombre,
    string? Observacion,
    string? MetodoEntrega,
    bool Reposicion,
    string? EstadoVentaId,
    IEnumerable<VentaImpresionDetalleCreateRequest> Detalles);

public record VentaImpresionDetalleCreateRequest(
    int ProductoId,
    int TipoMaquinaId,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PrecioExtra,
    string? ArchivoDisenio,
    string? ArchivoDisenioNombre,
    string? Observacion,
    string? EstadoItem,
    bool? CheckImpresion);

public record VentaImpresionCompletaUpdateRequest(
    int ClienteId,
    string FormaPagoId,
    int VendedorId,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    DateTime? FechaEntrega,
    string? ComprobantePago,
    string? ComprobantePagoNombre,
    string? Observacion,
    string? MetodoEntrega,
    bool Reposicion,
    string? EstadoVentaId,
    DateTime? FechaModificacion,
    bool CambiarEstado,
    IEnumerable<VentaImpresionDetalleUpdateRequest> Detalles);

public record ActualizarPagoVentaRequest(
    string FormaPagoId,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    string? ComprobantePago,
    string? ComprobantePagoNombre);

public record PagoVentaImpresionDto(
    int Id,
    int VentaImpresionId,
    DateTime FechaHora,
    int UsuarioId,
    string FormaPagoId,
    decimal Monto,
    string? RutaComprobante,
    string? NombreComprobante);

public record PagoVentaImpresionRequest(
    int VentaImpresionId,
    string FormaPagoId,
    decimal Monto,
    string? RutaComprobante,
    string? NombreComprobante);
public record EliminarPedidoRequest(string Observacion);

public record VentaImpresionDetalleUpdateRequest(
    int? Id,
    int ProductoId,
    int TipoMaquinaId,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PrecioExtra,
    string? ArchivoDisenio,
    string? ArchivoDisenioNombre,
    string? Observacion,
    string? EstadoItem,
    bool? CheckImpresion);

public record VentaImpresionCabDto(
    int Id,
    int ClienteId,
    string? Cliente,
    string FormaPagoId,
    string? FormaPago,
    decimal TotalVenta,
    decimal MontoEnvioTransportadora,
    string EstadoVentaId,
    string? EstadoVenta,
    int VendedorId,
    string? Vendedor,
    decimal? MontoPagado,
    string? EstadoPagadoId,
    string? EstadoPagado,
    DateTime? FechaCreacion,
    DateTime? FechaModificacion,
    DateTime? FechaEntrega,
    string? ComprobantePago,
    string? ComprobantePagoNombre,
    string? Observacion,
    string? MetodoEntrega,
    string? MetodoEntregaNombre,
    bool Reposicion,
    int? DeliveryUsuarioId,
    string? DeliveryUsuario,
    DateTime? FechaTomaDelivery,
    IEnumerable<VentaImpresionDetDto> Detalles,
    int? SucursalId = null,
    string? Sucursal = null);

public record VentaImpresionDetDto(
    int Id,
    int CabId,
    int ProductoId,
    string? Producto,
    int TipoMaquinaId,
    string? TipoMaquina,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PrecioExtra,
    decimal? PrecioTotal,
    string? ArchivoDisenio,
    string? ArchivoDisenioNombre,
    string? Observacion,
    string EstadoItem,
    string EstadoItemNombre,
    bool? CheckImpresion,
    DateTime? FechaModificacion);

public record ImpresionArchivoDto(
    int DetalleId,
    int PedidoId,
    DateTime FechaCarga,
    DateTime? FechaEntrega,
    string Cliente,
    string Vendedor,
    int TipoMaquinaId,
    string TipoMaquina,
    string Producto,
    decimal Cantidad,
    string? ArchivoDisenioNombre,
    string EstadoVenta,
    bool Impreso);

public record ImpresionMarcarDto(
    int DetalleId,
    int PedidoId,
    bool DetalleImpreso,
    bool PedidoCompleto,
    string EstadoVentaId,
    string? EstadoVenta);

public record PedidoFlujoEventoDto(
    DateTime FechaHora,
    int? UsuarioId,
    string Usuario,
    string Accion,
    string EstadoAnteriorId,
    string EstadoAnterior,
    string EstadoNuevoId,
    string EstadoNuevo,
    string? Comentario,
    int? DetalleId,
    string? Producto);
public record ImpresionDevolverRequest(string? Observacion);

public record ImpresionDevolverDto(
    int DetalleId,
    int PedidoId,
    string EstadoVentaId,
    string? EstadoVenta,
    string Observacion);

public record NotificacionDto(long Id, string Tipo, string Titulo, string Mensaje, int PedidoId, int? DetalleId, string? Producto, string Cliente, string? Comentario, bool Leida, DateTime FechaCreacion, DateTime? FechaLectura);
public record NotificacionesResumenDto(int NoLeidas, IEnumerable<NotificacionDto> Items);

