USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Ecocycle')
BEGIN
    ALTER DATABASE Ecocycle SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Ecocycle;
END
GO

CREATE DATABASE Ecocycle;
GO

ALTER DATABASE Ecocycle SET 
    RECOVERY SIMPLE,
    AUTO_UPDATE_STATISTICS_ASYNC ON,
    AUTO_CREATE_STATISTICS ON,
    AUTO_UPDATE_STATISTICS ON;
GO

USE Ecocycle;
GO

/* 2. CONFIGURACIÓN GENERAL */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO
SET NOCOUNT ON;
GO

/* 3. ESQUEMAS (BOUNDED CONTEXTS) */
CREATE SCHEMA [identity];
GO
CREATE SCHEMA rewards;
GO
CREATE SCHEMA waste;
GO
CREATE SCHEMA education;
GO
CREATE SCHEMA audit;
GO
CREATE SCHEMA config;
GO
CREATE SCHEMA maintenance;
GO

/* 4. TABLAS DE CATÁLOGO */
CREATE TABLE [identity].TiposRecompensa (
    TipoRecompensaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE [identity].TiposNotificacion (
    TipoNotificacionId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE [identity].EstadosFeedback (
    EstadoFeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE [identity].TiposFeedback (
    TipoFeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE waste.CalidadesResiduo (
    CalidadResiduoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    FactorBase DECIMAL(5,2) NOT NULL
);
GO

CREATE TABLE education.TiposContenido (
    TipoContenidoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE education.CategoriasContenido (
    CategoriaContenidoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE [identity].TiposArchivo (
    TipoArchivoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

/* 5. TABLAS PRINCIPALES */
CREATE TABLE [identity].Archivos (
    ArchivoId INT IDENTITY(1,1) PRIMARY KEY,
    Url NVARCHAR(500) NOT NULL,
    TipoArchivoId INT NOT NULL,
    EsExterno BIT NOT NULL DEFAULT 0,
    Proveedor NVARCHAR(100) NULL,
    Descripcion NVARCHAR(200) NULL,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE [identity].Niveles (
    NivelId INT IDENTITY(1,1) PRIMARY KEY,
    NombreNivel NVARCHAR(50) NOT NULL,
    PuntosMinimoNecesario INT NOT NULL,
    PuntosMaximo INT NOT NULL,
    InsigniaArchivoId INT NULL,
    CONSTRAINT CHK_Niveles_Puntos CHECK (PuntosMinimoNecesario < PuntosMaximo)
);
GO

CREATE TABLE config.Tenants (
    TenantId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Subdominio NVARCHAR(100) NOT NULL UNIQUE,
    ConfiguracionJson NVARCHAR(MAX) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    FechaExpiracion DATETIME2 NULL
);
GO

CREATE TABLE [identity].Usuarios (
    UsuarioId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    CorreoElectronico NVARCHAR(255) NOT NULL,
    ContrasenaHash NVARCHAR(255) NOT NULL,
    FechaRegistro DATETIME2 DEFAULT GETDATE(),
    FechaUltimoInicioSesion DATETIME2 NULL,
    NivelIdActual INT NOT NULL,
    IntentosFallidos INT NOT NULL DEFAULT 0,
    BloqueadoHasta DATETIME2 NULL,
    UltimoCambioPassword DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT UQ_Usuarios_Tenant_Email UNIQUE (TenantId, CorreoElectronico),
    CONSTRAINT CHK_Usuarios_Email CHECK (
        CorreoElectronico LIKE '%_@_%._%' 
        AND CorreoElectronico NOT LIKE '%[^a-zA-Z0-9@._-]%'
    )
);
GO

CREATE TABLE [identity].PreferenciasUsuarios (
    PreferenciaUsuarioId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    Clave NVARCHAR(100) NOT NULL,
    Valor NVARCHAR(500) NOT NULL,
    FechaActualizacion DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT UQ_PreferenciasUsuarios_Usuario_Clave UNIQUE (UsuarioId, Clave)
);
GO

ALTER TABLE [identity].PreferenciasUsuarios ADD
    Tema NVARCHAR(20) NULL,
    Idioma NVARCHAR(10) NULL,
    NotificacionesEmail BIT NULL DEFAULT 1,
    NotificacionesPush BIT NULL DEFAULT 1;
GO

CREATE TABLE waste.TiposResiduos (
    TipoResiduoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    CalidadResiduoId INT NOT NULL,
    AporteNutricional NVARCHAR(500) NULL,
    RelacionCarbono INT NULL,
    RelacionNitrogeno INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    EliminadoEn DATETIME2 NULL,
    CONSTRAINT CHK_TiposResiduos_RelacionCN CHECK (
        (RelacionCarbono IS NULL AND RelacionNitrogeno IS NULL) OR
        (RelacionCarbono > 0 AND RelacionNitrogeno > 0)
    )
);
GO

CREATE TABLE waste.RegistrosResiduos (
    RegistroResiduoId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    TipoResiduoId INT NOT NULL,
    PesoKg DECIMAL(8,3) NOT NULL,
    FechaRegistro DATETIME2 DEFAULT GETDATE(),
    EvidenciaArchivoId INT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'PENDIENTE',
    ValidadoPor INT NULL,
    FechaValidacion DATETIME2 NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CHK_RegistrosResiduos_Peso CHECK (PesoKg > 0),
    CONSTRAINT CHK_RegistrosResiduos_Estado CHECK (Estado IN ('PENDIENTE', 'VALIDADO', 'RECHAZADO'))
);
GO

CREATE TABLE rewards.Recompensas (
    RecompensaId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    TipoRecompensaId INT NOT NULL,
    CostoPuntos INT NOT NULL,
    StockDisponible INT NULL,
    EsIlimitado BIT NOT NULL DEFAULT 0,
    FechaVigenciaDesde DATE NULL,
    FechaVigenciaHasta DATE NULL,
    ImagenArchivoId INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CHK_Recompensas_Costo CHECK (CostoPuntos > 0),
    CONSTRAINT CHK_Recompensas_Stock CHECK (
        (EsIlimitado = 1 AND StockDisponible IS NULL) OR
        (EsIlimitado = 0 AND StockDisponible IS NOT NULL AND StockDisponible >= 0)
    ),
    CONSTRAINT CHK_Recompensas_Vigencia CHECK (
        FechaVigenciaDesde IS NULL OR 
        FechaVigenciaHasta IS NULL OR 
        FechaVigenciaDesde <= FechaVigenciaHasta
    )
);
GO

CREATE TABLE rewards.CanjesRecompensas (
    CanjeId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    RecompensaId INT NOT NULL,
    FechaCanje DATETIME2 DEFAULT GETDATE(),
    PuntosGastados INT NOT NULL,
    ComprobanteArchivoId INT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'COMPLETADO',
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CHK_CanjesRecompensas_Puntos CHECK (PuntosGastados > 0),
    CONSTRAINT CHK_CanjesRecompensas_Estado CHECK (Estado IN ('COMPLETADO', 'CANCELADO', 'REVERSADO'))
);
GO

CREATE TABLE rewards.PuntosHistoricos (
    PuntoHistoricoId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    FechaCambio DATETIME2 DEFAULT GETDATE(),
    Monto INT NOT NULL,
    TipoMovimiento NVARCHAR(20) NOT NULL,
    SaldoPosterior INT NOT NULL,
    Motivo NVARCHAR(255) NOT NULL,
    DocumentoReferencia NVARCHAR(100) NULL,
    RegistroResiduoOrigenId INT NULL,
    CanjeOrigenId INT NULL,
    CONSTRAINT CHK_PuntosHistoricos_Tipo CHECK (TipoMovimiento IN ('CREDITO', 'DEBITO', 'REVERSO', 'AJUSTE')),
    CONSTRAINT CHK_PuntosHistoricos_Origen CHECK (
        (RegistroResiduoOrigenId IS NOT NULL AND CanjeOrigenId IS NULL) OR
        (RegistroResiduoOrigenId IS NULL AND CanjeOrigenId IS NOT NULL) OR
        (TipoMovimiento = 'AJUSTE' AND RegistroResiduoOrigenId IS NULL AND CanjeOrigenId IS NULL)
    )
);
GO

CREATE TABLE education.ContenidoEducativo (
    ContenidoId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    Titulo NVARCHAR(200) NOT NULL,
    TipoContenidoId INT NOT NULL,
    CategoriaContenidoId INT NOT NULL,
    RecursoArchivoId INT NULL,
    EsExterno BIT NOT NULL DEFAULT 0,
    FuenteExterna NVARCHAR(500) NULL,
    FechaPublicacion DATE DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CHK_ContenidoEducativo_Externo CHECK (
        (EsExterno = 1 AND FuenteExterna IS NOT NULL) OR
        (EsExterno = 0 AND RecursoArchivoId IS NOT NULL)
    )
);
GO

CREATE TABLE education.UsuariosContenidoVisto (
    UsuarioContenidoVistoId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    ContenidoId INT NOT NULL,
    FechaVisionado DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT UQ_UsuariosContenidoVisto UNIQUE (UsuarioId, ContenidoId)
);
GO

CREATE TABLE [identity].Notificaciones (
    NotificacionId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    TipoNotificacionId INT NOT NULL,
    Mensaje NVARCHAR(500) NOT NULL,
    FechaEnvio DATETIME2 DEFAULT GETDATE(),
    Leida BIT NOT NULL DEFAULT 0
);
GO

CREATE TABLE [identity].FeedbackUsuarios (
    FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    UsuarioId INT NOT NULL,
    TipoFeedbackId INT NOT NULL,
    Mensaje NVARCHAR(1000) NOT NULL,
    Fecha DATETIME2 DEFAULT GETDATE(),
    EstadoFeedbackId INT NOT NULL
);
GO

CREATE TABLE config.ConfiguracionGeneral (
    ConfiguracionGeneralId INT IDENTITY(1,1) PRIMARY KEY,
    PuntosPorKgCompostado DECIMAL(5,2) NOT NULL DEFAULT 0.8,
    CO2PorKgResiduo DECIMAL(5,2) NOT NULL DEFAULT 0.5,
    DiasRetencionAuditoria INT NOT NULL DEFAULT 90,
    MaxIntentosLogin INT NOT NULL DEFAULT 5,
    MinutosBloqueo INT NOT NULL DEFAULT 30,
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedBy INT NULL,
    CONSTRAINT CK_ConfiguracionGeneral_Singleton CHECK (ConfiguracionGeneralId = 1)
);
GO

/* 6. TABLAS DE SEGURIDAD */
CREATE TABLE [identity].Roles (
    RolId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL DEFAULT 1,
    Nombre NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(200) NULL,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT UQ_Roles_Tenant_Nombre UNIQUE (TenantId, Nombre)
);
GO

CREATE TABLE [identity].Permisos (
    PermisoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NULL
);
GO

CREATE TABLE [identity].RolesPermisos (
    RolPermisoId INT IDENTITY(1,1) PRIMARY KEY,
    RolId INT NOT NULL,
    PermisoId INT NOT NULL,
    CONSTRAINT UQ_RolesPermisos_Rol_Permiso UNIQUE (RolId, PermisoId)
);
GO

CREATE TABLE [identity].UsuariosRoles (
    UsuarioRolId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    RolId INT NOT NULL,
    FechaAsignacion DATETIME2 DEFAULT GETDATE(),
    AsignadoPor INT NULL,
    CONSTRAINT UQ_UsuariosRoles_Usuario_Rol UNIQUE (UsuarioId, RolId)
);
GO

/* 7. TABLA DE OUTBOX PARA EVENTOS */
CREATE TABLE audit.OutboxMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EventType NVARCHAR(200) NOT NULL,
    EventPayload NVARCHAR(MAX) NOT NULL,
    AggregateType NVARCHAR(100) NOT NULL,
    AggregateId NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    ProcessedAt DATETIME2 NULL,
    ErrorCount INT NOT NULL DEFAULT 0,
    LastError NVARCHAR(MAX) NULL
);
GO

CREATE INDEX IX_OutboxMessages_Unprocessed ON audit.OutboxMessages(ProcessedAt) WHERE ProcessedAt IS NULL;
GO

/* 8. TABLAS DE AUDITORÍA */
CREATE TABLE audit.Auditoria (
    AuditoriaId INT IDENTITY(1,1) PRIMARY KEY,
    Tabla NVARCHAR(100) NOT NULL,
    Accion NVARCHAR(20) NOT NULL,
    RegistroId INT NOT NULL,
    UsuarioId INT NULL,
    UsuarioEmail NVARCHAR(255) NULL,
    IPAddress NVARCHAR(50) NULL,
    HostName NVARCHAR(100) NULL,
    Fecha DATETIME2 DEFAULT GETDATE(),
    ValorAnterior NVARCHAR(MAX) NULL,
    ValorNuevo NVARCHAR(MAX) NULL,
    ColumnaModificada NVARCHAR(100) NULL,
    TenantId INT NULL,
    CONSTRAINT CHK_Auditoria_Accion CHECK (Accion IN ('INSERT', 'UPDATE', 'DELETE', 'LOGIN', 'LOGOUT'))
);
GO

CREATE TABLE audit.AuditoriaLogin (
    AuditoriaLoginId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    UsuarioEmail NVARCHAR(255) NULL,
    Accion NVARCHAR(20) NOT NULL,
    IPAddress NVARCHAR(50) NULL,
    HostName NVARCHAR(100) NULL,
    Mensaje NVARCHAR(500) NULL,
    Fecha DATETIME2 DEFAULT GETDATE(),
    TenantId INT NULL
);
GO

CREATE TABLE audit.AuditoriaConfiguracion (
    ConfiguracionId INT IDENTITY(1,1) PRIMARY KEY,
    Tabla NVARCHAR(100) NOT NULL UNIQUE,
    Activa BIT NOT NULL DEFAULT 1,
    AuditarInsert BIT NOT NULL DEFAULT 1,
    AuditarUpdate BIT NOT NULL DEFAULT 1,
    AuditarDelete BIT NOT NULL DEFAULT 1,
    FechaConfiguracion DATETIME2 DEFAULT GETDATE(),
    ConfiguradoPor INT NULL
);
GO

-- Tabla de migraciones
CREATE TABLE __EFMigrationsHistory (
    MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion NVARCHAR(32) NOT NULL
);
GO

/* 9. FOREIGN KEYS */
ALTER TABLE [identity].Archivos ADD CONSTRAINT FK_Archivos_TipoArchivo 
    FOREIGN KEY (TipoArchivoId) REFERENCES [identity].TiposArchivo(TipoArchivoId);
GO

ALTER TABLE [identity].Niveles ADD CONSTRAINT FK_Niveles_InsigniaArchivo 
    FOREIGN KEY (InsigniaArchivoId) REFERENCES [identity].Archivos(ArchivoId);
GO

ALTER TABLE [identity].Usuarios ADD CONSTRAINT FK_Usuarios_NivelIdActual 
    FOREIGN KEY (NivelIdActual) REFERENCES [identity].Niveles(NivelId);
GO

ALTER TABLE [identity].Usuarios ADD CONSTRAINT FK_Usuarios_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE [identity].PreferenciasUsuarios ADD CONSTRAINT FK_PreferenciasUsuarios_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].Notificaciones ADD CONSTRAINT FK_Notificaciones_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].Notificaciones ADD CONSTRAINT FK_Notificaciones_TipoNotificacion 
    FOREIGN KEY (TipoNotificacionId) REFERENCES [identity].TiposNotificacion(TipoNotificacionId);
GO

ALTER TABLE [identity].FeedbackUsuarios ADD CONSTRAINT FK_FeedbackUsuarios_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].FeedbackUsuarios ADD CONSTRAINT FK_FeedbackUsuarios_TipoFeedback 
    FOREIGN KEY (TipoFeedbackId) REFERENCES [identity].TiposFeedback(TipoFeedbackId);
GO

ALTER TABLE [identity].FeedbackUsuarios ADD CONSTRAINT FK_FeedbackUsuarios_EstadoFeedback 
    FOREIGN KEY (EstadoFeedbackId) REFERENCES [identity].EstadosFeedback(EstadoFeedbackId);
GO

ALTER TABLE [identity].Roles ADD CONSTRAINT FK_Roles_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE [identity].RolesPermisos ADD CONSTRAINT FK_RolesPermisos_Rol 
    FOREIGN KEY (RolId) REFERENCES [identity].Roles(RolId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].RolesPermisos ADD CONSTRAINT FK_RolesPermisos_Permiso 
    FOREIGN KEY (PermisoId) REFERENCES [identity].Permisos(PermisoId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].UsuariosRoles ADD CONSTRAINT FK_UsuariosRoles_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].UsuariosRoles ADD CONSTRAINT FK_UsuariosRoles_Rol 
    FOREIGN KEY (RolId) REFERENCES [identity].Roles(RolId) ON DELETE CASCADE;
GO

ALTER TABLE [identity].UsuariosRoles ADD CONSTRAINT FK_UsuariosRoles_AsignadoPor 
    FOREIGN KEY (AsignadoPor) REFERENCES [identity].Usuarios(UsuarioId);
GO

ALTER TABLE waste.TiposResiduos ADD CONSTRAINT FK_TiposResiduos_CalidadResiduo 
    FOREIGN KEY (CalidadResiduoId) REFERENCES waste.CalidadesResiduo(CalidadResiduoId);
GO

ALTER TABLE waste.RegistrosResiduos ADD CONSTRAINT FK_RegistrosResiduos_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE waste.RegistrosResiduos ADD CONSTRAINT FK_RegistrosResiduos_TipoResiduo 
    FOREIGN KEY (TipoResiduoId) REFERENCES waste.TiposResiduos(TipoResiduoId);
GO

ALTER TABLE waste.RegistrosResiduos ADD CONSTRAINT FK_RegistrosResiduos_EvidenciaArchivo 
    FOREIGN KEY (EvidenciaArchivoId) REFERENCES [identity].Archivos(ArchivoId);
GO

ALTER TABLE waste.RegistrosResiduos ADD CONSTRAINT FK_RegistrosResiduos_ValidadoPor 
    FOREIGN KEY (ValidadoPor) REFERENCES [identity].Usuarios(UsuarioId);
GO

ALTER TABLE waste.RegistrosResiduos ADD CONSTRAINT FK_RegistrosResiduos_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE rewards.Recompensas ADD CONSTRAINT FK_Recompensas_TipoRecompensa 
    FOREIGN KEY (TipoRecompensaId) REFERENCES [identity].TiposRecompensa(TipoRecompensaId);
GO

ALTER TABLE rewards.Recompensas ADD CONSTRAINT FK_Recompensas_ImagenArchivo 
    FOREIGN KEY (ImagenArchivoId) REFERENCES [identity].Archivos(ArchivoId);
GO

ALTER TABLE rewards.Recompensas ADD CONSTRAINT FK_Recompensas_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE rewards.CanjesRecompensas ADD CONSTRAINT FK_CanjesRecompensas_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE rewards.CanjesRecompensas ADD CONSTRAINT FK_CanjesRecompensas_Recompensa 
    FOREIGN KEY (RecompensaId) REFERENCES rewards.Recompensas(RecompensaId);
GO

ALTER TABLE rewards.CanjesRecompensas ADD CONSTRAINT FK_CanjesRecompensas_ComprobanteArchivo 
    FOREIGN KEY (ComprobanteArchivoId) REFERENCES [identity].Archivos(ArchivoId);
GO

ALTER TABLE rewards.CanjesRecompensas ADD CONSTRAINT FK_CanjesRecompensas_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE rewards.PuntosHistoricos ADD CONSTRAINT FK_PuntosHistoricos_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE rewards.PuntosHistoricos ADD CONSTRAINT FK_PuntosHistoricos_RegistroResiduoOrigen 
    FOREIGN KEY (RegistroResiduoOrigenId) REFERENCES waste.RegistrosResiduos(RegistroResiduoId);
GO

ALTER TABLE rewards.PuntosHistoricos ADD CONSTRAINT FK_PuntosHistoricos_CanjeOrigen 
    FOREIGN KEY (CanjeOrigenId) REFERENCES rewards.CanjesRecompensas(CanjeId);
GO

ALTER TABLE rewards.PuntosHistoricos ADD CONSTRAINT FK_PuntosHistoricos_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE education.ContenidoEducativo ADD CONSTRAINT FK_ContenidoEducativo_TipoContenido 
    FOREIGN KEY (TipoContenidoId) REFERENCES education.TiposContenido(TipoContenidoId);
GO

ALTER TABLE education.ContenidoEducativo ADD CONSTRAINT FK_ContenidoEducativo_CategoriaContenido 
    FOREIGN KEY (CategoriaContenidoId) REFERENCES education.CategoriasContenido(CategoriaContenidoId);
GO

ALTER TABLE education.ContenidoEducativo ADD CONSTRAINT FK_ContenidoEducativo_RecursoArchivo 
    FOREIGN KEY (RecursoArchivoId) REFERENCES [identity].Archivos(ArchivoId);
GO

ALTER TABLE education.ContenidoEducativo ADD CONSTRAINT FK_ContenidoEducativo_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE education.UsuariosContenidoVisto ADD CONSTRAINT FK_UsuariosContenidoVisto_Usuario 
    FOREIGN KEY (UsuarioId) REFERENCES [identity].Usuarios(UsuarioId) ON DELETE CASCADE;
GO

ALTER TABLE education.UsuariosContenidoVisto ADD CONSTRAINT FK_UsuariosContenidoVisto_Contenido 
    FOREIGN KEY (ContenidoId) REFERENCES education.ContenidoEducativo(ContenidoId) ON DELETE CASCADE;
GO

ALTER TABLE education.UsuariosContenidoVisto ADD CONSTRAINT FK_UsuariosContenidoVisto_Tenant 
    FOREIGN KEY (TenantId) REFERENCES config.Tenants(TenantId);
GO

ALTER TABLE config.ConfiguracionGeneral ADD CONSTRAINT FK_ConfiguracionGeneral_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES [identity].Usuarios(UsuarioId);
GO

/* 10. ÍNDICES OPTIMIZADOS */
CREATE INDEX IX_Usuarios_Tenant_Email ON [identity].Usuarios(TenantId, CorreoElectronico) INCLUDE(UsuarioId, ContrasenaHash, Activo, BloqueadoHasta);
CREATE INDEX IX_Usuarios_NivelIdActual ON [identity].Usuarios(NivelIdActual);
CREATE INDEX IX_Usuarios_Activo ON [identity].Usuarios(Activo) WHERE Activo = 1;
CREATE INDEX IX_Usuarios_BloqueadoHasta ON [identity].Usuarios(BloqueadoHasta) WHERE BloqueadoHasta IS NOT NULL;

CREATE INDEX IX_PreferenciasUsuarios_UsuarioId ON [identity].PreferenciasUsuarios(UsuarioId);
CREATE INDEX IX_PreferenciasUsuarios_Clave ON [identity].PreferenciasUsuarios(Clave);

CREATE INDEX IX_Notificaciones_UsuarioId_Leida ON [identity].Notificaciones(UsuarioId, Leida) INCLUDE(Mensaje, FechaEnvio);
CREATE INDEX IX_Notificaciones_FechaEnvio ON [identity].Notificaciones(FechaEnvio DESC);

CREATE INDEX IX_FeedbackUsuarios_UsuarioId ON [identity].FeedbackUsuarios(UsuarioId);
CREATE INDEX IX_FeedbackUsuarios_EstadoFeedbackId ON [identity].FeedbackUsuarios(EstadoFeedbackId);
CREATE INDEX IX_FeedbackUsuarios_Fecha ON [identity].FeedbackUsuarios(Fecha DESC);

CREATE INDEX IX_RegistrosResiduos_Usuario_Fecha ON waste.RegistrosResiduos(UsuarioId, FechaRegistro DESC) INCLUDE(PesoKg, TipoResiduoId, Estado);
CREATE INDEX IX_RegistrosResiduos_TipoResiduoId ON waste.RegistrosResiduos(TipoResiduoId);
CREATE INDEX IX_RegistrosResiduos_FechaRegistro ON waste.RegistrosResiduos(FechaRegistro DESC);
CREATE INDEX IX_RegistrosResiduos_Tenant ON waste.RegistrosResiduos(TenantId);
CREATE INDEX IX_RegistrosResiduos_Estado ON waste.RegistrosResiduos(Estado) WHERE Estado = 'PENDIENTE';

CREATE INDEX IX_TiposResiduos_Activo ON waste.TiposResiduos(Activo) WHERE Activo = 1;
CREATE INDEX IX_TiposResiduos_CalidadResiduoId ON waste.TiposResiduos(CalidadResiduoId);

CREATE INDEX IX_CanjesRecompensas_Usuario_Fecha ON rewards.CanjesRecompensas(UsuarioId, FechaCanje DESC) INCLUDE(RecompensaId, PuntosGastados, Estado);
CREATE INDEX IX_CanjesRecompensas_RecompensaId ON rewards.CanjesRecompensas(RecompensaId);
CREATE INDEX IX_CanjesRecompensas_Tenant ON rewards.CanjesRecompensas(TenantId);

CREATE INDEX IX_PuntosHistoricos_Usuario_Fecha ON rewards.PuntosHistoricos(UsuarioId, FechaCambio DESC);
CREATE INDEX IX_PuntosHistoricos_Tenant ON rewards.PuntosHistoricos(TenantId);
CREATE INDEX IX_PuntosHistoricos_Origen ON rewards.PuntosHistoricos(RegistroResiduoOrigenId, CanjeOrigenId);

CREATE INDEX IX_Recompensas_TipoRecompensaId ON rewards.Recompensas(TipoRecompensaId);
CREATE INDEX IX_Recompensas_CostoPuntos ON rewards.Recompensas(CostoPuntos);
CREATE INDEX IX_Recompensas_Vigencia ON rewards.Recompensas(FechaVigenciaDesde, FechaVigenciaHasta) WHERE Activo = 1;
CREATE INDEX IX_Recompensas_Tenant ON rewards.Recompensas(TenantId);

CREATE INDEX IX_ContenidoEducativo_TipoContenidoId ON education.ContenidoEducativo(TipoContenidoId);
CREATE INDEX IX_ContenidoEducativo_CategoriaContenidoId ON education.ContenidoEducativo(CategoriaContenidoId);
CREATE INDEX IX_ContenidoEducativo_FechaPublicacion ON education.ContenidoEducativo(FechaPublicacion DESC);
CREATE INDEX IX_ContenidoEducativo_Activo ON education.ContenidoEducativo(Activo) WHERE Activo = 1;
CREATE INDEX IX_ContenidoEducativo_Tenant ON education.ContenidoEducativo(TenantId);

CREATE INDEX IX_UsuariosContenidoVisto_UsuarioId ON education.UsuariosContenidoVisto(UsuarioId);
CREATE INDEX IX_UsuariosContenidoVisto_ContenidoId ON education.UsuariosContenidoVisto(ContenidoId);

CREATE INDEX IX_UsuariosRoles_RolId ON [identity].UsuariosRoles(RolId);
CREATE INDEX IX_UsuariosRoles_UsuarioId ON [identity].UsuariosRoles(UsuarioId);
CREATE INDEX IX_RolesPermisos_PermisoId ON [identity].RolesPermisos(PermisoId);
CREATE INDEX IX_Roles_Tenant ON [identity].Roles(TenantId);

CREATE INDEX IX_Archivos_TipoArchivoId ON [identity].Archivos(TipoArchivoId);
CREATE INDEX IX_Archivos_FechaCreacion ON [identity].Archivos(FechaCreacion DESC);
CREATE INDEX IX_Archivos_Activo ON [identity].Archivos(Activo) WHERE Activo = 1;

CREATE INDEX IX_Auditoria_Tabla_RegistroId ON audit.Auditoria(Tabla, RegistroId);
CREATE INDEX IX_Auditoria_Fecha ON audit.Auditoria(Fecha DESC);
CREATE INDEX IX_Auditoria_UsuarioId ON audit.Auditoria(UsuarioId);
CREATE INDEX IX_Auditoria_Tenant ON audit.Auditoria(TenantId);
CREATE INDEX IX_Auditoria_Tabla_Fecha ON audit.Auditoria(Tabla, Fecha DESC);

CREATE INDEX IX_AuditoriaLogin_UsuarioId ON audit.AuditoriaLogin(UsuarioId);
CREATE INDEX IX_AuditoriaLogin_Fecha ON audit.AuditoriaLogin(Fecha DESC);
CREATE INDEX IX_AuditoriaLogin_Accion ON audit.AuditoriaLogin(Accion);
GO

/* 11. FUNCIONES */
CREATE FUNCTION [identity].fn_TenantPredicate(@TenantId INT)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS Result WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS INT);
GO

CREATE FUNCTION [identity].fn_EsAdministrador (@UsuarioId INT)
RETURNS BIT
AS
BEGIN
    DECLARE @EsAdmin BIT = 0;
    
    IF EXISTS (
        SELECT 1 
        FROM [identity].UsuariosRoles ur
        INNER JOIN [identity].Roles r ON ur.RolId = r.RolId
        WHERE ur.UsuarioId = @UsuarioId AND r.Nombre = 'Administrador'
    )
    BEGIN
        SET @EsAdmin = 1;
    END
    
    RETURN @EsAdmin;
END
GO

CREATE FUNCTION [identity].fn_ObtenerIPAddress()
RETURNS NVARCHAR(50)
AS
BEGIN
    RETURN CAST(CONNECTIONPROPERTY('client_net_address') AS NVARCHAR(50));
END
GO

CREATE FUNCTION [identity].fn_ObtenerHostName()
RETURNS NVARCHAR(100)
AS
BEGIN
    RETURN CAST(CONNECTIONPROPERTY('client_hostname') AS NVARCHAR(100));
END
GO

/* 12. SEED DATA / INSERTS */
INSERT INTO config.Tenants (Nombre, Subdominio) VALUES ('Default', 'default');
GO

INSERT INTO [identity].TiposRecompensa (Nombre) VALUES ('Digital'), ('Tangible');
INSERT INTO [identity].TiposNotificacion (Nombre) VALUES ('Recordatorio'), ('Logro'), ('RecompensaDisponible');
INSERT INTO [identity].EstadosFeedback (Nombre) VALUES ('Pendiente'), ('Resuelto');
INSERT INTO [identity].TiposFeedback (Nombre) VALUES ('Sugerencia'), ('Problema'), ('Idea');
INSERT INTO [identity].TiposArchivo (Nombre) VALUES ('Imagen'), ('Video'), ('PDF'), ('EnlaceExterno');
GO

INSERT INTO waste.CalidadesResiduo (Nombre, FactorBase) VALUES ('Alta', 15), ('Media', 12);
GO

INSERT INTO education.TiposContenido (Nombre) VALUES ('Guia practica'), ('Video'), ('Infografia'), ('Articulo');
INSERT INTO education.CategoriasContenido (Nombre) VALUES ('Compostaje domestico'), ('Separacion de residuos'), ('Impacto ambiental');
GO

INSERT INTO [identity].Niveles (NombreNivel, PuntosMinimoNecesario, PuntosMaximo, InsigniaArchivoId) VALUES
('Principiante', 0, 999, NULL),
('Aprendiz', 1000, 4999, NULL),
('Experto', 5000, 14999, NULL),
('Maestro Compostero', 15000, 999999, NULL);
GO

INSERT INTO waste.TiposResiduos (Nombre, CalidadResiduoId, AporteNutricional, RelacionCarbono, RelacionNitrogeno) VALUES
('Cascaras de fruta', 1, 'Alto en potasio y fosforo', 20, 1),
('Restos de verduras', 1, 'Alto en nitrogeno', 15, 1),
('Cascaras de huevo', 2, 'Aporte de calcio', NULL, NULL),
('Borra de cafe', 1, 'Alto en nitrogeno', 20, 1),
('Restos de pan', 2, 'Carbono estructural', 50, 1),
('Hojas secas', 2, 'Carbono estructural', 60, 1),
('Cascaras de citricos', 2, 'Moderado, puede acidificar', 25, 1),
('Restos de te', 1, 'Alto en nitrogeno', 20, 1);
GO

INSERT INTO rewards.Recompensas (TenantId, Nombre, Descripcion, TipoRecompensaId, CostoPuntos, StockDisponible, EsIlimitado, FechaVigenciaDesde, FechaVigenciaHasta, ImagenArchivoId) VALUES
(1, 'Insignia Verde', 'Insignia digital por compromiso ambiental', 1, 500, NULL, 1, '2025-01-01', NULL, NULL),
(1, 'Nivel Avanzado', 'Acceso a contenido exclusivo', 1, 2000, NULL, 1, '2025-01-01', NULL, NULL),
(1, 'Kit de Compostaje', 'Kit basico para compostaje domestico', 2, 5000, 50, 0, '2025-01-01', '2025-12-31', NULL),
(1, 'Semillas Organicas', 'Paquete de semillas de hortalizas', 2, 1500, 100, 0, '2025-01-01', '2025-12-31', NULL);
GO

INSERT INTO [identity].Roles (TenantId, Nombre, Descripcion) VALUES 
(1, 'Administrador', 'Rol con capacidad para crear, editar y eliminar contenido educativo'),
(1, 'Usuario', 'Rol basico para usuarios regulares del sistema');
GO

INSERT INTO [identity].Permisos (Nombre, Descripcion) VALUES 
('contenido.crear', 'Permite crear nuevo contenido educativo'),
('contenido.editar', 'Permite editar contenido educativo existente'),
('contenido.eliminar', 'Permite eliminar contenido educativo'),
('contenido.ver_todos', 'Permite ver todo el contenido educativo'),
('usuarios.ver_todos', 'Permite ver todos los usuarios'),
('usuarios.asignar_roles', 'Permite asignar roles a usuarios'),
('reportes.ver', 'Permite ver reportes y metricas');
GO

DECLARE @RolAdminId INT = (SELECT RolId FROM [identity].Roles WHERE Nombre = 'Administrador' AND TenantId = 1);
DECLARE @RolUsuarioId INT = (SELECT RolId FROM [identity].Roles WHERE Nombre = 'Usuario' AND TenantId = 1);

INSERT INTO [identity].RolesPermisos (RolId, PermisoId)
SELECT @RolAdminId, PermisoId FROM [identity].Permisos;

INSERT INTO [identity].RolesPermisos (RolId, PermisoId)
SELECT @RolUsuarioId, PermisoId FROM [identity].Permisos WHERE Nombre IN ('contenido.ver_todos');
GO

INSERT INTO config.ConfiguracionGeneral (PuntosPorKgCompostado, CO2PorKgResiduo, DiasRetencionAuditoria, MaxIntentosLogin, MinutosBloqueo) 
VALUES (0.8, 0.5, 90, 5, 30);
GO

-- Usuario Administrador
DECLARE @RolAdminId INT;
DECLARE @AdminUserId INT;
DECLARE @TenantId INT = (SELECT TenantId FROM config.Tenants WHERE Nombre = 'Default');

SELECT @RolAdminId = RolId FROM [identity].Roles WHERE Nombre = 'Administrador' AND TenantId = @TenantId;

IF NOT EXISTS (SELECT 1 FROM [identity].Usuarios WHERE CorreoElectronico = 'admin@ecocycle.com' AND TenantId = @TenantId)
BEGIN
    INSERT INTO [identity].Usuarios (TenantId, CorreoElectronico, ContrasenaHash, FechaRegistro, NivelIdActual)
    VALUES (
        @TenantId,
        'admin@ecocycle.com',
        CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'Admin123!'), 2),
        GETDATE(),
        (SELECT NivelId FROM [identity].Niveles WHERE NombreNivel = 'Maestro Compostero')
    );
    
    SET @AdminUserId = SCOPE_IDENTITY();
    
    INSERT INTO [identity].UsuariosRoles (UsuarioId, RolId, AsignadoPor)
    VALUES (@AdminUserId, @RolAdminId, NULL);
    
    PRINT 'Usuario administrador creado exitosamente.';
    PRINT 'Email: admin@ecocycle.com';
    PRINT 'Contrasena: Admin123! (CAMBIAR EN PRODUCCION)';
