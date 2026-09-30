namespace PrintKm.Services;

public record PagedView<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record ClienteView(
    int Id,
    string Name,
    string Document,
    string Phone,
    string Email,
    int? CityId,
    string City,
    int? TransportadoraId,
    string Transportadora,
    bool Carrier,
    string Status);

public record PedidoView(
    string Id,
    int ClientId,
    string FormaPagoId,
    int VendedorId,
    string EstadoVentaId,
    string? EstadoPagadoId,
    string Date,
    string Client,
    string Seller,
    string Status,
    string Delivery,
    string DeliveryMethodId,
    string DeliveryMethod,
    decimal MontoEnvioTransportadora,
    string DeliveryUser,
    string DeliveryTakenAt,
    string PaymentMethod,
    string PaymentStatus,
    decimal TotalVenta,
    decimal MontoPagado,
    string PaidAmount,
    string ProofPath,
    string ProofName,
    string Notes,
    bool Reposicion,
    List<PedidoDetalleView> Details,
    int? SucursalId = null,
    string? Sucursal = null);

public record PedidoDetalleView(
    int Id,
    int ProductId,
    int MachineId,
    string Product,
    string Machine,
    string Quantity,
    string UnitPrice,
    string ExtraPrice,
    string DesignPath,
    string DesignName,
    bool CheckImpresion,
    string StateItem,
    string StateItemName,
    string Observation);

public record DashboardView(
    DashboardMetrics Metrics,
    IEnumerable<DashboardOrderView> RecentOrders,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record DashboardMetrics(int LoadedToday, int Printed, int MissingPrint, string Delivered);

public record DashboardOrderView(
    string Id,
    string Client,
    string Seller,
    string Type,
    string Meters,
    string Status,
    string Delivery);
