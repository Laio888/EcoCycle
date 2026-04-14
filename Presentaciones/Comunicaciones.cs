using Dominio.Nucleo;
using System.Text;

namespace Presentaciones
{
    public class Comunicaciones
    {
        private static readonly HttpClient _httpClient = new HttpClient() { Timeout = new TimeSpan(0, 4, 0) };

        private string? URL = string.Empty;
        private string? llave = null;

        public Comunicaciones(string url = "http://localhost:5100")
        {
            URL = url.EndsWith("/") ? url : url + "/";
        }

        // Mantener compatibilidad: Url (principal) y UrlLlave / UrlToken
        public Dictionary<string, object> ConstruirUrl(Dictionary<string, object> data, string Metodo)
        {
            data["Url"] = URL + Metodo;
            data["UrlLlave"] = URL + "Token/Llave";
            data["UrlToken"] = URL + "Token/Autenticar"; // compatibilidad con otros proyectos
            return data;
        }

        // Método existente (lógica)
        public async Task<Dictionary<string, object>> Ejecutar(Dictionary<string, object> datos)
        {
            var respuesta = new Dictionary<string, object>();
            try
            {
                // Obtener llave/token
                var llaveResp = await Llave(datos);
                if (llaveResp == null || llaveResp.ContainsKey("Error"))
                    return llaveResp ?? new Dictionary<string, object> { ["Error"] = "lbErrorLlave" };

                //  Nuevo: pedir token JWT
                var tokenResp = await Token(datos);
                if (tokenResp == null || tokenResp.ContainsKey("Error"))
                    return tokenResp ?? new Dictionary<string, object> { ["Error"] = "lbErrorToken" };
                // final

                // limpiar respuesta temporal
                respuesta.Clear();

                if (!datos.TryGetValue("Url", out var urlObj) || urlObj == null)
                    return new Dictionary<string, object> { ["Error"] = "lbUrlNoProporcionada" };

                var url = urlObj.ToString() ?? string.Empty;

                // preparar payload
                datos.Remove("Url");
                datos.Remove("UrlLlave");
                datos.Remove("UrlToken");
                datos["Llave"] = llave ?? string.Empty;

                var stringData = JsonConversor.ConvertirAString(datos);

                using var content = new StringContent(stringData, Encoding.UTF8, "application/json");
                //var message = await _httpClient.PostAsync(url, content);
                using var message = await _httpClient.PostAsync(url, content);

                //cambiado (if (!message.IsSuccessStatusCode)
                //return new Dictionary<string, object> { ["Error"] = "lbErrorComunicacion", ["Status"] = (int)message.StatusCode };)
                
                if (!message.IsSuccessStatusCode)
                {
                    return new Dictionary<string, object>
                    {
                        ["Error"] = "lbErrorComunicacion",
                        ["Status"] = (int)message.StatusCode,
                        ["Mensaje"] = await message.Content.ReadAsStringAsync()
                    };
                }

                var resp = await message.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(resp))
                    return new Dictionary<string, object> { ["Error"] = "lbRespuestaVacia" };

                resp = Replace(resp);

                //cambiado
                // try
                // {
                //    respuesta = JsonConversor.ConvertirAObjeto(resp);
                //}
                //catch (Exception ex)
                //{
                //    return new Dictionary<string, object> { ["Error"] = "lbErrorParseRespuesta", ["Detalle"] = ex.Message, ["Raw"] = resp };
                //}

                try
                {
                    respuesta = JsonConversor.ConvertirAObjeto(resp);
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object>
                    {
                        ["Error"] = "lbErrorParseRespuesta",
                        ["Detalle"] = ex.Message,
                        ["Raw"] = resp
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["Error"] = "lbExcepcionComunicacion", ["Detalle"] = ex.ToString() };
            }
        }

        // ---- ADAPTADOR: Execute -> Ejecutar (compatibilidad)
        public Task<Dictionary<string, object>> Execute(Dictionary<string, object> datos)
        {
            // solo forward a Ejecutar para que el resto del código no falle
            return Ejecutar(datos);
        }