END
GO

INSERT INTO audit.AuditoriaConfiguracion (Tabla, Activa, AuditarInsert, AuditarUpdate, AuditarDelete) VALUES
('[identity].Usuarios', 1, 1, 1, 1),
('[identity].Niveles', 1, 1, 1, 1),
('waste.TiposResiduos', 1, 1, 1, 1),
('waste.RegistrosResiduos', 1, 1, 1, 1),
('rewards.Recompensas', 1, 1, 1, 1),
('rewards.CanjesRecompensas', 1, 1, 1, 1),
('rewards.PuntosHistoricos', 1, 1, 1, 1),
('education.ContenidoEducativo', 1, 1, 1, 1),
('[identity].Notificaciones', 1, 1, 1, 0),
('[identity].FeedbackUsuarios', 1, 1, 1, 1),
('[identity].PreferenciasUsuarios', 1, 1, 1, 1),
('[identity].Roles', 1, 1, 1, 1),
('[identity].UsuariosRoles', 1, 1, 1, 1);
GO

/* 13. STORED PROCEDURES */
CREATE PROCEDURE audit.sp_RegistrarAuditoria
    @Tabla NVARCHAR(100),
    @Accion NVARCHAR(20),
    @RegistroId INT,
    @UsuarioId INT = NULL,
    @ValorAnterior NVARCHAR(MAX) = NULL,
    @ValorNuevo NVARCHAR(MAX) = NULL,
    @ColumnaModificada NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Auditar BIT = 1;
    DECLARE @TenantId INT = CAST(SESSION_CONTEXT(N'TenantId') AS INT);
    
    SELECT @Auditar = ISNULL(Activa, 1)
    FROM audit.AuditoriaConfiguracion 
    WHERE Tabla = @Tabla;
    
    IF @Auditar = 1
    BEGIN
        IF @UsuarioId IS NULL
            SET @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS INT);
        
        DECLARE @UsuarioEmail NVARCHAR(255) = NULL;
        IF @UsuarioId IS NOT NULL
            SELECT @UsuarioEmail = CorreoElectronico FROM [identity].Usuarios WHERE UsuarioId = @UsuarioId;
        
        INSERT INTO audit.Auditoria (
            Tabla, Accion, RegistroId, UsuarioId, UsuarioEmail,
            IPAddress, HostName, Fecha, ValorAnterior, ValorNuevo, ColumnaModificada, TenantId
        ) VALUES (
            @Tabla, @Accion, @RegistroId, @UsuarioId, @UsuarioEmail,
            [identity].fn_ObtenerIPAddress(), [identity].fn_ObtenerHostName(),
            GETDATE(), @ValorAnterior, @ValorNuevo, @ColumnaModificada, @TenantId
        );
    END
