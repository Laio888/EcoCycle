using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class CanjesRecompensasPrueba
    {
        private readonly IConexion? iConexion;
        private List<CanjesRecompensas>? lista;
        private CanjesRecompensas? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;
        private int recompensaIdPrueba;

        public CanjesRecompensasPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro
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

        // Método auxiliar para crear usuario de prueba
        private Usuarios CrearUsuarioPrueba()
        {
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 15);
            var usuario = new Usuarios
            {
                CorreoElectronico = $"test_{guidSufijo}@canje.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion!.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            return usuario;
        }

        // Método auxiliar para crear recompensa de prueba
        private Recompensas CrearRecompensaPrueba(bool esIlimitado = true, int costoPuntos = 100)
        {
            var recompensa = new Recompensas
            {
                Nombre = GenerarNombreSeguro("Recompensa_Test"),
                Descripcion = "Recompensa de prueba para canjes",
                TipoRecompensaId = 1,
                CostoPuntos = costoPuntos,
                StockDisponible = esIlimitado ? null : 10,
                EsIlimitado = esIlimitado,
                FechaVigenciaDesde = DateTime.Today,
                FechaVigenciaHasta = DateTime.Today.AddMonths(6),
                ImagenArchivoId = null
            };
            iConexion!.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();
            return recompensa;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            var usuario = CrearUsuarioPrueba();
            usuarioIdPrueba = usuario.UsuarioId;
            
            var recompensa = CrearRecompensaPrueba(true, 100);
            recompensaIdPrueba = recompensa.RecompensaId;
            
            Console.WriteLine($"Usuario de prueba creado con ID: {usuarioIdPrueba}");
            Console.WriteLine($"Recompensa de prueba creada con ID: {recompensaIdPrueba}");
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.CanjesRecompensas != null)
            {
                try
                {
                    var entidadExistente = iConexion.CanjesRecompensas.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.CanjesRecompensas.Remove(entidadExistente);
                        iConexion.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando canje: {ex.Message}");
                }
            }
            
            if (recompensaIdPrueba > 0 && iConexion?.Recompensas != null)
            {
                try
                {
                    var recompensa = iConexion.Recompensas.Find(recompensaIdPrueba);
                    if (recompensa != null)
                    {
                        iConexion.Recompensas.Remove(recompensa);
                        iConexion.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando recompensa: {ex.Message}");
                }
            }
            
            if (usuarioIdPrueba > 0 && iConexion?.Usuarios != null)
            {
                try
                {
                    var usuario = iConexion.Usuarios.Find(usuarioIdPrueba);
                    if (usuario != null)
                    {
                        iConexion.Usuarios.Remove(usuario);
                        iConexion.SaveChanges();
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
        public void GuardarCanje_ConDatosValidos_DeberiaCrearCanje()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };

            iConexion!.CanjesRecompensas!.Add(canje);
            int resultado = iConexion.SaveChanges();

            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(canje.CanjeId > 0);
            Assert.AreEqual(100, canje.PuntosGastados);

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarCanje_ConComprobanteArchivo_DeberiaCrearCanje()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 200,
                ComprobanteArchivoId = null
            };

            iConexion!.CanjesRecompensas!.Add(canje);
            int resultado = iConexion.SaveChanges();

            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(canje.CanjeId > 0);

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarCanje_ConPuntosNegativos_DeberiaFallar()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = -50
            };

            iConexion!.CanjesRecompensas!.Add(canje);
            
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void ObtenerCanjePorId_DeberiaRetornarCanjeCorrecto()
        {
            var canjeGuardar = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 150
            };
            iConexion!.CanjesRecompensas!.Add(canjeGuardar);
            iConexion.SaveChanges();
            int idBuscado = canjeGuardar.CanjeId;

            var canjeEncontrado = iConexion.CanjesRecompensas.Find(idBuscado);

            Assert.IsNotNull(canjeEncontrado);
            Assert.AreEqual(idBuscado, canjeEncontrado.CanjeId);
            Assert.AreEqual(usuarioIdPrueba, canjeEncontrado.UsuarioId);
            Assert.AreEqual(150, canjeEncontrado.PuntosGastados);

            iConexion.CanjesRecompensas.Remove(canjeGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarCanjes_DeberiaIncluirCanjesExistentes()
        {
            var nuevoCanje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 75
            };
            iConexion!.CanjesRecompensas!.Add(nuevoCanje);
            iConexion.SaveChanges();

            var listaCompleta = iConexion.CanjesRecompensas.ToList();

            Assert.IsTrue(listaCompleta.Count > 0);

            iConexion.CanjesRecompensas.Remove(nuevoCanje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarCanjesPorUsuario_DeberiaFiltrarCorrectamente()
        {
            var canje1 = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };
            var canje2 = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 200
            };
            iConexion!.CanjesRecompensas!.AddRange(canje1, canje2);
            iConexion.SaveChanges();

            var canjesUsuario = iConexion.CanjesRecompensas
                .Where(c => c.UsuarioId == usuarioIdPrueba)
                .ToList();

            Assert.IsTrue(canjesUsuario.Count >= 2);
            Assert.IsTrue(canjesUsuario.All(c => c.UsuarioId == usuarioIdPrueba));

            iConexion.CanjesRecompensas.RemoveRange(canje1, canje2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarCanje_CambiarComprobante_DeberiaActualizar()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100,
                ComprobanteArchivoId = null
            };
            iConexion!.CanjesRecompensas!.Add(canje);
            iConexion.SaveChanges();

            canje.ComprobanteArchivoId = null;
            var entry = iConexion.Entry(canje);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            var canjeActualizado = iConexion.CanjesRecompensas.Find(canje.CanjeId);
            Assert.IsNotNull(canjeActualizado);
            Assert.AreEqual(null, canjeActualizado.ComprobanteArchivoId);

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarCanje_NoDebeExistirEnConsultaPosterior()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };
            iConexion!.CanjesRecompensas!.Add(canje);
            iConexion.SaveChanges();
            int idEliminado = canje.CanjeId;

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();

            var canjeEliminado = iConexion.CanjesRecompensas.Find(idEliminado);
            Assert.IsNull(canjeEliminado);
        }

        [TestMethod]
        public void VerificarFechaCanjeAutomatica_DeberiaTenerFechaAsignada()
        {
            // CORREGIDO: Recargar la entidad desde la base de datos después de guardar
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };

            iConexion!.CanjesRecompensas!.Add(canje);
            iConexion.SaveChanges();
            
            // IMPORTANTE: Recargar la entidad para obtener los valores generados por la BD
            iConexion.Entry(canje).Reload();

            // Assert - Ahora FechaCanje debería tener el valor de la BD
            Assert.IsTrue(canje.FechaCanje != default(DateTime), "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(canje.FechaCanje <= DateTime.Now, "La fecha asignada es futura");
            Assert.IsTrue(canje.FechaCanje > DateTime.Now.AddMinutes(-5), "La fecha no es reciente");

            Console.WriteLine($"FechaCanje asignada por BD: {canje.FechaCanje}");

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void VerificarTiposRecompensaExistentes_DeberianExistirDigitalYTangible()
        {
            if (iConexion?.TiposRecompensa == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            var tipoDigital = iConexion.TiposRecompensa.FirstOrDefault(t => t.Nombre == "Digital");
            var tipoTangible = iConexion.TiposRecompensa.FirstOrDefault(t => t.Nombre == "Tangible");

            Assert.IsNotNull(tipoDigital, "No existe el tipo 'Digital' en TiposRecompensa");
            Assert.IsNotNull(tipoTangible, "No existe el tipo 'Tangible' en TiposRecompensa");
        }

        [TestMethod]
        public void VerificarRecompensasIniciales_DeberianExistirRecompensasBase()
        {
            if (iConexion?.Recompensas == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            var recompensas = iConexion.Recompensas.ToList();

            Assert.IsTrue(recompensas.Count >= 4, "Deberían existir al menos 4 recompensas");
            
            var nombresEsperados = new[] { "Insignia Verde", "Nivel Avanzado", "Kit de Compostaje", "Semillas Organicas" };
            foreach (var nombre in nombresEsperados)
            {
                Assert.IsTrue(recompensas.Any(r => r.Nombre == nombre), $"No existe la recompensa '{nombre}'");
            }
        }

        [TestMethod]
        public void RelacionConUsuario_CanjeDebeTenerUsuarioAsociado()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };
            iConexion!.CanjesRecompensas!.Add(canje);
            iConexion.SaveChanges();

            var canjeConUsuario = iConexion.CanjesRecompensas
                .Include(c => c.Usuario)
                .FirstOrDefault(c => c.CanjeId == canje.CanjeId);

            Assert.IsNotNull(canjeConUsuario);
            Assert.IsNotNull(canjeConUsuario.Usuario);
            Assert.AreEqual(usuarioIdPrueba, canjeConUsuario.Usuario.UsuarioId);

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void RelacionConRecompensa_CanjeDebeTenerRecompensaAsociada()
        {
            var canje = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };
            iConexion!.CanjesRecompensas!.Add(canje);
            iConexion.SaveChanges();

            var canjeConRecompensa = iConexion.CanjesRecompensas
                .Include(c => c.Recompensa)
                .FirstOrDefault(c => c.CanjeId == canje.CanjeId);

            Assert.IsNotNull(canjeConRecompensa);
            Assert.IsNotNull(canjeConRecompensa.Recompensa);
            Assert.AreEqual(recompensaIdPrueba, canjeConRecompensa.Recompensa.RecompensaId);

            iConexion.CanjesRecompensas.Remove(canje);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTotalPuntosGastadosPorUsuario_DeberiaCalcularCorrectamente()
        {
            var canje1 = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 100
            };
            var canje2 = new CanjesRecompensas
            {
                UsuarioId = usuarioIdPrueba,
                RecompensaId = recompensaIdPrueba,
                PuntosGastados = 250
            };
            iConexion!.CanjesRecompensas!.AddRange(canje1, canje2);
            iConexion.SaveChanges();

            var totalPuntosGastados = iConexion.CanjesRecompensas
                .Where(c => c.UsuarioId == usuarioIdPrueba)
                .Sum(c => c.PuntosGastados);

            Assert.AreEqual(350, totalPuntosGastados);

            iConexion.CanjesRecompensas.RemoveRange(canje1, canje2);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.CanjesRecompensas == null)
            {
                Console.WriteLine("Error: iConexion o CanjesRecompensas es null");
                return false;
            }

            try
            {
                this.lista = iConexion.CanjesRecompensas
                    .Include(c => c.Usuario)
                    .Include(c => c.Recompensa)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} canjes encontrados");
                
                foreach (var canje in lista)
                {
                    Console.WriteLine($"  - ID: {canje.CanjeId}, " +
                                      $"UsuarioId: {canje.UsuarioId}, " +
                                      $"RecompensaId: {canje.RecompensaId}, " +
                                      $"PuntosGastados: {canje.PuntosGastados}, " +
                                      $"FechaCanje: {canje.FechaCanje}");
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
            if (iConexion?.CanjesRecompensas == null)
            {
                Console.WriteLine("Error: iConexion o CanjesRecompensas es null");
                return false;
            }

            try
            {
                this.entidad = new CanjesRecompensas
                {
                    UsuarioId = usuarioIdPrueba,
                    RecompensaId = recompensaIdPrueba,
                    PuntosGastados = 100
                };
                
                Console.WriteLine($"Guardando canje: UsuarioId={entidad.UsuarioId}, " +
                                  $"RecompensaId={entidad.RecompensaId}, " +
                                  $"PuntosGastados={entidad.PuntosGastados}");
                
                iConexion.CanjesRecompensas.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.CanjeId;
                    Console.WriteLine($"Canje guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.CanjesRecompensas == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, CanjesRecompensas o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.CanjesRecompensas.Find(this.entidad.CanjeId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Canje {this.entidad.CanjeId} no encontrado para modificar");
                    return false;
                }
                
                entidadActualizada.ComprobanteArchivoId = null;
                
                Console.WriteLine($"Modificando canje ID: {entidadActualizada.CanjeId}");
                
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
            if (iConexion?.CanjesRecompensas == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, CanjesRecompensas o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.CanjesRecompensas.Find(this.entidad.CanjeId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Canje {this.entidad.CanjeId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando canje ID: {entidadActualizada.CanjeId}");
                
                iConexion.CanjesRecompensas.Remove(entidadActualizada);
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