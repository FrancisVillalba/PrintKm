using System.Text.Json;
using PrintKm.Services;
using Microsoft.AspNetCore.Mvc;

namespace PrintKm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdministracionController : ControllerBase
{
    private static readonly Dictionary<string, AdminModule> Modules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["usuarios"] = new("api/Usuarios", true, true, true),
        ["sucursales"] = new("api/Sucursales", true, true, false),
        ["personas"] = new("api/Personas", true, true, true),
        ["perfiles"] = new("api/Perfiles", true, true, true),
        ["transportadoras"] = new("api/Transportadoras", true, true, true),
        ["productos"] = new("api/Productos", true, true, true),
        ["productoComisiones"] = new("api/ProductoComisiones", true, true, true),
        ["gruposVenta"] = new("api/GruposVenta", true, true, true),
        ["maquinas"] = new("api/TiposMaquina", true, true, true),
        ["formasPago"] = new("api/FormasPago", true, true, true),
        ["estadosPago"] = new("api/EstadosPago", true, true, true),
        ["estadosVenta"] = new("api/EstadosVenta", true, true, true),
        ["tiposDocumento"] = new("api/TiposDocumento", true, true, true),
        ["tiposCliente"] = new("api/TiposCliente", true, true, true),
        ["configuraciones"] = new("api/Configuraciones", true, true, true),
        ["departamentos"] = new("api/Departamentos", true, true, true),
        ["ciudades"] = new("api/Ciudades", true, true, true)
    };

    private readonly IBackendApiClient _backendApiClient;

    public AdministracionController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpGet("opciones")]
    public async Task<ActionResult<AdminOptionsDto>> GetOptions(CancellationToken cancellationToken)
    {
        var perfilesTask = _backendApiClient.GetAsync<IEnumerable<PerfilDto>>("api/Perfiles", cancellationToken);
        var personasEndpoint = AdministrationPermissions.CanView(HttpContext.Session, "usuarios")
            ? "api/Usuarios/personas" : "api/Personas";
        var personasTask = _backendApiClient.GetAsync<IEnumerable<PersonaDto>>(personasEndpoint, cancellationToken);
        var maquinasTask = _backendApiClient.GetAsync<IEnumerable<TipoMaquinaDto>>("api/TiposMaquina", cancellationToken);
        var departamentosTask = _backendApiClient.GetAsync<IEnumerable<DepartamentoDto>>("api/Departamentos", cancellationToken);
        var tiposDocumentoTask = _backendApiClient.GetAsync<IEnumerable<CatalogStringDto>>("api/TiposDocumento", cancellationToken);
        var productosTask = _backendApiClient.GetAsync<IEnumerable<ProductoDto>>("api/Productos", cancellationToken);
        var sucursalesEndpoint = AdministrationPermissions.CanView(HttpContext.Session, "usuarios")
            ? "api/Usuarios/sucursales" : "api/Sucursales";
        var sucursalesTask = _backendApiClient.GetAsync<IEnumerable<SucursalDto>>(sucursalesEndpoint, cancellationToken);
        var usuariosTask = _backendApiClient.GetAsync<IEnumerable<UsuarioDto>>("api/Usuarios", cancellationToken);

        await Task.WhenAll(perfilesTask, personasTask, maquinasTask, departamentosTask, tiposDocumentoTask, productosTask, usuariosTask, sucursalesTask);

        return Ok(new AdminOptionsDto(
            await perfilesTask ?? [],
            await personasTask ?? [],
            await maquinasTask ?? [],
            await departamentosTask ?? [],
            await tiposDocumentoTask ?? [],
            await productosTask ?? [],
            await usuariosTask ?? [],
            await sucursalesTask ?? []));
    }

    [HttpGet("{module}")]
    public async Task<IActionResult> GetAll(
        string module,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!AdministrationPermissions.CanView(HttpContext.Session, module)) return Forbid();

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = BuildListQuery(search, page, pageSize);
        var result = await _backendApiClient.GetResultAsync<JsonElement>($"{config.Endpoint}?{query}", cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
        }

        return Ok(NormalizePagedResult(result.Value, search, page, pageSize));
    }

    [HttpPost("{module}")]
    public async Task<IActionResult> Create(string module, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!config.CanCreate)
        {
            return BadRequest(new { message = "Este modulo es solo lectura." });
        }

        if (!AdministrationPermissions.CanCreate(HttpContext.Session, module)) return Forbid();

        var result = await _backendApiClient.PostResultAsync<JsonElement>(config.Endpoint, body, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("{module}/{id}")]
    public async Task<IActionResult> Update(string module, string id, [FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!config.CanEdit)
        {
            return BadRequest(new { message = "Este modulo es solo lectura." });
        }

        if (!AdministrationPermissions.CanEdit(HttpContext.Session, module)) return Forbid();

        var result = await _backendApiClient.PutResultAsync<JsonElement>($"{config.Endpoint}/{Uri.EscapeDataString(id)}", body, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    [HttpDelete("{module}/{id}")]
    public async Task<IActionResult> Delete(string module, string id, CancellationToken cancellationToken)
    {
        if (!TryGetModule(module, out var config))
        {
            return NotFound(new { message = "Modulo administrativo no encontrado." });
        }

        if (!config.CanDelete)
        {
            return BadRequest(new { message = "Este modulo es solo lectura." });
        }

        if (!AdministrationPermissions.CanDelete(HttpContext.Session, module)) return Forbid();

        var deleted = await _backendApiClient.DeleteAsync($"{config.Endpoint}/{Uri.EscapeDataString(id)}", cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo eliminar el registro." });
    }

    [HttpGet("permisos/formularios")]
    public async Task<IActionResult> GetFormularios(CancellationToken cancellationToken)
    {
        if (!AdministrationPermissions.CanView(HttpContext.Session, "accesos")) return Forbid();

        var result = await _backendApiClient.GetResultAsync<JsonElement>("api/Permisos/formularios", cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("permisos/perfil/{perfilId:int}")]
    public async Task<IActionResult> GetPermisosPerfil(int perfilId, CancellationToken cancellationToken)
    {
        if (!AdministrationPermissions.CanView(HttpContext.Session, "accesos")) return Forbid();

        var result = await _backendApiClient.GetResultAsync<JsonElement>($"api/Permisos/perfil/{perfilId}", cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("permisos/perfil-formulario")]
    public async Task<IActionResult> SavePermiso([FromBody] PerfilFormularioPermisoRequest request, CancellationToken cancellationToken)
    {
        if (!AdministrationPermissions.CanEdit(HttpContext.Session, "accesos")) return Forbid();

        var result = await _backendApiClient.PostResultAsync<PerfilFormularioPermisoDto>("api/Permisos/perfil-formulario", request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    private static bool TryGetModule(string module, out AdminModule config)
    {
        return Modules.TryGetValue(module, out config!);
    }

    private IActionResult ToActionResult<T>(BackendApiResult<T> result)
    {
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    private static string BuildListQuery(string? search, int page, int pageSize)
    {
        var filters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add($"search={Uri.EscapeDataString(search)}");
        }

        return string.Join("&", filters);
    }

    private static object NormalizePagedResult(JsonElement value, string? search, int page, int pageSize)
    {
        if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("items", out _))
        {
            return value;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return new PagedResponse<JsonElement>([], page, pageSize, 0, 1);
        }

        var items = value.EnumerateArray()
            .Where(item => MatchesSearch(item, search))
            .ToList();
        var totalItems = items.Count;
        var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)pageSize), 1);
        page = Math.Min(page, totalPages);

        return new PagedResponse<JsonElement>(
            items.Skip((page - 1) * pageSize).Take(pageSize),
            page,
            pageSize,
            totalItems,
            totalPages);
    }

    private static bool MatchesSearch(JsonElement item, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var term = search.Trim();
        foreach (var property in item.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            {
                var value = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.ToString();
                if (!string.IsNullOrWhiteSpace(value) && value.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private record AdminModule(string Endpoint, bool CanCreate, bool CanEdit, bool CanDelete);
}