END
GO

CREATE PROCEDURE rewards.sp_ObtenerSaldoPuntos
    @UsuarioId INT,
    @Saldo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT @Saldo = ISNULL(SUM(Monto), 0)
    FROM rewards.PuntosHistoricos
    WHERE UsuarioId = @UsuarioId;
    
    RETURN 0;
END
GO

CREATE PROCEDURE rewards.sp_RegistrarMovimientoPuntos
    @UsuarioId INT,
    @Monto INT,
    @TipoMovimiento NVARCHAR(20),
    @Motivo NVARCHAR(255),
    @DocumentoReferencia NVARCHAR(100) = NULL,
    @RegistroResiduoOrigenId INT = NULL,
    @CanjeOrigenId INT = NULL,
    @NuevoMovimientoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @SaldoActual INT;
        DECLARE @SaldoPosterior INT;
        DECLARE @TenantId INT = CAST(SESSION_CONTEXT(N'TenantId') AS INT);
        
        SELECT @SaldoActual = ISNULL(SUM(Monto), 0)
        FROM rewards.PuntosHistoricos
        WHERE UsuarioId = @UsuarioId;
        
        SET @SaldoPosterior = @SaldoActual + @Monto;
        
        IF @TipoMovimiento IN ('DEBITO', 'REVERSO') AND @SaldoPosterior < 0
        BEGIN
            RAISERROR('Saldo insuficiente para realizar esta operacion.', 16, 1);
            RETURN;
        END
        
        INSERT INTO rewards.PuntosHistoricos (
            TenantId, UsuarioId, Monto, TipoMovimiento, SaldoPosterior, Motivo,
            DocumentoReferencia, RegistroResiduoOrigenId, CanjeOrigenId
        ) VALUES (
            @TenantId, @UsuarioId, @Monto, @TipoMovimiento, @SaldoPosterior, @Motivo,
            @DocumentoReferencia, @RegistroResiduoOrigenId, @CanjeOrigenId
        );
        
        SET @NuevoMovimientoId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* 14. VISTAS */
