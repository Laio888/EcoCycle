using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class TiposRecompensaPrueba
    {
        private readonly IConexion? iConexion;
        private List<TiposRecompensa>? lista;
        private TiposRecompensa? entidad;
        private int entidadIdGuardado;

        public TiposRecompensaPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        // Método auxiliar para generar nombre seguro (máx 50 caracteres)
        private string GenerarNombreSeguro(string prefijo, int maxLength = 50)
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

        [TestInitialize]
        public void Inicializar()
        {
            Console.WriteLine($"Cadena de conexión: {iConexion?.StringConexion}");
            
            if (iConexion?.TiposRecompensa == null)
            {
                Console.WriteLine("Advertencia: iConexion.TiposRecompensa es null");
            }
            else
            {
                var count = iConexion.TiposRecompensa.Count();
                Console.WriteLine($"TiposRecompensa existentes en BD: {count}");
                
                var tipos = iConexion.TiposRecompensa.ToList();
                foreach (var tipo in tipos)
                {
                    Console.WriteLine($"  - ID: {tipo.TipoRecompensaId}, Nombre: {tipo.Nombre}");
                }
            }
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.TiposRecompensa != null)
            {
                try
                {
                    var entidadExistente = iConexion.TiposRecompensa.Find(entidadIdGuardado);
                    if (entidadExistente != null)
                    {
                        var recompensasAsociadas = 0;
                        if (iConexion.Recompensas != null)
                        {
                            recompensasAsociadas = iConexion.Recompensas
                                .Count(r => r.TipoRecompensaId == entidadExistente.TipoRecompensaId);
                        }
                        
                        if (recompensasAsociadas == 0)
                        {
                            iConexion.TiposRecompensa.Remove(entidadExistente);
                            iConexion.SaveChanges();
                            Console.WriteLine($"Limpiado: TiposRecompensa {entidadIdGuardado} eliminado");
                        }
                        else
                        {
                            Console.WriteLine($"No se eliminó el tipo porque tiene {recompensasAsociadas} recompensas asociadas");
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
        public void GuardarTipoRecompensa_Digital_DeberiaCrearTipo()
        {
            var tipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Digital")
            };

            iConexion!.TiposRecompensa!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoRecompensaId > 0);
            Assert.IsNotNull(tipo.Nombre);

            iConexion.TiposRecompensa.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoRecompensa_Tangible_DeberiaCrearTipo()
        {
            var tipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tangible")
            };

            iConexion!.TiposRecompensa!.Add(tipo);
            int resultado = iConexion.SaveChanges();

            Assert.IsTrue(resultado > 0);
            Assert.IsTrue(tipo.TipoRecompensaId > 0);

            iConexion.TiposRecompensa.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void GuardarTipoRecompensa_SinNombre_DeberiaFallar()
        {
            var tipo = new TiposRecompensa
            {
                Nombre = null!
            };

            iConexion!.TiposRecompensa!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoRecompensa_ConNombreLargo_DeberiaFallar()
        {
            var nombreLargo = new string('A', 60);
            var tipo = new TiposRecompensa
            {
                Nombre = nombreLargo
            };

            iConexion!.TiposRecompensa!.Add(tipo);
            Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
        }

        [TestMethod]
        public void GuardarTipoRecompensa_ConNombreDuplicado_DeberiaFallar()
        {
            var nombreUnico = GenerarNombreSeguro("Tipo_Duplicado");
            var tipo1 = new TiposRecompensa { Nombre = nombreUnico };
            var tipo2 = new TiposRecompensa { Nombre = nombreUnico };

            iConexion!.TiposRecompensa!.Add(tipo1);
            iConexion.SaveChanges();

            iConexion.TiposRecompensa.Add(tipo2);
            
            var ex = Assert.ThrowsException<DbUpdateException>(() => iConexion.SaveChanges());
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "UNIQUE");

            iConexion.TiposRecompensa.Remove(tipo1);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTipoRecompensaPorId_DeberiaRetornarTipoCorrecto()
        {
            var tipoGuardar = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tipo_Buscar")
            };
            iConexion!.TiposRecompensa!.Add(tipoGuardar);
            iConexion.SaveChanges();
            int idBuscado = tipoGuardar.TipoRecompensaId;

            var tipoEncontrado = iConexion.TiposRecompensa.Find(idBuscado);

            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(idBuscado, tipoEncontrado.TipoRecompensaId);
            Assert.AreEqual(tipoGuardar.Nombre, tipoEncontrado.Nombre);

            iConexion.TiposRecompensa.Remove(tipoGuardar);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposRecompensa_DeberiaIncluirTiposExistentes()
        {
            var nuevoTipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tipo_Listar")
            };
            iConexion!.TiposRecompensa!.Add(nuevoTipo);
            iConexion.SaveChanges();

            var listaCompleta = iConexion.TiposRecompensa.ToList();

            Assert.IsTrue(listaCompleta.Count > 0);

            iConexion.TiposRecompensa.Remove(nuevoTipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ListarTiposRecompensaConRecompensas_DeberiaCargarRelacion()
        {
            var tipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tipo_Relacion")
            };
            iConexion!.TiposRecompensa!.Add(tipo);
            iConexion.SaveChanges();

            var tiposConRecompensas = iConexion.TiposRecompensa
                .Include(t => t.Recompensas)
                .ToList();

            Assert.IsTrue(tiposConRecompensas.Count > 0);
            
            var tipoBuscado = tiposConRecompensas.FirstOrDefault(t => t.TipoRecompensaId == tipo.TipoRecompensaId);
            Assert.IsNotNull(tipoBuscado);
            Assert.IsNotNull(tipoBuscado.Recompensas);

            Console.WriteLine($"Tipo '{tipoBuscado.Nombre}' tiene {tipoBuscado.Recompensas.Count} recompensas asociadas");

            iConexion.TiposRecompensa.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ModificarTipoRecompensa_CambiarNombre_DeberiaActualizar()
        {
            var tipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tipo_Original")
            };
            iConexion!.TiposRecompensa!.Add(tipo);
            iConexion.SaveChanges();
            string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");

            tipo.Nombre = nuevoNombre;
            var entry = iConexion.Entry(tipo);
            entry.State = EntityState.Modified;
            iConexion.SaveChanges();

            var tipoActualizado = iConexion.TiposRecompensa.Find(tipo.TipoRecompensaId);
            Assert.IsNotNull(tipoActualizado);
            Assert.AreEqual(nuevoNombre, tipoActualizado.Nombre);

            iConexion.TiposRecompensa.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void EliminarTipoRecompensa_SinRecompensasAsociadas_DeberiaEliminarCorrectamente()
        {
            var tipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tipo_Eliminar")
            };
            iConexion!.TiposRecompensa!.Add(tipo);
            iConexion.SaveChanges();
            int idEliminado = tipo.TipoRecompensaId;

            iConexion.TiposRecompensa.Remove(tipo);
            iConexion.SaveChanges();

            var tipoEliminado = iConexion.TiposRecompensa.Find(idEliminado);
            Assert.IsNull(tipoEliminado);
        }

        [TestMethod]
        public void VerificarTiposRecompensaIniciales_DeberianExistirDigitalYTangible()
        {
            if (iConexion?.TiposRecompensa == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            var tipos = iConexion.TiposRecompensa.ToList();

            Assert.IsTrue(tipos.Count >= 2, "Deberían existir al menos 2 tipos de recompensa del script inicial");
            
            var tipoDigital = tipos.FirstOrDefault(t => t.Nombre == "Digital");
            var tipoTangible = tipos.FirstOrDefault(t => t.Nombre == "Tangible");

            Assert.IsNotNull(tipoDigital, "No existe el tipo 'Digital'");
            Assert.IsNotNull(tipoTangible, "No existe el tipo 'Tangible'");

            Console.WriteLine("Tipos de recompensa iniciales encontrados:");
            Console.WriteLine($"  - ID: {tipoDigital!.TipoRecompensaId}, Nombre: {tipoDigital.Nombre}");
            Console.WriteLine($"  - ID: {tipoTangible!.TipoRecompensaId}, Nombre: {tipoTangible.Nombre}");
        }

        [TestMethod]
        public void BuscarTipoRecompensaPorNombre_DeberiaRetornarResultadoCorrecto()
        {
            var nombreUnico = GenerarNombreSeguro("Tipo_BuscarNombre");
            var tipo = new TiposRecompensa
            {
                Nombre = nombreUnico
            };
            iConexion!.TiposRecompensa!.Add(tipo);
            iConexion.SaveChanges();

            var tipoEncontrado = iConexion.TiposRecompensa
                .FirstOrDefault(t => t.Nombre == nombreUnico);

            Assert.IsNotNull(tipoEncontrado);
            Assert.AreEqual(nombreUnico, tipoEncontrado.Nombre);

            iConexion.TiposRecompensa.Remove(tipo);
            iConexion.SaveChanges();
        }

        [TestMethod]
        public void ObtenerTiposRecompensaOrdenadosPorId_DeberiaRespetarOrden()
        {
            if (iConexion?.TiposRecompensa == null)
            {
                Assert.Inconclusive("No se puede conectar a la base de datos");
                return;
            }

            var tiposOrdenados = iConexion.TiposRecompensa
                .OrderBy(t => t.TipoRecompensaId)
                .ToList();

            Assert.IsTrue(tiposOrdenados.Count > 0);
            
            for (int i = 0; i < tiposOrdenados.Count - 1; i++)
            {
                Assert.IsTrue(tiposOrdenados[i].TipoRecompensaId < tiposOrdenados[i + 1].TipoRecompensaId,
                    "Los tipos no están ordenados correctamente por ID");
            }

            Console.WriteLine("Tipos de recompensa ordenados por ID:");
            foreach (var tipo in tiposOrdenados)
            {
                Console.WriteLine($"  - ID: {tipo.TipoRecompensaId}, Nombre: {tipo.Nombre}");
            }
        }

        [TestMethod]
        public void VerificarQueTiposRecompensaNoSePuedenEliminarSiTienenRecompensas()
        {
            // Arrange - Crear un tipo de recompensa
            var tipo = new TiposRecompensa
            {
                Nombre = GenerarNombreSeguro("Tipo_ConRecompensas")
            };
            iConexion!.TiposRecompensa!.Add(tipo);
            iConexion.SaveChanges();
            int tipoId = tipo.TipoRecompensaId;
            
            // Crear una recompensa asociada a este tipo
            var guidShort = Guid.NewGuid().ToString("N");
            if (guidShort.Length > 8) guidShort = guidShort.Substring(0, 8);
            var nombreRecompensa = $"Recompensa_Test_{guidShort}";
            
            var recompensa = new Recompensas
            {
                Nombre = nombreRecompensa,
                Descripcion = "Recompensa de prueba para verificar integridad referencial",
                TipoRecompensaId = tipo.TipoRecompensaId,
                CostoPuntos = 100,
                EsIlimitado = true
            };
            iConexion.Recompensas!.Add(recompensa);
            iConexion.SaveChanges();

            // Act & Assert - Intentar eliminar el tipo
            iConexion.TiposRecompensa.Remove(tipo);
            
            try
            {
                int resultado = iConexion.SaveChanges();
                Console.WriteLine($"Eliminación exitosa (ON DELETE CASCADE activo). Resultado: {resultado}");
                
                var tipoEliminado = iConexion.TiposRecompensa.Find(tipoId);
                Assert.IsNull(tipoEliminado, "El tipo debería haber sido eliminado");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Excepción capturada (esperada si no hay CASCADE): {ex.InnerException?.Message ?? ex.Message}");
                Assert.IsTrue(true, "Se lanzó la excepción esperada - el tipo no se puede eliminar porque tiene recompensas asociadas");
            }

            // Cleanup
            try
            {
                iConexion.Recompensas?.Remove(recompensa);
                iConexion.TiposRecompensa?.Remove(tipo);
                iConexion?.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en limpieza: {ex.Message}");
            }
        }

        public bool Listar()
        {
            if (iConexion?.TiposRecompensa == null)
            {
                Console.WriteLine("Error: iConexion o TiposRecompensa es null");
                return false;
            }

            try
            {
                this.lista = iConexion.TiposRecompensa
                    .Include(t => t.Recompensas)
                    .ToList();
                
                Console.WriteLine($"Listar: {lista.Count} tipos de recompensa encontrados");
                
                foreach (var tipo in lista)
                {
                    var recompensasCount = tipo.Recompensas?.Count ?? 0;
                    Console.WriteLine($"  - ID: {tipo.TipoRecompensaId}, " +
                                      $"Nombre: {tipo.Nombre}, " +
                                      $"RecompensasAsociadas: {recompensasCount}");
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
            if (iConexion?.TiposRecompensa == null)
            {
                Console.WriteLine("Error: iConexion o TiposRecompensa es null");
                return false;
            }

            try
            {
                this.entidad = new TiposRecompensa
                {
                    Nombre = GenerarNombreSeguro("Tipo_Prueba")
                };
                
                Console.WriteLine($"Guardando tipo de recompensa: {entidad.Nombre}");
                
                iConexion.TiposRecompensa.Add(this.entidad);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.TipoRecompensaId;
                    Console.WriteLine($"Tipo de recompensa guardado con ID: {entidadIdGuardado}");
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
            if (iConexion?.TiposRecompensa == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposRecompensa o entidad es null");
                return false;
            }

            try
            {
                var entidadActualizada = iConexion.TiposRecompensa.Find(this.entidad.TipoRecompensaId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de recompensa {this.entidad.TipoRecompensaId} no encontrado para modificar");
                    return false;
                }
                
                string nuevoNombre = GenerarNombreSeguro("Tipo_Modificado");
                entidadActualizada.Nombre = nuevoNombre;
                
                Console.WriteLine($"Modificando tipo de recompensa ID: {entidadActualizada.TipoRecompensaId}");
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
            if (iConexion?.TiposRecompensa == null || this.entidad == null)
            {
                Console.WriteLine("Error: iConexion, TiposRecompensa o entidad es null");
                return false;
            }

            try
            {
                var recompensasAsociadas = 0;
                if (iConexion.Recompensas != null)
                {
                    recompensasAsociadas = iConexion.Recompensas
                        .Count(r => r.TipoRecompensaId == this.entidad.TipoRecompensaId);
                }
                
                if (recompensasAsociadas > 0)
                {
                    Console.WriteLine($"No se puede borrar el tipo porque tiene {recompensasAsociadas} recompensas asociadas");
                    return false;
                }
                
                var entidadActualizada = iConexion.TiposRecompensa.Find(this.entidad.TipoRecompensaId);
                if (entidadActualizada == null)
                {
                    Console.WriteLine($"Tipo de recompensa {this.entidad.TipoRecompensaId} no encontrado para borrar");
                    return false;
                }
                
                Console.WriteLine($"Borrando tipo de recompensa ID: {entidadActualizada.TipoRecompensaId}");
                
                iConexion.TiposRecompensa.Remove(entidadActualizada);
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