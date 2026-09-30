using System.Text.Json;

namespace PrintKm.Services;

public static class AdministrationPermissions
{
    private static readonly IReadOnlyDictionary<string, string> ModuleForms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["sucursales"] = "Usuarios", ["usuarios"] = "Usuarios", ["personas"] = "Personas", ["perfiles"] = "Perfiles", ["accesos"] = "Accesos",
        ["transportadoras"] = "Transportadoras", ["productos"] = "Productos", ["productoComisiones"] = "Comisiones por producto",
        ["gruposVenta"] = "Grupo de ventas", ["maquinas"] = "Tipos de maquina", ["formasPago"] = "Formas de pago",
        ["estadosPago"] = "Estados de pago", ["estadosVenta"] = "Estados de venta", ["tiposDocumento"] = "Tipos documento",
        ["tiposCliente"] = "Tipos cliente", ["configuraciones"] = "Configuraciones", ["departamentos"] = "Departamentos", ["ciudades"] = "Ciudades"
    };

    public static bool CanView(ISession session, string module) => HasPermission(session, module, "puedeVer");
    public static bool CanCreate(ISession session, string module) => HasPermission(session, module, "puedeCrear");
    public static bool CanEdit(ISession session, string module) => HasPermission(session, module, "puedeEditar");
    public static bool CanDelete(ISession session, string module) => HasPermission(session, module, "puedeEliminar");

    private static bool HasPermission(ISession session, string module, string permissionName)
    {
        if (!ModuleForms.TryGetValue(module, out var formName)) return false;
        var json = session.GetString("BackendPermissions");
        if (string.IsNullOrWhiteSpace(json)) return true;

        try
        {
            using var document = JsonDocument.Parse(json);
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (!TryGetProperty(item, "formulario", out var form) ||
                    !string.Equals(form.GetString(), formName, StringComparison.OrdinalIgnoreCase)) continue;
                return TryGetProperty(item, permissionName, out var permission) && permission.GetBoolean();
            }
        }
        catch (JsonException) { return false; }

        return false;
    }

    private static bool TryGetProperty(JsonElement item, string name, out JsonElement value) =>
        item.TryGetProperty(name, out value) || item.TryGetProperty(char.ToUpperInvariant(name[0]) + name[1..], out value);
}
