using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class EstadosFeedbackPrueba
    {
        private readonly IConexion? iConexion;
        private List<EstadosFeedback>? lista;
        private EstadosFeedback? entidad;
        private int entidadIdGuardado;

        public EstadosFeedbackPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 50 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 50)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
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
            
            if (iConexion?.EstadosFeedback == null)
            {
                Console.WriteLine("Advertencia: iConexion.EstadosFeedback es null");
            }
            else
            {
                var count = iConexion.EstadosFeedback.Count();
                Console.WriteLine($"EstadosFeedback existentes en BD: {count}");
                
                // Mostrar los estados existentes
                var estados = iConexion.EstadosFeedback.ToList();
                foreach (var estado in estados)
                {
                    Console.WriteLine($"  - ID: {estado.EstadoFeedbackId}, Nombre: {estado.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.EstadosFeedback != null)
            {
                try
                {
                    var entidadExistente = iConexion.EstadosFeedback.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene feedbacks asociados
                        var feedbacksAsociados = 0;
                        if (iConexion.FeedbackUsuarios != null)
                        {
                            feedbacksAsociados = iConexion.FeedbackUsuarios
                                .Count(f => f.EstadoFeedbackId == entidadExistente.EstadoFeedbackId);
                        }
                        
                        if (feedbacksAsociados == 0)
                        {
                            iConexion.EstadosFeedback.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: EstadosFeedback {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el estado porque tiene {feedbacksAsociados} feedbacks asociados");
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
        public void GuardarEstado_ConDatosValidos_DeberiaCrearEstado()
        {
            // Arrange
            var estado = new EstadosFeedback
            {
                Nombre = GenerarNombreSeguro("Estado_Test")
            };

            // Act
            iConexion!.EstadosFeedback!.Add(estado);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(estado.EstadoFeedbackId > 0);
            Assert.IsNotNull(estado.Nombre);

            // Cleanup
            iConexion.EstadosFeedback.Remove(estado);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarEstado_ConNombreLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 60); // 60 caracteres, excede el límite de 50
            var estado = new EstadosFeedback
            {
                Nombre = nombreLargo
            };

            // Act & Assert
            iConexion!.EstadosFeedback!.Add(estado);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarEstado_SinNombre_DeberiaFallar()
        {
            // Arrange
            var estado = new EstadosFeedback
            {
                Nombre = null!  // Nombre requerido
            };

            // Act & Assert
            iConexion!.EstadosFeedback!.Add(estado);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerEstadoPorId_DeberiaRetornarEstadoCorrecto()
        {
            // Arrange
            var estadoGuardar = new EstadosFeedback
            {
                Nombre = GenerarNombreSeguro("Estado_Buscar")
            };
            iConexion!.EstadosFeedback!.Add(estadoGuardar);
            iConexion.SaveChanges();
            int idBuscado = estadoGuardar.EstadoFeedbackId;

            // Act
            var estadoEncontrado = iConexion.EstadosFeedback.Find(idBuscado);

            // Assert
            Assert.IsNotNull(estadoEncontrado);
            Assert.AreEqual(idBuscado, estadoEncontrado.EstadoFeedbackId);
            Assert.AreEqual(estadoGuardar.Nombre, estadoEncontrado.Nombre);

            // Cleanup
            iConexion.EstadosFeedback.Remove(estadoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarEstados_DeberiaIncluirEstadosExistentes()
        {
            // Arrange
            var nuevoEstado = new EstadosFeedback
            {
                Nombre = GenerarNombreSeguro("Estado_Listar")
            };
            iConexion!.EstadosFeedback!.Add(nuevoEstado);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.EstadosFeedback.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.EstadosFeedback.Remove(nuevoEstado);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarEstadosConFeedbacks_DeberiaCargarRelacion()
        {
            // Arrange - Primero crear un estado
            var estado = new EstadosFeedback
            {
                Nombre = GenerarNombreSeguro("Estado_Relacion")
            };
            iConexion!.EstadosFeedback!.Add(estado);
            iConexion.SaveChanges();

            // Act
            var estadosConFeedbacks = iConexion.EstadosFeedback
                .Include(e => e.FeedbacksUsuarios)
                .ToList();

            // Assert
            Assert.IsTrue(estadosConFeedbacks.Count > 0);
            
            // Verificar que la propiedad de navegación existe
            var estadoBuscado = estadosConFeedbacks.FirstOrDefault(e => e.EstadoFeedbackId == estado.EstadoFeedbackId);
            Assert.IsNotNull(estadoBuscado);
            Assert.IsNotNull(estadoBuscado.FeedbacksUsuarios);

            Console.WriteLine($"Estado '{estadoBuscado.Nombre}' tiene {estadoBuscado.FeedbacksUsuarios.Count} feedbacks asociados");

            // Cleanup
            iConexion.EstadosFeedback.Remove(estado);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarEstado_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var estado = new EstadosFeedback
            {
                Nombre = GenerarNombreSeguro("Estado_Original")
            };
            iConexion!.EstadosFeedback!.Add(estado);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Estado_Modificado");

            // Act
            estado.Nombre = nuevoNombre;
            var entry = iConexion.Entry(estado);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var estadoActualizado = iConexion.EstadosFeedback.Find(estado.EstadoFeedbackId);
            Assert.IsNotNull(estadoActualizado);
            Assert.AreEqual(nuevoNombre, estadoActualizado.Nombre);

            // Cleanup
            iConexion.EstadosFeedback.Remove(estado);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarEstado_SinFeedbacksAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var estado = new EstadosFeedback
            {
                Nombre = GenerarNombreSeguro("Estado_Eliminar")
            };
            iConexion!.EstadosFeedback!.Add(estado);
            iConexion.SaveChanges();
            int idEliminado = estado.EstadoFeedbackId;

            // Act
            iConexion.EstadosFeedback.Remove(estado);
            iConexion.SaveChanges();

            // Assert
            var estadoEliminado = iConexion.EstadosFeedback.Find(idEliminado);
            Assert.IsNull(estadoEliminado);
        }

        [TestMethod]
        public void VerificarEstadosIniciales_DeberianExistirPendienteYResuelto()
        {
            if (iConexion?.EstadosFeedback == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var estados = iConexion.EstadosFeedback.ToList();

            // Assert - Verificar datos iniciales del script SQL
            Assert.IsTrue(estados.Count >= 2, "Deberían existir al menos 2 estados del script inicial");
            
            var estadoPendiente = estados.FirstOrDefault(e => e.Nombre == "Pendiente");
            var estadoResuelto = estados.FirstOrDefault(e => e.Nombre == "Resuelto");

            Assert.IsNotNull(estadoPendiente, "No existe el estado 'Pendiente' en la BD");
            Assert.IsNotNull(estadoResuelto, "No existe el estado 'Resuelto' en la BD");

            Console.WriteLine("Estados iniciales encontrados:");
            Console.WriteLine($"  - ID: {estadoPendiente!.EstadoFeedbackId}, Nombre: {estadoPendiente.Nombre}");
            Console.WriteLine($"  - ID: {estadoResuelto!.EstadoFeedbackId}, Nombre: {estadoResuelto.Nombre}");
        }

        [TestMethod]
        public void BuscarEstadoPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Estado_BuscarNombre");
            var estado = new EstadosFeedback
            {
                Nombre = nombreUnico
            };
            iConexion!.EstadosFeedback!.Add(estado);
            iConexion.SaveChanges();

            // Act
            var estadoEncontrado = iConexion.EstadosFeedback
                .FirstOrDefault(e => e.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(estadoEncontrado);
            Assert.AreEqual(nombreUnico, estadoEncontrado.Nombre);

            // Cleanup
            iConexion.EstadosFeedback.Remove(estado);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void PrevenirDuplicados_DeberiaPermitirNombresUnicos()
        {
            // Arrange
            var nombreComun = GenerarNombreSeguro("Estado_Duplicado");
            var estado1 = new EstadosFeedback { Nombre = nombreComun };
            var estado2 = new EstadosFeedback { Nombre = nombreComun };

            // Act
            iConexion!.EstadosFeedback!.Add(estado1);
            iConexion.SaveChanges();

            iConexion.EstadosFeedback.Add(estado2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo Nombre
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.EstadosFeedback.Remove(estado1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerEstadosOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.EstadosFeedback == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var estadosOrdenados = iConexion.EstadosFeedback
                .OrderBy(e => e.EstadoFeedbackId)
                .ToList();

            // Assert
            Assert.IsTrue(estadosOrdenados.Count > 0);
            
            // Verificar que están ordenados ascendentemente
            for (int i = 0; i < estadosOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(estadosOrdenados[i].EstadoFeedbackId < estadosOrdenados[i + 1].EstadoFeedbackId,
                    "Los estados no están ordenados correctamente por ID");
            }

            Console.WriteLine("Estados ordenados por ID:");
            foreach (var estado in estadosOrdenados)
            {
                Console.WriteLine($"  - ID: {estado.EstadoFeedbackId}, Nombre: {estado.Nombre}");
            }
        }

        public bool Listar()
        {
            if (iConexion?.EstadosFeedback == null)
            {
                Console.WriteLine("Error: iConexion o EstadosFeedback es null");
                return false;
            }

            try
            {
                this.lista = iConexion.EstadosFeedback
                    .Include(e => e.FeedbacksUsuarios)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} estados encontrados");
                
                foreach (var estado in lista)
                {
                    var feedbacksCount = estado.FeedbacksUsuarios?.Count ?? 0;
                    Console.WriteLine($"  - ID: {estado.EstadoFeedbackId}, " +
                                      $"Nombre: {estado.Nombre}, " +
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
            if (iConexion?.EstadosFeedback == null)
            {
                Console.WriteLine("Error: iConexion o EstadosFeedback es null");
                return false;
            }

            try
            {
                this.entidad = new EstadosFeedback
                {
                    Nombre = GenerarNombreSeguro("Estado_Prueba")
                };
                
                Console.WriteLine($"Guardando estado: {entidad.Nombre}");
                
                iConexion.EstadosFeedback.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.EstadoFeedbackId;
                    Console.WriteLine($"Estado guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.EstadosFeedback == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, EstadosFeedback o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.EstadosFeedback.Find(this.entidad.EstadoFeedbackId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Estado {this.entidad.EstadoFeedbackId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Estado_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando estado ID: {entidadActualizada.EstadoFeedbackId}");
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
            if (iConexion?.EstadosFeedback == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, EstadosFeedback o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene feedbacks asociados
                var feedbacksAsociados = 0;
                if (iConexion.FeedbackUsuarios != null)
                {
                    feedbacksAsociados = iConexion.FeedbackUsuarios
                        .Count(f => f.EstadoFeedbackId == this.entidad.EstadoFeedbackId);
                }
                
                if (feedbacksAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el estado porque tiene {feedbacksAsociados} feedbacks asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.EstadosFeedback.Find(this.entidad.EstadoFeedbackId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Estado {this.entidad.EstadoFeedbackId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando estado ID: {entidadActualizada.EstadoFeedbackId}");
                
                iConexion.EstadosFeedback.Remove(entidadActualizada);
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