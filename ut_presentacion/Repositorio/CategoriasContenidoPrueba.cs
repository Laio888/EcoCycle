using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class CategoriasContenidoPrueba
    {
        private readonly IConexion? iConexion;
        private List<CategoriasContenido>? lista;
        private CategoriasContenido? entidad;
        private int entidadIdGuardado;

        public CategoriasContenidoPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 100 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 100)
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
            
            if (iConexion?.CategoriasContenido == null)
            {
                Console.WriteLine("Advertencia: iConexion.CategoriasContenido es null");
            }
            else
            {
                var count = iConexion.CategoriasContenido.Count();
                Console.WriteLine($"CategoriasContenido existentes en BD: {count}");
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.CategoriasContenido != null)
            {
                try
                {
                    var entidadExistente = iConexion.CategoriasContenido.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene contenido educativo asociado
                        var contenidoAsociado = 0;
                        if (iConexion.ContenidoEducativo != null)
                        {
                            contenidoAsociado = iConexion.ContenidoEducativo
                                .Count(c => c.CategoriaContenidoId == entidadExistente.CategoriaContenidoId);
                        }
                        
                        if (contenidoAsociado == 0)
                        {
                            iConexion.CategoriasContenido.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: CategoriaContenido {entidadIdGuardado} eliminada");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó la categoría porque tiene {contenidoAsociado} contenidos asociados");
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
        public void GuardarCategoria_ConDatosValidos_DeberiaCrearCategoria()
        {
            // Arrange
            var categoria = new CategoriasContenido
            {
                Nombre = GenerarNombreSeguro("Categoria_Test")
            };

            // Act
            iConexion!.CategoriasContenido!.Add(categoria);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(categoria.CategoriaContenidoId > 0);
            Assert.IsNotNull(categoria.Nombre);

            // Cleanup
            iConexion.CategoriasContenido.Remove(categoria);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarCategoria_ConNombreLargo_DeberiaTruncarOFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 150); // 150 caracteres, excede el límite de 100
            var categoria = new CategoriasContenido
            {
                Nombre = nombreLargo
            };

            // Act & Assert
            iConexion!.CategoriasContenido!.Add(categoria);
            
            // Debería lanzar una excepción por el tamaño del campo
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarCategoria_SinNombre_DeberiaFallar()
        {
            // Arrange
            var categoria = new CategoriasContenido
            {
                Nombre = null!  // Nombre requerido
            };

            // Act & Assert
            iConexion!.CategoriasContenido!.Add(categoria);
            
            // Debería lanzar una excepción por violación de NOT NULL
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerCategoriaPorId_DeberiaRetornarCategoriaCorrecta()
        {
            // Arrange
            var categoriaGuardar = new CategoriasContenido
            {
                Nombre = GenerarNombreSeguro("Categoria_Buscar")
            };
            iConexion!.CategoriasContenido!.Add(categoriaGuardar);
            iConexion.SaveChanges();
            int idBuscado = categoriaGuardar.CategoriaContenidoId;

            // Act
            var categoriaEncontrada = iConexion.CategoriasContenido.Find(idBuscado);

            // Assert
            Assert.IsNotNull(categoriaEncontrada);
            Assert.AreEqual(idBuscado, categoriaEncontrada.CategoriaContenidoId);
            Assert.AreEqual(categoriaGuardar.Nombre, categoriaEncontrada.Nombre);

            // Cleanup
            iConexion.CategoriasContenido.Remove(categoriaGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarCategorias_DeberiaIncluirCategoriasExistentes()
        {
            // Arrange
            var nuevaCategoria = new CategoriasContenido
            {
                Nombre = GenerarNombreSeguro("Categoria_Listar")
            };
            iConexion!.CategoriasContenido!.Add(nuevaCategoria);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.CategoriasContenido.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.CategoriasContenido.Remove(nuevaCategoria);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarCategoriasConContenido_DeberiaCargarRelacion()
        {
            // Arrange - Primero crear una categoría
            var categoria = new CategoriasContenido
            {
                Nombre = GenerarNombreSeguro("Categoria_Relacion")
            };
            iConexion!.CategoriasContenido!.Add(categoria);
            iConexion.SaveChanges();

            // Act
            var categoriasConContenido = iConexion.CategoriasContenido
                .Include(c => c.ContenidosEducativos)
                .ToList();

            // Assert
            Assert.IsTrue(categoriasConContenido.Count > 0);
            
            // Verificar que la propiedad de navegación existe
            var categoriaBuscada = categoriasConContenido.FirstOrDefault(c => c.CategoriaContenidoId == categoria.CategoriaContenidoId);
            Assert.IsNotNull(categoriaBuscada);
            Assert.IsNotNull(categoriaBuscada.ContenidosEducativos);

            Console.WriteLine($"Categoría '{categoriaBuscada.Nombre}' tiene {categoriaBuscada.ContenidosEducativos.Count} contenidos asociados");

            // Cleanup
            iConexion.CategoriasContenido.Remove(categoria);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarCategoria_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var categoria = new CategoriasContenido
            {
                Nombre = GenerarNombreSeguro("Categoria_Original")
            };
            iConexion!.CategoriasContenido!.Add(categoria);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Categoria_Modificada");

            // Act
            categoria.Nombre = nuevoNombre;
            var entry = iConexion.Entry(categoria);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var categoriaActualizada = iConexion.CategoriasContenido.Find(categoria.CategoriaContenidoId);
            Assert.IsNotNull(categoriaActualizada);
            Assert.AreEqual(nuevoNombre, categoriaActualizada.Nombre);

            // Cleanup
            iConexion.CategoriasContenido.Remove(categoria);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarCategoria_SinContenidoAsociado_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var categoria = new CategoriasContenido
            {
                Nombre = GenerarNombreSeguro("Categoria_Eliminar")
            };
            iConexion!.CategoriasContenido!.Add(categoria);
            iConexion.SaveChanges();
            int idEliminado = categoria.CategoriaContenidoId;

            // Act
            iConexion.CategoriasContenido.Remove(categoria);
            iConexion.SaveChanges();

            // Assert
            var categoriaEliminada = iConexion.CategoriasContenido.Find(idEliminado);
            Assert.IsNull(categoriaEliminada);
        }

        [TestMethod]
        public void VerificarCategoriasIniciales_DeberianExistirTresCategoriasSegunScript()
        {
            if (iConexion?.CategoriasContenido == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var categorias = iConexion.CategoriasContenido.ToList();

            // Assert - Verificar datos iniciales del script SQL
            Assert.IsTrue(categorias.Count >= 3, "Deberían existir al menos 3 categorías del script inicial");
            
            var nombresEsperados = new[] { "Compostaje domestico", "Separacion de residuos", "Impacto ambiental" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(categorias.Any(c => c.Nombre == nombre), 
                    $"No existe la categoría '{nombre}' en la BD");
            }

            Console.WriteLine("Categorías iniciales encontradas:");
            foreach (var cat in categorias.Where(c => nombresEsperados.Contains(c.Nombre)))
            {
                Console.WriteLine($"  - ID: {cat.CategoriaContenidoId}, Nombre: {cat.Nombre}");
            }
        }

        [TestMethod]
        public void BuscarCategoriaPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Categoria_BuscarNombre");
            var categoria = new CategoriasContenido
            {
                Nombre = nombreUnico
            };
            iConexion!.CategoriasContenido!.Add(categoria);
            iConexion.SaveChanges();

            // Act
            var categoriaEncontrada = iConexion.CategoriasContenido
                .FirstOrDefault(c => c.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(categoriaEncontrada);
            Assert.AreEqual(nombreUnico, categoriaEncontrada.Nombre);

            // Cleanup
            iConexion.CategoriasContenido.Remove(categoria);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void PrevenirDuplicados_DeberiaPermitirNombresUnicos()
        {
            // Arrange
            var nombreComun = GenerarNombreSeguro("Categoria_Duplicada");
            var categoria1 = new CategoriasContenido { Nombre = nombreComun };
            var categoria2 = new CategoriasContenido { Nombre = nombreComun };

            // Act
            iConexion!.CategoriasContenido!.Add(categoria1);
            iConexion.SaveChanges();

            iConexion.CategoriasContenido.Add(categoria2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo Nombre
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.CategoriasContenido.Remove(categoria1);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.CategoriasContenido == null)
            {
                Console.WriteLine("Error: iConexion o CategoriasContenido es null");
                return false;
            }

            try
            {
                this.lista = iConexion.CategoriasContenido
                    .Include(c => c.ContenidosEducativos)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} categorías encontradas");
                
                foreach (var categoria in lista)
                {
                    var contenidosCount = categoria.ContenidosEducativos?.Count ?? 0;
                    Console.WriteLine($"  - ID: {categoria.CategoriaContenidoId}, " +
                                      $"Nombre: {categoria.Nombre}, " +
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
            if (iConexion?.CategoriasContenido == null)
            {
                Console.WriteLine("Error: iConexion o CategoriasContenido es null");
                return false;
            }

            try
            {
                this.entidad = new CategoriasContenido
                {
                    Nombre = GenerarNombreSeguro("Categoria_Prueba")
                };
                
                Console.WriteLine($"Guardando categoría: {entidad.Nombre}");
                
                iConexion.CategoriasContenido.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.CategoriaContenidoId;
                    Console.WriteLine($"Categoría guardada con ID: {entidadIdGuardado}");
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
            if (iConexion?.CategoriasContenido == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, CategoriasContenido o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.CategoriasContenido.Find(this.entidad.CategoriaContenidoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Categoría {this.entidad.CategoriaContenidoId} no encontrada para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Categoria_Modificada");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando categoría ID: {entidadActualizada.CategoriaContenidoId}");
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error al modificar: {ex.Message}");
                return false;
            }
        }

        public bool Borrar()
        {
            if (iConexion?.CategoriasContenido == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, CategoriasContenido o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene contenido educativo asociado
                var contenidoAsociado = 0;
                if (iConexion.ContenidoEducativo != null)
                {
                    contenidoAsociado = iConexion.ContenidoEducativo
                        .Count(c => c.CategoriaContenidoId == this.entidad.CategoriaContenidoId);
                }
                
                if (contenidoAsociado > 0)
                {
                    Console.WriteLine($"No se puede borrar la categoría porque tiene {contenidoAsociado} contenidos asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.CategoriasContenido.Find(this.entidad.CategoriaContenidoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Categoría {this.entidad.CategoriaContenidoId} no encontrada para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando categoría ID: {entidadActualizada.CategoriaContenidoId}");
                
                iConexion.CategoriasContenido.Remove(entidadActualizada);
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