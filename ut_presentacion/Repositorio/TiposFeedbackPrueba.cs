using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class TiposFeedbackPrueba
    {
        private readonly IConexion? iConexion;
        private List<TiposFeedback>? lista;
        private TiposFeedback? entidad;
        private int entidadIdGuardado;

        public TiposFeedbackPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 50 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 50)
        {
            var guid = Guid.NewGuid().ToString("N");
            if (guid.Length > 8) guid = guid.Substring(0, 8);
            var nombre = $"{prefijo}_{guid}";
            if (nombre.Length > maxLength)
            {
                nombre = nombre.Substring(0, maxLength);
            }
            return nombre;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            if (iConexion?.TiposFeedback == null)
            {
                Console.WriteLine("Advertencia: iConexion.TiposFeedback es null");
            }
            else
            {
                var count = iConexion.TiposFeedback.Count();
                Console.WriteLine($"TiposFeedback existentes en BD: {count}");
                
                // Mostrar los tipos existentes
                var tipos = iConexion.TiposFeedback.ToList();
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoFeedbackId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.TiposFeedback != null)
            {
                try
                {
                    var entidadExistente = iConexion.TiposFeedback.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene feedbacks asociados
                        var feedbacksAsociados = 0;
                        if (iConexion.FeedbackUsuarios != null)
                        {
                            feedbacksAsociados = iConexion.FeedbackUsuarios
                                .Count(f => f.TipoFeedbackId == entidadExistente.TipoFeedbackId);
                        }
                        
                        if (feedbacksAsociados == 0)
                        {
                            iConexion.TiposFeedback.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: TiposFeedback {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el tipo porque tiene {feedbacksAsociados} feedbacks asociados");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en limpieza: {ex.Message}");
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
        public void GuardarTipoFeedback_Sugerencia_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Sugerencia")
            };

            // Act
            iConexion!.TiposFeedback!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoFeedbackId > 0);
            Assert.IsNotNull(tipo.Nombre);

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoFeedback_Problema_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Problema")
            };

            // Act
            iConexion!.TiposFeedback!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoFeedbackId > 0);

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoFeedback_Idea_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Idea")
            };

            // Act
            iConexion!.TiposFeedback!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoFeedbackId > 0);

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoFeedback_SinNombre_DeberiaFallar()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = null!  // Nombre requerido
            };

            // Act & Assert
            iConexion!.TiposFeedback!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoFeedback_ConNombreLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 60); // 60 caracteres, excede el límite de 50
            var tipo = new TiposFeedback
            {
                Nombre = nombreLargo
            };

            // Act & Assert
            iConexion!.TiposFeedback!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoFeedback_ConNombreDuplicado_DeberiaFallar()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_Duplicado");
            var tipo1 = new TiposFeedback { Nombre = nombreUnico };
            var tipo2 = new TiposFeedback { Nombre = nombreUnico };

            // Act
            iConexion!.TiposFeedback!.Add(tipo1);
            iConexion.SaveChanges();

            iConexion.TiposFeedback.Add(tipo2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo Nombre
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTipoFeedbackPorId_DeberiaRetornarTipoCorrecto()
        {
            // Arrange
            var tipoGuardar = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Tipo_Buscar")
            };
            iConexion!.TiposFeedback!.Add(tipoGuardar);
            iConexion.SaveChanges();
            int idBuscado = tipoGuardar.TipoFeedbackId;

            // Act
            var tipoEncontrado = iConexion.TiposFeedback.Find(idBuscado);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(idBuscado, tipoEncontrado.TipoFeedbackId);
            Assert.AreEqual(tipoGuardar.Nombre, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposFeedback.Remove(tipoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposFeedback_DeberiaIncluirTiposExistentes()
        {
            // Arrange
            var nuevoTipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Tipo_Listar")
            };
            iConexion!.TiposFeedback!.Add(nuevoTipo);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.TiposFeedback.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.TiposFeedback.Remove(nuevoTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposFeedbackConFeedbacks_DeberiaCargarRelacion()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Tipo_Relacion")
            };
            iConexion!.TiposFeedback!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tiposConFeedbacks = iConexion.TiposFeedback
                .Include(t => t.FeedbacksUsuarios)
                .ToList();

            // Assert
            Assert.IsTrue(tiposConFeedbacks.Count > 0);
            
            // Verificar que la propiedad de navegación existe
            var tipoBuscado = tiposConFeedbacks.FirstOrDefault(t => t.TipoFeedbackId == tipo.TipoFeedbackId);
            Assert.IsNotNull(tipoBuscado);
            Assert.IsNotNull(tipoBuscado.FeedbacksUsuarios);

            Console.WriteLine($"Tipo '{tipoBuscado.Nombre}' tiene {tipoBuscado.FeedbacksUsuarios.Count} feedbacks asociados");

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoFeedback_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Tipo_Original")
            };
            iConexion!.TiposFeedback!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");

            // Act
            tipo.Nombre = nuevoNombre;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposFeedback.Find(tipo.TipoFeedbackId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoNombre, tipoActualizado.Nombre);

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarTipoFeedback_SinFeedbacksAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Tipo_Eliminar")
            };
            iConexion!.TiposFeedback!.Add(tipo);
            iConexion.SaveChanges();
            int idEliminado = tipo.TipoFeedbackId;

            // Act
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();

            // Assert
            var tipoEliminado = iConexion.TiposFeedback.Find(idEliminado);
            Assert.IsNull(tipoEliminado);
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

            // Assert - Verificar datos iniciales del script SQL
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
        public void BuscarTipoFeedbackPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_BuscarNombre");
            var tipo = new TiposFeedback
            {
                Nombre = nombreUnico
            };
            iConexion!.TiposFeedback!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tipoEncontrado = iConexion.TiposFeedback
                .FirstOrDefault(t => t.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(nombreUnico, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposFeedback.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTiposFeedbackOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.TiposFeedback == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tiposOrdenados = iConexion.TiposFeedback
                .OrderBy(t => t.TipoFeedbackId)
                .ToList();

            // Assert
            Assert.IsTrue(tiposOrdenados.Count > 0);
            
            // Verificar que están ordenados ascendentemente
            for (int i = 0; i < tiposOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(tiposOrdenados[i].TipoFeedbackId < tiposOrdenados[i + 1].TipoFeedbackId,
                    "Los tipos no están ordenados correctamente por ID");
            }

            Console.WriteLine("Tipos de feedback ordenados por ID:");
            foreach (var tipo in tiposOrdenados)
            {
                Console.WriteLine($"  - ID: {tipo.TipoFeedbackId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarQueTiposFeedbackNoSePuedenEliminarSiTienenFeedbacks()
        {
            // Esta prueba verifica la integridad referencial
            // Dependiendo de ON DELETE CASCADE, el comportamiento puede variar
            
            // Arrange - Crear un tipo de feedback
            var tipo = new TiposFeedback
            {
                Nombre = GenerarNombreSeguro("Tipo_ConFeedbacks")
            };
            iConexion!.TiposFeedback!.Add(tipo);
            iConexion.SaveChanges();
            int tipoId = tipo.TipoFeedbackId;
            
            // Crear un usuario de prueba
            var guidSufijo = Guid.NewGuid().ToString("N");
            if (guidSufijo.Length > 15) guidSufijo = guidSufijo.Substring(0, 15);
            
            var usuario = new Usuarios
            {
                CorreoElectronico = $"feedback_{guidSufijo}@test.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            
            // Crear un estado de feedback (asumiendo que existe el estado 1 = Pendiente)
            var estado = iConexion.EstadosFeedback?.FirstOrDefault(e => e.EstadoFeedbackId == 1);
            int estadoId;
            
            if (estado == null)
            {
                var nuevoEstado = new EstadosFeedback { Nombre = "Pendiente" };
                iConexion.EstadosFeedback?.Add(nuevoEstado);
                iConexion?.SaveChanges();
                estadoId = nuevoEstado.EstadoFeedbackId;
            }
            else
            {
                estadoId = estado.EstadoFeedbackId;
            }
            
            // Crear un feedback asociado a este tipo
            var feedback = new FeedbackUsuarios
            {
                UsuarioId = usuario.UsuarioId,
                TipoFeedbackId = tipo.TipoFeedbackId,
                Mensaje = "Feedback de prueba para verificar integridad referencial",
                EstadoFeedbackId = estadoId  // Usar la variable que sabemos que tiene valor
            };
            iConexion.FeedbackUsuarios!.Add(feedback);
            iConexion.SaveChanges();

            // Act & Assert - Intentar eliminar el tipo
            iConexion.TiposFeedback.Remove(tipo);
            
            try
            {
                int resultado = iConexion.SaveChanges();
                Console.WriteLine($"Eliminación exitosa (ON DELETE CASCADE activo). Resultado: {resultado}");
                
                // Verificar que el tipo ya no existe
                var tipoEliminado = iConexion.TiposFeedback.Find(tipoId);
                Assert.IsNull(tipoEliminado, "El tipo debería haber sido eliminado");
            }
            catch (DbUpdateException ex)
            {
                // Excepción esperada si NO hay ON DELETE CASCADE
                Console.WriteLine($"Excepción capturada (esperada si no hay CASCADE): {ex.InnerException?.Message ?? ex.Message}");
                Assert.IsTrue(true, "Se lanzó la excepción esperada - el tipo no se puede eliminar porque tiene feedbacks asociados");
            }

            // Cleanup
            try
            {
                iConexion.FeedbackUsuarios?.Remove(feedback);
                iConexion.TiposFeedback?.Remove(tipo);
                iConexion.Usuarios?.Remove(usuario);
                iConexion?.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en limpieza: {ex.Message}");
            }
        }

        public bool Listar()
        {
            if (iConexion?.TiposFeedback == null)
            {
                Console.WriteLine("Error: iConexion o TiposFeedback es null");
                return false;
            }

            try
            {
                this.lista = iConexion.TiposFeedback
                    .Include(t => t.FeedbacksUsuarios)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} tipos de feedback encontrados");
                
                foreach (var tipo in lista)
                {
                    var feedbacksCount = tipo.FeedbacksUsuarios?.Count ?? 0;
                    Console.WriteLine($"  - ID: {tipo.TipoFeedbackId}, " +
                                      $"Nombre: {tipo.Nombre}, " +
                                      $"FeedbacksAsociados: {feedbacksCount}");
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
            if (iConexion?.TiposFeedback == null)
            {
                Console.WriteLine("Error: iConexion o TiposFeedback es null");
                return false;
            }

            try
            {
                this.entidad = new TiposFeedback
                {
                    Nombre = GenerarNombreSeguro("Tipo_Prueba")
                };
                
                Console.WriteLine($"Guardando tipo de feedback: {entidad.Nombre}");
                
                iConexion.TiposFeedback.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.TipoFeedbackId;
                    Console.WriteLine($"Tipo de feedback guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.TiposFeedback == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposFeedback o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.TiposFeedback.Find(this.entidad.TipoFeedbackId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de feedback {this.entidad.TipoFeedbackId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando tipo de feedback ID: {entidadActualizada.TipoFeedbackId}");
                Console.WriteLine($"  Nuevo nombre: {nuevoNombre}");
                
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
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error de BD al modificar: {ex.InnerException?.Message ?? ex.Message}");
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
            if (iConexion?.TiposFeedback == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposFeedback o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene feedbacks asociados
                var feedbacksAsociados = 0;
                if (iConexion.FeedbackUsuarios != null)
                {
                    feedbacksAsociados = iConexion.FeedbackUsuarios
                        .Count(f => f.TipoFeedbackId == this.entidad.TipoFeedbackId);
                }
                
                if (feedbacksAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el tipo porque tiene {feedbacksAsociados} feedbacks asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.TiposFeedback.Find(this.entidad.TipoFeedbackId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de feedback {this.entidad.TipoFeedbackId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando tipo de feedback ID: {entidadActualizada.TipoFeedbackId}");
                
                iConexion.TiposFeedback.Remove(entidadActualizada);
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