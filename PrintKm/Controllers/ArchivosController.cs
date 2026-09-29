using PrintKm.Services;
using Microsoft.AspNetCore.Mvc;

namespace PrintKm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchivosController : ControllerBase
{
    private const long MaxUploadBytes = 500L * 1024L * 1024L;
    private readonly IBackendApiClient _backendApiClient;

    public ArchivosController(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> Upload(IFormFile archivo, [FromForm] string? carpeta = null, CancellationToken cancellationToken = default)
    {
        if (archivo.Length == 0)
        {
            return BadRequest(new { message = "El archivo esta vacio." });
        }

        var result = await _backendApiClient.PostFileResultAsync<ArchivoUploadResponse>(
            "api/Archivos/upload",
            archivo,
            new Dictionary<string, string?> { ["carpeta"] = carpeta },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string ruta, [FromQuery] string? nombreDescarga = null, [FromQuery] bool inline = false, CancellationToken cancellationToken = default)
    {
        var query = $"ruta={Uri.EscapeDataString(ruta)}";
        if (!string.IsNullOrWhiteSpace(nombreDescarga))
        {
            query += $"&nombreDescarga={Uri.EscapeDataString(nombreDescarga)}";
        }

        var file = await _backendApiClient.GetAsync<ArchivoBase64Response>($"api/Archivos/base64?{query}", cancellationToken);
        if (file is null || string.IsNullOrWhiteSpace(file.Base64))
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo descargar el archivo desde EvaluSystemBack." });
        }

        var bytes = Convert.FromBase64String(file.Base64);
        return inline
            ? File(bytes, file.ContentType)
            : File(bytes, file.ContentType, file.NombreArchivo);
    }
}
