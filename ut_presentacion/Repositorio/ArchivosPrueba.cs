using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class ArchivosPrueba
    {
        private readonly IConexion? iConexion;
        private List<Archivos>? lista;
        private Archivos? entidad;
        private int entidadIdGuardado;

        public ArchivosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
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
        public void GuardarArchivoImagen_DeberiaCrearArchivo()
        {
            // Arrange
            var archivo = EntidadesNucleo.Archivos(EntidadesNucleo.TiposArchivoIds.Imagen);

            // Act
            iConexion!.Archivos!.Add(archivo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(archivo.ArchivoId > 0);
            Assert.IsFalse(archivo.EsExterno);
            Assert.AreEqual(EntidadesNucleo.TiposArchivoIds.Imagen, archivo.TipoArchivoId);

            // Cleanup
            iConexion.Archivos.Remove(archivo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarArchivoExterno_DeberiaCrearArchivoConEsExternoTrue()
        {
            // Arrange
            var archivo = new Archivos
            {
                Url = "https://www.youtube.com/watch?v=ejemplo",
                TipoArchivoId = EntidadesNucleo.TiposArchivoIds.EnlaceExterno,
                EsExterno = true,
                Proveedor = "YouTube",
                Descripcion = "Video tutorial de compostaje"
            };

            // Act
            iConexion!.Archivos!.Add(archivo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(archivo.EsExterno);
            Assert.IsNotNull(archivo.Proveedor);
            Assert.AreEqual("YouTube", archivo.Proveedor);

            // Cleanup
            iConexion.Archivos.Remove(archivo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarArchivoPDF_DeberiaCrearArchivo()
        {
            // Arrange
            var archivo = new Archivos
            {
                Url = "https://storage.ecocycle.com/manual_compostaje.pdf",
                TipoArchivoId = EntidadesNucleo.TiposArchivoIds.PDF,
                EsExterno = false,
                Proveedor = "Local",
                Descripcion = "Manual de compostaje para principiantes"
            };

            // Act
            iConexion!.Archivos!.Add(archivo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsFalse(archivo.EsExterno);
            Assert.AreEqual(EntidadesNucleo.TiposArchivoIds.PDF, archivo.TipoArchivoId);

            // Cleanup
            iConexion.Archivos.Remove(archivo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerArchivoPorId_DeberiaRetornarArchivoCorrecto()
        {
            // Arrange
            var archivoGuardar = EntidadesNucleo.Archivos(EntidadesNucleo.TiposArchivoIds.Imagen);
            iConexion!.Archivos!.Add(archivoGuardar);
            iConexion.SaveChanges();
            int idBuscado = archivoGuardar.ArchivoId;

            // Act
            var archivoEncontrado = iConexion.Archivos.Find(idBuscado);

            // Assert
            Assert.IsNotNull(archivoEncontrado);
            Assert.AreEqual(idBuscado, archivoEncontrado.ArchivoId);
            Assert.AreEqual(archivoGuardar.Url, archivoEncontrado.Url);

            // Cleanup
            iConexion.Archivos.Remove(archivoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarArchivosPorTipo_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var archivoImagen = EntidadesNucleo.Archivos(EntidadesNucleo.TiposArchivoIds.Imagen);
            var archivoVideo = new Archivos
            {
                Url = "https://storage.ecocycle.com/video_compostaje.mp4",
                TipoArchivoId = EntidadesNucleo.TiposArchivoIds.Video,
                EsExterno = false,
                Proveedor = "Local",
                Descripcion = "Video de compostaje"
            };

            iConexion!.Archivos!.AddRange(archivoImagen, archivoVideo);
            iConexion.SaveChanges();

            // Act
            var archivosImagen = iConexion.Archivos
                .Where(a => a.TipoArchivoId == EntidadesNucleo.TiposArchivoIds.Imagen)
                .ToList();

            // Assert
            Assert.IsTrue(archivosImagen.Count > 0);
            Assert.IsTrue(archivosImagen.All(a => a.TipoArchivoId == EntidadesNucleo.TiposArchivoIds.Imagen));

            // Cleanup
            iConexion.Archivos.RemoveRange(archivoImagen, archivoVideo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarArchivo_CambiarDescripcion_DeberiaActualizar()
        {
            // Arrange
            var archivo = EntidadesNucleo.Archivos(EntidadesNucleo.TiposArchivoIds.Imagen);
            iConexion!.Archivos!.Add(archivo);
            iConexion.SaveChanges();
            string nuevaDescripcion = "Descripción modificada en prueba";

            // Act
            archivo.Descripcion = nuevaDescripcion;
            var entry = iConexion.Entry(archivo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var archivoActualizado = iConexion.Archivos.Find(archivo.ArchivoId);
            Assert.IsNotNull(archivoActualizado);
            Assert.AreEqual(nuevaDescripcion, archivoActualizado.Descripcion);

            // Cleanup
            iConexion.Archivos.Remove(archivo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarArchivo_NoDebeExistirEnConsultaPosterior()
        {
            // Arrange
            var archivo = EntidadesNucleo.Archivos(EntidadesNucleo.TiposArchivoIds.Imagen);
            iConexion!.Archivos!.Add(archivo);
            iConexion.SaveChanges();
            int idEliminado = archivo.ArchivoId;

            // Act
            iConexion.Archivos.Remove(archivo);
            iConexion.SaveChanges();

            // Assert
            var archivoEliminado = iConexion.Archivos.Find(idEliminado);
            Assert.IsNull(archivoEliminado);
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Verificar que los TiposArchivo existen en la BD
            VerificarTiposArchivoExisten();
        }

        private void VerificarTiposArchivoExisten()
        {
            if (iConexion?.TiposArchivo == null) 
            {
                Console.WriteLine("Advertencia: iConexion.TiposArchivo es null");
                return;
            }

            var tiposExistentes = iConexion.TiposArchivo.ToList();
            
            if (tiposExistentes.Count == 0)
            {
                Console.WriteLine("ADVERTENCIA: No hay TiposArchivo en la BD. Insertando datos iniciales...");
                
                // Insertar tipos de archivo si no existen
                var tipos = new[]
                {
                    new TiposArchivo { Nombre = "Imagen" },
                    new TiposArchivo { Nombre = "Video" },
                    new TiposArchivo { Nombre = "PDF" },
                    new TiposArchivo { Nombre = "EnlaceExterno" }
                };
                
                iConexion.TiposArchivo.AddRange(tipos);
                iConexion.SaveChanges();
                Console.WriteLine("TiposArchivo insertados correctamente");
            }
            else
            {
                Console.WriteLine($"TiposArchivo existentes: {tiposExistentes.Count}");
                foreach (var tipo in tiposExistentes)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoArchivoId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.Archivos != null)
            {
                try
                {
                    var entidadExistente = iConexion.Archivos.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.Archivos.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: Archivo {entidadIdGuardado} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en limpieza: {ex.Message}");
                }
            }
        }

        public bool Listar()
        {
            if (iConexion?.Archivos == null)
            {
                Console.WriteLine("Error: iConexion o Archivos es null");
                return false;
            }

            try
            {
                this.lista = iConexion.Archivos
                    .Include(a => a.TipoArchivo)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} archivos encontrados");
                
                foreach (var archivo in lista)
                {
                    Console.WriteLine($"  - ID: {archivo.ArchivoId}, URL: {archivo.Url}, Tipo: {archivo.TipoArchivo?.Nombre}");
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
            if (iConexion?.Archivos == null)
            {
                Console.WriteLine("Error: iConexion o Archivos es null");
                return false;
            }

            try
            {
                this.entidad = EntidadesNucleo.Archivos(EntidadesNucleo.TiposArchivoIds.Imagen);
                
                Console.WriteLine($"Guardando archivo: {entidad.Url}, TipoArchivoId: {entidad.TipoArchivoId}");
                
                iConexion.Archivos.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.ArchivoId;
                    Console.WriteLine($"Archivo guardado con ID: {entidadIdGuardado}");
                    return true;
                }
                
                Console.WriteLine("No se guardó ningún archivo");
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
            if (iConexion?.Archivos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Archivos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.Archivos.Find(this.entidad.ArchivoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Archivo {this.entidad.ArchivoId} no encontrado para modificar");
                    return false;
                }
                
                // CORREGIDO: Usar variable auxiliar para el GUID truncado
                var guidShort = Guid.NewGuid().ToString("N").Substring(0, 8);
                string nuevaUrl = $"https://storage.ecocycle.com/modified_{guidShort}.pdf";
                
                entidadActualizada.Url = nuevaUrl;
                entidadActualizada.Descripcion = "Archivo modificado en prueba";
                
                Console.WriteLine($"Modificando archivo ID: {entidadActualizada.ArchivoId}");
                Console.WriteLine($"  Nueva URL: {nuevaUrl}");
                
                var entry = iConexion.Entry(entidadActualizada);
                entry.State = EntityState.Modified;
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    // Actualizar la entidad local
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
            if (iConexion?.Archivos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Archivos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.Archivos.Find(this.entidad.ArchivoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Archivo {this.entidad.ArchivoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando archivo ID: {entidadActualizada.ArchivoId}");
                
                iConexion.Archivos.Remove(entidadActualizada);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = 0;
                    Console.WriteLine("Archivo borrado correctamente");
                    return true;
                }
                
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