        private async Task<Dictionary<string, object>> Llave(Dictionary<string, object> datos)
        {
            var respuesta = new Dictionary<string, object>();
            try
            {
                if (!datos.TryGetValue("UrlLlave", out var urlObj) || urlObj == null)
                    return new Dictionary<string, object> { ["Error"] = "lbUrlLlaveNoProporcionada" };

                var url = urlObj.ToString() ?? string.Empty;

                var temp = new Dictionary<string, object>
                {
                    ["Entidad"] = new Dictionary<string, object>()
                    {
                        { "Nombre", "Pepito@email.com" },
                        { "Contraseña", "JHGjkhtu6387456yssdf" }
                    }
                };

                var stringData = JsonConversor.ConvertirAString(temp);
                using var content = new StringContent(stringData, Encoding.UTF8, "application/json");
                var mensaje = await _httpClient.PostAsync(url, content);

                if (!mensaje.IsSuccessStatusCode)
                    return new Dictionary<string, object> { ["Error"] = "lbErrorComunicacion", ["Status"] = (int)mensaje.StatusCode };

                var resp = await mensaje.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(resp))
                    return new Dictionary<string, object> { ["Error"] = "lbRespuestaVacia" };

                resp = Replace(resp);
                try
                {
                    respuesta = JsonConversor.ConvertirAObjeto(resp);
                }
                catch (Exception ex)
                {
                    return new Dictionary<string, object> { ["Error"] = "lbErrorParseLlave", ["Detalle"] = ex.Message, ["Raw"] = resp };
                }

                if (respuesta.TryGetValue("Llave", out var llaveObj) && llaveObj != null)
                    llave = llaveObj.ToString();

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["Error"] = "lbExcepcionLlave", ["Detalle"] = ex.ToString() };
            }
        }

        // inicio
            private async Task<Dictionary<string, object>> Token(Dictionary<string, object> datos)
        {
            var respuesta = new Dictionary<string, object>();
            try
            {
                if (!datos.TryGetValue("UrlToken", out var urlObj) || urlObj == null)
                    return new Dictionary<string, object> { ["Error"] = "lbUrlTokenNoProporcionada" };

                var url = urlObj.ToString() ?? string.Empty;

                var payload = new Dictionary<string, object>()
        {
            { "Usuario", DatosGenerales.usuario_datos }  // debe coincidir con backend
        };

                var stringData = JsonConversor.ConvertirAString(payload);
                using var content = new StringContent(stringData, Encoding.UTF8, "application/json");

                var mensaje = await _httpClient.PostAsync(url, content);

                if (!mensaje.IsSuccessStatusCode)
                    return new Dictionary<string, object> { ["Error"] = "lbErrorComunicacionToken", ["Status"] = (int)mensaje.StatusCode };

                var resp = await mensaje.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(resp))
                    return new Dictionary<string, object> { ["Error"] = "lbRespuestaVaciaToken" };

                resp = Replace(resp);
                respuesta = JsonConversor.ConvertirAObjeto(resp);

                if (respuesta.TryGetValue("Token", out var tokenObj) && tokenObj != null)
                    datos["Bearer"] = "Bearer " + tokenObj.ToString();

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["Error"] = "lbExcepcionToken", ["Detalle"] = ex.ToString() };
            }
        }
        // final

        /*
         * {
         * "Entidad": {
         * "Nombre": "Pepito@email.com",
         * "Contraseña": "JHGjkhtu6387456yssdf"
         * }
         * }
         */

        private string Replace(string resp)
        {
            if (resp == null) return string.Empty;
            // la "mutilación"
            return resp.Replace("\\\\r\\\\n", "")
                       .Replace("\\r\\n", "")
                       .Replace("\\", "")
                       .Replace("\\\"", "\"")
                       .Replace("\"", "'")
                       .Replace("'[", "[")
                       .Replace("]'", "]")
                       .Replace("'{", "{'")
                       .Replace("\\\\", "\\")
                       .Replace("'}'", "'}")
                       .Replace("}'", "}")
                       .Replace("\\n", "")
                       .Replace("\\r", "")
                       .Replace(" ", "")
                       .Replace("'{", "{")
                       .Replace("\"", "")
                       .Replace(" ", "")
                       .Replace("null", "''");
        }
    }
}