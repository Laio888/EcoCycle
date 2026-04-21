using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel : PageModel
{
    public int Puntos { get; set; } = 1450;
    public int Nivel { get; set; } = 3;
    public int Ranking { get; set; } = 4;
    public double CO2Reducido { get; set; } = 45.8;

    public List<string> Meses { get; set; } = new();
    public List<int> KgMensual { get; set; } = new();
    public List<int> PuntosMensual { get; set; } = new();
    public List<int> CO2Mensual { get; set; } = new();

    public List<Registro> Historial { get; set; } = new();

    public void OnGet()
    {
        Meses = new() { "Ene", "Feb", "Mar", "Abr", "May", "Jun" };

        KgMensual = new() { 5, 10, 15, 20, 22, 25 };
        PuntosMensual = new() { 200, 250, 300, 350, 400, 450 };
        CO2Mensual = new() { 5, 8, 10, 14, 17, 20 };

        var hoy = DateTime.Now;

        Historial = new List<Registro>
        {
            new Registro(hoy, "Orgánico", 2.8, "Alta"),
            new Registro(hoy.AddDays(-3), "Papel", 2.1, "Alta"),
            new Registro(hoy.AddDays(-4), "Orgánico", 1.9, "Baja"),
            new Registro(hoy.AddDays(-7), "Papel", 2.9, "Alta")
        };
    }
}

public class Registro
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public double Peso { get; set; }
    public string Calidad { get; set; } = string.Empty;

    public Registro(DateTime fecha, string tipo, double peso, string calidad)
    {
        Fecha = fecha;
        Tipo = tipo;
        Peso = peso;
        Calidad = calidad;
    }
}