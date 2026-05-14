using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class FeedbackUsuariosPrueba
    {
        private readonly IConexion? iConexion;
        private List<FeedbackUsuarios>? lista;
        private FeedbackUsuarios? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;
        private int tipoFeedbackIdPrueba;
        private int estadoFeedbackIdPrueba;

        public FeedbackUsuariosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar mensaje seguro (máx 1000 caracteres)
        private string GenerarMensajeSeguro(string prefijo, int maxLength = 1000)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var mensaje = $"{prefijo}_{guid} - Este es un mensaje de prueba para feedback de usuarios.";
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
                CorreoElectronico = $"feedback_{guidSufijo}@test.com",
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
            
            // Obtener TipoFeedback válido (1=Sugerencia, 2=Problema, 3=Idea)
            if (iConexion?.TiposFeedback != null)
            {
                var tipoExistente = iConexion.TiposFeedback.FirstOrDefault();
                if (tipoExistente != null)
                {
                    tipoFeedbackIdPrueba = tipoExistente.TipoFeedbackId;
                    Console.WriteLine($"Usando TipoFeedback existente ID: {tipoFeedbackIdPrueba}, Nombre: {tipoExistente.Nombre}");
                }
                else
                {
                    // Crear un tipo de feedback de prueba
                    var nuevoTipo = new TiposFeedback { Nombre = $"Tipo_Test_{Guid.NewGuid():N}".Substring(0, 50) };
                    iConexion.TiposFeedback.Add(nuevoTipo);
                    iConexion.SaveChanges();
                    tipoFeedbackIdPrueba = nuevoTipo.TipoFeedbackId;
                    Console.WriteLine($"TipoFeedback creado para prueba ID: {tipoFeedbackIdPrueba}");
                }
            }
            
            // Obtener EstadoFeedback válido (1=Pendiente, 2=Resuelto)
            if (iConexion?.EstadosFeedback != null)
            {
                var estadoExistente = iConexion.EstadosFeedback.FirstOrDefault();
                if (estadoExistente != null)
                {
                    estadoFeedbackIdPrueba = estadoExistente.EstadoFeedbackId;
                    Console.WriteLine($"Usando EstadoFeedback existente ID: {estadoFeedbackIdPrueba}, Nombre: {estadoExistente.Nombre}");
                }
                else
                {
                    // Crear un estado de feedback de prueba
                    var nuevoEstado = new EstadosFeedback { Nombre = $"Estado_Test_{Guid.NewGuid():N}".Substring(0, 50) };
                    iConexion.EstadosFeedback.Add(nuevoEstado);
                    iConexion.SaveChanges();
                    estadoFeedbackIdPrueba = nuevoEstado.EstadoFeedbackId;
                    Console.WriteLine($"EstadoFeedback creado para prueba ID: {estadoFeedbackIdPrueba}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar feedback creado
            if (entidadIdGuardado > 0 && iConexion?.FeedbackUsuarios != null)
            {
                try
                {
                    var entidadExistente = iConexion.FeedbackUsuarios.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.FeedbackUsuarios.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: FeedbackUsuarios {entidadIdGuardado} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando feedback: {ex.Message}");
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
        public void GuardarFeedback_Sugerencia_ConDatosValidos_DeberiaCrearFeedback()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Sugerencia"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };

            // Act
            iConexion!.FeedbackUsuarios!.Add(feedback);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(feedback.FeedbackId > 0);
            Assert.IsNotNull(feedback.Mensaje);

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarFeedback_Problema_ConDatosValidos_DeberiaCrearFeedback()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Problema - Descripción del error encontrado"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };

            // Act
            iConexion!.FeedbackUsuarios!.Add(feedback);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(feedback.FeedbackId > 0);

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarFeedback_Idea_ConDatosValidos_DeberiaCrearFeedback()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Idea - Propuesta de mejora para el sistema"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };

            // Act
            iConexion!.FeedbackUsuarios!.Add(feedback);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(feedback.FeedbackId > 0);

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarFeedback_SinMensaje_DeberiaFallar()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = null!,  // Mensaje requerido
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };

            // Act & Assert
            iConexion!.FeedbackUsuarios!.Add(feedback);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarFeedback_ConMensajeMuyLargo_DeberiaFallar()
        {
            // Arrange
            var mensajeLargo = new string('A', 1100); // 1100 caracteres, excede el límite de 1000
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = mensajeLargo,
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };

            // Act & Assert
            iConexion!.FeedbackUsuarios!.Add(feedback);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerFeedbackPorId_DeberiaRetornarFeedbackCorrecto()
        {
            // Arrange
            var feedbackGuardar = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Buscar"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.Add(feedbackGuardar);
            iConexion.SaveChanges();
            int idBuscado = feedbackGuardar.FeedbackId;

            // Act
            var feedbackEncontrado = iConexion.FeedbackUsuarios.Find(idBuscado);

            // Assert
            Assert.IsNotNull(feedbackEncontrado);
            Assert.AreEqual(idBuscado, feedbackEncontrado.FeedbackId);
            Assert.AreEqual(feedbackGuardar.Mensaje, feedbackEncontrado.Mensaje);

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedbackGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarFeedbacks_DeberiaIncluirFeedbacksExistentes()
        {
            // Arrange
            var nuevoFeedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Listar"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.Add(nuevoFeedback);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.FeedbackUsuarios.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(nuevoFeedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarFeedbacksConRelaciones_DeberiaCargarUsuarioTipoYEstado()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Relaciones"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.Add(feedback);
            iConexion.SaveChanges();

            // Act
            var feedbackConRelaciones = iConexion.FeedbackUsuarios
                .Include(f => f.Usuario)
                .Include(f => f.TipoFeedback)
                .Include(f => f.EstadoFeedback)
                .FirstOrDefault(f => f.FeedbackId == feedback.FeedbackId);

            // Assert
            Assert.IsNotNull(feedbackConRelaciones);
            Assert.IsNotNull(feedbackConRelaciones.Usuario);
            Assert.IsNotNull(feedbackConRelaciones.TipoFeedback);
            Assert.IsNotNull(feedbackConRelaciones.EstadoFeedback);

            Console.WriteLine($"Feedback: {feedbackConRelaciones.Mensaje}");
            Console.WriteLine($"  Usuario: {feedbackConRelaciones.Usuario.CorreoElectronico}");
            Console.WriteLine($"  Tipo: {feedbackConRelaciones.TipoFeedback.Nombre}");
            Console.WriteLine($"  Estado: {feedbackConRelaciones.EstadoFeedback.Nombre}");

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarFeedbacksPorUsuario_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var feedback1 = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Usuario1"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            var feedback2 = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Usuario2"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.AddRange(feedback1, feedback2);
            iConexion.SaveChanges();

            // Act
            var feedbacksUsuario = iConexion.FeedbackUsuarios
                .Where(f => f.UsuarioId == usuarioIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(feedbacksUsuario.Count >= 2);
            Assert.IsTrue(feedbacksUsuario.All(f => f.UsuarioId == usuarioIdPrueba));

            // Cleanup
            iConexion.FeedbackUsuarios.RemoveRange(feedback1, feedback2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarFeedback_CambiarEstado_DeberiaActualizar()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Original"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.Add(feedback);
            iConexion.SaveChanges();
            
            // Obtener un estado diferente (si existe Resuelto, usarlo)
            var nuevoEstado = iConexion.EstadosFeedback!
                .FirstOrDefault(e => e.EstadoFeedbackId != estadoFeedbackIdPrueba);
            
            if (nuevoEstado == null)
            {
                // Si no hay otro estado, crear uno
                nuevoEstado = new EstadosFeedback { Nombre = $"Estado_Test_{Guid.NewGuid():N}".Substring(0, 50) };
                iConexion.EstadosFeedback!.Add(nuevoEstado);
                iConexion.SaveChanges();
            }
            
            int nuevoEstadoId = nuevoEstado.EstadoFeedbackId;

            // Act
            feedback.EstadoFeedbackId = nuevoEstadoId;
            var entry = iConexion.Entry(feedback);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var feedbackActualizado = iConexion.FeedbackUsuarios.Find(feedback.FeedbackId);
            Assert.IsNotNull(feedbackActualizado);
            Assert.AreEqual(nuevoEstadoId, feedbackActualizado.EstadoFeedbackId);

            Console.WriteLine($"Estado cambiado a: {nuevoEstado.Nombre}");

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
            
            // Limpiar estado creado si fue necesario
            if (nuevoEstado.EstadoFeedbackId != 1 && nuevoEstado.EstadoFeedbackId != 2)
            {
                iConexion.EstadosFeedback!.Remove(nuevoEstado);
                iConexion.SaveChanges();
            }
        }

        [TestMethod]
        public void ModificarFeedback_CambiarMensaje_DeberiaActualizar()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Original"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.Add(feedback);
            iConexion.SaveChanges();
            string nuevoMensaje = GenerarMensajeSeguro("Feedback_Modificado");

            // Act
            feedback.Mensaje = nuevoMensaje;
            var entry = iConexion.Entry(feedback);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var feedbackActualizado = iConexion.FeedbackUsuarios.Find(feedback.FeedbackId);
            Assert.IsNotNull(feedbackActualizado);
            Assert.AreEqual(nuevoMensaje, feedbackActualizado.Mensaje);

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarFeedback_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Eliminar"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };
            iConexion!.FeedbackUsuarios!.Add(feedback);
            iConexion.SaveChanges();
            int idEliminado = feedback.FeedbackId;

            // Act
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();

            // Assert
            var feedbackEliminado = iConexion.FeedbackUsuarios.Find(idEliminado);
            Assert.IsNull(feedbackEliminado);
        }

        [TestMethod]
        public void VerificarFechaAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Fecha"),
                EstadoFeedbackId = estadoFeedbackIdPrueba
            };

            // Act
            iConexion!.FeedbackUsuarios!.Add(feedback);
            iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(feedback).Reload();

            // Assert
            Assert.IsTrue(feedback.Fecha != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(feedback.Fecha <= DateTime.Now, 
                "La fecha asignada es futura");

            Console.WriteLine($"Fecha asignada por BD: {feedback.Fecha:yyyy-MM-dd HH:mm:ss}");

            // Cleanup
            iConexion.FeedbackUsuarios.Remove(feedback);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void VerificarTiposFeedbackIniciales_DeberianExistirSugerenciaProblemaIdea()
        {
            if (iConexion?.TiposFeedback == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tipos = iConexion.TiposFeedback.ToList();

            // Assert
            Assert.IsTrue(tipos.Count >= 3, "Deberían existir al menos 3 tipos de feedback del script inicial");
            
            var nombresEsperados = new[] { "Sugerencia", "Problema", "Idea" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(tipos.Any(t => t.Nombre == nombre), 
                    $"No existe el tipo de feedback '{nombre}'");
            }

            Console.WriteLine("Tipos de feedback iniciales encontrados:");
            foreach (var tipo in tipos.Where(t => nombresEsperados.Contains(t.Nombre)))
            {
                Console.WriteLine($"  - ID: {tipo.TipoFeedbackId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarEstadosFeedbackIniciales_DeberianExistirPendienteYResuelto()
        {
            if (iConexion?.EstadosFeedback == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var estados = iConexion.EstadosFeedback.ToList();

            // Assert
            Assert.IsTrue(estados.Count >= 2, "Deberían existir al menos 2 estados del script inicial");
            
            var estadoPendiente = estados.FirstOrDefault(e => e.Nombre == "Pendiente");
            var estadoResuelto = estados.FirstOrDefault(e => e.Nombre == "Resuelto");

            Assert.IsNotNull(estadoPendiente, "No existe el estado 'Pendiente'");
            Assert.IsNotNull(estadoResuelto, "No existe el estado 'Resuelto'");

            Console.WriteLine("Estados de feedback iniciales encontrados:");
            Console.WriteLine($"  - ID: {estadoPendiente!.EstadoFeedbackId}, Nombre: {estadoPendiente.Nombre}");
            Console.WriteLine($"  - ID: {estadoResuelto!.EstadoFeedbackId}, Nombre: {estadoResuelto.Nombre}");
        }

        [TestMethod]
        public void FiltrarFeedbacksPorEstado_DeberiaRetornarResultadosCorrectos()
        {
            // Arrange
            var feedback1 = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Pendiente"),
                EstadoFeedbackId = 1  // Pendiente (asumiendo que existe)
            };
            var feedback2 = new FeedbackUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                TipoFeedbackId = tipoFeedbackIdPrueba,
                Mensaje = GenerarMensajeSeguro("Feedback_Resuelto"),
                EstadoFeedbackId = 2  // Resuelto (asumiendo que existe)
            };
            iConexion!.FeedbackUsuarios!.AddRange(feedback1, feedback2);
            iConexion.SaveChanges();

            // Act
            var feedbacksPendientes = iConexion.FeedbackUsuarios
                .Where(f => f.EstadoFeedbackId == 1)
                .ToList();

            // Assert
            Assert.IsTrue(feedbacksPendientes.Count >= 1);

            // Cleanup
            iConexion.FeedbackUsuarios.RemoveRange(feedback1, feedback2);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.FeedbackUsuarios == null)
            {
                Console.WriteLine("Error: iConexion o FeedbackUsuarios es null");
                return false;
            }

            try
            {
                this.lista = iConexion.FeedbackUsuarios
                    .Include(f => f.Usuario)
                    .Include(f => f.TipoFeedback)
                    .Include(f => f.EstadoFeedback)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} feedbacks encontrados");
                
                foreach (var feedback in lista)
                {
                    Console.WriteLine($"  - ID: {feedback.FeedbackId}, " +
                                      $"Usuario: {feedback.Usuario?.CorreoElectronico ?? "N/A"}, " +
                                      $"Tipo: {feedback.TipoFeedback?.Nombre ?? "N/A"}, " +
                                      $"Estado: {feedback.EstadoFeedback?.Nombre ?? "N/A"}, " +
                                      $"Fecha: {feedback.Fecha:yyyy-MM-dd HH:mm}");
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
            if (iConexion?.FeedbackUsuarios == null)
            {
                Console.WriteLine("Error: iConexion o FeedbackUsuarios es null");
                return false;
            }

            try
            {
                this.entidad = new FeedbackUsuarios
                {
                    UsuarioId = usuarioIdPrueba,
                    TipoFeedbackId = tipoFeedbackIdPrueba,
                    Mensaje = GenerarMensajeSeguro("Feedback_Prueba"),
                    EstadoFeedbackId = estadoFeedbackIdPrueba
                };
                
                Console.WriteLine($"Guardando feedback: {entidad.Mensaje}");
                
                iConexion.FeedbackUsuarios.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.FeedbackId;
                    Console.WriteLine($"Feedback guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.FeedbackUsuarios == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, FeedbackUsuarios o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.FeedbackUsuarios.Find(this.entidad.FeedbackId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Feedback {this.entidad.FeedbackId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoMensaje = GenerarMensajeSeguro("Feedback_Modificado");
                entidadActualizada.Mensaje = nuevoMensaje;
                
                Console.WriteLine($"Modificando feedback ID: {entidadActualizada.FeedbackId}");
                Console.WriteLine($"  Nuevo mensaje: {nuevoMensaje}");
                
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
            if (iConexion?.FeedbackUsuarios == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, FeedbackUsuarios o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.FeedbackUsuarios.Find(this.entidad.FeedbackId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Feedback {this.entidad.FeedbackId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando feedback ID: {entidadActualizada.FeedbackId}");
                
                iConexion.FeedbackUsuarios.Remove(entidadActualizada);
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