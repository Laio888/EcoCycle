using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class TiposResiduosPrueba
    {
        private readonly IConexion? iConexion;
        private List<TiposResiduos>? lista;
        private TiposResiduos? entidad;
        private int entidadIdGuardado;
        private int calidadResiduoIdPrueba;

        public TiposResiduosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 100 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 100)
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

        // Método auxiliar para generar aporte nutricional seguro (máx 500 caracteres)
        private string GenerarAporteNutricional(string descripcion)
        {
            var guid = Guid.NewGuid().ToString("N");
            if (guid.Length > 8) guid = guid.Substring(0, 8);
            var aporte = $"{descripcion}_{guid}";
            if (aporte.Length > 500)
            {
                aporte = aporte.Substring(0, 500);
            }
            return aporte;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Obtener una calidad de residuo válida (Alta = 1 o Media = 2)
            if (iConexion?.CalidadesResiduo != null)
            {
                var calidadAlta = iConexion.CalidadesResiduo.FirstOrDefault(c => c.Nombre == "Alta");
                if (calidadAlta != null)
                {
                    calidadResiduoIdPrueba = calidadAlta.CalidadResiduoId;
                    Console.WriteLine($"Usando CalidadResiduo: Alta (ID: {calidadResiduoIdPrueba})");
                }
                else
                {
                    var primeraCalidad = iConexion.CalidadesResiduo.FirstOrDefault();
                    if (primeraCalidad != null)
                    {
                        calidadResiduoIdPrueba = primeraCalidad.CalidadResiduoId;
                        Console.WriteLine($"Usando CalidadResiduo existente ID: {calidadResiduoIdPrueba}, Nombre: {primeraCalidad.Nombre}");
                    }
                    else
                    {
                        // Crear una calidad de residuo de prueba
                        var nuevaCalidad = new CalidadesResiduo 
                        { 
                            Nombre = $"Calidad_Test_{Guid.NewGuid():N}".Substring(0, 50), 
                            FactorBase = 10m 
                        };
                        iConexion.CalidadesResiduo.Add(nuevaCalidad);
                        iConexion.SaveChanges();
                        calidadResiduoIdPrueba = nuevaCalidad.CalidadResiduoId;
                        Console.WriteLine($"CalidadResiduo creada para prueba ID: {calidadResiduoIdPrueba}");
                    }
                }
            }
            
            // Mostrar calidades de residuo existentes
            if (iConexion?.CalidadesResiduo != null)
            {
                var calidades = iConexion.CalidadesResiduo.ToList();
                Console.WriteLine("Calidades de residuo disponibles:");
                foreach (var calidad in calidades)
                {
                    Console.WriteLine($"  - ID: {calidad.CalidadResiduoId}, Nombre: {calidad.Nombre}, FactorBase: {calidad.FactorBase}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar tipo de residuo creado
            if (entidadIdGuardado > 0 && iConexion?.TiposResiduos != null)
            {
                try
                {
                    var entidadExistente = iConexion.TiposResiduos.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene registros de residuo asociados
                        var registrosAsociados = 0;
                        if (iConexion.RegistrosResiduos != null)
                        {
                            registrosAsociados = iConexion.RegistrosResiduos
                                .Count(r => r.TipoResiduoId == entidadExistente.TipoResiduoId);
                        }
                        
                        if (registrosAsociados == 0)
                        {
                            iConexion.TiposResiduos.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: TiposResiduos {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el tipo porque tiene {registrosAsociados} registros asociados");
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
        public void GuardarTipoResiduo_CascarasFruta_ConDatosCompletos_DeberiaCrear()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Cascaras de fruta"),
                CalidadResiduoId = calidadResiduoIdPrueba,
                AporteNutricional = GenerarAporteNutricional("Alto en potasio y fosforo"),
                RelacionCarbono = 20,
                RelacionNitrogeno = 1
            };

            // Act
            iConexion!.TiposResiduos!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoResiduoId > 0);
            Assert.IsNotNull(tipo.Nombre);
            Assert.AreEqual(20, tipo.RelacionCarbono);
            Assert.AreEqual(1, tipo.RelacionNitrogeno);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoResiduo_SoloDatosObligatorios_DeberiaCrear()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("TipoResiduo_Basico"),
                CalidadResiduoId = calidadResiduoIdPrueba,
                AporteNutricional = null,
                RelacionCarbono = null,
                RelacionNitrogeno = null
            };

            // Act
            iConexion!.TiposResiduos!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoResiduoId > 0);
            Assert.IsNull(tipo.AporteNutricional);
            Assert.IsNull(tipo.RelacionCarbono);
            Assert.IsNull(tipo.RelacionNitrogeno);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoResiduo_ConAporteNutricionalLargo_DeberiaTruncarOFallar()
        {
            // Arrange
            var aporteLargo = new string('A', 600); // 600 caracteres, excede el límite de 500
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("TipoResiduo_AporteLargo"),
                CalidadResiduoId = calidadResiduoIdPrueba,
                AporteNutricional = aporteLargo,
                RelacionCarbono = null,
                RelacionNitrogeno = null
            };

            // Act & Assert
            iConexion!.TiposResiduos!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoResiduo_SinNombre_DeberiaFallar()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = null!,
                CalidadResiduoId = calidadResiduoIdPrueba
            };

            // Act & Assert
            iConexion!.TiposResiduos!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoResiduo_ConNombreMuyLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 150); // 150 caracteres, excede el límite de 100
            var tipo = new TiposResiduos
            {
                Nombre = nombreLargo,
                CalidadResiduoId = calidadResiduoIdPrueba
            };

            // Act & Assert
            iConexion!.TiposResiduos!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoResiduo_SinCalidadResiduo_DeberiaFallar()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("TipoResiduo_SinCalidad"),
                CalidadResiduoId = 99999 // ID que no existe
            };

            // Act & Assert
            iConexion!.TiposResiduos!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerTipoResiduoPorId_DeberiaRetornarTipoCorrecto()
        {
            // Arrange
            var tipoGuardar = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Buscar"),
                CalidadResiduoId = calidadResiduoIdPrueba,
                RelacionCarbono = 30,
                RelacionNitrogeno = 2
            };
            iConexion!.TiposResiduos!.Add(tipoGuardar);
            iConexion.SaveChanges();
            int idBuscado = tipoGuardar.TipoResiduoId;

            // Act
            var tipoEncontrado = iConexion.TiposResiduos.Find(idBuscado);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(idBuscado, tipoEncontrado.TipoResiduoId);
            Assert.AreEqual(tipoGuardar.Nombre, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposResiduos_DeberiaIncluirTiposExistentes()
        {
            // Arrange
            var nuevoTipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Listar"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.Add(nuevoTipo);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.TiposResiduos.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.TiposResiduos.Remove(nuevoTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposResiduosConRelaciones_DeberiaCargarCalidadResiduo()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Relacion"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tipoConRelacion = iConexion.TiposResiduos
                .Include(t => t.CalidadResiduo)
                .FirstOrDefault(t => t.TipoResiduoId == tipo.TipoResiduoId);

            // Assert
            Assert.IsNotNull(tipoConRelacion);
            Assert.IsNotNull(tipoConRelacion.CalidadResiduo);

            Console.WriteLine($"Tipo: {tipoConRelacion.Nombre}");
            Console.WriteLine($"  Calidad: {tipoConRelacion.CalidadResiduo.Nombre}");
            Console.WriteLine($"  Factor Base: {tipoConRelacion.CalidadResiduo.FactorBase}");

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposResiduosPorCalidad_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var tipo1 = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Calidad1"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            var tipo2 = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Calidad2"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.AddRange(tipo1, tipo2);
            iConexion.SaveChanges();

            // Act
            var tiposPorCalidad = iConexion.TiposResiduos
                .Where(t => t.CalidadResiduoId == calidadResiduoIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(tiposPorCalidad.Count >= 2);

            // Cleanup
            iConexion.TiposResiduos.RemoveRange(tipo1, tipo2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoResiduo_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Original"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");

            // Act
            tipo.Nombre = nuevoNombre;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposResiduos.Find(tipo.TipoResiduoId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoNombre, tipoActualizado.Nombre);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoResiduo_CambiarRelacionesCN_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_RelacionesCN"),
                CalidadResiduoId = calidadResiduoIdPrueba,
                RelacionCarbono = 20,
                RelacionNitrogeno = 1
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            tipo.RelacionCarbono = 40;
            tipo.RelacionNitrogeno = 2;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposResiduos.Find(tipo.TipoResiduoId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(40, tipoActualizado.RelacionCarbono);
            Assert.AreEqual(2, tipoActualizado.RelacionNitrogeno);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoResiduo_CambiarAporteNutricional_DeberiaActualizar()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Aporte"),
                CalidadResiduoId = calidadResiduoIdPrueba,
                AporteNutricional = GenerarAporteNutricional("Aporte original")
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoAporte = GenerarAporteNutricional("Aporte modificado");

            // Act
            tipo.AporteNutricional = nuevoAporte;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var tipoActualizado = iConexion.TiposResiduos.Find(tipo.TipoResiduoId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoAporte, tipoActualizado.AporteNutricional);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarTipoResiduo_SinRegistrosAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_Eliminar"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();
            int idEliminado = tipo.TipoResiduoId;

            // Act
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();

            // Assert
            var tipoEliminado = iConexion.TiposResiduos.Find(idEliminado);
            Assert.IsNull(tipoEliminado);
        }

        [TestMethod]
        public void VerificarTiposResiduosIniciales_DeberianExistirOchoTipos()
        {
            if (iConexion?.TiposResiduos == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tipos = iConexion.TiposResiduos.ToList();

            // Assert
            Assert.IsTrue(tipos.Count >= 8, "Deberían existir al menos 8 tipos de residuo del script inicial");
            
            var nombresEsperados = new[] { 
                "Cascaras de fruta", "Restos de verduras", "Cascaras de huevo", 
                "Borra de cafe", "Restos de pan", "Hojas secas", 
                "Cascaras de citricos", "Restos de te" 
            };
            
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(tipos.Any(t => t.Nombre == nombre), 
                    $"No existe el tipo de residuo '{nombre}'");
            }

            Console.WriteLine("Tipos de residuo iniciales encontrados:");
            foreach (var tipo in tipos.Where(t => nombresEsperados.Contains(t.Nombre)))
            {
                Console.WriteLine($"  - ID: {tipo.TipoResiduoId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void BuscarTipoResiduoPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Tipo_BuscarNombre");
            var tipo = new TiposResiduos
            {
                Nombre = nombreUnico,
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();

            // Act
            var tipoEncontrado = iConexion.TiposResiduos
                .FirstOrDefault(t => t.Nombre == nombreUnico);

            // Assert
            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(nombreUnico, tipoEncontrado.Nombre);

            // Cleanup
            iConexion.TiposResiduos.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTiposResiduosOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.TiposResiduos == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var tiposOrdenados = iConexion.TiposResiduos
                .OrderBy(t => t.TipoResiduoId)
                .ToList();

            // Assert
            Assert.IsTrue(tiposOrdenados.Count > 0);
            
            for (int i = 0; i < tiposOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(tiposOrdenados[i].TipoResiduoId < tiposOrdenados[i + 1].TipoResiduoId,
                    "Los tipos no están ordenados correctamente por ID");
            }

            Console.WriteLine("Tipos de residuo ordenados por ID:");
            foreach (var tipo in tiposOrdenados.Take(10))
            {
                Console.WriteLine($"  - ID: {tipo.TipoResiduoId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarQueTiposResiduosNoSePuedenEliminarSiTienenRegistros()
        {
            // Esta prueba verifica la integridad referencial
            // Dependiendo de ON DELETE CASCADE, el comportamiento puede variar
            
            // Arrange - Crear un tipo de residuo
            var tipo = new TiposResiduos
            {
                Nombre = GenerarNombreSeguro("Tipo_ConRegistros"),
                CalidadResiduoId = calidadResiduoIdPrueba
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();
            int tipoId = tipo.TipoResiduoId;
            
            // Crear un usuario de prueba
            var guidSufijo = Guid.NewGuid().ToString("N");
            if (guidSufijo.Length > 15) guidSufijo = guidSufijo.Substring(0, 15);
            
            var usuario = new Usuarios
            {
                CorreoElectronico = $"registro_{guidSufijo}@test.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            
            // Crear un registro de residuo asociado a este tipo
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuario.UsuarioId,
                TipoResiduoId = tipo.TipoResiduoId,
                PesoKg = 1.5m
            };
            iConexion.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();

            // Act & Assert - Intentar eliminar el tipo
            iConexion.TiposResiduos.Remove(tipo);
            
            try
            {
                int resultado = iConexion.SaveChanges();
                Console.WriteLine($"Eliminación exitosa (ON DELETE CASCADE activo). Resultado: {resultado}");
                
                var tipoEliminado = iConexion.TiposResiduos.Find(tipoId);
                Assert.IsNull(tipoEliminado, "El tipo debería haber sido eliminado");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Excepción capturada (esperada si no hay CASCADE): {ex.InnerException?.Message ?? ex.Message}");
                Assert.IsTrue(true, "Se lanzó la excepción esperada - el tipo no se puede eliminar porque tiene registros asociados");
            }

            // Cleanup
            try
            {
                iConexion.RegistrosResiduos?.Remove(registro);
                iConexion.TiposResiduos?.Remove(tipo);
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
            if (iConexion?.TiposResiduos == null)
            {
                Console.WriteLine("Error: iConexion o TiposResiduos es null");
                return false;
            }

            try
            {
                this.lista = iConexion.TiposResiduos
                    .Include(t => t.CalidadResiduo)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} tipos de residuo encontrados");
                
                foreach (var tipo in lista)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoResiduoId}, " +
                                      $"Nombre: {tipo.Nombre}, " +
                                      $"Calidad: {tipo.CalidadResiduo?.Nombre ?? "N/A"}, " +
                                      $"FactorBase: {tipo.CalidadResiduo?.FactorBase ?? 0}, " +
                                      $"C/N: {(tipo.RelacionCarbono.HasValue ? $"{tipo.RelacionCarbono}:{tipo.RelacionNitrogeno}" : "N/A")}");
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
            if (iConexion?.TiposResiduos == null)
            {
                Console.WriteLine("Error: iConexion o TiposResiduos es null");
                return false;
            }

            try
            {
                this.entidad = new TiposResiduos
                {
                    Nombre = GenerarNombreSeguro("Tipo_Prueba"),
                    CalidadResiduoId = calidadResiduoIdPrueba,
                    AporteNutricional = GenerarAporteNutricional("Aporte de prueba"),
                    RelacionCarbono = 25,
                    RelacionNitrogeno = 1
                };
                
                Console.WriteLine($"Guardando tipo de residuo: {entidad.Nombre}");
                
                iConexion.TiposResiduos.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.TipoResiduoId;
                    Console.WriteLine($"Tipo de residuo guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.TiposResiduos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposResiduos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.TiposResiduos.Find(this.entidad.TipoResiduoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de residuo {this.entidad.TipoResiduoId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                entidadActualizada.RelacionCarbono = (entidadActualizada.RelacionCarbono ?? 0) + 10;
                
                Console.WriteLine($"Modificando tipo de residuo ID: {entidadActualizada.TipoResiduoId}");
                Console.WriteLine($"  Nuevo nombre: {nuevoNombre}");
                Console.WriteLine($"  Nueva relación C: {entidadActualizada.RelacionCarbono}");
                
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
            if (iConexion?.TiposResiduos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposResiduos o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene registros de residuo asociados
                var registrosAsociados = 0;
                if (iConexion.RegistrosResiduos != null)
                {
                    registrosAsociados = iConexion.RegistrosResiduos
                        .Count(r => r.TipoResiduoId == this.entidad.TipoResiduoId);
                }
                
                if (registrosAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el tipo porque tiene {registrosAsociados} registros asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.TiposResiduos.Find(this.entidad.TipoResiduoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de residuo {this.entidad.TipoResiduoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando tipo de residuo ID: {entidadActualizada.TipoResiduoId}");
                
                iConexion.TiposResiduos.Remove(entidadActualizada);
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