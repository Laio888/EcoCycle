using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Aplicacion.Implementaciones;
using Aplicacion.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace asp_servicios
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration? Configuration { set; get; }

        public void ConfigureServices(WebApplicationBuilder builder, IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(x => { x.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(x => { x.AllowSynchronousIO = true; });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddEndpointsApiExplorer();
            // services.AddSwaggerGen();

            // Repositorio (contexto)
            services.AddScoped<IConexion>(provider =>
            {
                var conexion = new Conexion();
                var connectionString = Configuration?.GetConnectionString("SQLServerConnection") ?? Configuration?["StringConexion"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("No se encontró la cadena de conexión SQLServerConnection en la configuración. Revisa Secrets.json o el archivo appsettings.");
                }
                conexion.StringConexion = connectionString;
                return conexion;
            });

            // Aplicaciones (una por cada entidad de la BD)
            services.AddScoped<IRegistroResiduoAplicacion, RegistroResiduoAplicacion>();
            //services.AddScoped<IUsuariosAplicacion, UsuariosAplicacion>();

            // Controladores se registran automáticamente en AddControllers.
            services.AddCors(o => o.AddDefaultPolicy(b => b.AllowAnyOrigin()));
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // app.UseSwagger();
                // app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            app.UseRouting(); 

            app.UseCors();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}