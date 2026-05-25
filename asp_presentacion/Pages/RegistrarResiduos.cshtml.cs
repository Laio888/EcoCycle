using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class RegistrarResiduosModel : PageModel
{
    [BindProperty]
    public double Peso { get; set; }

    [BindProperty]
    public string? ResidOrg { get; set; }

    [BindProperty]
    public string? CalidadRes { get; set; }

    public double Puntos { get; set; }
    public int Puntos_por_kg { get; set; }
    public double NumCalidad { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Puntos = 0.0;
        Puntos_por_kg = 0;
        NumCalidad = 1.0;
        ErrorMessage = null;

        if (Peso <= 0)
        {
            ErrorMessage = "Ingresa un peso mayor que 0.";
            return Page();
        }

        switch (ResidOrg)
        {
            case "frutas":
                Puntos_por_kg = 10;
                break;
            case "verduras":
                Puntos_por_kg = 12;
                break;
            case "huevo":
                Puntos_por_kg = 15;
                break;
            case "cafe":
                Puntos_por_kg = 15;
                break;
            case "te":
                Puntos_por_kg = 12;
                break;
            case "jardin":
                Puntos_por_kg = 10;
                break;
            case "cereales":
                Puntos_por_kg = 20;
                break;
            default:
                Puntos_por_kg = 0;
                break;
        }

        switch (CalidadRes)
        {
            case "alta":
                NumCalidad = 1.8;
                break;
            case "media":
                NumCalidad = 1.4;
                break;
            default:
                NumCalidad = 1.0;
                break;
        }

        Puntos = Puntos_por_kg * Peso * NumCalidad;

        try
        {
            using var client = new HttpClient();
            var body = JsonSerializer.Serialize(new
            {
                UsuarioId = 1,
                TipoResiduoId = ObtenerTipoResiduoId(ResidOrg),
                PesoKg = (decimal)Peso
            });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5250/api/RegistrosResiduos/Registrar", content);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"No se pudo guardar el registro en el backend: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al conectar con el backend: " + ex.Message;
        }

        return Page();
    }

    private int ObtenerTipoResiduoId(string? residuo)
    {
        return residuo switch
        {
            "frutas" => 1,
            "verduras" => 2,
            "huevo" => 3,
            "cafe" => 4,
            "te" => 5,
            "jardin" => 6,
            "cereales" => 7,
            _ => 1,
        };
    }
}
