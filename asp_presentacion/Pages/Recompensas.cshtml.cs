using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RecompensasModel : PageModel
{
    private static int puntosGlobal = 1450;

    public int PuntosUsuario { get; set; }
    public List<Recompensa> Recompensas { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    public void OnGet()
    {
        PuntosUsuario = puntosGlobal;
        Recompensas = ObtenerRecompensas();
    }

    public IActionResult OnPost(int id)
    {
        Recompensas = ObtenerRecompensas();
        var recompensa = Recompensas.FirstOrDefault(r => r.Id == id);

        if (recompensa != null)
        {
            if (puntosGlobal >= recompensa.PuntosNecesarios)
            {
                puntosGlobal -= recompensa.PuntosNecesarios;
                if (puntosGlobal < 0) puntosGlobal = 0;

                Mensaje = $"🎉 ¡Felicidades! Canjeaste: {recompensa.Nombre}. Gracias por tu impacto ambiental 🌱";
            }
            else
            {
                int puntosFaltantes = recompensa.PuntosNecesarios - puntosGlobal;
                Mensaje = $"❌ No tienes suficientes puntos. Te faltan {puntosFaltantes} puntos para canjear {recompensa.Nombre}";
            }
        }

        PuntosUsuario = puntosGlobal;
        return Page();
    }

    private List<Recompensa> ObtenerRecompensas()
    {
        return new List<Recompensa>
        {
            new Recompensa(1, "🌱 Kit de semillas", "Cultiva alimentos en casa y reduce tu huella ecológica", 50),
            new Recompensa(2, "🪴 Maceta ecológica", "Hecha con materiales reciclados y biodegradable", 80),
            new Recompensa(3, "🥕 Descuento en mercado", "Apoya productos locales sostenibles", 100),
            new Recompensa(4, "🌳 Árbol plantado", "Contribuyes a la reforestación en tu nombre", 150),
            new Recompensa(5, "♻️ Kit compostaje", "Empieza a compostar en casa fácilmente", 200),
            new Recompensa(6, "🎓 Curso ecológico", "Aprende sostenibilidad avanzada con expertos", 120)
        };
    }
}

public class Recompensa
{
    public int Id { get; set; }
    public string Icono { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int PuntosNecesarios { get; set; }

    public Recompensa(int id, string nombre, string desc, int pts)
    {
        Id = id;
        Icono = nombre.Split(" ")[0];
        Nombre = nombre;
        Descripcion = desc;
        PuntosNecesarios = pts;
    }
}