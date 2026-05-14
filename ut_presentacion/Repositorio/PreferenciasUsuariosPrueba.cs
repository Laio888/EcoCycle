using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class PreferenciasUsuariosPrueba
    {
        private readonly IConexion? iConexion;
        private List<PreferenciasUsuarios>? lista;
        private PreferenciasUsuarios? entidad;
        private int entidadIdGuardado;
        private int usuarioIdPrueba;

        public PreferenciasUsuariosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar clave segura (máx 100 caracteres)
        private string GenerarClaveSegura(string prefijo, int maxLength = 100)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var clave = $"{prefijo}_{guid}";
            if (clave.Length > maxLength)
            {
                clave = clave.Substring(0, maxLength);
            }
            return clave;
        }

        // Método auxiliar para generar valor seguro (máx 500 caracteres)
        private string GenerarValorSeguro(string prefijo, int maxLength = 500)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var valor = $"{prefijo}_{guid}_valor_de_prueba_para_preferencias";
            if (valor.Length > maxLength)
            {
                valor = valor.Substring(0, maxLength);
            }
            return valor;
        }

        // Método auxiliar para crear usuario de prueba
        private Usuarios CrearUsuarioPrueba()
        {
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 15);
            var usuario = new Usuarios
            {
                CorreoElectronico = $"pref_{guidSufijo}@test.com",
                ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20),
                FechaRegistro = DateTime.Now,
                FechaUltimoInicioSesion = null,
                NivelIdActual = 1
            };
            iConexion!.Usuarios!.Add(usuario);
            iConexion.SaveChanges();
            return usuario;
        }

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            // Crear usuario de prueba
            var usuario = CrearUsuarioPrueba();
            usuarioIdPrueba = usuario.UsuarioId;
            Console.WriteLine($"Usuario de prueba creado con ID: {usuarioIdPrueba}");
        }

        [TestCleanup]
        public void Limpiar()
        {
            // Limpiar preferencia creada
            if (entidadIdGuardado > 0 && iConexion?.PreferenciasUsuarios != null)
            {
                try
                {
                    var entidadExistente = iConexion.PreferenciasUsuarios.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        iConexion.PreferenciasUsuarios.Remove(entidadExistente);
                        iConexion.SaveChanges();
                        Console.WriteLine($"Limpiado: Preferencia {entidadIdGuardado} eliminada");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error limpiando preferencia: {ex.Message}");
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
        public void GuardarPreferencia_TemaOscuro_DeberiaCrearPreferencia()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = "tema",
                Valor = "oscuro"
            };

            // Act
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(preferencia.PreferenciaUsuarioId > 0);
            Assert.AreEqual("tema", preferencia.Clave);
            Assert.AreEqual("oscuro", preferencia.Valor);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarPreferencia_Idioma_DeberiaCrearPreferencia()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = "idioma",
                Valor = "es-ES"
            };

            // Act
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(preferencia.PreferenciaUsuarioId > 0);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarPreferencia_NotificacionesEmail_DeberiaCrearPreferencia()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = "notificaciones_email",
                Valor = "true"
            };

            // Act
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(preferencia.PreferenciaUsuarioId > 0);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarPreferencia_SinClave_DeberiaFallar()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = null!,  // Clave requerida
                Valor = "test"
            };

            // Act & Assert
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarPreferencia_SinValor_DeberiaFallar()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = "test",
                Valor = null!  // Valor requerido
            };

            // Act & Assert
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarPreferencia_ConClaveMuyLarga_DeberiaFallar()
        {
            // Arrange
            var claveLarga = new string('A', 150); // 150 caracteres, excede el límite de 100
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = claveLarga,
                Valor = "test"
            };

            // Act & Assert
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarPreferencia_ConValorMuyLargo_DeberiaFallar()
        {
            // Arrange
            var valorLargo = new string('A', 600); // 600 caracteres, excede el límite de 500
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = "test",
                Valor = valorLargo
            };

            // Act & Assert
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void PrevenirDuplicados_MismaClaveMismoUsuario_DeberiaFallar()
        {
            // Arrange
            var claveUnica = GenerarClaveSegura("clave_unica");
            var preferencia1 = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = claveUnica,
                Valor = "valor1"
            };
            var preferencia2 = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = claveUnica,
                Valor = "valor2"
            };

            // Act
            iConexion!.PreferenciasUsuarios!.Add(preferencia1);
            iConexion.SaveChanges();

            iConexion.PreferenciasUsuarios.Add(preferencia2);
            
            // Assert - La BD tiene restricción UNIQUE (UsuarioId, Clave)
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void PermitirMismaClave_DiferenteUsuario_DeberiaFuncionar()
        {
            // Arrange
            var claveUnica = GenerarClaveSegura("clave_compartida");  // CORREGIDO
            
            // Crear segundo usuario
            var segundoUsuario = CrearUsuarioPrueba();
            
            var preferencia1 = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = claveUnica,
                Valor = "valor1"
            };
            var preferencia2 = new PreferenciasUsuarios
            {
                UsuarioId = segundoUsuario.UsuarioId,
                Clave = claveUnica,
                Valor = "valor2"
            };

            // Act
            iConexion!.PreferenciasUsuarios!.AddRange(preferencia1, preferencia2);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.AreEqual(2, resultado);

            // Cleanup
            iConexion.PreferenciasUsuarios.RemoveRange(preferencia1, preferencia2);
            iConexion.SaveChanges();
            
            // Limpiar segundo usuario
            iConexion.Usuarios!.Remove(segundoUsuario);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerPreferenciaPorId_DeberiaRetornarPreferenciaCorrecta()
        {
            // Arrange
            var preferenciaGuardar = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_buscar"),
                Valor = GenerarValorSeguro("valor_buscar")
            };
            iConexion!.PreferenciasUsuarios!.Add(preferenciaGuardar);
            iConexion.SaveChanges();
            int idBuscado = preferenciaGuardar.PreferenciaUsuarioId;

            // Act
            var preferenciaEncontrada = iConexion.PreferenciasUsuarios.Find(idBuscado);

            // Assert
            Assert.IsNotNull(preferenciaEncontrada);
            Assert.AreEqual(idBuscado, preferenciaEncontrada.PreferenciaUsuarioId);
            Assert.AreEqual(preferenciaGuardar.Clave, preferenciaEncontrada.Clave);
            Assert.AreEqual(preferenciaGuardar.Valor, preferenciaEncontrada.Valor);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferenciaGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarPreferencias_DeberiaIncluirPreferenciasExistentes()
        {
            // Arrange
            var nuevaPreferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_listar"),
                Valor = GenerarValorSeguro("valor_listar")
            };
            iConexion!.PreferenciasUsuarios!.Add(nuevaPreferencia);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.PreferenciasUsuarios.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(nuevaPreferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarPreferenciasPorUsuario_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var preferencia1 = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_usuario1"),
                Valor = "valor1"
            };
            var preferencia2 = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_usuario2"),
                Valor = "valor2"
            };
            iConexion!.PreferenciasUsuarios!.AddRange(preferencia1, preferencia2);
            iConexion.SaveChanges();

            // Act
            var preferenciasUsuario = iConexion.PreferenciasUsuarios
                .Where(p => p.UsuarioId == usuarioIdPrueba)
                .ToList();

            // Assert
            Assert.IsTrue(preferenciasUsuario.Count >= 2);
            Assert.IsTrue(preferenciasUsuario.All(p => p.UsuarioId == usuarioIdPrueba));

            // Cleanup
            iConexion.PreferenciasUsuarios.RemoveRange(preferencia1, preferencia2);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarPreferenciasConUsuario_DeberiaCargarRelacion()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_relacion"),
                Valor = "valor_relacion"
            };
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            iConexion.SaveChanges();

            // Act
            var preferenciaConUsuario = iConexion.PreferenciasUsuarios
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.PreferenciaUsuarioId == preferencia.PreferenciaUsuarioId);

            // Assert
            Assert.IsNotNull(preferenciaConUsuario);
            Assert.IsNotNull(preferenciaConUsuario.Usuario);
            Assert.AreEqual(usuarioIdPrueba, preferenciaConUsuario.Usuario.UsuarioId);

            Console.WriteLine($"Preferencia: {preferenciaConUsuario.Clave} = {preferenciaConUsuario.Valor}");
            Console.WriteLine($"  Usuario: {preferenciaConUsuario.Usuario.CorreoElectronico}");

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarPreferencia_CambiarValor_DeberiaActualizar()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_original"),
                Valor = "valor_original"
            };
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            iConexion.SaveChanges();
            string nuevoValor = GenerarValorSeguro("valor_modificado");

            // Act
            preferencia.Valor = nuevoValor;
            var entry = iConexion.Entry(preferencia);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var preferenciaActualizada = iConexion.PreferenciasUsuarios.Find(preferencia.PreferenciaUsuarioId);
            Assert.IsNotNull(preferenciaActualizada);
            Assert.AreEqual(nuevoValor, preferenciaActualizada.Valor);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarPreferencia_CambiarClave_DeberiaActualizar()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_original"),
                Valor = "valor_test"
            };
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            iConexion.SaveChanges();
            string nuevaClave = GenerarClaveSegura("clave_modificada");

            // Act
            preferencia.Clave = nuevaClave;
            var entry = iConexion.Entry(preferencia);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var preferenciaActualizada = iConexion.PreferenciasUsuarios.Find(preferencia.PreferenciaUsuarioId);
            Assert.IsNotNull(preferenciaActualizada);
            Assert.AreEqual(nuevaClave, preferenciaActualizada.Clave);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarPreferencia_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_eliminar"),
                Valor = "valor_eliminar"
            };
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            iConexion.SaveChanges();
            int idEliminado = preferencia.PreferenciaUsuarioId;

            // Act
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();

            // Assert
            var preferenciaEliminada = iConexion.PreferenciasUsuarios.Find(idEliminado);
            Assert.IsNull(preferenciaEliminada);
        }

        [TestMethod]
        public void VerificarFechaActualizacionAutomatica_DeberiaTenerFechaAsignada()
        {
            // Arrange
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = GenerarClaveSegura("clave_fecha"),
                Valor = "valor_fecha"
            };

            // Act
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            int resultado = iConexion.SaveChanges();
            
            // Recargar para obtener la fecha generada por la BD
            iConexion.Entry(preferencia).Reload();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(preferencia.FechaActualizacion != default(DateTime), 
                "La fecha no fue asignada por la base de datos");
            Assert.IsTrue(preferencia.FechaActualizacion <= DateTime.Now, 
                "La fecha asignada es futura");

            Console.WriteLine($"FechaActualizacion asignada por BD: {preferencia.FechaActualizacion:yyyy-MM-dd HH:mm:ss}");

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerPreferenciaPorClave_DeberiaRetornarValorCorrecto()
        {
            // Arrange
            var claveUnica = GenerarClaveSegura("clave_consulta");
            var valorEsperado = "valor_consulta_especifico";
            var preferencia = new PreferenciasUsuarios
            {
                UsuarioId = usuarioIdPrueba,
                Clave = claveUnica,
                Valor = valorEsperado
            };
            iConexion!.PreferenciasUsuarios!.Add(preferencia);
            iConexion.SaveChanges();

            // Act
            var preferenciaEncontrada = iConexion.PreferenciasUsuarios
                .FirstOrDefault(p => p.UsuarioId == usuarioIdPrueba && p.Clave == claveUnica);

            // Assert
            Assert.IsNotNull(preferenciaEncontrada);
            Assert.AreEqual(valorEsperado, preferenciaEncontrada.Valor);

            // Cleanup
            iConexion.PreferenciasUsuarios.Remove(preferencia);
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.PreferenciasUsuarios == null)
            {
                Console.WriteLine("Error: iConexion o PreferenciasUsuarios es null");
                return false;
            }

            try
            {
                this.lista = iConexion.PreferenciasUsuarios
                    .Include(p => p.Usuario)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} preferencias encontradas");
                
                foreach (var pref in lista)
                {
                    Console.WriteLine($"  - ID: {pref.PreferenciaUsuarioId}, " +
                                      $"UsuarioId: {pref.UsuarioId}, " +
                                      $"Clave: {pref.Clave}, " +
                                      $"Valor: {pref.Valor}, " +
                                      $"FechaActualizacion: {pref.FechaActualizacion:yyyy-MM-dd HH:mm}");
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
            if (iConexion?.PreferenciasUsuarios == null)
            {
                Console.WriteLine("Error: iConexion o PreferenciasUsuarios es null");
                return false;
            }

            try
            {
                this.entidad = new PreferenciasUsuarios
                {
                    UsuarioId = usuarioIdPrueba,
                    Clave = GenerarClaveSegura("clave_prueba"),
                    Valor = GenerarValorSeguro("valor_prueba")
                };
                
                Console.WriteLine($"Guardando preferencia: {entidad.Clave} = {entidad.Valor}");
                
                iConexion.PreferenciasUsuarios.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.PreferenciaUsuarioId;
                    Console.WriteLine($"Preferencia guardada con ID: {entidadIdGuardado}");
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
            if (iConexion?.PreferenciasUsuarios == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, PreferenciasUsuarios o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.PreferenciasUsuarios.Find(this.entidad.PreferenciaUsuarioId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Preferencia {this.entidad.PreferenciaUsuarioId} no encontrada para modificar");
                    return false;
                }
                
                string nuevoValor = GenerarValorSeguro("valor_modificado");
                entidadActualizada.Valor = nuevoValor;
                
                Console.WriteLine($"Modificando preferencia ID: {entidadActualizada.PreferenciaUsuarioId}");
                Console.WriteLine($"  Nuevo valor: {nuevoValor}");
                
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
            if (iConexion?.PreferenciasUsuarios == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, PreferenciasUsuarios o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.PreferenciasUsuarios.Find(this.entidad.PreferenciaUsuarioId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Preferencia {this.entidad.PreferenciaUsuarioId} no encontrada para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando preferencia ID: {entidadActualizada.PreferenciaUsuarioId}");
                
                iConexion.PreferenciasUsuarios.Remove(entidadActualizada);
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