CREATE VIEW rewards.vw_SaldoPuntosUsuario
AS
SELECT 
    UsuarioId,
    SUM(Monto) AS SaldoActual,
    SUM(CASE WHEN Monto > 0 THEN Monto ELSE 0 END) AS TotalGanado,
    SUM(CASE WHEN Monto < 0 THEN ABS(Monto) ELSE 0 END) AS TotalGastado,
    MAX(FechaCambio) AS UltimoMovimiento
FROM rewards.PuntosHistoricos
GROUP BY UsuarioId;
GO

CREATE VIEW waste.vw_MetricasImpacto
AS
SELECT 
    r.UsuarioId,
    CAST(r.FechaRegistro AS DATE) AS FechaCalculo,
    SUM(r.PesoKg) AS ResiduosRegistradosTotalKg,
    SUM(r.PesoKg * 0.8) AS KgCompostadosAprovechados,
    SUM(r.PesoKg * 0.5) AS CO2EstimadoAhorradoKg
FROM waste.RegistrosResiduos r
WHERE r.Estado = 'VALIDADO'
GROUP BY r.UsuarioId, CAST(r.FechaRegistro AS DATE);
GO

CREATE VIEW audit.vw_AuditoriaCompleta
AS
SELECT 
    a.AuditoriaId,
    a.Tabla,
    a.Accion,
    a.RegistroId,
    a.UsuarioId,
    a.UsuarioEmail,
    a.IPAddress,
    a.HostName,
    a.Fecha,
    a.ValorAnterior,
    a.ValorNuevo,
    a.ColumnaModificada,
    a.TenantId
