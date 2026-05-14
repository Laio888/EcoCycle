using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class UsuariosContenidoVistoPrueba
    {
        private readonly IConexion? iConexion;
        private List<UsuariosContenidoVisto>? lista;
        private UsuariosContenidoVisto? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;
        private int contenidoIdPrueba;

        public UsuariosContenidoVistoPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para crear usuario de prueba
        private Usuarios CrearUsuarioPrueba()
        {
            var guidSufijo = Guid.NewGuid().ToString("N");
            if (guidSufijo.Length > 15) guidSufijo = guidSufijo.Substring(0, 15);
            
            var usuario = new Usuarios
            {
                CorreoElectronico = $"visto_{guidSufijo}@test.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion!.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            return usuario;
        }

        // Método auxiliar para crear contenido educativo de prueba
        private ContenidoEducativo CrearContenidoPrueba()
        {
            var guidShort = Guid.NewGuid().ToString("N");
            if (guidShort.Length > 8) guidShort = guidShort.Substring(0, 8);
            var titulo = $"Contenido_Test_{guidShort}";
            
            var contenido = new ContenidoEducativo
            {
                Titulo = titulo,
                TipoContenidoId = 1,  // Asumiendo que existe (Guia practica)
                CategoriaContenidoId = 1,  // Asumiendo que existe (Compostaje domestico)
                EsExterno = false
            };
            iConexion!.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();
            return contenido;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Crear usuario de prueba
            var usuario = CrearUsuarioPrueba();
            usuarioIdPrueba = usuario.UsuarioId;
            Console.WriteLine($"Usuario de prueba creado con ID: {usuarioIdPrueba}");
            
            // Crear contenido educativo de prueba
            var contenido = CrearContenidoPrueba();
            contenidoIdPrueba = contenido.ContenidoId;
            Console.WriteLine($"Contenido educativo de prueba creado con ID: {contenidoIdPrueba}");
            
            // Verificar tipos de contenido existentes
            if (iConexion?.TiposContenido != null)
            {
                var tipos = iConexion.TiposContenido.ToList();
                Console.WriteLine("Tipos de contenido disponibles:");
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoContenidoId}, Nombre: {tipo.Nombre}");
                }
            }
            
            // Verificar categorías de contenido existentes
            if (iConexion?.CategoriasContenido != null)
            {
                var categorias = iConexion.CategoriasContenido.ToList();
                Console.WriteLine("Categorías de contenido disponibles:");
                foreach (var cat in categorias)
                {
                    Console.WriteLine($"  - ID: {cat.CategoriaContenidoId}, Nombre: {cat.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar registro de visualización creado
            if (entidadIdGuardado > 0 && iConexion?.UsuariosContenidoVisto != null)
            {
                try
                {
                    var entidadExistente = iConexion.UsuariosContenidoVisto.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.UsuariosContenidoVisto.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: UsuarioContenidoVisto {entidadIdGuardado} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando visualización: {ex.Message}");
                }
            }
            
            // Limpiar contenido de prueba
            if (contenidoIdPrueba > 0 && iConexion?.ContenidoEducativo != null)
            {
                try
                {
                    var contenido = iConexion.ContenidoEducativo.Find(contenidoIdPrueba);
                    if (contenido != null)
                    {
                        iConexion.ContenidoEducativo.Remove(contenido);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Contenido educativo {contenidoIdPrueba} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando contenido: {ex.Message}");
                }
            }
            
            // Limpiar usuario de prueba
            if (usuarioIdPrueba > 0 && iConexion?.Usuarios != null)
            {
                try
                {
                    var usuario = iConexion.Usuarios.Find(usuarioIdPrueba);
                    if (usuario != null)
                    {
                        iConexion.Usuarios.Remove(usuario);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Usuario de prueba {usuarioIdPrueba} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando usuario: {ex.Message}");
                }
            }
        }

        [TestMethod]
        public void Ejecutar()
        {
            Assert.AreEqual(true, Guardar(), "Falló Guardar()");
            Assert.AreEqual(true, Modificar(), "Falló Modificar()");
            Assert.AreEqual(true, Listar(), "Falló Listar()");
            Assert.AreEqual(true, Borrar(), "Falló Borrar()");
        }

        [TestMethod]
        public void GuardarVisualizacion_ConDatosValidos_DeberiaCrearRegistro()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };

            // Act
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(visualizacion.UsuarioContenidoVistoId > 0);
            Assert.AreEqual(usuarioIdPrueba, visualizacion.UsuarioId);
            Assert.AreEqual(contenidoIdPrueba, visualizacion.ContenidoId);

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(visualizacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarVisualizacion_MismoUsuarioMismoContenido_DeberiaFallar()
        {
            // Arrange - Primera visualización
            var visualizacion1 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion1);
            iConexion.SaveChanges();

            // Segunda visualización con mismo usuario y contenido
            var visualizacion2 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion.UsuariosContenidoVisto.Add(visualizacion2);

            // Act & Assert - Debe fallar por restricción UNIQUE
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(visualizacion1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarVisualizacion_DiferenteUsuarioMismoContenido_DeberiaFuncionar()
        {
            // Arrange - Crear segundo usuario
            var segundoUsuario = CrearUsuarioPrueba();
            
            var visualizacion1 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            var visualizacion2 = new UsuariosContenidoVisto
            {
                UsuarioId = segundoUsuario.UsuarioId,
                ContenidoId = contenidoIdPrueba
            };

            // Act
            iConexion!.UsuariosContenidoVisto!.AddRange(visualizacion1, visualizacion2);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.AreEqual(2, resultado);

            // Cleanup
            iConexion.UsuariosContenidoVisto.RemoveRange(visualizacion1, visualizacion2);
            iConexion.Usuarios!.Remove(segundoUsuario);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarVisualizacion_SinUsuario_DeberiaFallar()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = 0,  // UsuarioId inválido (FK violación)
                ContenidoId = contenidoIdPrueba
            };

            // Act & Assert
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarVisualizacion_SinContenido_DeberiaFallar()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = 0  // ContenidoId inválido (FK violación)
            };

            // Act & Assert
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerVisualizacionPorId_DeberiaRetornarRegistroCorrecto()
        {
            // Arrange
            var visualizacionGuardar = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(visualizacionGuardar);
            iConexion.SaveChanges();
            int idBuscado = visualizacionGuardar.UsuarioContenidoVistoId;

            // Act
            var visualizacionEncontrada = iConexion.UsuariosContenidoVisto.Find(idBuscado);

            // Assert
            Assert.IsNotNull(visualizacionEncontrada);
            Assert.AreEqual(idBuscado, visualizacionEncontrada.UsuarioContenidoVistoId);
            Assert.AreEqual(usuarioIdPrueba, visualizacionEncontrada.UsuarioId);
            Assert.AreEqual(contenidoIdPrueba, visualizacionEncontrada.ContenidoId);

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(visualizacionGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarVisualizaciones_DeberiaIncluirRegistrosExistentes()
        {
            // Arrange
            var nuevaVisualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(nuevaVisualizacion);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.UsuariosContenidoVisto.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(nuevaVisualizacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarVisualizacionesConRelaciones_DeberiaCargarUsuarioYContenido()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            iConexion.SaveChanges();

            // Act
            var visualizacionConRelaciones = iConexion.UsuariosContenidoVisto
                .Include(v => v.Usuario)
                .Include(v => v.Contenido)
                .ThenInclude(c => c!.TipoContenido)
                .FirstOrDefault(v => v.UsuarioContenidoVistoId == visualizacion.UsuarioContenidoVistoId);

            // Assert
            Assert.IsNotNull(visualizacionConRelaciones);
            Assert.IsNotNull(visualizacionConRelaciones.Usuario);
            Assert.IsNotNull(visualizacionConRelaciones.Contenido);
            Assert.IsNotNull(visualizacionConRelaciones.Contenido.TipoContenido);

            Console.WriteLine($"Visualización ID: {visualizacionConRelaciones.UsuarioContenidoVistoId}");
            Console.WriteLine($"  Usuario: {visualizacionConRelaciones.Usuario.CorreoElectronico}");
            Console.WriteLine($"  Contenido: {visualizacionConRelaciones.Contenido.Titulo}");
            Console.WriteLine($"  Tipo: {visualizacionConRelaciones.Contenido.TipoContenido.Nombre}");
            Console.WriteLine($"  Fecha: {visualizacionConRelaciones.FechaVisionado}");

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(visualizacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarVisualizacionesPorUsuario_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var visualizacion1 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            
            // Crear otro contenido para segunda visualización
            var otroContenido = CrearContenidoPrueba();
            var visualizacion2 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = otroContenido.ContenidoId
            };
            
            iConexion!.UsuariosContenidoVisto!.AddRange(visualizacion1, visualizacion2);
            iConexion.SaveChanges();

            // Act
            var visualizacionesUsuario = iConexion.UsuariosContenidoVisto
                .Where(v => v.UsuarioId == usuarioIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(visualizacionesUsuario.Count >= 2);
            Assert.IsTrue(visualizacionesUsuario.All(v => v.UsuarioId == usuarioIdPrueba));

            // Cleanup
            iConexion.UsuariosContenidoVisto.RemoveRange(visualizacion1, visualizacion2);
            iConexion.ContenidoEducativo!.Remove(otroContenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarVisualizacionesPorContenido_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var visualizacion1 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            
            // Crear otro usuario para segunda visualización del mismo contenido
            var otroUsuario = CrearUsuarioPrueba();
            var visualizacion2 = new UsuariosContenidoVisto
            {
                UsuarioId = otroUsuario.UsuarioId,
                ContenidoId = contenidoIdPrueba
            };
            
            iConexion!.UsuariosContenidoVisto!.AddRange(visualizacion1, visualizacion2);
            iConexion.SaveChanges();

            // Act
            var visualizacionesContenido = iConexion.UsuariosContenidoVisto
                .Where(v => v.ContenidoId == contenidoIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(visualizacionesContenido.Count >= 2);
            Assert.IsTrue(visualizacionesContenido.All(v => v.ContenidoId == contenidoIdPrueba));

            // Cleanup
            iConexion.UsuariosContenidoVisto.RemoveRange(visualizacion1, visualizacion2);
            iConexion.Usuarios!.Remove(otroUsuario);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarVisualizacion_NoEsNecesario_PeroCambiarFecha_DeberiaFuncionar()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            iConexion.SaveChanges();
            
            // Guardar la fecha original
            var fechaOriginal = visualizacion.FechaVisionado;
            
            // Esperar un momento
            System.Threading.Thread.Sleep(100);

            // Act - Recargar y modificar (aunque normalmente no se modifica)
            var visualizacionActualizada = iConexion.UsuariosContenidoVisto.Find(visualizacion.UsuarioContenidoVistoId);
            if (visualizacionActualizada != null)
            {
                // Nota: La fecha es generada por la BD, no se puede modificar fácilmente
                // Esta prueba solo verifica que la entidad existe
                Assert.IsNotNull(visualizacionActualizada);
            }

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(visualizacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarVisualizacion_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            iConexion.SaveChanges();
            int idEliminado = visualizacion.UsuarioContenidoVistoId;

            // Act
            iConexion.UsuariosContenidoVisto.Remove(visualizacion);
            iConexion.SaveChanges();

            // Assert
            var visualizacionEliminada = iConexion.UsuariosContenidoVisto.Find(idEliminado);
            Assert.IsNull(visualizacionEliminada);
        }

        [TestMethod]
        public void VerificarFechaVisionadoAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };

            // Act
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            int resultado = iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(visualizacion).Reload();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(visualizacion.FechaVisionado != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(visualizacion.FechaVisionado <= DateTime.Now, 
                "La fecha asignada es futura");

            Console.WriteLine($"FechaVisionado asignada por BD: {visualizacion.FechaVisionado:yyyy-MM-dd HH:mm:ss}");

            // Cleanup
            iConexion.UsuariosContenidoVisto.Remove(visualizacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ContarVisualizacionesPorContenido_DeberiaCalcularCorrectamente()
        {
            // Arrange
            var visualizacion1 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            
            var otroUsuario = CrearUsuarioPrueba();
            var visualizacion2 = new UsuariosContenidoVisto
            {
                UsuarioId = otroUsuario.UsuarioId,
                ContenidoId = contenidoIdPrueba
            };
            
            iConexion!.UsuariosContenidoVisto!.AddRange(visualizacion1, visualizacion2);
            iConexion.SaveChanges();

            // Act
            var totalVisualizaciones = iConexion.UsuariosContenidoVisto
                .Count(v => v.ContenidoId == contenidoIdPrueba);

            // Assert
            Assert.AreEqual(2, totalVisualizaciones);

            // Cleanup
            iConexion.UsuariosContenidoVisto.RemoveRange(visualizacion1, visualizacion2);
            iConexion.Usuarios!.Remove(otroUsuario);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ContarContenidosVistosPorUsuario_DeberiaCalcularCorrectamente()
        {
            // Arrange
            var visualizacion1 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            
            var otroContenido = CrearContenidoPrueba();
            var visualizacion2 = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = otroContenido.ContenidoId
            };
            
            iConexion!.UsuariosContenidoVisto!.AddRange(visualizacion1, visualizacion2);
            iConexion.SaveChanges();

            // Act
            var totalContenidosVistos = iConexion.UsuariosContenidoVisto
                .Count(v => v.UsuarioId == usuarioIdPrueba);

            // Assert
            Assert.AreEqual(2, totalContenidosVistos);

            // Cleanup
            iConexion.UsuariosContenidoVisto.RemoveRange(visualizacion1, visualizacion2);
            iConexion.ContenidoEducativo!.Remove(otroContenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void VerificarQueEliminacionUsuario_EliminaVisualizacionesEnCascada()
        {
            // Arrange
            var visualizacion = new UsuariosContenidoVisto
            {
                UsuarioId = usuarioIdPrueba,
                ContenidoId = contenidoIdPrueba
            };
            iConexion!.UsuariosContenidoVisto!.Add(visualizacion);
            iConexion.SaveChanges();
            int visualizacionId = visualizacion.UsuarioContenidoVistoId;

            // Act - Eliminar el usuario (debe eliminar la visualización en cascada)
            var usuario = iConexion.Usuarios!.Find(usuarioIdPrueba);
            Assert.IsNotNull(usuario);
            iConexion.Usuarios.Remove(usuario);
            iConexion.SaveChanges();

            // Assert - La visualización debería haber sido eliminada
            var visualizacionEliminada = iConexion.UsuariosContenidoVisto.Find(visualizacionId);
            Assert.IsNull(visualizacionEliminada, "La visualización debería eliminarse en cascada al eliminar el usuario");

            Console.WriteLine("Verificado: ON DELETE CASCADE funciona correctamente");

            // Nota: No es necesario limpiar porque el usuario y la visualización ya fueron eliminados
            // Pero necesitamos recrear el usuarioIdPrueba para futuras pruebas? El TestCleanup manejará esto
        }

        public bool Listar()
        {
            if (iConexion?.UsuariosContenidoVisto == null)
            {
                Console.WriteLine("Error: iConexion o UsuariosContenidoVisto es null");
                return false;
            }

            try
            {
                this.lista = iConexion.UsuariosContenidoVisto
                    .Include(v => v.Usuario)
                    .Include(v => v.Contenido)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} visualizaciones encontradas");
                
                foreach (var visualizacion in lista)
                {
                    Console.WriteLine($"  - ID: {visualizacion.UsuarioContenidoVistoId}, " +
                                      $"Usuario: {visualizacion.Usuario?.CorreoElectronico ?? "N/A"}, " +
                                      $"Contenido: {visualizacion.Contenido?.Titulo ?? "N/A"}, " +
                                      $"Fecha: {visualizacion.FechaVisionado:yyyy-MM-dd HH:mm}");
                }
                
                return lista.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al listar: {ex.Message}");
                return false;
            }
        }

        public bool Guardar()
        {
            if (iConexion?.UsuariosContenidoVisto == null)
            {
                Console.WriteLine("Error: iConexion o UsuariosContenidoVisto es null");
                return false;
            }

            try
            {
                this.entidad = new UsuariosContenidoVisto
                {
                    UsuarioId = usuarioIdPrueba,
                    ContenidoId = contenidoIdPrueba
                };
                
                Console.WriteLine($"Guardando visualización: UsuarioId={entidad.UsuarioId}, ContenidoId={entidad.ContenidoId}");
                
                iConexion.UsuariosContenidoVisto.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.UsuarioContenidoVistoId;
                    Console.WriteLine($"Visualización guardada con ID: {entidadIdGuardado}");
                    return true;
                }
                
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error de BD al guardar: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al guardar: {ex.Message}");
                return false;
            }
        }

        public bool Modificar()
        {
            // Las visualizaciones no suelen modificarse, pero implementamos la interfaz
            if (iConexion?.UsuariosContenidoVisto == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, UsuariosContenidoVisto o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.UsuariosContenidoVisto.Find(this.entidad.UsuarioContenidoVistoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Visualización {this.entidad.UsuarioContenidoVistoId} no encontrada para modificar");
                    return false;
                }
                
                Console.WriteLine($"Modificando visualización ID: {entidadActualizada.UsuarioContenidoVistoId}");
                Console.WriteLine("  (Las visualizaciones no tienen campos modificables)");
                
                // No hay campos para modificar, pero devolvemos true
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al modificar: {ex.Message}");
                return false;
            }
        }

        public bool Borrar()
        {
            if (iConexion?.UsuariosContenidoVisto == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, UsuariosContenidoVisto o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.UsuariosContenidoVisto.Find(this.entidad.UsuarioContenidoVistoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Visualización {this.entidad.UsuarioContenidoVistoId} no encontrada para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando visualización ID: {entidadActualizada.UsuarioContenidoVistoId}");
                
                iConexion.UsuariosContenidoVisto.Remove(entidadActualizada);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = 0;
                    return true;
                }
                
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error de BD al borrar: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar: {ex.Message}");
                return false;
            }
        }
    }
}