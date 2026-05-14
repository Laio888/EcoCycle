using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class NotificacionesPrueba
    {
        private readonly IConexion? iConexion;
        private List<Notificaciones>? lista;
        private Notificaciones? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;
        private int tipoNotificacionIdPrueba;

        public NotificacionesPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar mensaje seguro (máx 500 caracteres)
        private string GenerarMensajeSeguro(string prefijo, int maxLength = 500)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var mensaje = $"{prefijo}_{guid} - Esta es una notificación de prueba para el sistema Ecocycle.";
            if (mensaje.Length > maxLength)
            {
                mensaje = mensaje.Substring(0, maxLength);
            }
            return mensaje;
        }

        // Método auxiliar para crear usuario de prueba
        private Usuarios CrearUsuarioPrueba()
        {
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 15);
            var usuario = new Usuarios
            {
                CorreoElectronico = $"notif_{guidSufijo}@test.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion!.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            return usuario;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Crear usuario de prueba
            var usuario = CrearUsuarioPrueba();
            usuarioIdPrueba = usuario.UsuarioId;
            Console.WriteLine($"Usuario de prueba creado con ID: {usuarioIdPrueba}");
            
            // Obtener TipoNotificacion válido (1=Recordatorio, 2=Logro, 3=RecompensaDisponible)
            if (iConexion?.TiposNotificacion != null)
            {
                var tipoExistente = iConexion.TiposNotificacion.FirstOrDefault();
                if (tipoExistente != null)
                {
                    tipoNotificacionIdPrueba = tipoExistente.TipoNotificacionId;
                    Console.WriteLine($"Usando TipoNotificacion existente ID: {tipoNotificacionIdPrueba}, Nombre: {tipoExistente.Nombre}");
                }
                else
                {
                    // Crear un tipo de notificación de prueba
                    var nuevoTipo = new TiposNotificacion { Nombre = $"Tipo_Test_{Guid.NewGuid():N}".Substring(0, 50) };
                    iConexion.TiposNotificacion.Add(nuevoTipo);
                    iConexion.SaveChanges();
                    tipoNotificacionIdPrueba = nuevoTipo.TipoNotificacionId;
                    Console.WriteLine($"TipoNotificacion creado para prueba ID: {tipoNotificacionIdPrueba}");
                }
            }
            
            // Mostrar tipos de notificación existentes
            if (iConexion?.TiposNotificacion != null)
            {
                var tipos = iConexion.TiposNotificacion.ToList();
                Console.WriteLine("Tipos de notificación disponibles:");
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoNotificacionId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar notificación creada
            if (entidadIdGuardado > 0 && iConexion?.Notificaciones != null)
            {
                try
                {
                    var entidadExistente = iConexion.Notificaciones.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.Notificaciones.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: Notificacion {entidadIdGuardado} eliminada");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando notificación: {ex.Message}");
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
        public void GuardarNotificacion_Recordatorio_ConDatosValidos_DeberiaCrearNotificacion()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Recordatorio"),
                Leida = false
            };

            // Act
            iConexion!.Notificaciones!.Add(notificacion);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(notificacion.NotificacionId > 0);
            Assert.IsFalse(notificacion.Leida);
            Assert.IsNotNull(notificacion.Mensaje);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarNotificacion_Logro_ConDatosValidos_DeberiaCrearNotificacion()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("¡Felicidades! Has desbloqueado el nivel Experto"),
                Leida = false
            };

            // Act
            iConexion!.Notificaciones!.Add(notificacion);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(notificacion.NotificacionId > 0);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarNotificacion_RecompensaDisponible_ConDatosValidos_DeberiaCrearNotificacion()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("¡Nueva recompensa disponible! Canjea tus puntos"),
                Leida = false
            };

            // Act
            iConexion!.Notificaciones!.Add(notificacion);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(notificacion.NotificacionId > 0);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarNotificacion_MarcadaComoLeida_DeberiaCrearConLeidaTrue()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Leida"),
                Leida = true
            };

            // Act
            iConexion!.Notificaciones!.Add(notificacion);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(notificacion.Leida);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarNotificacion_SinMensaje_DeberiaFallar()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = null!,  // Mensaje requerido
                Leida = false
            };

            // Act & Assert
            iConexion!.Notificaciones!.Add(notificacion);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarNotificacion_ConMensajeMuyLargo_DeberiaFallar()
        {
            // Arrange
            var mensajeLargo = new string('A', 600); // 600 caracteres, excede el límite de 500
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = mensajeLargo,
                Leida = false
            };

            // Act & Assert
            iConexion!.Notificaciones!.Add(notificacion);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerNotificacionPorId_DeberiaRetornarNotificacionCorrecta()
        {
            // Arrange
            var notificacionGuardar = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Buscar"),
                Leida = false
            };
            iConexion!.Notificaciones!.Add(notificacionGuardar);
            iConexion.SaveChanges();
            int idBuscado = notificacionGuardar.NotificacionId;

            // Act
            var notificacionEncontrada = iConexion.Notificaciones.Find(idBuscado);

            // Assert
            Assert.IsNotNull(notificacionEncontrada);
            Assert.AreEqual(idBuscado, notificacionEncontrada.NotificacionId);
            Assert.AreEqual(notificacionGuardar.Mensaje, notificacionEncontrada.Mensaje);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacionGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarNotificaciones_DeberiaIncluirNotificacionesExistentes()
        {
            // Arrange
            var nuevaNotificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Listar"),
                Leida = false
            };
            iConexion!.Notificaciones!.Add(nuevaNotificacion);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.Notificaciones.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.Notificaciones.Remove(nuevaNotificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarNotificacionesConRelaciones_DeberiaCargarUsuarioYTipo()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Relaciones"),
                Leida = false
            };
            iConexion!.Notificaciones!.Add(notificacion);
            iConexion.SaveChanges();

            // Act
            var notificacionConRelaciones = iConexion.Notificaciones
                .Include(n => n.Usuario)
                .Include(n => n.TipoNotificacion)
                .FirstOrDefault(n => n.NotificacionId == notificacion.NotificacionId);

            // Assert
            Assert.IsNotNull(notificacionConRelaciones);
            Assert.IsNotNull(notificacionConRelaciones.Usuario);
            Assert.IsNotNull(notificacionConRelaciones.TipoNotificacion);

            Console.WriteLine($"Notificación: {notificacionConRelaciones.Mensaje}");
            Console.WriteLine($"  Usuario: {notificacionConRelaciones.Usuario.CorreoElectronico}");
            Console.WriteLine($"  Tipo: {notificacionConRelaciones.TipoNotificacion.Nombre}");
            Console.WriteLine($"  Leída: {notificacionConRelaciones.Leida}");
            Console.WriteLine($"  Fecha: {notificacionConRelaciones.FechaEnvio}");

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarNotificacionesPorUsuario_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var notificacion1 = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Usuario1"),
                Leida = false
            };
            var notificacion2 = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Usuario2"),
                Leida = false
            };
            iConexion!.Notificaciones!.AddRange(notificacion1, notificacion2);
            iConexion.SaveChanges();

            // Act
            var notificacionesUsuario = iConexion.Notificaciones
                .Where(n => n.UsuarioId == usuarioIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(notificacionesUsuario.Count >= 2);
            Assert.IsTrue(notificacionesUsuario.All(n => n.UsuarioId == usuarioIdPrueba));

            // Cleanup
            iConexion.Notificaciones.RemoveRange(notificacion1, notificacion2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarNotificacionesNoLeidas_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var notificacionNoLeida = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_NoLeida"),
                Leida = false
            };
            var notificacionLeida = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Leida"),
                Leida = true
            };
            iConexion!.Notificaciones!.AddRange(notificacionNoLeida, notificacionLeida);
            iConexion.SaveChanges();

            // Act
            var notificacionesNoLeidas = iConexion.Notificaciones
                .Where(n => n.Leida == false)
                .ToList();

            // Assert
            Assert.IsTrue(notificacionesNoLeidas.Count >= 1);
            Assert.IsTrue(notificacionesNoLeidas.All(n => n.Leida == false));

            // Cleanup
            iConexion.Notificaciones.RemoveRange(notificacionNoLeida, notificacionLeida);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarNotificacion_MarcarComoLeida_DeberiaActualizar()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Original"),
                Leida = false
            };
            iConexion!.Notificaciones!.Add(notificacion);
            iConexion.SaveChanges();

            // Act
            notificacion.Leida = true;
            var entry = iConexion.Entry(notificacion);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var notificacionActualizada = iConexion.Notificaciones.Find(notificacion.NotificacionId);
            Assert.IsNotNull(notificacionActualizada);
            Assert.IsTrue(notificacionActualizada.Leida);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarNotificacion_CambiarMensaje_DeberiaActualizar()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Original"),
                Leida = false
            };
            iConexion!.Notificaciones!.Add(notificacion);
            iConexion.SaveChanges();
            string nuevoMensaje = GenerarMensajeSeguro("Notificacion_Modificada");

            // Act
            notificacion.Mensaje = nuevoMensaje;
            var entry = iConexion.Entry(notificacion);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var notificacionActualizada = iConexion.Notificaciones.Find(notificacion.NotificacionId);
            Assert.IsNotNull(notificacionActualizada);
            Assert.AreEqual(nuevoMensaje, notificacionActualizada.Mensaje);

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void MarcarTodasComoLeidas_ParaUnUsuario_DeberiaActualizar()
        {
            // Arrange
            var notificacion1 = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Marcar1"),
                Leida = false
            };
            var notificacion2 = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Marcar2"),
                Leida = false
            };
            iConexion!.Notificaciones!.AddRange(notificacion1, notificacion2);
            iConexion.SaveChanges();

            // Obtener los IDs de las notificaciones recién creadas
            var idsNotificaciones = new[] { notificacion1.NotificacionId, notificacion2.NotificacionId };
            
            // Act - Actualizar directamente en la base de datos usando SQL
            foreach (var id in idsNotificaciones)
            {
                var notif = iConexion.Notificaciones.Find(id);
                if (notif != null)
                {
                    notif.Leida = true;
                    iConexion.Entry(notif).State = EntityState.Modified;
                }
            }
            iConexion.SaveChanges();

            // Assert - Verificar que todas están marcadas como leídas
            var todasLeidas = iConexion.Notificaciones
                .Where(n => idsNotificaciones.Contains(n.NotificacionId))
                .All(n => n.Leida == true);
            
            Assert.IsTrue(todasLeidas, "No todas las notificaciones fueron marcadas como leídas");

            // Cleanup
            iConexion.Notificaciones.RemoveRange(notificacion1, notificacion2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarNotificacion_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Eliminar"),
                Leida = false
            };
            iConexion!.Notificaciones!.Add(notificacion);
            iConexion.SaveChanges();
            int idEliminado = notificacion.NotificacionId;

            // Act
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();

            // Assert
            var notificacionEliminada = iConexion.Notificaciones.Find(idEliminado);
            Assert.IsNull(notificacionEliminada);
        }

        [TestMethod]
        public void VerificarFechaEnvioAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var notificacion = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notificacion_Fecha"),
                Leida = false
            };

            // Act
            iConexion!.Notificaciones!.Add(notificacion);
            iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(notificacion).Reload();

            // Assert
            Assert.IsTrue(notificacion.FechaEnvio != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(notificacion.FechaEnvio <= DateTime.Now, 
                "La fecha asignada es futura");

            Console.WriteLine($"FechaEnvio asignada por BD: {notificacion.FechaEnvio:yyyy-MM-dd HH:mm:ss}");

            // Cleanup
            iConexion.Notificaciones.Remove(notificacion);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void VerificarTiposNotificacionIniciales_DeberianExistirRecordatorioLogroRecompensa()
        {
            if (iConexion?.TiposNotificacion == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tipos = iConexion.TiposNotificacion.ToList();

            // Assert
            Assert.IsTrue(tipos.Count >= 3, "Deberían existir al menos 3 tipos de notificación del script inicial");
            
            var nombresEsperados = new[] { "Recordatorio", "Logro", "RecompensaDisponible" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(tipos.Any(t => t.Nombre == nombre), 
                    $"No existe el tipo de notificación '{nombre}'");
            }

            Console.WriteLine("Tipos de notificación iniciales encontrados:");
            foreach (var tipo in tipos.Where(t => nombresEsperados.Contains(t.Nombre)))
            {
                Console.WriteLine($"  - ID: {tipo.TipoNotificacionId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void ContarNotificacionesNoLeidasPorUsuario_DeberiaCalcularCorrectamente()
        {
            // Arrange
            var notificacionNoLeida1 = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notif_NoLeida1"),
                Leida = false
            };
            var notificacionNoLeida2 = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notif_NoLeida2"),
                Leida = false
            };
            var notificacionLeida = new Notificaciones
            {
                UsuarioId = usuarioIdPrueba,
                TipoNotificacionId = tipoNotificacionIdPrueba,
                Mensaje = GenerarMensajeSeguro("Notif_Leida"),
                Leida = true
            };
            iConexion!.Notificaciones!.AddRange(notificacionNoLeida1, notificacionNoLeida2, notificacionLeida);
            iConexion.SaveChanges();

            // Act
            var totalNoLeidas = iConexion.Notificaciones
                .Count(n => n.UsuarioId == usuarioIdPrueba && n.Leida == false);

            // Assert
            Assert.AreEqual(2, totalNoLeidas);

            // Cleanup
            iConexion.Notificaciones.RemoveRange(notificacionNoLeida1, notificacionNoLeida2, notificacionLeida);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.Notificaciones == null)
            {
                Console.WriteLine("Error: iConexion o Notificaciones es null");
                return false;
            }

            try
            {
                this.lista = iConexion.Notificaciones
                    .Include(n => n.Usuario)
                    .Include(n => n.TipoNotificacion)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} notificaciones encontradas");
                
                foreach (var notificacion in lista)
                {
                    Console.WriteLine($"  - ID: {notificacion.NotificacionId}, " +
                                      $"Usuario: {notificacion.Usuario?.CorreoElectronico ?? "N/A"}, " +
                                      $"Tipo: {notificacion.TipoNotificacion?.Nombre ?? "N/A"}, " +
                                      $"Mensaje: {notificacion.Mensaje?[..Math.Min(50, notificacion.Mensaje?.Length ?? 0)]}..., " +
                                      $"Leída: {notificacion.Leida}, " +
                                      $"Fecha: {notificacion.FechaEnvio:yyyy-MM-dd HH:mm}");
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
            if (iConexion?.Notificaciones == null)
            {
                Console.WriteLine("Error: iConexion o Notificaciones es null");
                return false;
            }

            try
            {
                this.entidad = new Notificaciones
                {
                    UsuarioId = usuarioIdPrueba,
                    TipoNotificacionId = tipoNotificacionIdPrueba,
                    Mensaje = GenerarMensajeSeguro("Notificacion_Prueba"),
                    Leida = false
                };
                
                Console.WriteLine($"Guardando notificación: {entidad.Mensaje}");
                
                iConexion.Notificaciones.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.NotificacionId;
                    Console.WriteLine($"Notificación guardada con ID: {entidadIdGuardado}");
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
            if (iConexion?.Notificaciones == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Notificaciones o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.Notificaciones.Find(this.entidad.NotificacionId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Notificación {this.entidad.NotificacionId} no encontrada para modificar");
                    return false;
                }
                
                entidadActualizada.Leida = true;
                
                Console.WriteLine($"Modificando notificación ID: {entidadActualizada.NotificacionId}");
                Console.WriteLine($"  Marcada como leída: {entidadActualizada.Leida}");
                
                var entry = iConexion.Entry(entidadActualizada);
                entry.State = EntityState.Modified;
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    this.entidad = entidadActualizada;
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al modificar: {ex.Message}");
                return false;
            }
        }

        public bool Borrar()
        {
            if (iConexion?.Notificaciones == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Notificaciones o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.Notificaciones.Find(this.entidad.NotificacionId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Notificación {this.entidad.NotificacionId} no encontrada para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando notificación ID: {entidadActualizada.NotificacionId}");
                
                iConexion.Notificaciones.Remove(entidadActualizada);
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