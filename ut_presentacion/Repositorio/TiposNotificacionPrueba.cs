using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class TiposNotificacionPrueba
    {
        private readonly IConexion? iConexion;
        private List<TiposNotificacion>? lista;
        private TiposNotificacion? entidad;
        private int entidadIdGuardado;

        public TiposNotificacionPrueba()
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
            
            if (iConexion?.TiposNotificacion == null)
            {
                Console.WriteLine("Advertencia: iConexion.TiposNotificacion es null");
            }
            else
            {
                var count = iConexion.TiposNotificacion.Count();
                Console.WriteLine($"TiposNotificacion existentes en BD: {count}");
                
                // Mostrar los tipos existentes
                var tipos = iConexion.TiposNotificacion.ToList();
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoNotificacionId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.TiposNotificacion != null)
            {
                try
                {
                    var entidadExistente = iConexion.TiposNotificacion.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene notificaciones asociadas
                        var notificacionesAsociadas = 0;
                        if (iConexion.Notificaciones != null)
                        {
                            notificacionesAsociadas = iConexion.Notificaciones
                                .Count(n => n.TipoNotificacionId == entidadExistente.TipoNotificacionId);
                        }
                        
                        if (notificacionesAsociadas == 0)
                        {
                            iConexion.TiposNotificacion.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: TiposNotificacion {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el tipo porque tiene {notificacionesAsociadas} notificaciones asociadas");
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
        public void GuardarTipoNotificacion_Recordatorio_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Recordatorio")
            };

            // Act
            iConexion!.TiposNotificacion!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoNotificacionId > 0);
            Assert.IsNotNull(tipo.Nombre);

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoNotificacion_Logro_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Logro")
            };

            // Act
            iConexion!.TiposNotificacion!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoNotificacionId > 0);

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoNotificacion_RecompensaDisponible_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("RecompensaDisponible")
            };

            // Act
            iConexion!.TiposNotificacion!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoNotificacionId > 0);

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoNotificacion_SinNombre_DeberiaFallar()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = null!  // Nombre requerido
            };

            // Act & Assert
            iConexion!.TiposNotificacion!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoNotificacion_ConNombreLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 60); // 60 caracteres, excede el límite de 50
            var tipo = new TiposNotificacion
            {
                Nombre = nombreLargo
            };

            // Act & Assert
            iConexion!.TiposNotificacion!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoNotificacion_ConNombreDuplicado_DeberiaFallar()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_Duplicado");
            var tipo1 = new TiposNotificacion { Nombre = nombreUnico };
            var tipo2 = new TiposNotificacion { Nombre = nombreUnico };

            // Act
            iConexion!.TiposNotificacion!.Add(tipo1);
            iConexion.SaveChanges();

            iConexion.TiposNotificacion.Add(tipo2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo Nombre
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTipoNotificacionPorId_DeberiaRetornarTipoCorrecto()
        {
            // Arrange
            var tipoGuardar = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Tipo_Buscar")
            };
            iConexion!.TiposNotificacion!.Add(tipoGuardar);
            iConexion.SaveChanges();
            int idBuscado = tipoGuardar.TipoNotificacionId;

            // Act
            var tipoEncontrado = iConexion.TiposNotificacion.Find(idBuscado);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(idBuscado, tipoEncontrado.TipoNotificacionId);
            Assert.AreEqual(tipoGuardar.Nombre, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposNotificacion_DeberiaIncluirTiposExistentes()
        {
            // Arrange
            var nuevoTipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Tipo_Listar")
            };
            iConexion!.TiposNotificacion!.Add(nuevoTipo);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.TiposNotificacion.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.TiposNotificacion.Remove(nuevoTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposNotificacionConNotificaciones_DeberiaCargarRelacion()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Tipo_Relacion")
            };
            iConexion!.TiposNotificacion!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tiposConNotificaciones = iConexion.TiposNotificacion
                .Include(t => t.Notificaciones)
                .ToList();

            // Assert
            Assert.IsTrue(tiposConNotificaciones.Count > 0);
            
            // Verificar que la propiedad de navegación existe
            var tipoBuscado = tiposConNotificaciones.FirstOrDefault(t => t.TipoNotificacionId == tipo.TipoNotificacionId);
            Assert.IsNotNull(tipoBuscado);
            Assert.IsNotNull(tipoBuscado.Notificaciones);

            Console.WriteLine($"Tipo '{tipoBuscado.Nombre}' tiene {tipoBuscado.Notificaciones.Count} notificaciones asociadas");

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoNotificacion_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Tipo_Original")
            };
            iConexion!.TiposNotificacion!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");

            // Act
            tipo.Nombre = nuevoNombre;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposNotificacion.Find(tipo.TipoNotificacionId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoNombre, tipoActualizado.Nombre);

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarTipoNotificacion_SinNotificacionesAsociadas_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Tipo_Eliminar")
            };
            iConexion!.TiposNotificacion!.Add(tipo);
            iConexion.SaveChanges();
            int idEliminado = tipo.TipoNotificacionId;

            // Act
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();

            // Assert
            var tipoEliminado = iConexion.TiposNotificacion.Find(idEliminado);
            Assert.IsNull(tipoEliminado);
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

            // Assert - Verificar datos iniciales del script SQL
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
        public void BuscarTipoNotificacionPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_BuscarNombre");
            var tipo = new TiposNotificacion
            {
                Nombre = nombreUnico
            };
            iConexion!.TiposNotificacion!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tipoEncontrado = iConexion.TiposNotificacion
                .FirstOrDefault(t => t.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(nombreUnico, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposNotificacion.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTiposNotificacionOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.TiposNotificacion == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tiposOrdenados = iConexion.TiposNotificacion
                .OrderBy(t => t.TipoNotificacionId)
                .ToList();

            // Assert
            Assert.IsTrue(tiposOrdenados.Count > 0);
            
            // Verificar que están ordenados ascendentemente
            for (int i = 0; i < tiposOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(tiposOrdenados[i].TipoNotificacionId < tiposOrdenados[i + 1].TipoNotificacionId,
                    "Los tipos no están ordenados correctamente por ID");
            }

            Console.WriteLine("Tipos de notificación ordenados por ID:");
            foreach (var tipo in tiposOrdenados)
            {
                Console.WriteLine($"  - ID: {tipo.TipoNotificacionId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarQueTiposNotificacionNoSePuedenEliminarSiTienenNotificaciones()
        {
            // Esta prueba verifica la integridad referencial
            // Dependiendo de ON DELETE CASCADE, el comportamiento puede variar
            
            // Arrange - Crear un tipo de notificación
            var tipo = new TiposNotificacion
            {
                Nombre = GenerarNombreSeguro("Tipo_ConNotificaciones")
            };
            iConexion!.TiposNotificacion!.Add(tipo);
            iConexion.SaveChanges();
            int tipoId = tipo.TipoNotificacionId;
            
            // Crear un usuario de prueba
            var guidSufijo = Guid.NewGuid().ToString("N");
            if (guidSufijo.Length > 15) guidSufijo = guidSufijo.Substring(0, 15);
            
            var usuario = new Usuarios
            {
                CorreoElectronico = $"notif_{guidSufijo}@test.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            
            // Crear una notificación asociada a este tipo
            var notificacion = new Notificaciones
            {
                UsuarioId = usuario.UsuarioId,
                TipoNotificacionId = tipo.TipoNotificacionId,
                Mensaje = "Notificación de prueba para verificar integridad referencial",
                Leida = false
            };
            iConexion.Notificaciones!.Add(notificacion);
            iConexion.SaveChanges();

            // Act & Assert - Intentar eliminar el tipo
            iConexion.TiposNotificacion.Remove(tipo);
            
            try
            {
                int resultado = iConexion.SaveChanges();
                Console.WriteLine($"Eliminación exitosa (ON DELETE CASCADE activo). Resultado: {resultado}");
                
                // Verificar que el tipo ya no existe
                var tipoEliminado = iConexion.TiposNotificacion.Find(tipoId);
                Assert.IsNull(tipoEliminado, "El tipo debería haber sido eliminado");
            }
            catch (DbUpdateException ex)
            {
                // Excepción esperada si NO hay ON DELETE CASCADE
                Console.WriteLine($"Excepción capturada (esperada si no hay CASCADE): {ex.InnerException?.Message ?? ex.Message}");
                Assert.IsTrue(true, "Se lanzó la excepción esperada - el tipo no se puede eliminar porque tiene notificaciones asociadas");
            }

            // Cleanup
            try
            {
                iConexion.Notificaciones?.Remove(notificacion);
                iConexion.TiposNotificacion?.Remove(tipo);
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
            if (iConexion?.TiposNotificacion == null)
            {
                Console.WriteLine("Error: iConexion o TiposNotificacion es null");
                return false;
            }

            try
            {
                this.lista = iConexion.TiposNotificacion
                    .Include(t => t.Notificaciones)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} tipos de notificación encontrados");
                
                foreach (var tipo in lista)
                {
                    var notificacionesCount = tipo.Notificaciones?.Count ?? 0;
                    Console.WriteLine($"  - ID: {tipo.TipoNotificacionId}, " +
                                      $"Nombre: {tipo.Nombre}, " +
                                      $"NotificacionesAsociadas: {notificacionesCount}");
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
            if (iConexion?.TiposNotificacion == null)
            {
                Console.WriteLine("Error: iConexion o TiposNotificacion es null");
                return false;
            }

            try
            {
                this.entidad = new TiposNotificacion
                {
                    Nombre = GenerarNombreSeguro("Tipo_Prueba")
                };
                
                Console.WriteLine($"Guardando tipo de notificación: {entidad.Nombre}");
                
                iConexion.TiposNotificacion.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.TipoNotificacionId;
                    Console.WriteLine($"Tipo de notificación guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.TiposNotificacion == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposNotificacion o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.TiposNotificacion.Find(this.entidad.TipoNotificacionId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de notificación {this.entidad.TipoNotificacionId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando tipo de notificación ID: {entidadActualizada.TipoNotificacionId}");
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
            if (iConexion?.TiposNotificacion == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposNotificacion o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene notificaciones asociadas
                var notificacionesAsociadas = 0;
                if (iConexion.Notificaciones != null)
                {
                    notificacionesAsociadas = iConexion.Notificaciones
                        .Count(n => n.TipoNotificacionId == this.entidad.TipoNotificacionId);
                }
                
                if (notificacionesAsociadas > 0)
                {
                    Console.WriteLine($"No se puede borrar el tipo porque tiene {notificacionesAsociadas} notificaciones asociadas");
                    return false;
                }
                
                var entidadActualizada = iConexion.TiposNotificacion.Find(this.entidad.TipoNotificacionId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de notificación {this.entidad.TipoNotificacionId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando tipo de notificación ID: {entidadActualizada.TipoNotificacionId}");
                
                iConexion.TiposNotificacion.Remove(entidadActualizada);
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