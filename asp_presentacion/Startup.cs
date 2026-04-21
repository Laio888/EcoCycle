using Presentaciones.Implementaciones;
using Presentaciones.Interfaces;

namespace asp_presentacion
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
            // Presentaciones
            services.AddScoped<IUsuariosPresentacion, UsuariosPresentacion>();
            services.AddScoped<IAuditoriasPresentacion, AuditoriasPresentacion>();

            //services.AddScoped<IUsuariosRolesPresentacion, UsuariosRolesPresentacion>();
            //services.AddScoped<IRolesPresentacion, RolesPresentacion>();
            //services.AddScoped<IUsuariosPermisosPresentacion, UsuariosPermisosPresentacion>();

            // Servicios base
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddRazorPages();
            
            // Configuración de sesión
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.MapRazorPages();
            app.Run();
        }
    }
}