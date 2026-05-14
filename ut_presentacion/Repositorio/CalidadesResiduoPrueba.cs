using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class CalidadesResiduoPrueba
    {
        private readonly IConexion? iConexion;
        private List<CalidadesResiduo>? lista;
        private CalidadesResiduo? entidad;
        private int entidadIdGuardado;

        public CalidadesResiduoPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para crear nombres seguros (máx 50 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 50)
        {
            var guid = Guid.NewGuid().ToString("N"); // 32 caracteres
            var nombre = $"{prefijo}_{guid}";
            
            // Truncar si es necesario
            if (nombre.Length > maxLength)
            {
                nombre = nombre.Substring(0, maxLength);
            }
            
            return nombre;
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
        public void GuardarCalidadAlta_DeberiaCrearCalidad()
        {
            // Arrange
            var calidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Alta"),
                FactorBase = 20.5m
            };

            // Act
            iConexion!.CalidadesResiduo!.Add(calidad);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(calidad.CalidadResiduoId > 0);
            Assert.AreEqual(20.5m, calidad.FactorBase);

            // Cleanup
            iConexion.CalidadesResiduo.Remove(calidad);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarCalidadMedia_DeberiaCrearCalidad()
        {
            // Arrange
            var calidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Media"),
                FactorBase = 12.0m
            };

            // Act
            iConexion!.CalidadesResiduo!.Add(calidad);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.AreEqual(12.0m, calidad.FactorBase);

            // Cleanup
            iConexion.CalidadesResiduo.Remove(calidad);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarCalidadConFactorBaseDecimal_DeberiaRespetarPrecision()
        {
            // Arrange
            var calidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Precision"),
                FactorBase = 15.75m
            };

            // Act
            iConexion!.CalidadesResiduo!.Add(calidad);
            iConexion.SaveChanges();

            // Assert
            var calidadGuardada = iConexion.CalidadesResiduo.Find(calidad.CalidadResiduoId);
            Assert.IsNotNull(calidadGuardada);
            Assert.AreEqual(15.75m, calidadGuardada.FactorBase);

            // Cleanup
            iConexion.CalidadesResiduo.Remove(calidad);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerCalidadPorId_DeberiaRetornarCalidadCorrecta()
        {
            // Arrange
            var calidadGuardar = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Buscar"),
                FactorBase = 18.0m
            };
            iConexion!.CalidadesResiduo!.Add(calidadGuardar);
            iConexion.SaveChanges();
            int idBuscado = calidadGuardar.CalidadResiduoId;

            // Act
            var calidadEncontrada = iConexion.CalidadesResiduo.Find(idBuscado);

            // Assert
            Assert.IsNotNull(calidadEncontrada);
            Assert.AreEqual(idBuscado, calidadEncontrada.CalidadResiduoId);
            Assert.AreEqual(calidadGuardar.Nombre, calidadEncontrada.Nombre);
            Assert.AreEqual(calidadGuardar.FactorBase, calidadEncontrada.FactorBase);

            // Cleanup
            iConexion.CalidadesResiduo.Remove(calidadGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarCalidadesResiduo_DeberiaIncluirDatosExistentes()
        {
            // Arrange
            var nuevaCalidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Listar"),
                FactorBase = 10.0m
            };
            iConexion!.CalidadesResiduo!.Add(nuevaCalidad);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.CalidadesResiduo.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count >= 3);
            Assert.IsTrue(listaCompleta.Any(c => c.Nombre == "Alta"));
            Assert.IsTrue(listaCompleta.Any(c => c.Nombre == "Media"));

            // Cleanup
            iConexion.CalidadesResiduo.Remove(nuevaCalidad);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarCalidad_CambiarFactorBase_DeberiaActualizar()
        {
            // Arrange
            var calidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Modificar"),
                FactorBase = 10.0m
            };
            iConexion!.CalidadesResiduo!.Add(calidad);
            iConexion.SaveChanges();
            decimal nuevoFactorBase = 25.5m;

            // Act
            calidad.FactorBase = nuevoFactorBase;
            var entry = iConexion.Entry(calidad);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var calidadActualizada = iConexion.CalidadesResiduo.Find(calidad.CalidadResiduoId);
            Assert.IsNotNull(calidadActualizada);
            Assert.AreEqual(nuevoFactorBase, calidadActualizada.FactorBase);

            // Cleanup
            iConexion.CalidadesResiduo.Remove(calidad);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarCalidad_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var calidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Original"),
                FactorBase = 10.0m
            };
            iConexion!.CalidadesResiduo!.Add(calidad);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Calidad_Modificado");

            // Act
            calidad.Nombre = nuevoNombre;
            var entry = iConexion.Entry(calidad);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var calidadActualizada = iConexion.CalidadesResiduo.Find(calidad.CalidadResiduoId);
            Assert.IsNotNull(calidadActualizada);
            Assert.AreEqual(nuevoNombre, calidadActualizada.Nombre);

            // Cleanup
            iConexion.CalidadesResiduo.Remove(calidad);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarCalidad_NoDebeExistirEnConsultaPosterior()
        {
            // Arrange
            var calidad = new CalidadesResiduo
            {
                Nombre = GenerarNombreSeguro("Calidad_Eliminar"),
                FactorBase = 10.0m
            };
            iConexion!.CalidadesResiduo!.Add(calidad);
            iConexion.SaveChanges();
            int idEliminado = calidad.CalidadResiduoId;

            // Act
            iConexion.CalidadesResiduo.Remove(calidad);
            iConexion.SaveChanges();

            // Assert
            var calidadEliminada = iConexion.CalidadesResiduo.Find(idEliminado);
            Assert.IsNull(calidadEliminada);
        }

        [TestMethod]
        public void VerificarCalidadesIniciales_DeberianExistirAltaYMedia()
        {
            if (iConexion?.CalidadesResiduo == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var calidadAlta = iConexion.CalidadesResiduo
                .FirstOrDefault(c => c.Nombre == "Alta");
            var calidadMedia = iConexion.CalidadesResiduo
                .FirstOrDefault(c => c.Nombre == "Media");

            // Assert
            Assert.IsNotNull(calidadAlta, "No existe la calidad 'Alta' en la BD");
            Assert.IsNotNull(calidadMedia, "No existe la calidad 'Media' en la BD");
            
            if (calidadAlta != null)
            {
                Assert.AreEqual(15m, calidadAlta.FactorBase);
            }
            
            if (calidadMedia != null)
            {
                Assert.AreEqual(12m, calidadMedia.FactorBase);
            }
        }

        [TestMethod]
        public void RelacionConTiposResiduos_CalidadDebeTenerTiposAsociados()
        {
            if (iConexion?.CalidadesResiduo == null || iConexion?.TiposResiduos == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Arrange
            var calidadAlta = iConexion.CalidadesResiduo
                .FirstOrDefault(c => c.Nombre == "Alta");

            if (calidadAlta == null)
            {
                Assert.Inconclusive("No existe la calidad 'Alta' en la BD");
                return;
            }

            // Act
            var tiposResiduosAsociados = iConexion.TiposResiduos
                .Where(tr => tr != null && tr.CalidadResiduoId == calidadAlta.CalidadResiduoId)
                .ToList();

            // Assert
            Assert.IsTrue(tiposResiduosAsociados.Count > 0);
            
            Console.WriteLine($"Calidad '{calidadAlta.Nombre}' tiene {tiposResiduosAsociados.Count} tipos de residuo:");
            foreach (var tipo in tiposResiduosAsociados)
            {
                Console.WriteLine($"  - {tipo?.Nombre ?? "null"}");
            }
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            if (iConexion?.CalidadesResiduo == null)
            {
                Console.WriteLine("Advertencia: iConexion.CalidadesResiduo es null");
            }
            else
            {
                var count = iConexion.CalidadesResiduo.Count();
                Console.WriteLine($"CalidadesResiduo existentes en BD: {count}");
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.CalidadesResiduo != null)
            {
                try
                {
                    var entidadExistente = iConexion.CalidadesResiduo.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        var tiposAsociados = 0;
                        if (iConexion.TiposResiduos != null)
                        {
                            tiposAsociados = iConexion.TiposResiduos
                                .Count(tr => tr != null && tr.CalidadResiduoId == entidadExistente.CalidadResiduoId);
                        }
                        
                        if (tiposAsociados == 0)
                        {
                            iConexion.CalidadesResiduo.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: CalidadResiduo {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó CalidadResiduo {entidadIdGuardado} porque tiene {tiposAsociados} tipos asociados");
                        }
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
            if (iConexion?.CalidadesResiduo == null)
            {
                Console.WriteLine("Error: iConexion o CalidadesResiduo es null");
                return false;
            }

            try
            {
                this.lista = iConexion.CalidadesResiduo
                    .Include(c => c.TiposResiduos)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} calidades encontradas");
                
                foreach (var calidad in lista)
                {
                    var tiposCount = calidad.TiposResiduos?.Count ?? 0;
                    Console.WriteLine($"  - ID: {calidad.CalidadResiduoId}, " +
                                      $"Nombre: {calidad.Nombre}, " +
                                      $"FactorBase: {calidad.FactorBase}, " +
                                      $"TiposAsociados: {tiposCount}");
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
            if (iConexion?.CalidadesResiduo == null)
            {
                Console.WriteLine("Error: iConexion o CalidadesResiduo es null");
                return false;
            }

            try
            {
                this.entidad = new CalidadesResiduo
                {
                    Nombre = GenerarNombreSeguro("Calidad_Test"),
                    FactorBase = 10.0m
                };
                
                Console.WriteLine($"Guardando calidad: {entidad.Nombre}, FactorBase: {entidad.FactorBase}");
                
                iConexion.CalidadesResiduo.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.CalidadResiduoId;
                    Console.WriteLine($"Calidad guardada con ID: {entidadIdGuardado}");
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
            if (iConexion?.CalidadesResiduo == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, CalidadesResiduo o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.CalidadesResiduo.Find(this.entidad.CalidadResiduoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Calidad {this.entidad.CalidadResiduoId} no encontrada para modificar");
                    return false;
                }
                
                decimal nuevoFactorBase = entidadActualizada.FactorBase + 5;
                entidadActualizada.FactorBase = nuevoFactorBase;
                
                Console.WriteLine($"Modificando calidad ID: {entidadActualizada.CalidadResiduoId}");
                Console.WriteLine($"  Nuevo FactorBase: {nuevoFactorBase}");
                
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
            if (iConexion?.CalidadesResiduo == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, CalidadesResiduo o entidad es null");
                return false;
            }

            try
            {
                var tiposAsociados = 0;
                if (iConexion.TiposResiduos != null)
                {
                    tiposAsociados = iConexion.TiposResiduos
                        .Count(tr => tr != null && tr.CalidadResiduoId == this.entidad.CalidadResiduoId);
                }
                
                if (tiposAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar la calidad porque tiene {tiposAsociados} tipos de residuo asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.CalidadesResiduo.Find(this.entidad.CalidadResiduoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Calidad {this.entidad.CalidadResiduoId} no encontrada para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando calidad ID: {entidadActualizada.CalidadResiduoId}");
                
                iConexion.CalidadesResiduo.Remove(entidadActualizada);
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
                Console.WriteLine($"Error de FK al borrar: {ex.InnerException?.Message ?? ex.Message}");
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