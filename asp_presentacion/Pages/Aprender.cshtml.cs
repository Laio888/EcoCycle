// ============================================
// EcoCycle – PageModel página Aprender
// asp_presentacion/Pages/Aprender.cshtml.cs
// ============================================

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcoCycle.asp_presentacion.Pages
{
    public class AprenderModel : PageModel
    {
        // ── Datos del usuario para el navbar ──
        public int UserPoints { get; private set; }
        public int UserLevel { get; private set; }
        public string LevelName { get; private set; } = string.Empty;

        // ── Aquí puedes inyectar servicios del proyecto asp_servicios ──
        // private readonly IUsuarioService _usuarioService;
        // private readonly IArticuloService _articuloService;
        //
        // public AprenderModel(IUsuarioService usuarioService, IArticuloService articuloService)
        // {
        //     _usuarioService  = usuarioService;
        //     _articuloService = articuloService;
        // }

        public void OnGet()
        {
            // TODO: reemplazar con datos reales del usuario autenticado
            // var usuario = await _usuarioService.ObtenerUsuarioActualAsync(User);
            // UserPoints = usuario.PuntosTotales;
            // UserLevel  = usuario.Nivel;
            // LevelName  = usuario.NombreNivel;

            // Datos de ejemplo mientras conectas el backend
            UserPoints = 850;
            UserLevel = 3;
            LevelName = "Intermedio";
        }
    }
}