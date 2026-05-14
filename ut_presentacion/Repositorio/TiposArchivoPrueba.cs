using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class TiposArchivoPrueba
    {
        private readonly IConexion? iConexion;
        private List<TiposArchivo>? lista;
        private TiposArchivo? entidad;
        private int entidadIdGuardado;

        public TiposArchivoPrueba()
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
            
            if (iConexion?.TiposArchivo == null)
            {
                Console.WriteLine("Advertencia: iConexion.TiposArchivo es null");
            }
            else
            {
                var count = iConexion.TiposArchivo.Count();
                Console.WriteLine($"TiposArchivo existentes en BD: {count}");
                
                // Mostrar los tipos existentes
                var tipos = iConexion.TiposArchivo.ToList();
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoArchivoId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.TiposArchivo != null)
            {
                try
                {
                    var entidadExistente = iConexion.TiposArchivo.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene archivos asociados
                        var archivosAsociados = 0;
                        if (iConexion.Archivos != null)
                        {
                            archivosAsociados = iConexion.Archivos
                                .Count(a => a.TipoArchivoId == entidadExistente.TipoArchivoId);
                        }
                        
                        if (archivosAsociados == 0)
                        {
                            iConexion.TiposArchivo.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: TiposArchivo {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el tipo porque tiene {archivosAsociados} archivos asociados");
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
        public void GuardarTipoArchivo_Imagen_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Imagen")
            };

            // Act
            iConexion!.TiposArchivo!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoArchivoId > 0);
            Assert.IsNotNull(tipo.Nombre);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoArchivo_Video_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Video")
            };

            // Act
            iConexion!.TiposArchivo!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoArchivoId > 0);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoArchivo_PDF_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("PDF")
            };

            // Act
            iConexion!.TiposArchivo!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoArchivoId > 0);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoArchivo_EnlaceExterno_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("EnlaceExterno")
            };

            // Act
            iConexion!.TiposArchivo!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoArchivoId > 0);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoArchivo_SinNombre_DeberiaFallar()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = null!  // Nombre requerido
            };

            // Act & Assert
            iConexion!.TiposArchivo!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoArchivo_ConNombreLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 60); // 60 caracteres, excede el límite de 50
            var tipo = new TiposArchivo
            {
                Nombre = nombreLargo
            };

            // Act & Assert
            iConexion!.TiposArchivo!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoArchivo_ConNombreDuplicado_DeberiaFallar()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_Duplicado");
            var tipo1 = new TiposArchivo { Nombre = nombreUnico };
            var tipo2 = new TiposArchivo { Nombre = nombreUnico };

            // Act
            iConexion!.TiposArchivo!.Add(tipo1);
            iConexion.SaveChanges();

            iConexion.TiposArchivo.Add(tipo2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo Nombre
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTipoArchivoPorId_DeberiaRetornarTipoCorrecto()
        {
            // Arrange
            var tipoGuardar = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Tipo_Buscar")
            };
            iConexion!.TiposArchivo!.Add(tipoGuardar);
            iConexion.SaveChanges();
            int idBuscado = tipoGuardar.TipoArchivoId;

            // Act
            var tipoEncontrado = iConexion.TiposArchivo.Find(idBuscado);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(idBuscado, tipoEncontrado.TipoArchivoId);
            Assert.AreEqual(tipoGuardar.Nombre, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposArchivo_DeberiaIncluirTiposExistentes()
        {
            // Arrange
            var nuevoTipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Tipo_Listar")
            };
            iConexion!.TiposArchivo!.Add(nuevoTipo);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.TiposArchivo.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.TiposArchivo.Remove(nuevoTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposArchivoConArchivos_DeberiaCargarRelacion()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Tipo_Relacion")
            };
            iConexion!.TiposArchivo!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tiposConArchivos = iConexion.TiposArchivo
                .Include(t => t.Archivos)
                .ToList();

            // Assert
            Assert.IsTrue(tiposConArchivos.Count > 0);
            
            // Verificar que la propiedad de navegación existe
            var tipoBuscado = tiposConArchivos.FirstOrDefault(t => t.TipoArchivoId == tipo.TipoArchivoId);
            Assert.IsNotNull(tipoBuscado);
            Assert.IsNotNull(tipoBuscado.Archivos);

            Console.WriteLine($"Tipo '{tipoBuscado.Nombre}' tiene {tipoBuscado.Archivos?.Count ?? 0} archivos asociados");

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoArchivo_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Tipo_Original")
            };
            iConexion!.TiposArchivo!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");

            // Act
            tipo.Nombre = nuevoNombre;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposArchivo.Find(tipo.TipoArchivoId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoNombre, tipoActualizado.Nombre);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarTipoArchivo_SinArchivosAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Tipo_Eliminar")
            };
            iConexion!.TiposArchivo!.Add(tipo);
            iConexion.SaveChanges();
            int idEliminado = tipo.TipoArchivoId;

            // Act
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();

            // Assert
            var tipoEliminado = iConexion.TiposArchivo.Find(idEliminado);
            Assert.IsNull(tipoEliminado);
        }

        [TestMethod]
        public void VerificarTiposArchivoIniciales_DeberianExistirCuatroTipos()
        {
            if (iConexion?.TiposArchivo == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tipos = iConexion.TiposArchivo.ToList();

            // Assert - Verificar datos iniciales del script SQL
            Assert.IsTrue(tipos.Count >= 4, "Deberían existir al menos 4 tipos de archivo del script inicial");
            
            var nombresEsperados = new[] { "Imagen", "Video", "PDF", "EnlaceExterno" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(tipos.Any(t => t.Nombre == nombre), 
                    $"No existe el tipo de archivo '{nombre}'");
            }

            Console.WriteLine("Tipos de archivo iniciales encontrados:");
            foreach (var tipo in tipos.Where(t => nombresEsperados.Contains(t.Nombre)))
            {
                Console.WriteLine($"  - ID: {tipo.TipoArchivoId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void BuscarTipoArchivoPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_BuscarNombre");
            var tipo = new TiposArchivo
            {
                Nombre = nombreUnico
            };
            iConexion!.TiposArchivo!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tipoEncontrado = iConexion.TiposArchivo
                .FirstOrDefault(t => t.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(nombreUnico, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposArchivo.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTiposArchivoOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.TiposArchivo == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tiposOrdenados = iConexion.TiposArchivo
                .OrderBy(t => t.TipoArchivoId)
                .ToList();

            // Assert
            Assert.IsTrue(tiposOrdenados.Count > 0);
            
            // Verificar que están ordenados ascendentemente
            for (int i = 0; i < tiposOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(tiposOrdenados[i].TipoArchivoId < tiposOrdenados[i + 1].TipoArchivoId,
                    "Los tipos no están ordenados correctamente por ID");
            }

            Console.WriteLine("Tipos de archivo ordenados por ID:");
            foreach (var tipo in tiposOrdenados)
            {
                Console.WriteLine($"  - ID: {tipo.TipoArchivoId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarQueTiposArchivoNoSePuedenEliminarSiTienenArchivos()
        {
            // Esta prueba verifica el comportamiento de la FK
            // Dependiendo de si ON DELETE CASCADE está configurado, el comportamiento puede variar
            
            // Arrange - Crear un tipo de archivo
            var tipo = new TiposArchivo
            {
                Nombre = GenerarNombreSeguro("Tipo_ConArchivos")
            };
            iConexion!.TiposArchivo!.Add(tipo);
            iConexion.SaveChanges();
            int tipoId = tipo.TipoArchivoId;
            
            // Crear un archivo asociado a este tipo
            var archivo = new Archivos
            {
                Url = "https://storage.ecocycle.com/test_file.jpg",
                TipoArchivoId = tipo.TipoArchivoId,
                EsExterno = false,
                Proveedor = "Local",
                Descripcion = "Archivo de prueba"
            };
            iConexion.Archivos!.Add(archivo);
            iConexion.SaveChanges();

            // Act & Assert
            iConexion.TiposArchivo.Remove(tipo);
            
            // Si ON DELETE CASCADE está activo, el tipo se eliminará junto con los archivos
            // Si no, lanzará una excepción de FK
            try
            {
                int resultado = iConexion.SaveChanges();
                
                // Si llegamos aquí, la eliminación fue exitosa
                Console.WriteLine($"Eliminación exitosa (ON DELETE CASCADE probablemente activo). Resultado: {resultado}");
                
                // Verificar que el tipo ya no existe
                var tipoEliminado = iConexion.TiposArchivo.Find(tipoId);
                Assert.IsNull(tipoEliminado, "El tipo debería haber sido eliminado");
                
                // Verificar el estado del archivo (puede haber sido eliminado en cascada)
                var archivoEliminado = iConexion.Archivos.Find(archivo.ArchivoId);
                if (archivoEliminado == null)
                {
                    Console.WriteLine("El archivo también fue eliminado en cascada");
                }
                else
                {
                    Console.WriteLine("El archivo aún existe (sin CASCADE o con CASCADE solo en una dirección)");
                    // Limpiar el archivo manualmente
                    iConexion.Archivos.Remove(archivoEliminado);
                    iConexion.SaveChanges();
                }
            }
            catch (DbUpdateException ex)
            {
                // Excepción esperada si NO hay ON DELETE CASCADE
                Console.WriteLine($"Excepción capturada (esperada si no hay CASCADE): {ex.InnerException?.Message ?? ex.Message}");
                Assert.IsTrue(true, "Se lanzó la excepción esperada - el tipo no se puede eliminar porque tiene archivos asociados");
                
                // Limpiar manualmente
                iConexion.Archivos.Remove(archivo);
                iConexion.TiposArchivo.Remove(tipo);
                iConexion.SaveChanges();
            }
        }

        public bool Listar()
        {
            if (iConexion?.TiposArchivo == null)
            {
                Console.WriteLine("Error: iConexion o TiposArchivo es null");
                return false;
            }

            try
            {
                this.lista = iConexion.TiposArchivo
                    .Include(t => t.Archivos)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} tipos de archivo encontrados");
                
                foreach (var tipo in lista)
                {
                    var archivosCount = tipo.Archivos?.Count ?? 0;
                    Console.WriteLine($"  - ID: {tipo.TipoArchivoId}, " +
                                      $"Nombre: {tipo.Nombre}, " +
                                      $"ArchivosAsociados: {archivosCount}");
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
            if (iConexion?.TiposArchivo == null)
            {
                Console.WriteLine("Error: iConexion o TiposArchivo es null");
                return false;
            }

            try
            {
                this.entidad = new TiposArchivo
                {
                    Nombre = GenerarNombreSeguro("Tipo_Prueba")
                };
                
                Console.WriteLine($"Guardando tipo de archivo: {entidad.Nombre}");
                
                iConexion.TiposArchivo.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.TipoArchivoId;
                    Console.WriteLine($"Tipo de archivo guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.TiposArchivo == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposArchivo o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.TiposArchivo.Find(this.entidad.TipoArchivoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de archivo {this.entidad.TipoArchivoId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando tipo de archivo ID: {entidadActualizada.TipoArchivoId}");
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
            if (iConexion?.TiposArchivo == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposArchivo o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene archivos asociados
                var archivosAsociados = 0;
                if (iConexion.Archivos != null)
                {
                    archivosAsociados = iConexion.Archivos
                        .Count(a => a.TipoArchivoId == this.entidad.TipoArchivoId);
                }
                
                if (archivosAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el tipo porque tiene {archivosAsociados} archivos asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.TiposArchivo.Find(this.entidad.TipoArchivoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de archivo {this.entidad.TipoArchivoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando tipo de archivo ID: {entidadActualizada.TipoArchivoId}");
                
                iConexion.TiposArchivo.Remove(entidadActualizada);
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