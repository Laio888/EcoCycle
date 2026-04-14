using asp_servicios.Controllers;
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
            services.AddScoped<IConexion, Conexion>();

            // Aplicaciones (una por cada entidad de la BD)
            services.AddScoped<IUsuariosAplicacion, UsuariosAplicacion>();

            // Controladores (si quieres inyectarlos directamente)
            services.AddScoped<TokenController>();
            // (añade aquí otros controladores si necesitas inyectarlos manualmente)

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