FROM audit.Auditoria a;
GO

/* 15. MANTENIMIENTO */
CREATE PROCEDURE maintenance.sp_RebuildIndexes
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX) = '';
    
    SELECT @SQL = @SQL + 
        'ALTER INDEX ALL ON ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' REBUILD WITH (ONLINE = ON, SORT_IN_TEMPDB = ON);' + CHAR(13)
    FROM sys.tables
    WHERE is_ms_shipped = 0;
    
    EXEC sp_executesql @SQL;
    
    PRINT 'Indices reconstruidos exitosamente.';
END
GO

CREATE PROCEDURE maintenance.sp_UpdateStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX) = '';
    
    SELECT @SQL = @SQL + 
        'UPDATE STATISTICS ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' WITH FULLSCAN;' + CHAR(13)
    FROM sys.tables
    WHERE is_ms_shipped = 0;
    
    EXEC sp_executesql @SQL;
    
    PRINT 'Estadisticas actualizadas exitosamente.';
END
GO

CREATE PROCEDURE maintenance.sp_PurgeAuditoria
    @DiasRetencion INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FechaCorte DATETIME2 = DATEADD(DAY, -@DiasRetencion, GETDATE());
    
    DELETE FROM audit.Auditoria WHERE Fecha < @FechaCorte;
    DELETE FROM audit.AuditoriaLogin WHERE Fecha < @FechaCorte;
    
    PRINT 'Auditoria limpiada exitosamente. Registros anteriores a ' + CAST(@FechaCorte AS NVARCHAR(50)) + ' eliminados.';
