using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asp_presentacion.Pages
{
    public class RegistroModel : PageModel
    {
        public string? ErrorMessage { get; set; }
        public bool Exito { get; set; } = false;

        public void OnGet()
        {
        }

        public IActionResult OnPost(string nombre, string apellido, string correo, string contrasena, string contrasena2)
        {
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) ||
                string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena) ||
                string.IsNullOrEmpty(contrasena2))
            {
                ErrorMessage = "Por favor completa todos los campos.";
                return Page();
            }

            if (contrasena.Length < 8)
            {
                ErrorMessage = "La contraseña debe tener al menos 8 caracteres.";
                return Page();
            }

            if (contrasena != contrasena2)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return Page();
            }

            Exito = true;
            return Page();
        }
    }
}
