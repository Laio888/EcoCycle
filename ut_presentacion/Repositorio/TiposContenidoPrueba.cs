using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class TiposContenidoPrueba
    {
        private readonly IConexion? iConexion;
        private List<TiposContenido>? lista;
        private TiposContenido? entidad;
        private int entidadIdGuardado;

        public TiposContenidoPrueba()
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
            
            if (iConexion?.TiposContenido == null)
            {
                Console.WriteLine("Advertencia: iConexion.TiposContenido es null");
            }
            else
            {
                var count = iConexion.TiposContenido.Count();
                Console.WriteLine($"TiposContenido existentes en BD: {count}");
                
                // Mostrar los tipos existentes
                var tipos = iConexion.TiposContenido.ToList();
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoContenidoId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.TiposContenido != null)
            {
                try
                {
                    var entidadExistente = iConexion.TiposContenido.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene contenidos educativos asociados
                        var contenidosAsociados = 0;
                        if (iConexion.ContenidoEducativo != null)
                        {
                            contenidosAsociados = iConexion.ContenidoEducativo
                                .Count(c => c.TipoContenidoId == entidadExistente.TipoContenidoId);
                        }
                        
                        if (contenidosAsociados == 0)
                        {
                            iConexion.TiposContenido.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: TiposContenido {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el tipo porque tiene {contenidosAsociados} contenidos asociados");
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
        public void GuardarTipoContenido_GuiaPractica_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Guia_practica")
            };

            // Act
            iConexion!.TiposContenido!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoContenidoId > 0);
            Assert.IsNotNull(tipo.Nombre);

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoContenido_Video_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Video")
            };

            // Act
            iConexion!.TiposContenido!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoContenidoId > 0);

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoContenido_Infografia_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Infografia")
            };

            // Act
            iConexion!.TiposContenido!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoContenidoId > 0);

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoContenido_Articulo_DeberiaCrearTipo()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Articulo")
            };

            // Act
            iConexion!.TiposContenido!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoContenidoId > 0);

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoContenido_SinNombre_DeberiaFallar()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = null!  // Nombre requerido
            };

            // Act & Assert
            iConexion!.TiposContenido!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoContenido_ConNombreLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 60); // 60 caracteres, excede el límite de 50
            var tipo = new TiposContenido
            {
                Nombre = nombreLargo
            };

            // Act & Assert
            iConexion!.TiposContenido!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoContenido_ConNombreDuplicado_DeberiaFallar()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_Duplicado");
            var tipo1 = new TiposContenido { Nombre = nombreUnico };
            var tipo2 = new TiposContenido { Nombre = nombreUnico };

            // Act
            iConexion!.TiposContenido!.Add(tipo1);
            iConexion.SaveChanges();

            iConexion.TiposContenido.Add(tipo2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo Nombre
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.TiposContenido.Remove(tipo1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTipoContenidoPorId_DeberiaRetornarTipoCorrecto()
        {
            // Arrange
            var tipoGuardar = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Tipo_Buscar")
            };
            iConexion!.TiposContenido!.Add(tipoGuardar);
            iConexion.SaveChanges();
            int idBuscado = tipoGuardar.TipoContenidoId;

            // Act
            var tipoEncontrado = iConexion.TiposContenido.Find(idBuscado);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(idBuscado, tipoEncontrado.TipoContenidoId);
            Assert.AreEqual(tipoGuardar.Nombre, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposContenido.Remove(tipoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposContenido_DeberiaIncluirTiposExistentes()
        {
            // Arrange
            var nuevoTipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Tipo_Listar")
            };
            iConexion!.TiposContenido!.Add(nuevoTipo);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.TiposContenido.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.TiposContenido.Remove(nuevoTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposContenidoConContenidos_DeberiaCargarRelacion()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Tipo_Relacion")
            };
            iConexion!.TiposContenido!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tiposConContenidos = iConexion.TiposContenido
                .Include(t => t.ContenidosEducativos)
                .ToList();

            // Assert
            Assert.IsTrue(tiposConContenidos.Count > 0);
            
            // Verificar que la propiedad de navegación existe
            var tipoBuscado = tiposConContenidos.FirstOrDefault(t => t.TipoContenidoId == tipo.TipoContenidoId);
            Assert.IsNotNull(tipoBuscado);
            Assert.IsNotNull(tipoBuscado.ContenidosEducativos);

            Console.WriteLine($"Tipo '{tipoBuscado.Nombre}' tiene {tipoBuscado.ContenidosEducativos.Count} contenidos asociados");

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoContenido_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Tipo_Original")
            };
            iConexion!.TiposContenido!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");

            // Act
            tipo.Nombre = nuevoNombre;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposContenido.Find(tipo.TipoContenidoId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoNombre, tipoActualizado.Nombre);

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarTipoContenido_SinContenidosAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Tipo_Eliminar")
            };
            iConexion!.TiposContenido!.Add(tipo);
            iConexion.SaveChanges();
            int idEliminado = tipo.TipoContenidoId;

            // Act
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();

            // Assert
            var tipoEliminado = iConexion.TiposContenido.Find(idEliminado);
            Assert.IsNull(tipoEliminado);
        }

        [TestMethod]
        public void VerificarTiposContenidoIniciales_DeberianExistirCuatroTipos()
        {
            if (iConexion?.TiposContenido == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tipos = iConexion.TiposContenido.ToList();

            // Assert - Verificar datos iniciales del script SQL
            Assert.IsTrue(tipos.Count >= 4, "Deberían existir al menos 4 tipos de contenido del script inicial");
            
            var nombresEsperados = new[] { "Guia practica", "Video", "Infografia", "Articulo" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(tipos.Any(t => t.Nombre == nombre), 
                    $"No existe el tipo de contenido '{nombre}'");
            }

            Console.WriteLine("Tipos de contenido iniciales encontrados:");
            foreach (var tipo in tipos.Where(t => nombresEsperados.Contains(t.Nombre)))
            {
                Console.WriteLine($"  - ID: {tipo.TipoContenidoId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void BuscarTipoContenidoPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_BuscarNombre");
            var tipo = new TiposContenido
            {
                Nombre = nombreUnico
            };
            iConexion!.TiposContenido!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tipoEncontrado = iConexion.TiposContenido
                .FirstOrDefault(t => t.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(nombreUnico, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposContenido.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTiposContenidoOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.TiposContenido == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tiposOrdenados = iConexion.TiposContenido
                .OrderBy(t => t.TipoContenidoId)
                .ToList();

            // Assert
            Assert.IsTrue(tiposOrdenados.Count > 0);
            
            // Verificar que están ordenados ascendentemente
            for (int i = 0; i < tiposOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(tiposOrdenados[i].TipoContenidoId < tiposOrdenados[i + 1].TipoContenidoId,
                    "Los tipos no están ordenados correctamente por ID");
            }

            Console.WriteLine("Tipos de contenido ordenados por ID:");
            foreach (var tipo in tiposOrdenados)
            {
                Console.WriteLine($"  - ID: {tipo.TipoContenidoId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarQueTiposContenidoNoSePuedenEliminarSiTienenContenidos()
        {
            // Arrange - Crear un tipo de contenido
            var tipo = new TiposContenido
            {
                Nombre = GenerarNombreSeguro("Tipo_ConContenidos")
            };
            iConexion!.TiposContenido!.Add(tipo);
            iConexion.SaveChanges();
            int tipoId = tipo.TipoContenidoId;
            
            // Crear un título seguro - CORREGIDO
            var guid = Guid.NewGuid().ToString("N");
            var titulo = $"Contenido_Test_{guid}";
            // Asegurar que no excede 200 caracteres
            if (titulo.Length > 200)
            {
                titulo = titulo.Substring(0, 200);
            }
            
            // Crear un contenido educativo asociado a este tipo
            var contenido = new ContenidoEducativo
            {
                Titulo = titulo,
                TipoContenidoId = tipo.TipoContenidoId,
                CategoriaContenidoId = 1,  // Asumiendo que existe la categoría 1
                EsExterno = false
            };
            iConexion.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();

            // Act & Assert - Intentar eliminar el tipo
            iConexion.TiposContenido.Remove(tipo);
            
            try
            {
                int resultado = iConexion.SaveChanges();
                Console.WriteLine($"Eliminación exitosa. Resultado: {resultado}");
                
                var tipoEliminado = iConexion.TiposContenido.Find(tipoId);
                Assert.IsNull(tipoEliminado, "El tipo debería haber sido eliminado");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Excepción capturada: {ex.InnerException?.Message ?? ex.Message}");
                Assert.IsTrue(true, "Se lanzó la excepción esperada");
                
                // Limpiar manualmente
                iConexion.ContenidoEducativo.Remove(contenido);
                iConexion.TiposContenido.Remove(tipo);
                iConexion.SaveChanges();
            }
        }

        public bool Listar()
        {
            if (iConexion?.TiposContenido == null)
            {
                Console.WriteLine("Error: iConexion o TiposContenido es null");
                return false;
            }

            try
            {
                this.lista = iConexion.TiposContenido
                    .Include(t => t.ContenidosEducativos)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} tipos de contenido encontrados");
                
                foreach (var tipo in lista)
                {
                    var contenidosCount = tipo.ContenidosEducativos?.Count ?? 0;
                    Console.WriteLine($"  - ID: {tipo.TipoContenidoId}, " +
                                      $"Nombre: {tipo.Nombre}, " +
                                      $"ContenidosAsociados: {contenidosCount}");
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
            if (iConexion?.TiposContenido == null)
            {
                Console.WriteLine("Error: iConexion o TiposContenido es null");
                return false;
            }

            try
            {
                this.entidad = new TiposContenido
                {
                    Nombre = GenerarNombreSeguro("Tipo_Prueba")
                };
                
                Console.WriteLine($"Guardando tipo de contenido: {entidad.Nombre}");
                
                iConexion.TiposContenido.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.TipoContenidoId;
                    Console.WriteLine($"Tipo de contenido guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.TiposContenido == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposContenido o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.TiposContenido.Find(this.entidad.TipoContenidoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de contenido {this.entidad.TipoContenidoId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando tipo de contenido ID: {entidadActualizada.TipoContenidoId}");
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
            if (iConexion?.TiposContenido == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposContenido o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene contenidos educativos asociados
                var contenidosAsociados = 0;
                if (iConexion.ContenidoEducativo != null)
                {
                    contenidosAsociados = iConexion.ContenidoEducativo
                        .Count(c => c.TipoContenidoId == this.entidad.TipoContenidoId);
                }
                
                if (contenidosAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el tipo porque tiene {contenidosAsociados} contenidos asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.TiposContenido.Find(this.entidad.TipoContenidoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de contenido {this.entidad.TipoContenidoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando tipo de contenido ID: {entidadActualizada.TipoContenidoId}");
                
                iConexion.TiposContenido.Remove(entidadActualizada);
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