END
GO

/* 16. VALIDACIONES FINALES */
PRINT 'BASE DE DATOS ECOCYCLE - VERSION ENTERPRISE';
PRINT '';

DECLARE @TableCount INT = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE');
DECLARE @ProcCount INT = (SELECT COUNT(*) FROM sys.procedures WHERE SCHEMA_NAME(schema_id) NOT IN ('sys'));
DECLARE @FuncCount INT = (SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0);
DECLARE @ViewCount INT = (SELECT COUNT(*) FROM sys.views WHERE is_ms_shipped = 0);
DECLARE @IndexCount INT = (SELECT COUNT(*) FROM sys.indexes WHERE object_id IN (SELECT object_id FROM sys.tables WHERE is_ms_shipped = 0) AND index_id > 0);

PRINT 'ESTADISTICAS:';
PRINT '  - Tablas: ' + CAST(@TableCount AS NVARCHAR(10));
PRINT '  - Procedimientos: ' + CAST(@ProcCount AS NVARCHAR(10));
PRINT '  - Funciones: ' + CAST(@FuncCount AS NVARCHAR(10));
PRINT '  - Vistas: ' + CAST(@ViewCount AS NVARCHAR(10));
PRINT '  - Indices: ' + CAST(@IndexCount AS NVARCHAR(10));
PRINT '';

PRINT 'CREDENCIALES DE ADMINISTRADOR:';
PRINT '  Email: admin@ecocycle.com';
PRINT '  Contrasena: Admin123! (CAMBIAR EN PRODUCCION)';
PRINT '';

PRINT 'BASE DE DATOS LISTA PARA PRODUCCION';
GO