// ============================================
// EcoCycle – PageModel página Puntos y Ranking
// asp_presentacion/Pages/Puntos.cshtml.cs
// ============================================

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcoCycle.asp_presentacion.Pages
{
    // ── DTOs para esta página ──

    public class NivelDto
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int RangoMin { get; set; }
        public int RangoMax { get; set; }
    }

    public class RankingEntryDto
    {
        public int Posicion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Inicial { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "a";   // a–e → clase CSS av-a … av-e
        public string NivelNombre { get; set; } = string.Empty;
        public string BadgeCss { get; set; } = string.Empty; // inter | avanz | exper | princ
        public int Puntos { get; set; }
        public bool EsUsuarioActual { get; set; }
    }

    public class LogroDto
    {
        public string Emoji { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Obtenido { get; set; }
    }

    // ── PageModel ──

    public class PuntosModel : PageModel
    {
        // Navbar
        public int UserPoints { get; private set; }
        public int UserLevel { get; private set; }
        public string LevelName { get; private set; } = string.Empty;

        // Métricas
        public int RankingPosition { get; private set; }
        public int LogrosObtenidos { get; private set; }
        public int LogrosTotal { get; private set; }

        // Progreso
        public int NextLevelPoints { get; private set; }
        public int LevelProgressPercent { get; private set; }

        // Listas
        public List<NivelDto> Niveles { get; private set; } = new();
        public List<RankingEntryDto> Ranking { get; private set; } = new();
        public List<LogroDto> Logros { get; private set; } = new();

        // ── Inyección de servicios (descomentар cuando estén listos) ──
        // private readonly IUsuarioService  _usuarioService;
        // private readonly IRankingService  _rankingService;
        // private readonly ILogroService    _logroService;

        public void OnGet()
        {
            // TODO: reemplazar con llamadas reales a los servicios
            // var usuario = await _usuarioService.ObtenerUsuarioActualAsync(User);

            // ── Datos del usuario ──
            UserPoints = 850;
            UserLevel = 3;
            LevelName = "Intermedio";
            RankingPosition = 4;
            LogrosObtenidos = 3;
            LogrosTotal = 8;
            NextLevelPoints = 3000;
            LevelProgressPercent = (int)((double)UserPoints / NextLevelPoints * 100);

            // ── Niveles ──
            Niveles = new List<NivelDto>
            {
                new() { Numero = 1, Nombre = "Principiante",      RangoMin = 0,    RangoMax = 500  },
                new() { Numero = 2, Nombre = "Aprendiz",          RangoMin = 501,  RangoMax = 1000 },
                new() { Numero = 3, Nombre = "Intermedio",        RangoMin = 1001, RangoMax = 3000 },
                new() { Numero = 4, Nombre = "Avanzado",          RangoMin = 3001, RangoMax = 6000 },
                new() { Numero = 5, Nombre = "Experto Compostador", RangoMin = 6001, RangoMax = 99999 },
            };

            // ── Ranking ──
            Ranking = new List<RankingEntryDto>
            {
                new() { Posicion=1, Nombre="María González",  Inicial="M", AvatarColor="b", NivelNombre="Experto",     BadgeCss="exper", Puntos=6240 },
                new() { Posicion=2, Nombre="Carlos Herrera",  Inicial="C", AvatarColor="c", NivelNombre="Avanzado",    BadgeCss="avanz", Puntos=4820 },
                new() { Posicion=3, Nombre="Laura Martínez",  Inicial="L", AvatarColor="d", NivelNombre="Avanzado",    BadgeCss="avanz", Puntos=3410 },
                new() { Posicion=4, Nombre="Usuario Demo",    Inicial="U", AvatarColor="a", NivelNombre="Intermedio",  BadgeCss="inter", Puntos=850,  EsUsuarioActual=true },
                new() { Posicion=5, Nombre="Andrea López",    Inicial="A", AvatarColor="e", NivelNombre="Intermedio",  BadgeCss="inter", Puntos=720  },
                new() { Posicion=6, Nombre="Juan Pérez",      Inicial="J", AvatarColor="b", NivelNombre="Aprendiz",    BadgeCss="princ", Puntos=490  },
            };

            // ── Logros ──
            Logros = new List<LogroDto>
            {
                new() { Emoji="🌱", Nombre="Primer Paso",         Descripcion="Registra tu primer residuo orgánico",     Obtenido=true  },
                new() { Emoji="🔥", Nombre="Racha de 7 días",     Descripcion="Registra residuos 7 días seguidos",       Obtenido=true  },
                new() { Emoji="📚", Nombre="Aprendiz Verde",      Descripcion="Completa 3 artículos educativos",         Obtenido=true  },
                new() { Emoji="⚖️", Nombre="10 kg Compostados",  Descripcion="Acumula 10 kg en total",                  Obtenido=false },
                new() { Emoji="🏆", Nombre="Top 3",               Descripcion="Llega al top 3 del ranking mensual",     Obtenido=false },
                new() { Emoji="🌍", Nombre="Guardián del Planeta",Descripcion="Evita 50 kg de CO₂",                     Obtenido=false },
                new() { Emoji="🎁", Nombre="Primera Recompensa",  Descripcion="Canjea tu primera recompensa",           Obtenido=false },
                new() { Emoji="⭐", Nombre="Nivel Experto",       Descripcion="Alcanza el nivel 5",                     Obtenido=false },
            };
        }
    }
}