    using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class PuntosHistoricosPrueba
    {
        private readonly IConexion? iConexion;
        private List<PuntosHistoricos>? lista;
        private PuntosHistoricos? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;
        private int registroResiduoIdPrueba;
        private int canjeIdPrueba;

        public PuntosHistoricosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar motivo seguro (máx 255 caracteres)
        private string GenerarMotivoSeguro(string prefijo, int maxLength = 255)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var motivo = $"{prefijo}_{guid} - Registro de puntos para prueba en Ecocycle";
            if (motivo.Length > maxLength)
            {
                motivo = motivo.Substring(0, maxLength);
            }
            return motivo;
        }

        // Método auxiliar para crear usuario de prueba
        private Usuarios CrearUsuarioPrueba()
        {
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 15);
            var usuario = new Usuarios
            {
                CorreoElectronico = $"puntos_{guidSufijo}@test.com",
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
            // Usar un nombre único sin Substring problemático
            var nombreUnico = $"TipoResiduo_{Guid.NewGuid():N}";
            
            // Truncar a 100 caracteres si es necesario
            if (nombreUnico.Length > 100)
            {
                nombreUnico = nombreUnico.Substring(0, 100);
            }
            
            var tipo = new TiposResiduos
            {
                Nombre = nombreUnico,
                CalidadResiduoId = 1,  // Alta
                AporteNutricional = "Prueba",
                RelacionCarbono = 20,
                RelacionNitrogeno = 1
            };
            iConexion!.TiposResiduos!.Add(tipo);
            iConexion.SaveChanges();
            return tipo;
        }

        // Método auxiliar para crear registro de residuo
        private RegistrosResiduos CrearRegistroResiduo(int usuarioId, int tipoResiduoId, decimal pesoKg)
        {
            var registro = new RegistrosResiduos
            {
                UsuarioId = usuarioId,
                TipoResiduoId = tipoResiduoId,
                PesoKg = pesoKg,
                EvidenciaArchivoId = null
            };
            iConexion!.RegistrosResiduos!.Add(registro);
            iConexion.SaveChanges();
            return registro;
        }

        // Método auxiliar para crear recompensa de prueba - CORREGIDO
        private Recompensas CrearRecompensaPrueba()
        {
            var guidShort = Guid.NewGuid().ToString("N");
            if (guidShort.Length > 8)
            {
                guidShort = guidShort.Substring(0, 8);
            }
            var nombre = $"Recompensa_{guidShort}";
            
            // Asegurar que no excede 100 caracteres
            if (nombre.Length > 100)
            {
                nombre = nombre.Substring(0, 100);
            }
            
            var recompensa = new Recompensas
            {
                Nombre = nombre,
                Descripcion = "Recompensa de prueba para canjes",
                TipoRecompensaId = 1,  // Digital
                CostoPuntos = 100,
                StockDisponible = null,
                EsIlimitado = true,
                FechaVigenciaDesde = DateTime.Today,
                FechaVigenciaHasta = DateTime.Today.AddMonths(6),
                ImagenArchivoId = null
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();
            return recompensa;
        }

        // Método auxiliar para crear canje
        private CanjesRecompensas CrearCanje(int usuarioId, int recompensaId, int puntosGastados)
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioId,
                RecompensaId = recompensaId,
                PuntosGastados = puntosGastados,
                ComprobanteArchivoId = null
            };
            iConexion!.CanjesRecompensas!.Add(canje);
            iConexion.SaveChanges();
            return canje;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Crear usuario de prueba
            var usuario = CrearUsuarioPrueba();
            usuarioIdPrueba = usuario.UsuarioId;
            Console.WriteLine($"Usuario de prueba creado con ID: {usuarioIdPrueba}");
            
            // Crear tipo de residuo y registro para pruebas de ganancia
            var tipoResiduo = CrearTipoResiduoPrueba();
            var registro = CrearRegistroResiduo(usuarioIdPrueba, tipoResiduo.TipoResiduoId, 2.5m);
            registroResiduoIdPrueba = registro.RegistroResiduoId;
            Console.WriteLine($"RegistroResiduo de prueba creado con ID: {registroResiduoIdPrueba}");
            
            // Crear recompensa y canje para pruebas de gasto
            var recompensa = CrearRecompensaPrueba();
            var canje = CrearCanje(usuarioIdPrueba, recompensa.RecompensaId, 100);
            canjeIdPrueba = canje.CanjeId;
            Console.WriteLine($"Canje de prueba creado con ID: {canjeIdPrueba}");
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar punto histórico creado
            if (entidadIdGuardado > 0 && iConexion?.PuntosHistoricos != null)
            {
                try
                {
                    var entidadExistente = iConexion.PuntosHistoricos.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.PuntosHistoricos.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: PuntoHistorico {entidadIdGuardado} eliminado");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando punto histórico: {ex.Message}");
                }
            }
            
            // Limpiar canje de prueba
            if (canjeIdPrueba > 0 && iConexion?.CanjesRecompensas != null)
            {
                try
                {
                    var canje = iConexion.CanjesRecompensas.Find(canjeIdPrueba);
                    if (canje != null)
                    {
                        iConexion.CanjesRecompensas.Remove(canje);
                        iConexion.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando canje: {ex.Message}");
                }
            }
            
            // Limpiar recompensa asociada al canje
            if (iConexion?.Recompensas != null)
            {
                var recompensasPrueba = iConexion.Recompensas
                    .Where(r => r.Nombre.StartsWith("Recompensa_"))
                    .ToList();
                foreach (var recompensa in recompensasPrueba)
                {
                    try
                    {
                        iConexion.Recompensas.Remove(recompensa);
                        iConexion.SaveChanges();
                    }
                    catch { }
                }
            }
            
            // Limpiar registro de residuo de prueba
            if (registroResiduoIdPrueba > 0 && iConexion?.RegistrosResiduos != null)
            {
                try
                {
                    var registro = iConexion.RegistrosResiduos.Find(registroResiduoIdPrueba);
                    if (registro != null)
                    {
                        iConexion.RegistrosResiduos.Remove(registro);
                        iConexion.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando registro: {ex.Message}");
                }
            }
            
            // Limpiar tipo de residuo
            if (iConexion?.TiposResiduos != null)
            {
                var tiposPrueba = iConexion.TiposResiduos
                    .Where(t => t.Nombre.StartsWith("TipoResiduo_"))
                    .ToList();
                foreach (var tipo in tiposPrueba)
                {
                    try
                    {
                        iConexion.TiposResiduos.Remove(tipo);
                        iConexion.SaveChanges();
                    }
                    catch { }
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
        public void GuardarPunto_ConOrigenRegistroResiduo_DeberiaCrearPunto()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 150,
                Motivo = GenerarMotivoSeguro("Ganancia por registro de residuo"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };

            // Act
            iConexion!.PuntosHistoricos!.Add(punto);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(punto.PuntoHistoricoId > 0);
            Assert.IsNotNull(punto.RegistroResiduoOrigenId);
            Assert.IsNull(punto.CanjeOrigenId);
            Assert.AreEqual(150, punto.PuntosAcumulados);

            // Cleanup
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarPunto_ConOrigenCanje_DeberiaCrearPunto()
        {
            // Arrange - Nota: PuntosAcumulados debe ser >= 0 (CHECK constraint)
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,  // Gasto registrado como positivo, el saldo se calcula restando
                Motivo = GenerarMotivoSeguro("Gasto por canje de recompensa"),
                RegistroResiduoOrigenId = null,
                CanjeOrigenId = canjeIdPrueba
            };

            // Act
            iConexion!.PuntosHistoricos!.Add(punto);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(punto.PuntoHistoricoId > 0);
            Assert.IsNull(punto.RegistroResiduoOrigenId);
            Assert.IsNotNull(punto.CanjeOrigenId);

            // Cleanup
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarPunto_SinOrigen_DeberiaFallar()
        {
            // Arrange - Ambos orígenes null violan CHECK constraint
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 50,
                Motivo = GenerarMotivoSeguro("Punto sin origen"),
                RegistroResiduoOrigenId = null,
                CanjeOrigenId = null
            };

            // Act & Assert
            iConexion!.PuntosHistoricos!.Add(punto);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");

            // No cleanup necesario
        }

        [TestMethod]
        public void GuardarPunto_ConAmbosOrigenes_DeberiaFallar()
        {
            // Arrange - Ambos orígenes con valor violan CHECK constraint
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 50,
                Motivo = GenerarMotivoSeguro("Punto con dos orígenes"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = canjeIdPrueba
            };

            // Act & Assert
            iConexion!.PuntosHistoricos!.Add(punto);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");

            // No cleanup necesario
        }

        [TestMethod]
        public void GuardarPunto_ConPuntosNegativos_DeberiaFallar()
        {
            // Arrange - PuntosAcumulados negativos viola CHECK (PuntosAcumulados >= 0)
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = -50,
                Motivo = GenerarMotivoSeguro("Puntos negativos"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };

            // Act & Assert
            iConexion!.PuntosHistoricos!.Add(punto);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");

            // No cleanup necesario
        }

        [TestMethod]
        public void GuardarPunto_SinMotivo_DeberiaFallar()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 50,
                Motivo = null!,  // Motivo requerido
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };

            // Act & Assert
            iConexion!.PuntosHistoricos!.Add(punto);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarPunto_ConMotivoMuyLargo_DeberiaFallar()
        {
            // Arrange
            var motivoLargo = new string('A', 300); // 300 caracteres, excede el límite de 255
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 50,
                Motivo = motivoLargo,
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };

            // Act & Assert
            iConexion!.PuntosHistoricos!.Add(punto);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerPuntoPorId_DeberiaRetornarPuntoCorrecto()
        {
            // Arrange
            var puntoGuardar = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 200,
                Motivo = GenerarMotivoSeguro("Punto_Buscar"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            iConexion!.PuntosHistoricos!.Add(puntoGuardar);
            iConexion.SaveChanges();
            int idBuscado = puntoGuardar.PuntoHistoricoId;

            // Act
            var puntoEncontrado = iConexion.PuntosHistoricos.Find(idBuscado);

            // Assert
            Assert.IsNotNull(puntoEncontrado);
            Assert.AreEqual(idBuscado, puntoEncontrado.PuntoHistoricoId);
            Assert.AreEqual(puntoGuardar.PuntosAcumulados, puntoEncontrado.PuntosAcumulados);
            Assert.AreEqual(puntoGuardar.Motivo, puntoEncontrado.Motivo);

            // Cleanup
            iConexion.PuntosHistoricos.Remove(puntoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarPuntos_DeberiaIncluirPuntosExistentes()
        {
            // Arrange
            var nuevoPunto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 75,
                Motivo = GenerarMotivoSeguro("Punto_Listar"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            iConexion!.PuntosHistoricos!.Add(nuevoPunto);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.PuntosHistoricos.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.PuntosHistoricos.Remove(nuevoPunto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarPuntosConRelaciones_DeberiaCargarUsuarioYOrigenes()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 300,
                Motivo = GenerarMotivoSeguro("Punto_Relaciones"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            iConexion!.PuntosHistoricos!.Add(punto);
            iConexion.SaveChanges();

            // Act
            var puntoConRelaciones = iConexion.PuntosHistoricos
                .Include(p => p.Usuario)
                .Include(p => p.RegistroResiduoOrigen)
                .FirstOrDefault(p => p.PuntoHistoricoId == punto.PuntoHistoricoId);

            // Assert
            Assert.IsNotNull(puntoConRelaciones);
            Assert.IsNotNull(puntoConRelaciones.Usuario);
            Assert.IsNotNull(puntoConRelaciones.RegistroResiduoOrigen);

            Console.WriteLine($"Punto histórico: {puntoConRelaciones.Motivo}");
            Console.WriteLine($"  Usuario: {puntoConRelaciones.Usuario.CorreoElectronico}");
            Console.WriteLine($"  Puntos: {puntoConRelaciones.PuntosAcumulados}");
            Console.WriteLine($"  Fecha: {puntoConRelaciones.FechaCambio}");

            // Cleanup
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarPuntosPorUsuario_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var punto1 = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,
                Motivo = GenerarMotivoSeguro("Punto_Usuario1"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            var punto2 = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 200,
                Motivo = GenerarMotivoSeguro("Punto_Usuario2"),
                RegistroResiduoOrigenId = null,
                CanjeOrigenId = canjeIdPrueba
            };
            iConexion!.PuntosHistoricos!.AddRange(punto1, punto2);
            iConexion.SaveChanges();

            // Act
            var puntosUsuario = iConexion.PuntosHistoricos
                .Where(p => p.UsuarioId == usuarioIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(puntosUsuario.Count >= 2);
            Assert.IsTrue(puntosUsuario.All(p => p.UsuarioId == usuarioIdPrueba));

            // Cleanup
            iConexion.PuntosHistoricos.RemoveRange(punto1, punto2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarPunto_CambiarMotivo_DeberiaActualizar()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,
                Motivo = GenerarMotivoSeguro("Punto_Original"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            iConexion!.PuntosHistoricos!.Add(punto);
            iConexion.SaveChanges();
            string nuevoMotivo = GenerarMotivoSeguro("Punto_Modificado");

            // Act
            punto.Motivo = nuevoMotivo;
            var entry = iConexion.Entry(punto);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var puntoActualizado = iConexion.PuntosHistoricos.Find(punto.PuntoHistoricoId);
            Assert.IsNotNull(puntoActualizado);
            Assert.AreEqual(nuevoMotivo, puntoActualizado.Motivo);

            // Cleanup
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarPunto_CambiarPuntosAcumulados_DeberiaActualizar()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,
                Motivo = GenerarMotivoSeguro("Punto_CambioPuntos"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            iConexion!.PuntosHistoricos!.Add(punto);
            iConexion.SaveChanges();

            // Act
            punto.PuntosAcumulados = 350;
            var entry = iConexion.Entry(punto);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var puntoActualizado = iConexion.PuntosHistoricos.Find(punto.PuntoHistoricoId);
            Assert.IsNotNull(puntoActualizado);
            Assert.AreEqual(350, puntoActualizado.PuntosAcumulados);

            // Cleanup
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarPunto_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,
                Motivo = GenerarMotivoSeguro("Punto_Eliminar"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            iConexion!.PuntosHistoricos!.Add(punto);
            iConexion.SaveChanges();
            int idEliminado = punto.PuntoHistoricoId;

            // Act
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();

            // Assert
            var puntoEliminado = iConexion.PuntosHistoricos.Find(idEliminado);
            Assert.IsNull(puntoEliminado);
        }

        [TestMethod]
        public void VerificarFechaCambioAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var punto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,
                Motivo = GenerarMotivoSeguro("Punto_Fecha"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };

            // Act
            iConexion!.PuntosHistoricos!.Add(punto);
            int resultado = iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(punto).Reload();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(punto.FechaCambio != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(punto.FechaCambio <= DateTime.Now, 
                "La fecha asignada es futura");

            Console.WriteLine($"FechaCambio asignada por BD: {punto.FechaCambio:yyyy-MM-dd HH:mm:ss}");

            // Cleanup
            iConexion.PuntosHistoricos.Remove(punto);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void CalcularSaldoPuntos_SumaDePuntos_DeberiaCalcularCorrectamente()
        {
            // Arrange
            var punto1 = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 100,
                Motivo = GenerarMotivoSeguro("Punto_Saldo1"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            var punto2 = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 250,
                Motivo = GenerarMotivoSeguro("Punto_Saldo2"),
                RegistroResiduoOrigenId = null,
                CanjeOrigenId = canjeIdPrueba
            };
            iConexion!.PuntosHistoricos!.AddRange(punto1, punto2);
            iConexion.SaveChanges();

            // Act
            var totalPuntos = iConexion.PuntosHistoricos
                .Where(p => p.UsuarioId == usuarioIdPrueba)
                .Sum(p => p.PuntosAcumulados);

            // Assert
            Assert.AreEqual(350, totalPuntos);

            // Cleanup
            iConexion.PuntosHistoricos.RemoveRange(punto1, punto2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerPuntosPorTipoOrigen_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var puntoGanancia = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 150,
                Motivo = GenerarMotivoSeguro("Ganancia"),
                RegistroResiduoOrigenId = registroResiduoIdPrueba,
                CanjeOrigenId = null
            };
            var puntoGasto = new PuntosHistoricos
            {
                UsuarioId = usuarioIdPrueba,
                PuntosAcumulados = 80,
                Motivo = GenerarMotivoSeguro("Gasto"),
                RegistroResiduoOrigenId = null,
                CanjeOrigenId = canjeIdPrueba
            };
            iConexion!.PuntosHistoricos!.AddRange(puntoGanancia, puntoGasto);
            iConexion.SaveChanges();

            // Act
            var puntosGanancia = iConexion.PuntosHistoricos
                .Where(p => p.RegistroResiduoOrigenId != null)
                .Sum(p => p.PuntosAcumulados);
            
            var puntosGasto = iConexion.PuntosHistoricos
                .Where(p => p.CanjeOrigenId != null)
                .Sum(p => p.PuntosAcumulados);

            // Assert
            Assert.AreEqual(150, puntosGanancia);
            Assert.AreEqual(80, puntosGasto);

            // Cleanup
            iConexion.PuntosHistoricos.RemoveRange(puntoGanancia, puntoGasto);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.PuntosHistoricos == null)
            {
                Console.WriteLine("Error: iConexion o PuntosHistoricos es null");
                return false;
            }

            try
            {
                this.lista = iConexion.PuntosHistoricos
                    .Include(p => p.Usuario)
                    .Include(p => p.RegistroResiduoOrigen)
                    .Include(p => p.CanjeOrigen)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} puntos históricos encontrados");
                
                foreach (var punto in lista)
                {
                    string origen = punto.RegistroResiduoOrigenId != null 
                        ? $"Registro {punto.RegistroResiduoOrigenId}" 
                        : (punto.CanjeOrigenId != null ? $"Canje {punto.CanjeOrigenId}" : "Sin origen");
                    
                    Console.WriteLine($"  - ID: {punto.PuntoHistoricoId}, " +
                                      $"UsuarioId: {punto.UsuarioId}, " +
                                      $"Puntos: {punto.PuntosAcumulados}, " +
                                      $"Origen: {origen}, " +
                                      $"Motivo: {punto.Motivo}, " +
                                      $"Fecha: {punto.FechaCambio:yyyy-MM-dd HH:mm}");
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
            if (iConexion?.PuntosHistoricos == null)
            {
                Console.WriteLine("Error: iConexion o PuntosHistoricos es null");
                return false;
            }

            try
            {
                this.entidad = new PuntosHistoricos
                {
                    UsuarioId = usuarioIdPrueba,
                    PuntosAcumulados = 100,
                    Motivo = GenerarMotivoSeguro("Punto_Prueba"),
                    RegistroResiduoOrigenId = registroResiduoIdPrueba,
                    CanjeOrigenId = null
                };
                
                Console.WriteLine($"Guardando punto histórico: {entidad.Motivo}");
                
                iConexion.PuntosHistoricos.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.PuntoHistoricoId;
                    Console.WriteLine($"Punto histórico guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.PuntosHistoricos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, PuntosHistoricos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.PuntosHistoricos.Find(this.entidad.PuntoHistoricoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Punto histórico {this.entidad.PuntoHistoricoId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoMotivo = GenerarMotivoSeguro("Punto_Modificado");
                entidadActualizada.Motivo = nuevoMotivo;
                entidadActualizada.PuntosAcumulados = this.entidad.PuntosAcumulados + 50;
                
                Console.WriteLine($"Modificando punto histórico ID: {entidadActualizada.PuntoHistoricoId}");
                Console.WriteLine($"  Nuevo motivo: {nuevoMotivo}");
                Console.WriteLine($"  Nuevos puntos: {entidadActualizada.PuntosAcumulados}");
                
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
            if (iConexion?.PuntosHistoricos == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, PuntosHistoricos o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.PuntosHistoricos.Find(this.entidad.PuntoHistoricoId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Punto histórico {this.entidad.PuntoHistoricoId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando punto histórico ID: {entidadActualizada.PuntoHistoricoId}");
                
                iConexion.PuntosHistoricos.Remove(entidadActualizada);
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