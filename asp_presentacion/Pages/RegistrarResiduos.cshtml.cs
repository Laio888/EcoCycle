using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegistrarResiduoModel : PageModel
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

    public void OnPost()
    {
        Puntos = 0.0;
        ResidOrg ="";
        Puntos_por_kg = 0;
        CalidadRes = "";
        NumCalidad = 1.0;
    
        if (Peso > 0)
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

                case "pscafe":
                    Puntos_por_kg = 15;
                    break;

                case "blTe":
                    Puntos_por_kg = 12;
                    break;

                case "rdjardin":
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
                case "calidadAlta":
                    NumCalidad = 1.8;
                    break;

                case "calidadMedia":
                    NumCalidad = 1.4;
                    break;

                default:
                    NumCalidad = 1.0;
                    break;
            }

            Puntos = Puntos_por_kg * Peso * NumCalidad
        ;
    }
}