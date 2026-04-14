using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Dominio.Nucleo;
using Dominio.Entidades;
using Aplicacion.Interfaces;

namespace asp_servicios.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TokenController : ControllerBase
    {
        private Dictionary<string, object> ObtenerDatos()
        {
            var respuesta = new Dictionary<string, object>();
            try
            {
                using var reader = new StreamReader(Request.Body);
                var datos = reader.ReadToEnd();
                if (string.IsNullOrEmpty(datos))
                    datos = "{}";

                return JsonConversor.ConvertirAObjeto(datos);
            }
            catch (Exception ex)
            {
                respuesta["Error"] = ex.Message;
                return respuesta;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public string Autenticar()
        {
            var respuesta = new Dictionary<string, object>();
            try
            {
                var datos = ObtenerDatos();

                if (!datos.ContainsKey("Usuario") || datos["Usuario"].ToString()! != DatosGenerales.usuario_datos)
                {
                    respuesta["Error"] = "lbNoAutenticacion";
                    return JsonConversor.ConvertirAString(respuesta);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(DatosGenerales.clave);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, datos["Usuario"].ToString()!) }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                respuesta["Token"] = tokenHandler.WriteToken(token);

                return JsonConversor.ConvertirAString(respuesta);
            }
            catch (Exception ex)
            {
                respuesta["Error"] = ex.Message;
                return JsonConversor.ConvertirAString(respuesta);
            }
        }

        public bool Validate(Dictionary<string, object> data)
        {
            try
            {
                if (!data.ContainsKey("Bearer"))
                    return false;

                var authorizationHeader = data["Bearer"].ToString();
                if (string.IsNullOrEmpty(authorizationHeader))
                    return false;

                authorizationHeader = authorizationHeader.Replace("Bearer ", "");

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DatosGenerales.clave)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(authorizationHeader, validationParams, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public string Llave()
        {
            var respuesta = new Dictionary<string, object>();
            try
            {
                var datos = ObtenerDatos();

                if (!datos.ContainsKey("Entidad"))
                    throw new Exception("Falta el objeto Entidad en el cuerpo.");

                var entidad = JsonConversor.ConvertirAObjeto<Usuarios>(
                    JsonConversor.ConvertirAString(datos["Entidad"]));

                if (entidad.Nombre != "Pepito@email.com" || entidad.Contraseña != "JHGjkhtu6387456yssdf")
                    throw new Exception("Credenciales inválidas.");

                var llave = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{entidad.Nombre}:{DateTime.Now}"));
                respuesta["Llave"] = llave;
                respuesta["Respuesta"] = "OK";
                respuesta["Fecha"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                return JsonConversor.ConvertirAString(respuesta);
            }
            catch (Exception ex)
            {
                respuesta["Error"] = ex.Message;
                respuesta["Respuesta"] = "Error";
                return JsonConversor.ConvertirAString(respuesta);
            }
        }
    }
}

/*
 * Si l usuario es vacio, devuelve un error
*/