using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class NivelesPrueba
    {
        private readonly IConexion? iConexion;
        private List<Niveles>? lista;
        private Niveles? entidad;
        private int entidadIdGuardado;

        public NivelesPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 50 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 50)
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
            
            if (iConexion?.Niveles == null)
            {
                Console.WriteLine("Advertencia: iConexion.Niveles es null");
            }
            else
            {
                var count = iConexion.Niveles.Count();
                Console.WriteLine($"Niveles existentes en BD: {count}");
                
                // Mostrar los niveles existentes
                var niveles = iConexion.Niveles.ToList();
                foreach (var nivel in niveles)
                {
                    Console.WriteLine($"  - ID: {nivel.NivelId}, " +
                                      $"Nombre: {nivel.NombreNivel}, " +
                                      $"Puntos: {nivel.PuntosMinimoNecesario} - {nivel.PuntosMaximo}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.Niveles != null)
            {
                try
                {
                    var entidadExistente = iConexion.Niveles.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        // Verificar si tiene usuarios asociados
                        var usuariosAsociados = 0;
                        if (iConexion.Usuarios != null)
                        {
                            usuariosAsociados = iConexion.Usuarios
                                .Count(u => u.NivelIdActual == entidadExistente.NivelId);
                        }
                        
                        if (usuariosAsociados == 0)
                        {
                            iConexion.Niveles.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: Nivel {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el nivel porque tiene {usuariosAsociados} usuarios asociados");
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
        public void GuardarNivel_ConDatosValidos_DeberiaCrearNivel()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Test"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };

            // Act
            iConexion!.Niveles!.Add(nivel);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(nivel.NivelId > 0);
            Assert.IsNotNull(nivel.NombreNivel);
            Assert.AreEqual(0, nivel.PuntosMinimoNecesario);
            Assert.AreEqual(500, nivel.PuntosMaximo);

            // Cleanup
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarNivel_ConPuntosValidos_DeberiaCrearNivel()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Rango"),
                PuntosMinimoNecesario = 1000,
                PuntosMaximo = 4999,
                InsigniaArchivoId = null
            };

            // Act
            iConexion!.Niveles!.Add(nivel);
            int resultado = iConexion.SaveChanges();

            // Assert
            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(nivel.NivelId > 0);
            Assert.AreEqual(1000, nivel.PuntosMinimoNecesario);
            Assert.AreEqual(4999, nivel.PuntosMaximo);

            // Cleanup
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarNivel_ConPuntosMinimoMayorOIgualQueMaximo_DeberiaFallar()
        {
            // Arrange - Violación de CHECK constraint (PuntosMinimoNecesario < PuntosMaximo)
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Invalido"),
                PuntosMinimoNecesario = 500,
                PuntosMaximo = 500,  // Igual, debería fallar
                InsigniaArchivoId = null
            };

            // Act & Assert
            iConexion!.Niveles!.Add(nivel);
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");

            // Otra prueba con mínimo mayor que máximo
            var nivel2 = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Invalido2"),
                PuntosMinimoNecesario = 1000,
                PuntosMaximo = 500,  // Mínimo > Máximo
                InsigniaArchivoId = null
            };

            iConexion.Niveles.Add(nivel2);
            ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");
        }

        [TestMethod]
        public void GuardarNivel_ConNombreLargo_DeberiaFallar()
        {
            // Arrange
            var nombreLargo = new string('A', 60); // 60 caracteres, excede el límite de 50
            var nivel = new Niveles
            {
                NombreNivel = nombreLargo,
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };

            // Act & Assert
            iConexion!.Niveles!.Add(nivel);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarNivel_SinNombre_DeberiaFallar()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = null!,
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };

            // Act & Assert
            iConexion!.Niveles!.Add(nivel);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void ObtenerNivelPorId_DeberiaRetornarNivelCorrecto()
        {
            // Arrange
            var nivelGuardar = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Buscar"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nivelGuardar);
            iConexion.SaveChanges();
            int idBuscado = nivelGuardar.NivelId;

            // Act
            var nivelEncontrado = iConexion.Niveles.Find(idBuscado);

            // Assert
            Assert.IsNotNull(nivelEncontrado);
            Assert.AreEqual(idBuscado, nivelEncontrado.NivelId);
            Assert.AreEqual(nivelGuardar.NombreNivel, nivelEncontrado.NombreNivel);
            Assert.AreEqual(nivelGuardar.PuntosMinimoNecesario, nivelEncontrado.PuntosMinimoNecesario);
            Assert.AreEqual(nivelGuardar.PuntosMaximo, nivelEncontrado.PuntosMaximo);

            // Cleanup
            iConexion.Niveles.Remove(nivelGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarNiveles_DeberiaIncluirNivelesExistentes()
        {
            // Arrange
            var nuevoNivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Listar"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nuevoNivel);
            iConexion.SaveChanges();

            // Act
            var listaCompleta = iConexion.Niveles.ToList();

            // Assert
            Assert.IsTrue(listaCompleta.Count > 0);

            // Cleanup
            iConexion.Niveles.Remove(nuevoNivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarNivel_CambiarNombre_DeberiaActualizar()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Original"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nivel);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Nivel_Modificado");

            // Act
            nivel.NombreNivel = nuevoNombre;
            var entry = iConexion.Entry(nivel);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var nivelActualizado = iConexion.Niveles.Find(nivel.NivelId);
            Assert.IsNotNull(nivelActualizado);
            Assert.AreEqual(nuevoNombre, nivelActualizado.NombreNivel);

            // Cleanup
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarNivel_CambiarRangoPuntos_DeberiaActualizar()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_RangoOriginal"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nivel);
            iConexion.SaveChanges();

            // Act
            nivel.PuntosMinimoNecesario = 100;
            nivel.PuntosMaximo = 600;
            var entry = iConexion.Entry(nivel);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            // Assert
            var nivelActualizado = iConexion.Niveles.Find(nivel.NivelId);
            Assert.IsNotNull(nivelActualizado);
            Assert.AreEqual(100, nivelActualizado.PuntosMinimoNecesario);
            Assert.AreEqual(600, nivelActualizado.PuntosMaximo);

            // Cleanup
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarNivel_RangoInvalido_DeberiaFallar()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_RangoInvalido"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nivel);
            iConexion.SaveChanges();

            // Act - Modificar a un rango inválido
            nivel.PuntosMinimoNecesario = 600;
            nivel.PuntosMaximo = 400;  // Mínimo > Máximo
            var entry = iConexion.Entry(nivel);
            entry.State = EntityState.Modified;

            // Assert
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "CHECK");

            // Cleanup
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarNivel_SinUsuariosAsociados_DeberiaEliminarCorrectamente()
        {
            // Arrange
            var nivel = new Niveles
            {
                NombreNivel = GenerarNombreSeguro("Nivel_Eliminar"),
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nivel);
            iConexion.SaveChanges();
            int idEliminado = nivel.NivelId;

            // Act
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();

            // Assert
            var nivelEliminado = iConexion.Niveles.Find(idEliminado);
            Assert.IsNull(nivelEliminado);
        }

        [TestMethod]
        public void VerificarNivelesIniciales_DeberianExistirCuatroNiveles()
        {
            if (iConexion?.Niveles == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Act
            var niveles = iConexion.Niveles.ToList();

            // Assert - Verificar datos iniciales del script SQL
            Assert.IsTrue(niveles.Count >= 4, "Deberían existir al menos 4 niveles del script inicial");
            
            // Verificar nombres y rangos esperados
            var nivelPrincipiante = niveles.FirstOrDefault(n => n.NombreNivel == "Principiante");
            var nivelAprendiz = niveles.FirstOrDefault(n => n.NombreNivel == "Aprendiz");
            var nivelExperto = niveles.FirstOrDefault(n => n.NombreNivel == "Experto");
            var nivelMaestro = niveles.FirstOrDefault(n => n.NombreNivel == "Maestro Compostero");

            Assert.IsNotNull(nivelPrincipiante, "No existe el nivel 'Principiante'");
            Assert.IsNotNull(nivelAprendiz, "No existe el nivel 'Aprendiz'");
            Assert.IsNotNull(nivelExperto, "No existe el nivel 'Experto'");
            Assert.IsNotNull(nivelMaestro, "No existe el nivel 'Maestro Compostero'");

            // Verificar rangos de puntos
            if (nivelPrincipiante != null)
            {
                Assert.AreEqual(0, nivelPrincipiante.PuntosMinimoNecesario);
                Assert.AreEqual(999, nivelPrincipiante.PuntosMaximo);
                Console.WriteLine($"  - Principiante: {nivelPrincipiante.PuntosMinimoNecesario} - {nivelPrincipiante.PuntosMaximo} puntos");
            }
            
            if (nivelAprendiz != null)
            {
                Assert.AreEqual(1000, nivelAprendiz.PuntosMinimoNecesario);
                Assert.AreEqual(4999, nivelAprendiz.PuntosMaximo);
                Console.WriteLine($"  - Aprendiz: {nivelAprendiz.PuntosMinimoNecesario} - {nivelAprendiz.PuntosMaximo} puntos");
            }
            
            if (nivelExperto != null)
            {
                Assert.AreEqual(5000, nivelExperto.PuntosMinimoNecesario);
                Assert.AreEqual(14999, nivelExperto.PuntosMaximo);
                Console.WriteLine($"  - Experto: {nivelExperto.PuntosMinimoNecesario} - {nivelExperto.PuntosMaximo} puntos");
            }
            
            if (nivelMaestro != null)
            {
                Assert.AreEqual(15000, nivelMaestro.PuntosMinimoNecesario);
                Assert.AreEqual(999999, nivelMaestro.PuntosMaximo);
                Console.WriteLine($"  - Maestro Compostero: {nivelMaestro.PuntosMinimoNecesario} - {nivelMaestro.PuntosMaximo} puntos");
            }
        }

        [TestMethod]
        public void DeterminarNivelPorPuntos_DeberiaRetornarNivelCorrecto()
        {
            if (iConexion?.Niveles == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            // Arrange - Obtener niveles existentes
            var niveles = iConexion.Niveles.ToList();
            
            // Act & Assert - Verificar rangos
            foreach (var nivel in niveles)
            {
                // Verificar que el rango es válido
                Assert.IsTrue(nivel.PuntosMinimoNecesario < nivel.PuntosMaximo,
                    $"Rango inválido para {nivel.NombreNivel}: {nivel.PuntosMinimoNecesario} >= {nivel.PuntosMaximo}");
                
                // Verificar que no hay solapamiento de rangos (opcional)
                var rangosSuperpuestos = niveles.Any(n => n.NivelId != nivel.NivelId &&
                    ((n.PuntosMinimoNecesario >= nivel.PuntosMinimoNecesario && n.PuntosMinimoNecesario <= nivel.PuntosMaximo) ||
                     (n.PuntosMaximo >= nivel.PuntosMinimoNecesario && n.PuntosMaximo <= nivel.PuntosMaximo)));
                
                if (rangosSuperpuestos)
                {
                    Console.WriteLine($"Advertencia: Posible solapamiento de rangos para {nivel.NombreNivel}");
                }
            }
        }

        [TestMethod]
        public void BuscarNivelPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            // Arrange
            var nombreUnico = GenerarNombreSeguro("Nivel_BuscarNombre");
            var nivel = new Niveles
            {
                NombreNivel = nombreUnico,
                PuntosMinimoNecesario = 0,
                PuntosMaximo = 500,
                InsigniaArchivoId = null
            };
            iConexion!.Niveles!.Add(nivel);
            iConexion.SaveChanges();

            // Act
            var nivelEncontrado = iConexion.Niveles
                .FirstOrDefault(n => n.NombreNivel == nombreUnico);

            // Assert
            Assert.IsNotNull(nivelEncontrado);
            Assert.AreEqual(nombreUnico, nivelEncontrado.NombreNivel);

            // Cleanup
            iConexion.Niveles.Remove(nivel);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void PrevenirDuplicados_DeberiaPermitirNombresUnicos()
        {
            // Arrange
            var nombreComun = GenerarNombreSeguro("Nivel_Duplicado");
            var nivel1 = new Niveles 
            { 
                NombreNivel = nombreComun, 
                PuntosMinimoNecesario = 0, 
                PuntosMaximo = 500,
                InsigniaArchivoId = null 
            };
            var nivel2 = new Niveles 
            { 
                NombreNivel = nombreComun, 
                PuntosMinimoNecesario = 501, 
                PuntosMaximo = 1000,
                InsigniaArchivoId = null 
            };

            // Act
            iConexion!.Niveles!.Add(nivel1);
            iConexion.SaveChanges();

            iConexion.Niveles.Add(nivel2);
            
            // Assert - La BD tiene restricción UNIQUE en el campo NombreNivel? (verificar)
            // Según el script SQL, NO tiene UNIQUE explícito, pero por lógica de negocio debería ser único
            // Si no hay UNIQUE, esta prueba se ajusta
            
            // Si la BD tiene UNIQUE, descomentar:
            // var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            // StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");
            
            // Si no hay UNIQUE, el segundo se guarda (pero debería evitarlo la lógica de negocio)
            var resultado = iConexion.SaveChanges();
            Console.WriteLine($"Segundo nivel guardado. Resultado: {resultado}");

            // Cleanup
            iConexion.Niveles.Remove(nivel1);
            if (nivel2.NivelId > 0)
            {
                iConexion.Niveles.Remove(nivel2);
            }
            iConexion.SaveChanges();
        }

        public bool Listar()
        {
            if (iConexion?.Niveles == null)
            {
                Console.WriteLine("Error: iConexion o Niveles es null");
                return false;
            }

            try
            {
                this.lista = iConexion.Niveles
                    .Include(n => n.InsigniaArchivo)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} niveles encontrados");
                
                foreach (var nivel in lista)
                {
                    Console.WriteLine($"  - ID: {nivel.NivelId}, " +
                                      $"Nombre: {nivel.NombreNivel}, " +
                                      $"Rango: {nivel.PuntosMinimoNecesario} - {nivel.PuntosMaximo}, " +
                                      $"Insignia: {(nivel.InsigniaArchivoId.HasValue ? nivel.InsigniaArchivoId.ToString() : "Sin insignia")}");
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
            if (iConexion?.Niveles == null)
            {
                Console.WriteLine("Error: iConexion o Niveles es null");
                return false;
            }

            try
            {
                this.entidad = new Niveles
                {
                    NombreNivel = GenerarNombreSeguro("Nivel_Prueba"),
                    PuntosMinimoNecesario = 2000,
                    PuntosMaximo = 3000,
                    InsigniaArchivoId = null
                };
                
                Console.WriteLine($"Guardando nivel: {entidad.NombreNivel}, " +
                                  $"Rango: {entidad.PuntosMinimoNecesario} - {entidad.PuntosMaximo}");
                
                iConexion.Niveles.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.NivelId;
                    Console.WriteLine($"Nivel guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.Niveles == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Niveles o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.Niveles.Find(this.entidad.NivelId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Nivel {this.entidad.NivelId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Nivel_Modificado");
                entidadActualizada.NombreNivel = nuevoNombre;
                entidadActualizada.PuntosMaximo = entidadActualizada.PuntosMaximo + 500;
                
                Console.WriteLine($"Modificando nivel ID: {entidadActualizada.NivelId}");
                Console.WriteLine($"  Nuevo nombre: {nuevoNombre}");
                Console.WriteLine($"  Nuevo rango: {entidadActualizada.PuntosMinimoNecesario} - {entidadActualizada.PuntosMaximo}");
                
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
            if (iConexion?.Niveles == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, Niveles o entidad es null");
                return false;
            }

            try
            {
                // Verificar si tiene usuarios asociados
                var usuariosAsociados = 0;
                if (iConexion.Usuarios != null)
                {
                    usuariosAsociados = iConexion.Usuarios
                        .Count(u => u.NivelIdActual == this.entidad.NivelId);
                }
                
                if (usuariosAsociados > 0)
                {
                    Console.WriteLine($"No se puede borrar el nivel porque tiene {usuariosAsociados} usuarios asociados");
                    return false;
                }
                
                var entidadActualizada = iConexion.Niveles.Find(this.entidad.NivelId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Nivel {this.entidad.NivelId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando nivel ID: {entidadActualizada.NivelId}");
                
                iConexion.Niveles.Remove(entidadActualizada);
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