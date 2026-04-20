using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace asp_presentacion.Pages
{
    public class LoginModel : PageModel
    {
        public string? ErrorMessage { get; set; }
        public bool CuentaBloqueada { get; set; } = false;
        private static int _intentos = 0;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string correo, string contrasena)
        {
            if (_intentos >= 3)
            {
                CuentaBloqueada = true;
                ErrorMessage = "Tu cuenta ha sido bloqueada por múltiples intentos fallidos.";
                return Page();
            }

            try
            {
                using var client = new HttpClient();
                var body = JsonSerializer.Serialize(new
                {
                    Entidad = new
                    {
                        Nombre = correo,
                        Contraseña = contrasena
                    }
                });

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5250/Token/Llave", content);
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (resultado != null && resultado.ContainsKey("Llave"))
                {
                    _intentos = 0;
                    HttpContext.Session.SetString("Token", resultado["Llave"].GetString() ?? "");
                    HttpContext.Session.SetString("Usuario", correo);
                    return RedirectToPage("/Index");
                }
                else
                {
                    _intentos++;
                    ErrorMessage = "Correo o contraseña incorrectos. Intenta de nuevo.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al conectar con el servidor: " + ex.Message;
                return Page();
            }
        }
    }
}
