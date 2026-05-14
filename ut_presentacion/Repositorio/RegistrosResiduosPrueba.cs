using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class RegistrosResiduosPrueba
    {
        private readonly IConexion? iConexion;
        private List<RegistrosResiduos>? lista;
        private RegistrosResiduos? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;
        private int tipoResiduoIdPrueba;

        public RegistrosResiduosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para crear usuario de prueba
        private Usuarios CrearUsuarioPrueba()
        {
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
            iConexion!.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            return usuario;
        }

        // Método auxiliar para crear tipo de residuo de prueba
        private TiposResiduos CrearTipoResiduoPrueba()
        {
            var guidShort = Guid.NewGuid().ToString("N");
            if (guidShort.Length > 8) guidShort = guidShort.Substring(0, 8);
            var nombre = $"TipoResiduo_{guidShort}";
            
            var tipo = new TiposResiduos
            {
                Nombre = nombre,
                CalidadResiduoId = 1,  // Alta
                AporteNutricional = "Prueba",
                RelacionCarbono = 20,
                RelacionNitrogeno = 1
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();
            return tipo;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Crear usuario de prueba
            var usuario = CrearUsuarioPrueba();
            usuarioIdPrueba = usuario.UsuarioId;
            Console.WriteLine($"Usuario de prueba creado con ID: {usuarioIdPrueba}");
            
            // Crear tipo de residuo de prueba
            var tipoResiduo = CrearTipoResiduoPrueba();
            tipoResiduoIdPrueba = tipoResiduo.TipoResiduoId;
            Console.WriteLine($"TipoResiduo de prueba creado con ID: {tipoResiduoIdPrueba}");
            
            // Verificar calidades de residuo existentes
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
            // Limpiar registro creado
            if (entidadIdGuardado > 0 && iConexion?.RegistrosResiduos != null)
            {
                try
                {
                    var entidadExistente = iConexion.RegistrosResiduos.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.RegistrosResiduos.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: RegistroResiduo {entidadIdGuardado} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando registro: {ex.Message}");
                }
            }
            
            // Limpiar tipo de residuo
            if (tipoResiduoIdPrueba > 0 && iConexion?.TiposResiduos != null)
            {
                try
                {
                    var tipo = iConexion.TiposResiduos.Find(tipoResiduoIdPrueba);
                    if (tipo != null)
                    {
                        iConexion.TiposResiduos.Remove(tipo);
                        iConexion.SaveChanges();
                        Console.WriteLine($"TipoResiduo {tipoResiduoIdPrueba} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando tipo residuo: {ex.Message}");
                }
            }
            
            // Limpiar usuario de prueba
            if (usuarioIdPrueba > 0 && iConexion?.Usuarios != null)
            {
                try
                {
                    var usuario = iConexion.Usuarios.Find(usuarioIdPrueba);
                    if (usuario != null)
                    {
                        iConexion.Usuarios.Remove(usuario);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Usuario de prueba {usuarioIdPrueba} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando usuario: {ex.Message}");
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
        public void GuardarRegistro_ConPesoValido_DeberiaCrearRegistro()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 2.5m,
                EvidenciaArchivoId = null
            };

            // Act
            iConexion!.RegistrosResiduos!.Add(registro);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(registro.RegistroResiduoId > 0);
            Assert.AreEqual(2.5m, registro.PesoKg);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarRegistro_ConPesoMaximo_DeberiaCrearRegistro()
        {
            // Usar un valor que definitivamente funcione dentro del rango DECIMAL(8,3)
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 9999.999m,  // 4 dígitos enteros + 3 decimales = seguro
                EvidenciaArchivoId = null
            };

            // Act
            iConexion!.RegistrosResiduos!.Add(registro);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(registro.RegistroResiduoId > 0);
            Assert.AreEqual(9999.999m, registro.PesoKg);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarRegistro_ConPesoMuyPequeno_DeberiaFuncionar()
        {
            // Usar un valor que definitivamente supere el CHECK constraint
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 0.1m,  // 0.1 es definitivamente > 0
                EvidenciaArchivoId = null
            };

            // Act
            iConexion!.RegistrosResiduos!.Add(registro);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(registro.RegistroResiduoId > 0);
            Assert.AreEqual(0.1m, registro.PesoKg);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarRegistro_ConPesoCero_DeberiaFallar()
        {
            // Arrange - CHECK constraint (PesoKg > 0)
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 0m,
                EvidenciaArchivoId = null
            };

            // Act & Assert
            iConexion!.RegistrosResiduos!.Add(registro);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarRegistro_ConPesoNegativo_DeberiaFallar()
        {
            // Arrange - CHECK constraint (PesoKg > 0)
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = -1.5m,
                EvidenciaArchivoId = null
            };

            // Act & Assert
            iConexion!.RegistrosResiduos!.Add(registro);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarRegistro_ConPesoExcedeMaximo_DeberiaFallar()
        {
            // Arrange - DECIMAL(8,3) máximo es 99999.999, este excede
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 100000m,
                EvidenciaArchivoId = null
            };

            // Act & Assert
            iConexion!.RegistrosResiduos!.Add(registro);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerRegistroPorId_DeberiaRetornarRegistroCorrecto()
        {
            // Arrange
            var registroGuardar = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 3.75m,
                EvidenciaArchivoId = null
            };
            iConexion!.RegistrosResiduos!.Add(registroGuardar);
            iConexion.SaveChanges();
            int idBuscado = registroGuardar.RegistroResiduoId;

            // Act
            var registroEncontrado = iConexion.RegistrosResiduos.Find(idBuscado);

            // Assert
            Assert.IsNotNull(registroEncontrado);
            Assert.AreEqual(idBuscado, registroEncontrado.RegistroResiduoId);
            Assert.AreEqual(3.75m, registroEncontrado.PesoKg);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registroGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRegistros_DeberiaIncluirRegistrosExistentes()
        {
            // Arrange
            var nuevoRegistro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.25m,
                EvidenciaArchivoId = null
            };
            iConexion!.RegistrosResiduos!.Add(nuevoRegistro);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.RegistrosResiduos.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(nuevoRegistro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRegistrosConRelaciones_DeberiaCargarUsuarioYTipoResiduo()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 2.0m,
                EvidenciaArchivoId = null
            };
            iConexion!.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();

            // Act
            var registroConRelaciones = iConexion.RegistrosResiduos
                .Include(r => r.Usuario)
                .Include(r => r.TipoResiduo)
                .ThenInclude(t => t!.CalidadResiduo)
                .FirstOrDefault(r => r.RegistroResiduoId == registro.RegistroResiduoId);

            // Assert
            Assert.IsNotNull(registroConRelaciones);
            Assert.IsNotNull(registroConRelaciones.Usuario);
            Assert.IsNotNull(registroConRelaciones.TipoResiduo);
            Assert.IsNotNull(registroConRelaciones.TipoResiduo.CalidadResiduo);

            Console.WriteLine($"Registro ID: {registroConRelaciones.RegistroResiduoId}");
            Console.WriteLine($"  Usuario: {registroConRelaciones.Usuario.CorreoElectronico}");
            Console.WriteLine($"  Tipo Residuo: {registroConRelaciones.TipoResiduo.Nombre}");
            Console.WriteLine($"  Calidad: {registroConRelaciones.TipoResiduo.CalidadResiduo.Nombre}");
            Console.WriteLine($"  Peso: {registroConRelaciones.PesoKg} kg");
            Console.WriteLine($"  Factor Base: {registroConRelaciones.TipoResiduo.CalidadResiduo.FactorBase}");

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarRegistrosPorUsuario_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var registro1 = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.0m
            };
            var registro2 = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 2.0m
            };
            iConexion!.RegistrosResiduos!.AddRange(registro1, registro2);
            iConexion.SaveChanges();

            // Act
            var registrosUsuario = iConexion.RegistrosResiduos
                .Where(r => r.UsuarioId == usuarioIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(registrosUsuario.Count >= 2);
            Assert.IsTrue(registrosUsuario.All(r => r.UsuarioId == usuarioIdPrueba));

            // Cleanup
            iConexion.RegistrosResiduos.RemoveRange(registro1, registro2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void CalcularPuntosPorRegistro_DeberiaCalcularCorrectamente()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 2.5m
            };
            iConexion!.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();

            // Act - Recargar con relaciones para obtener el FactorBase
            var registroConRelaciones = iConexion.RegistrosResiduos
                .Include(r => r.TipoResiduo)
                .ThenInclude(t => t!.CalidadResiduo)
                .FirstOrDefault(r => r.RegistroResiduoId == registro.RegistroResiduoId);

            // Assert
            Assert.IsNotNull(registroConRelaciones);
            Assert.IsNotNull(registroConRelaciones.TipoResiduo);
            Assert.IsNotNull(registroConRelaciones.TipoResiduo.CalidadResiduo);
            
            var puntosEsperados = registroConRelaciones.PesoKg * registroConRelaciones.TipoResiduo.CalidadResiduo.FactorBase;
            Console.WriteLine($"Peso: {registroConRelaciones.PesoKg} kg");
            Console.WriteLine($"Factor Base: {registroConRelaciones.TipoResiduo.CalidadResiduo.FactorBase}");
            Console.WriteLine($"Puntos generados: {puntosEsperados}");

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarRegistro_CambiarPeso_DeberiaActualizar()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.0m,
                EvidenciaArchivoId = null
            };
            iConexion!.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();

            // Act
            registro.PesoKg = 5.5m;
            var entry = iConexion.Entry(registro);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var registroActualizado = iConexion.RegistrosResiduos.Find(registro.RegistroResiduoId);
            Assert.IsNotNull(registroActualizado);
            Assert.AreEqual(5.5m, registroActualizado.PesoKg);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarRegistro_CambiarTipoResiduo_DeberiaActualizar()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.0m
            };
            iConexion!.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();
            
            // Crear otro tipo de residuo
            var otroTipo = CrearTipoResiduoPrueba();
            int nuevoTipoId = otroTipo.TipoResiduoId;

            // Act
            registro.TipoResiduoId = nuevoTipoId;
            var entry = iConexion.Entry(registro);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var registroActualizado = iConexion.RegistrosResiduos.Find(registro.RegistroResiduoId);
            Assert.IsNotNull(registroActualizado);
            Assert.AreEqual(nuevoTipoId, registroActualizado.TipoResiduoId);

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.TiposResiduos!.Remove(otroTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarRegistro_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.0m
            };
            iConexion!.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();
            int idEliminado = registro.RegistroResiduoId;

            // Act
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();

            // Assert
            var registroEliminado = iConexion.RegistrosResiduos.Find(idEliminado);
            Assert.IsNull(registroEliminado);
        }

        [TestMethod]
        public void VerificarFechaRegistroAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.0m
            };

            // Act
            iConexion!.RegistrosResiduos!.Add(registro);
            int resultado = iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(registro).Reload();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(registro.FechaRegistro != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(registro.FechaRegistro <= DateTime.Now, 
                "La fecha asignada es futura");

            Console.WriteLine($"FechaRegistro asignada por BD: {registro.FechaRegistro:yyyy-MM-dd HH:mm:ss}");

            // Cleanup
            iConexion.RegistrosResiduos.Remove(registro);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void VerificarCalidadesResiduoIniciales_DeberianExistirAltaYMedia()
        {
            if (iConexion?.CalidadesResiduo == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var calidadAlta = iConexion.CalidadesResiduo.FirstOrDefault(c => c.Nombre == "Alta");
            var calidadMedia = iConexion.CalidadesResiduo.FirstOrDefault(c => c.Nombre == "Media");

            // Assert
            Assert.IsNotNull(calidadAlta, "No existe la calidad 'Alta'");
            Assert.IsNotNull(calidadMedia, "No existe la calidad 'Media'");
            
            if (calidadAlta != null && calidadMedia != null)
            {
                Assert.AreEqual(15m, calidadAlta.FactorBase);
                Assert.AreEqual(12m, calidadMedia.FactorBase);
                Console.WriteLine($"Calidad Alta: FactorBase {calidadAlta.FactorBase}");
                Console.WriteLine($"Calidad Media: FactorBase {calidadMedia.FactorBase}");
            }
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
            var tiposResiduos = iConexion.TiposResiduos.ToList();

            // Assert
            Assert.IsTrue(tiposResiduos.Count >= 8, "Deberían existir al menos 8 tipos de residuo del script inicial");
            
            var nombresEsperados = new[] { 
                "Cascaras de fruta", "Restos de verduras", "Cascaras de huevo", 
                "Borra de cafe", "Restos de pan", "Hojas secas", 
                "Cascaras de citricos", "Restos de te" 
            };
            
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(tiposResiduos.Any(t => t.Nombre == nombre), 
                    $"No existe el tipo de residuo '{nombre}'");
            }

            Console.WriteLine($"Total tipos de residuo encontrados: {tiposResiduos.Count}");
        }

        [TestMethod]
        public void SumarPesoTotalPorUsuario_DeberiaCalcularCorrectamente()
        {
            // Arrange
            var registro1 = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 1.5m
            };
            var registro2 = new RegistrosResiduos
            {
                UsuarioId = usuarioIdPrueba,
                TipoResiduoId = tipoResiduoIdPrueba,
                PesoKg = 2.3m
            };
            iConexion!.RegistrosResiduos!.AddRange(registro1, registro2);
            iConexion.SaveChanges();

            // Act
            var pesoTotal = iConexion.RegistrosResiduos
                .Where(r => r.UsuarioId == usuarioIdPrueba)
                .Sum(r => r.PesoKg);

            // Assert
            Assert.AreEqual(3.8m, pesoTotal);

            // Cleanup
            iConexion.RegistrosResiduos.RemoveRange(registro1, registro2);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.RegistrosResiduos == null)
            {
                Console.WriteLine("Error: iConexion o RegistrosResiduos es null");
                return false;
            }

            try
            {
                this.lista = iConexion.RegistrosResiduos
                    .Include(r => r.Usuario)
                    .Include(r => r.TipoResiduo)
                    .ThenInclude(t => t!.CalidadResiduo)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} registros encontrados");
                
                foreach (var registro in lista)
                {
                    Console.WriteLine($"  - ID: {registro.RegistroResiduoId}, " +
                                      $"Usuario: {registro.Usuario?.CorreoElectronico ?? "N/A"}, " +
                                      $"Tipo: {registro.TipoResiduo?.Nombre ?? "N/A"}, " +
                                      $"Peso: {registro.PesoKg} kg, " +
                                      $"Fecha: {registro.FechaRegistro:yyyy-MM-dd HH:mm}");
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
            if (iConexion?.RegistrosResiduos == null)
            {
                Console.WriteLine("Error: iConexion o RegistrosResiduos es null");
                return false;
            }

            try
            {
                this.entidad = new RegistrosResiduos
                {
                    UsuarioId = usuarioIdPrueba,
                    TipoResiduoId = tipoResiduoIdPrueba,
                    PesoKg = 1.0m,
                    EvidenciaArchivoId = null
                };
                
                Console.WriteLine($"Guardando registro: UsuarioId={entidad.UsuarioId}, Peso={entidad.PesoKg}kg");
                
                iConexion.RegistrosResiduos.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.RegistroResiduoId;
                    Console.WriteLine($"Registro guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.RegistrosResiduos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, RegistrosResiduos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.RegistrosResiduos.Find(this.entidad.RegistroResiduoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Registro {this.entidad.RegistroResiduoId} no encontrado para modificar");
                    return false;
                }
                
                decimal nuevoPeso = this.entidad.PesoKg + 0.5m;
                entidadActualizada.PesoKg = nuevoPeso;
                
                Console.WriteLine($"Modificando registro ID: {entidadActualizada.RegistroResiduoId}");
                Console.WriteLine($"  Nuevo peso: {nuevoPeso} kg");
                
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
            if (iConexion?.RegistrosResiduos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, RegistrosResiduos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.RegistrosResiduos.Find(this.entidad.RegistroResiduoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Registro {this.entidad.RegistroResiduoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando registro ID: {entidadActualizada.RegistroResiduoId}");
                
                iConexion.RegistrosResiduos.Remove(entidadActualizada);
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