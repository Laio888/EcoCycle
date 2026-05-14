using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class RecompensasPrueba
    {
        private readonly IConexion? iConexion;
        private List<Recompensas>? lista;
        private Recompensas? entidad;
        private int entidadIdGuardado;
        private int tipoRecompensaIdPrueba;

        public RecompensasPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 100 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 100)
        {
            var guid = Guid.NewGuid().ToString("N");
            if (guid.Length > 8)
            {
                guid = guid.Substring(0, 8);
            }
            var nombre = $"{prefijo}_{guid}";
            if (nombre.Length > maxLength)
            {
                nombre = nombre.Substring(0, maxLength);
            }
            return nombre;
        }

        // Método auxiliar para generar descripción segura (máx 500 caracteres)
        private string GenerarDescripcionSegura(string prefijo, int maxLength = 500)
        {
            var guid = Guid.NewGuid().ToString("N");
            if (guid.Length > 8)
            {
                guid = guid.Substring(0, 8);
            }
            var descripcion = $"{prefijo}_{guid} - Esta es una descripción de prueba para la recompensa. Incluye información sobre los beneficios y requisitos para canjearla.";
            if (descripcion.Length > maxLength)
            {
                descripcion = descripcion.Substring(0, maxLength);
            }
            return descripcion;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Obtener TipoRecompensa válido (1=Digital, 2=Tangible)
            if (iConexion?.TiposRecompensa != null)
            {
                var tipoDigital = iConexion.TiposRecompensa.FirstOrDefault(t => t.Nombre == "Digital");
                if (tipoDigital != null)
                {
                    tipoRecompensaIdPrueba = tipoDigital.TipoRecompensaId;
                    Console.WriteLine($"Usando TipoRecompensa: Digital (ID: {tipoRecompensaIdPrueba})");
                }
                else
                {
                    var tipoExistente = iConexion.TiposRecompensa.FirstOrDefault();
                    if (tipoExistente != null)
                    {
                        tipoRecompensaIdPrueba = tipoExistente.TipoRecompensaId;
                        Console.WriteLine($"Usando TipoRecompensa existente ID: {tipoRecompensaIdPrueba}, Nombre: {tipoExistente.Nombre}");
                    }
                    else
                    {
                        // Crear un tipo de recompensa de prueba
                        var nuevoTipo = new TiposRecompensa { Nombre = "Digital" };
                        iConexion.TiposRecompensa.Add(nuevoTipo);
                        iConexion.SaveChanges();
                        tipoRecompensaIdPrueba = nuevoTipo.TipoRecompensaId;
                        Console.WriteLine($"TipoRecompensa creado para prueba ID: {tipoRecompensaIdPrueba}");
                    }
                }
            }
            
            // Mostrar tipos de recompensa existentes
            if (iConexion?.TiposRecompensa != null)
            {
                var tipos = iConexion.TiposRecompensa.ToList();
                Console.WriteLine("Tipos de recompensa disponibles:");
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoRecompensaId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar recompensa creada
            if (entidadIdGuardado > 0 && iConexion?.Recompensas != null)
            {
                try
                {
                    var entidadExistente = iConexion.Recompensas.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene canjes asociados
                        var canjesAsociados = 0;
                        if (iConexion.CanjesRecompensas != null)
                        {
                            canjesAsociados = iConexion.CanjesRecompensas
                                .Count(c => c.RecompensaId == entidadExistente.RecompensaId);
                        }
                        
                        if (canjesAsociados == 0)
                        {
                            iConexion.Recompensas.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: Recompensa {entidadIdGuardado} eliminada");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó la recompensa porque tiene {canjesAsociados} canjes asociados");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando recompensa: {ex.Message}");
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
        public void GuardarRecompensa_Digital_Ilimitada_ConDatosValidos_DeberiaCrearRecompensa()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Digital"),
                Descripcion = GenerarDescripcionSegura("Recompensa digital ilimitada"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 500,
                StockDisponible = null,
                EsIlimitado = true,
                FechaVigenciaDesde = DateTime.Today,
                FechaVigenciaHasta = DateTime.Today.AddMonths(6),
                ImagenArchivoId = null
            };

            // Act
            iConexion!.Recompensas!.Add(recompensa);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(recompensa.RecompensaId > 0);
            Assert.IsTrue(recompensa.EsIlimitado);
            Assert.IsNull(recompensa.StockDisponible);
            Assert.AreEqual(500, recompensa.CostoPuntos);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarRecompensa_Tangible_ConStock_DeberiaCrearRecompensa()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Tangible"),
                Descripcion = GenerarDescripcionSegura("Recompensa tangible con stock limitado"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 1000,
                StockDisponible = 25,
                EsIlimitado = false,
                FechaVigenciaDesde = DateTime.Today,
                FechaVigenciaHasta = DateTime.Today.AddMonths(3),
                ImagenArchivoId = null
            };

            // Act
            iConexion!.Recompensas!.Add(recompensa);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(recompensa.RecompensaId > 0);
            Assert.IsFalse(recompensa.EsIlimitado);
            Assert.IsNotNull(recompensa.StockDisponible);
            Assert.AreEqual(25, recompensa.StockDisponible);
            Assert.AreEqual(1000, recompensa.CostoPuntos);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarRecompensa_SinFechaVigencia_DeberiaCrearRecompensa()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_SinFecha"),
                Descripcion = GenerarDescripcionSegura("Recompensa sin fecha de vigencia"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 300,
                StockDisponible = null,
                EsIlimitado = true,
                FechaVigenciaDesde = null,
                FechaVigenciaHasta = null,
                ImagenArchivoId = null
            };

            // Act
            iConexion!.Recompensas!.Add(recompensa);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(recompensa.RecompensaId > 0);
            Assert.IsNull(recompensa.FechaVigenciaDesde);
            Assert.IsNull(recompensa.FechaVigenciaHasta);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarRecompensa_SinNombre_DeberiaFallar()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = null!,
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarRecompensa_ConNombreMuyLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 150); // 150 caracteres, excede el límite de 100
            var recompensa = new Recompensas
            {
                Nombre = nombreLargo,
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarRecompensa_ConCostoPuntosCero_DeberiaFallar()
        {
            // Arrange - CHECK constraint (CostoPuntos > 0)
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_CostoCero"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 0,
                EsIlimitado = true
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarRecompensa_ConCostoPuntosNegativo_DeberiaFallar()
        {
            // Arrange - CHECK constraint (CostoPuntos > 0)
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_CostoNegativo"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = -100,
                EsIlimitado = true
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarRecompensa_IlimitadaConStock_DeberiaFallar()
        {
            // Arrange - CHECK constraint (EsIlimitado=1 y StockDisponible debe ser NULL)
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_IlimitadaConStock"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                StockDisponible = 10,
                EsIlimitado = true
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarRecompensa_TangibleSinStock_DeberiaFallar()
        {
            // Arrange - CHECK constraint (EsIlimitado=0 y StockDisponible debe ser NOT NULL)
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_TangibleSinStock"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                StockDisponible = null,
                EsIlimitado = false
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarRecompensa_ConFechaVigenciaInvalida_DeberiaFallar()
        {
            // Arrange - CHECK constraint (FechaVigenciaDesde <= FechaVigenciaHasta)
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_FechaInvalida"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true,
                FechaVigenciaDesde = DateTime.Today.AddMonths(6),
                FechaVigenciaHasta = DateTime.Today
            };

            // Act & Assert
            iConexion!.Recompensas!.Add(recompensa);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void ObtenerRecompensaPorId_DeberiaRetornarRecompensaCorrecta()
        {
            // Arrange
            var recompensaGuardar = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Buscar"),
                Descripcion = GenerarDescripcionSegura("Recompensa para búsqueda"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 200,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.Add(recompensaGuardar);
            iConexion.SaveChanges();
            int idBuscado = recompensaGuardar.RecompensaId;

            // Act
            var recompensaEncontrada = iConexion.Recompensas.Find(idBuscado);

            // Assert
            Assert.IsNotNull(recompensaEncontrada);
            Assert.AreEqual(idBuscado, recompensaEncontrada.RecompensaId);
            Assert.AreEqual(recompensaGuardar.Nombre, recompensaEncontrada.Nombre);
            Assert.AreEqual(recompensaGuardar.CostoPuntos, recompensaEncontrada.CostoPuntos);

            // Cleanup
            iConexion.Recompensas.Remove(recompensaGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRecompensas_DeberiaIncluirRecompensasExistentes()
        {
            // Arrange
            var nuevaRecompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Listar"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 150,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.Add(nuevaRecompensa);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.Recompensas.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.Recompensas.Remove(nuevaRecompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRecompensasConRelaciones_DeberiaCargarTipoRecompensa()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Relacion"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 300,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();

            // Act
            var recompensaConRelacion = iConexion.Recompensas
                .Include(r => r.TipoRecompensa)
                .FirstOrDefault(r => r.RecompensaId == recompensa.RecompensaId);

            // Assert
            Assert.IsNotNull(recompensaConRelacion);
            Assert.IsNotNull(recompensaConRelacion.TipoRecompensa);

            Console.WriteLine($"Recompensa: {recompensaConRelacion.Nombre}");
            Console.WriteLine($"  Tipo: {recompensaConRelacion.TipoRecompensa.Nombre}");
            Console.WriteLine($"  Costo: {recompensaConRelacion.CostoPuntos} puntos");
            Console.WriteLine($"  Ilimitada: {recompensaConRelacion.EsIlimitado}");

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRecompensasPorTipo_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var recompensa1 = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Tipo1"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true
            };
            var recompensa2 = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Tipo2"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 200,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.AddRange(recompensa1, recompensa2);
            iConexion.SaveChanges();

            // Act
            var recompensasPorTipo = iConexion.Recompensas
                .Where(r => r.TipoRecompensaId == tipoRecompensaIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(recompensasPorTipo.Count >= 2);
            Assert.IsTrue(recompensasPorTipo.All(r => r.TipoRecompensaId == tipoRecompensaIdPrueba));

            // Cleanup
            iConexion.Recompensas.RemoveRange(recompensa1, recompensa2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRecompensasDisponibles_ConStockYCantidad_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var recompensaDigital = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Digital_Disponible"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true,
                StockDisponible = null
            };
            var recompensaTangible = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Tangible_ConStock"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 200,
                EsIlimitado = false,
                StockDisponible = 5
            };
            var recompensaAgotada = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Tangible_Agotada"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 150,
                EsIlimitado = false,
                StockDisponible = 0
            };
            iConexion!.Recompensas!.AddRange(recompensaDigital, recompensaTangible, recompensaAgotada);
            iConexion.SaveChanges();

            // Act
            var recompensasConStock = iConexion.Recompensas
                .Where(r => r.EsIlimitado == true || (r.StockDisponible.HasValue && r.StockDisponible > 0))
                .ToList();

            // Assert
            Assert.IsTrue(recompensasConStock.Count >= 2);
            Assert.IsTrue(recompensasConStock.Any(r => r.EsIlimitado == true));
            Assert.IsTrue(recompensasConStock.Any(r => r.StockDisponible > 0));

            // Cleanup
            iConexion.Recompensas.RemoveRange(recompensaDigital, recompensaTangible, recompensaAgotada);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarRecompensa_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Original"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Recompensa_Modificada");

            // Act
            recompensa.Nombre = nuevoNombre;
            var entry = iConexion.Entry(recompensa);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var recompensaActualizada = iConexion.Recompensas.Find(recompensa.RecompensaId);
            Assert.IsNotNull(recompensaActualizada);
            Assert.AreEqual(nuevoNombre, recompensaActualizada.Nombre);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarRecompensa_CambiarCostoPuntos_DeberiaActualizar()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_CostoOriginal"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();

            // Act
            recompensa.CostoPuntos = 350;
            var entry = iConexion.Entry(recompensa);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var recompensaActualizada = iConexion.Recompensas.Find(recompensa.RecompensaId);
            Assert.IsNotNull(recompensaActualizada);
            Assert.AreEqual(350, recompensaActualizada.CostoPuntos);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarRecompensa_ReducirStock_DeberiaActualizar()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_StockOriginal"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                StockDisponible = 10,
                EsIlimitado = false
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();

            // Act
            recompensa.StockDisponible = 8;
            var entry = iConexion.Entry(recompensa);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var recompensaActualizada = iConexion.Recompensas.Find(recompensa.RecompensaId);
            Assert.IsNotNull(recompensaActualizada);
            Assert.AreEqual(8, recompensaActualizada.StockDisponible);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarRecompensa_CambiarAIlimitada_SinStock_DeberiaActualizar()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_CambioAIlimitada"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                StockDisponible = 10,
                EsIlimitado = false
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();

            // Act
            recompensa.EsIlimitado = true;
            recompensa.StockDisponible = null;
            var entry = iConexion.Entry(recompensa);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var recompensaActualizada = iConexion.Recompensas.Find(recompensa.RecompensaId);
            Assert.IsNotNull(recompensaActualizada);
            Assert.IsTrue(recompensaActualizada.EsIlimitado);
            Assert.IsNull(recompensaActualizada.StockDisponible);

            // Cleanup
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarRecompensa_SinCanjesAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Eliminar"),
                TipoRecompensaId = tipoRecompensaIdPrueba,
                CostoPuntos = 100,
                EsIlimitado = true
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();
            int idEliminado = recompensa.RecompensaId;

            // Act
            iConexion.Recompensas.Remove(recompensa);
            iConexion.SaveChanges();

            // Assert
            var recompensaEliminada = iConexion.Recompensas.Find(idEliminado);
            Assert.IsNull(recompensaEliminada);
        }

        [TestMethod]
        public void VerificarTiposRecompensaIniciales_DeberianExistirDigitalYTangible()
        {
            if (iConexion?.TiposRecompensa == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tipos = iConexion.TiposRecompensa.ToList();

            // Assert
            Assert.IsTrue(tipos.Count >= 2, "Deberían existir al menos 2 tipos de recompensa del script inicial");
            
            var tipoDigital = tipos.FirstOrDefault(t => t.Nombre == "Digital");
            var tipoTangible = tipos.FirstOrDefault(t => t.Nombre == "Tangible");

            Assert.IsNotNull(tipoDigital, "No existe el tipo 'Digital'");
            Assert.IsNotNull(tipoTangible, "No existe el tipo 'Tangible'");

            Console.WriteLine("Tipos de recompensa iniciales:");
            Console.WriteLine($"  - Digital: ID {tipoDigital!.TipoRecompensaId}");
            Console.WriteLine($"  - Tangible: ID {tipoTangible!.TipoRecompensaId}");
        }

        [TestMethod]
        public void VerificarRecompensasIniciales_DeberianExistirCuatroRecompensasBase()
        {
            if (iConexion?.Recompensas == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var recompensas = iConexion.Recompensas.ToList();

            // Assert
            Assert.IsTrue(recompensas.Count >= 4, "Deberían existir al menos 4 recompensas del script inicial");
            
            var nombresEsperados = new[] { "Insignia Verde", "Nivel Avanzado", "Kit de Compostaje", "Semillas Organicas" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(recompensas.Any(r => r.Nombre == nombre), 
                    $"No existe la recompensa '{nombre}'");
            }

            // Verificar propiedades específicas
            var insigniaVerde = recompensas.FirstOrDefault(r => r.Nombre == "Insignia Verde");
            if (insigniaVerde != null)
            {
                Assert.AreEqual(500, insigniaVerde.CostoPuntos);
                Assert.IsTrue(insigniaVerde.EsIlimitado);
                Console.WriteLine($"  - Insignia Verde: {insigniaVerde.CostoPuntos} puntos, Ilimitada: {insigniaVerde.EsIlimitado}");
            }
            
            var kitCompostaje = recompensas.FirstOrDefault(r => r.Nombre == "Kit de Compostaje");
            if (kitCompostaje != null)
            {
                Assert.AreEqual(5000, kitCompostaje.CostoPuntos);
                Assert.IsFalse(kitCompostaje.EsIlimitado);
                Assert.AreEqual(50, kitCompostaje.StockDisponible);
                Console.WriteLine($"  - Kit de Compostaje: {kitCompostaje.CostoPuntos} puntos, Stock: {kitCompostaje.StockDisponible}");
            }
        }

        public bool Listar()
        {
            if (iConexion?.Recompensas == null)
            {
                Console.WriteLine("Error: iConexion o Recompensas es null");
                return false;
            }

            try
            {
                this.lista = iConexion.Recompensas
                    .Include(r => r.TipoRecompensa)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} recompensas encontradas");
                
                foreach (var recompensa in lista)
                {
                    Console.WriteLine($"  - ID: {recompensa.RecompensaId}, " +
                                      $"Nombre: {recompensa.Nombre}, " +
                                      $"Tipo: {recompensa.TipoRecompensa?.Nombre ?? "N/A"}, " +
                                      $"Costo: {recompensa.CostoPuntos} puntos, " +
                                      $"Stock: {(recompensa.EsIlimitado ? "Ilimitado" : recompensa.StockDisponible?.ToString() ?? "0")}, " +
                                      $"Vigencia: {recompensa.FechaVigenciaDesde?.ToString("dd/MM/yyyy") ?? "N/A"} - {recompensa.FechaVigenciaHasta?.ToString("dd/MM/yyyy") ?? "N/A"}");
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
            if (iConexion?.Recompensas == null)
            {
                Console.WriteLine("Error: iConexion o Recompensas es null");
                return false;
            }

            try
            {
                this.entidad = new Recompensas
                {
                    Nombre = GenerarNombreSeguro("Recompensa_Prueba"),
                    Descripcion = GenerarDescripcionSegura("Recompensa de prueba"),
                    TipoRecompensaId = tipoRecompensaIdPrueba,
                    CostoPuntos = 100,
                    EsIlimitado = true,
                    StockDisponible = null
                };
                
                Console.WriteLine($"Guardando recompensa: {entidad.Nombre}");
                
                iConexion.Recompensas.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.RecompensaId;
                    Console.WriteLine($"Recompensa guardada con ID: {entidadIdGuardado}");
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
            if (iConexion?.Recompensas == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Recompensas o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.Recompensas.Find(this.entidad.RecompensaId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Recompensa {this.entidad.RecompensaId} no encontrada para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Recompensa_Modificada");
                entidadActualizada.Nombre = nuevoNombre;
                entidadActualizada.CostoPuntos = this.entidad.CostoPuntos + 50;
                
                Console.WriteLine($"Modificando recompensa ID: {entidadActualizada.RecompensaId}");
                Console.WriteLine($"  Nuevo nombre: {nuevoNombre}");
                Console.WriteLine($"  Nuevo costo: {entidadActualizada.CostoPuntos}");
                
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
            if (iConexion?.Recompensas == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Recompensas o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene canjes asociados
                var canjesAsociados = 0;
                if (iConexion.CanjesRecompensas != null)
                {
                    canjesAsociados = iConexion.CanjesRecompensas
                        .Count(c => c.RecompensaId == this.entidad.RecompensaId);
                }
                
                if (canjesAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar la recompensa porque tiene {canjesAsociados} canjes asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.Recompensas.Find(this.entidad.RecompensaId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Recompensa {this.entidad.RecompensaId} no encontrada para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando recompensa ID: {entidadActualizada.RecompensaId}");
                
                iConexion.Recompensas.Remove(entidadActualizada);
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