using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class ContenidoEducativoPrueba
    {
        private readonly IConexion? iConexion;
        private List<ContenidoEducativo>? lista;
        private ContenidoEducativo? entidad;
        private int entidadIdGuardado;
        private int tipoContenidoIdPrueba;
        private int categoriaContenidoIdPrueba;

        public ContenidoEducativoPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar título seguro (máx 200 caracteres)
        private string GenerarTituloSeguro(string prefijo, int maxLength = 200)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var titulo = $"{prefijo}_{guid}";
            if (titulo.Length > maxLength)
            {
                titulo = titulo.Substring(0, maxLength);
            }
            return titulo;
        }

        // Método auxiliar para generar fuente externa segura
        private string GenerarFuenteExterna()
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"https://ejemplo.com/contenido_{guid}";
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Obtener o crear un TipoContenido válido para las pruebas
            if (iConexion?.TiposContenido != null)
            {
                var tipoExistente = iConexion.TiposContenido.FirstOrDefault();
                if (tipoExistente != null)
                {
                    tipoContenidoIdPrueba = tipoExistente.TipoContenidoId;
                    Console.WriteLine($"Usando TipoContenido existente ID: {tipoContenidoIdPrueba}");
                }
                else
                {
                    // Crear un tipo de contenido de prueba
                    var nuevoTipo = new TiposContenido { Nombre = $"Tipo_Test_{Guid.NewGuid():N}".Substring(0, 50) };
                    iConexion.TiposContenido.Add(nuevoTipo);
                    iConexion.SaveChanges();
                    tipoContenidoIdPrueba = nuevoTipo.TipoContenidoId;
                    Console.WriteLine($"TipoContenido creado para prueba ID: {tipoContenidoIdPrueba}");
                }
            }
            
            // Obtener o crear una CategoriaContenido válida para las pruebas
            if (iConexion?.CategoriasContenido != null)
            {
                var categoriaExistente = iConexion.CategoriasContenido.FirstOrDefault();
                if (categoriaExistente != null)
                {
                    categoriaContenidoIdPrueba = categoriaExistente.CategoriaContenidoId;
                    Console.WriteLine($"Usando CategoriaContenido existente ID: {categoriaContenidoIdPrueba}");
                }
                else
                {
                    // Crear una categoría de prueba
                    var nuevaCategoria = new CategoriasContenido { Nombre = $"Categoria_Test_{Guid.NewGuid():N}".Substring(0, 100) };
                    iConexion.CategoriasContenido.Add(nuevaCategoria);
                    iConexion.SaveChanges();
                    categoriaContenidoIdPrueba = nuevaCategoria.CategoriaContenidoId;
                    Console.WriteLine($"CategoriaContenido creada para prueba ID: {categoriaContenidoIdPrueba}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar contenido creado
            if (entidadIdGuardado > 0 && iConexion?.ContenidoEducativo != null)
            {
                try
                {
                    var entidadExistente = iConexion.ContenidoEducativo.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.ContenidoEducativo.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: ContenidoEducativo {entidadIdGuardado} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando contenido: {ex.Message}");
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
        public void GuardarContenido_Interno_ConDatosValidos_DeberiaCrearContenido()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Interno"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                RecursoArchivoId = null,
                EsExterno = false,
                FuenteExterna = null
            };

            // Act
            iConexion!.ContenidoEducativo!.Add(contenido);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(contenido.ContenidoId > 0);
            Assert.IsFalse(contenido.EsExterno);

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarContenido_Externo_ConDatosValidos_DeberiaCrearContenido()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Externo"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                RecursoArchivoId = null,
                EsExterno = true,
                FuenteExterna = GenerarFuenteExterna()
            };

            // Act
            iConexion!.ContenidoEducativo!.Add(contenido);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(contenido.ContenidoId > 0);
            Assert.IsTrue(contenido.EsExterno);
            Assert.IsNotNull(contenido.FuenteExterna);

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarContenido_SinTitulo_DeberiaFallar()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = null!,
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };

            // Act & Assert
            iConexion!.ContenidoEducativo!.Add(contenido);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarContenido_ConTituloMuyLargo_DeberiaFallar()
        {
            // Arrange
            var tituloLargo = new string('A', 250); // 250 caracteres, excede el límite de 200
            var contenido = new ContenidoEducativo
            {
                Titulo = tituloLargo,
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };

            // Act & Assert
            iConexion!.ContenidoEducativo!.Add(contenido);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerContenidoPorId_DeberiaRetornarContenidoCorrecto()
        {
            // Arrange
            var contenidoGuardar = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Buscar"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };
            iConexion!.ContenidoEducativo!.Add(contenidoGuardar);
            iConexion.SaveChanges();
            int idBuscado = contenidoGuardar.ContenidoId;

            // Act
            var contenidoEncontrado = iConexion.ContenidoEducativo.Find(idBuscado);

            // Assert
            Assert.IsNotNull(contenidoEncontrado);
            Assert.AreEqual(idBuscado, contenidoEncontrado.ContenidoId);
            Assert.AreEqual(contenidoGuardar.Titulo, contenidoEncontrado.Titulo);

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenidoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarContenidos_DeberiaIncluirContenidosExistentes()
        {
            // Arrange
            var nuevoContenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Listar"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };
            iConexion!.ContenidoEducativo!.Add(nuevoContenido);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.ContenidoEducativo.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.ContenidoEducativo.Remove(nuevoContenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarContenidosConRelaciones_DeberiaCargarTipoYCategoria()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Relaciones"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };
            iConexion!.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();

            // Act
            var contenidoConRelaciones = iConexion.ContenidoEducativo
                .Include(c => c.TipoContenido)
                .Include(c => c.CategoriaContenido)
                .FirstOrDefault(c => c.ContenidoId == contenido.ContenidoId);

            // Assert
            Assert.IsNotNull(contenidoConRelaciones);
            Assert.IsNotNull(contenidoConRelaciones.TipoContenido);
            Assert.IsNotNull(contenidoConRelaciones.CategoriaContenido);

            Console.WriteLine($"Contenido: {contenidoConRelaciones.Titulo}");
            Console.WriteLine($"  Tipo: {contenidoConRelaciones.TipoContenido.Nombre}");
            Console.WriteLine($"  Categoría: {contenidoConRelaciones.CategoriaContenido.Nombre}");

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarContenido_CambiarTitulo_DeberiaActualizar()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Original"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };
            iConexion!.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();
            string nuevoTitulo = GenerarTituloSeguro("Contenido_Modificado");

            // Act
            contenido.Titulo = nuevoTitulo;
            var entry = iConexion.Entry(contenido);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var contenidoActualizado = iConexion.ContenidoEducativo.Find(contenido.ContenidoId);
            Assert.IsNotNull(contenidoActualizado);
            Assert.AreEqual(nuevoTitulo, contenidoActualizado.Titulo);

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarContenido_DeExternoAInterno_DeberiaActualizar()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Cambio"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = true,
                FuenteExterna = GenerarFuenteExterna()
            };
            iConexion!.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();

            // Act
            contenido.EsExterno = false;
            contenido.FuenteExterna = null;
            var entry = iConexion.Entry(contenido);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var contenidoActualizado = iConexion.ContenidoEducativo.Find(contenido.ContenidoId);
            Assert.IsNotNull(contenidoActualizado);
            Assert.IsFalse(contenidoActualizado.EsExterno);
            Assert.IsNull(contenidoActualizado.FuenteExterna);

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarContenido_SinUsuariosAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Eliminar"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };
            iConexion!.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();
            int idEliminado = contenido.ContenidoId;

            // Act
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();

            // Assert
            var contenidoEliminado = iConexion.ContenidoEducativo.Find(idEliminado);
            Assert.IsNull(contenidoEliminado);
        }

        [TestMethod]
        public void VerificarFechaPublicacionAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var contenido = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Fecha"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };

            // Act
            iConexion!.ContenidoEducativo!.Add(contenido);
            iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(contenido).Reload();

            // Assert
            Assert.IsTrue(contenido.FechaPublicacion != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(contenido.FechaPublicacion.Date <= DateTime.Today, 
                "La fecha asignada es futura");

            Console.WriteLine($"FechaPublicacion asignada por BD: {contenido.FechaPublicacion}");

            // Cleanup
            iConexion.ContenidoEducativo.Remove(contenido);
            iConexion.SaveChanges();
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

            // Assert
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
        public void VerificarCategoriasContenidoIniciales_DeberianExistirTresCategorias()
        {
            if (iConexion?.CategoriasContenido == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var categorias = iConexion.CategoriasContenido.ToList();

            // Assert
            Assert.IsTrue(categorias.Count >= 3, "Deberían existir al menos 3 categorías del script inicial");
            
            var nombresEsperados = new[] { "Compostaje domestico", "Separacion de residuos", "Impacto ambiental" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(categorias.Any(c => c.Nombre == nombre), 
                    $"No existe la categoría '{nombre}'");
            }

            Console.WriteLine("Categorías de contenido iniciales encontradas:");
            foreach (var cat in categorias.Where(c => nombresEsperados.Contains(c.Nombre)))
            {
                Console.WriteLine($"  - ID: {cat.CategoriaContenidoId}, Nombre: {cat.Nombre}");
            }
        }

        [TestMethod]
        public void FiltrarContenidosPorTipo_DeberiaRetornarResultadosCorrectos()
        {
            // Arrange
            var contenido1 = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Tipo1"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = false
            };
            var contenido2 = new ContenidoEducativo
            {
                Titulo = GenerarTituloSeguro("Contenido_Tipo2"),
                TipoContenidoId = tipoContenidoIdPrueba,
                CategoriaContenidoId = categoriaContenidoIdPrueba,
                EsExterno = true,
                FuenteExterna = GenerarFuenteExterna()
            };
            iConexion!.ContenidoEducativo!.AddRange(contenido1, contenido2);
            iConexion.SaveChanges();

            // Act
            var contenidosExternos = iConexion.ContenidoEducativo
                .Where(c => c.EsExterno == true)
                .ToList();

            // Assert
            Assert.IsTrue(contenidosExternos.Count >= 1);
            Assert.IsTrue(contenidosExternos.All(c => c.EsExterno == true));

            // Cleanup
            iConexion.ContenidoEducativo.RemoveRange(contenido1, contenido2);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.ContenidoEducativo == null)
            {
                Console.WriteLine("Error: iConexion o ContenidoEducativo es null");
                return false;
            }

            try
            {
                this.lista = iConexion.ContenidoEducativo
                    .Include(c => c.TipoContenido)
                    .Include(c => c.CategoriaContenido)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} contenidos encontrados");
                
                foreach (var contenido in lista)
                {
                    Console.WriteLine($"  - ID: {contenido.ContenidoId}, " +
                                      $"Título: {contenido.Titulo}, " +
                                      $"Tipo: {contenido.TipoContenido?.Nombre ?? "N/A"}, " +
                                      $"Categoría: {contenido.CategoriaContenido?.Nombre ?? "N/A"}, " +
                                      $"Externo: {contenido.EsExterno}, " +
                                      $"Fecha: {contenido.FechaPublicacion:yyyy-MM-dd}");
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
            if (iConexion?.ContenidoEducativo == null)
            {
                Console.WriteLine("Error: iConexion o ContenidoEducativo es null");
                return false;
            }

            try
            {
                this.entidad = new ContenidoEducativo
                {
                    Titulo = GenerarTituloSeguro("Contenido_Prueba"),
                    TipoContenidoId = tipoContenidoIdPrueba,
                    CategoriaContenidoId = categoriaContenidoIdPrueba,
                    EsExterno = false
                };
                
                Console.WriteLine($"Guardando contenido: {entidad.Titulo}");
                
                iConexion.ContenidoEducativo.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.ContenidoId;
                    Console.WriteLine($"Contenido guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.ContenidoEducativo == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, ContenidoEducativo o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.ContenidoEducativo.Find(this.entidad.ContenidoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Contenido {this.entidad.ContenidoId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoTitulo = GenerarTituloSeguro("Contenido_Modificado");
                entidadActualizada.Titulo = nuevoTitulo;
                
                Console.WriteLine($"Modificando contenido ID: {entidadActualizada.ContenidoId}");
                Console.WriteLine($"  Nuevo título: {nuevoTitulo}");
                
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
            if (iConexion?.ContenidoEducativo == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, ContenidoEducativo o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene usuarios asociados (UsuariosContenidoVisto)
                var usuariosAsociados = 0;
                if (iConexion.UsuariosContenidoVisto != null)
                {
                    usuariosAsociados = iConexion.UsuariosContenidoVisto
                        .Count(ucv => ucv.ContenidoId == this.entidad.ContenidoId);
                }
                
                if (usuariosAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el contenido porque tiene {usuariosAsociados} usuarios asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.ContenidoEducativo.Find(this.entidad.ContenidoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Contenido {this.entidad.ContenidoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando contenido ID: {entidadActualizada.ContenidoId}");
                
                iConexion.ContenidoEducativo.Remove(entidadActualizada);
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