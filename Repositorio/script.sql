-- BASE DE DATOS ECOCYCLE
CREATE DATABASE Ecocycle;
GO

USE Ecocycle;
GO

-- 1. TABLAS CATALOGO (dominios)
CREATE TABLE TiposRecompensa (
    TipoRecompensaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE TiposNotificacion (
    TipoNotificacionId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE EstadosFeedback (
    EstadoFeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE TiposFeedback (
    TipoFeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE CalidadesResiduo (
    CalidadResiduoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    FactorBase DECIMAL(5,2) NOT NULL
);
GO

CREATE TABLE TiposContenido (
    TipoContenidoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE CategoriasContenido (
    CategoriaContenidoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE TiposArchivo (
    TipoArchivoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 2. TABLAS PRINCIPALES
CREATE TABLE Archivos (
    ArchivoId INT IDENTITY(1,1) PRIMARY KEY,
    Url NVARCHAR(500) NOT NULL,
    TipoArchivoId INT NOT NULL,
    EsExterno BIT NOT NULL DEFAULT 0,
    Proveedor NVARCHAR(100) NULL,
    Descripcion NVARCHAR(200) NULL,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (TipoArchivoId) REFERENCES TiposArchivo(TipoArchivoId)
);
GO

CREATE TABLE Niveles (
    NivelId INT IDENTITY(1,1) PRIMARY KEY,
    NombreNivel NVARCHAR(50) NOT NULL,
    PuntosMinimoNecesario INT NOT NULL,
    PuntosMaximo INT NOT NULL,
    InsigniaArchivoId INT NULL,
    FOREIGN KEY (InsigniaArchivoId) REFERENCES Archivos(ArchivoId),
    CONSTRAINT CHK_Niveles_Puntos CHECK (PuntosMinimoNecesario < PuntosMaximo)
);
GO

CREATE TABLE Usuarios (
    UsuarioId INT IDENTITY(1,1) PRIMARY KEY,
    CorreoElectronico NVARCHAR(255) NOT NULL UNIQUE,
    ContrasenaHash NVARCHAR(255) NOT NULL,
    FechaRegistro DATETIME2 DEFAULT GETDATE(),
    FechaUltimoInicioSesion DATETIME2 NULL,
    NivelIdActual INT NOT NULL,
    FOREIGN KEY (NivelIdActual) REFERENCES Niveles(NivelId)
);
GO

CREATE TABLE PreferenciasUsuarios (
    PreferenciaUsuarioId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    Clave NVARCHAR(100) NOT NULL,
    Valor NVARCHAR(500) NOT NULL,
    FechaActualizacion DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT UQ_PreferenciasUsuarios_Usuario_Clave UNIQUE (UsuarioId, Clave)
);
GO

CREATE TABLE TiposResiduos (
    TipoResiduoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    CalidadResiduoId INT NOT NULL,
    AporteNutricional NVARCHAR(500) NULL,
    RelacionCarbono INT NULL,
    RelacionNitrogeno INT NULL,
    FOREIGN KEY (CalidadResiduoId) REFERENCES CalidadesResiduo(CalidadResiduoId)
);
GO

CREATE TABLE RegistrosResiduos (
    RegistroResiduoId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    TipoResiduoId INT NOT NULL,
    PesoKg DECIMAL(8,3) NOT NULL,
    FechaRegistro DATETIME2 DEFAULT GETDATE(),
    EvidenciaArchivoId INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (TipoResiduoId) REFERENCES TiposResiduos(TipoResiduoId),
    FOREIGN KEY (EvidenciaArchivoId) REFERENCES Archivos(ArchivoId),
    CONSTRAINT CHK_RegistrosResiduos_Peso CHECK (PesoKg > 0)
);
GO

CREATE TABLE CanjesRecompensas (
    CanjeId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    RecompensaId INT NOT NULL,
    FechaCanje DATETIME2 DEFAULT GETDATE(),
    PuntosGastados INT NOT NULL,
    ComprobanteArchivoId INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (ComprobanteArchivoId) REFERENCES Archivos(ArchivoId),
    CONSTRAINT CHK_CanjesRecompensas_Puntos CHECK (PuntosGastados > 0)
);
GO

CREATE TABLE PuntosHistoricos (
    PuntoHistoricoId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    FechaCambio DATETIME2 DEFAULT GETDATE(),
    PuntosAcumulados INT NOT NULL,
    Motivo NVARCHAR(255) NOT NULL,
    RegistroResiduoOrigenId INT NULL,
    CanjeOrigenId INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (RegistroResiduoOrigenId) REFERENCES RegistrosResiduos(RegistroResiduoId),
    FOREIGN KEY (CanjeOrigenId) REFERENCES CanjesRecompensas(CanjeId),
    CONSTRAINT CHK_PuntosHistoricos_Puntos CHECK (PuntosAcumulados >= 0),
    CONSTRAINT CHK_PuntosHistoricos_Origen CHECK (
        (RegistroResiduoOrigenId IS NOT NULL AND CanjeOrigenId IS NULL) OR
        (RegistroResiduoOrigenId IS NULL AND CanjeOrigenId IS NOT NULL)
    )
);
GO

CREATE TABLE Recompensas (
    RecompensaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    TipoRecompensaId INT NOT NULL,
    CostoPuntos INT NOT NULL,
    StockDisponible INT NULL,
    EsIlimitado BIT NOT NULL DEFAULT 0,
    FechaVigenciaDesde DATE NULL,
    FechaVigenciaHasta DATE NULL,
    ImagenArchivoId INT NULL,
    FOREIGN KEY (TipoRecompensaId) REFERENCES TiposRecompensa(TipoRecompensaId),
    FOREIGN KEY (ImagenArchivoId) REFERENCES Archivos(ArchivoId),
    CONSTRAINT CHK_Recompensas_Costo CHECK (CostoPuntos > 0),
    CONSTRAINT CHK_Recompensas_Stock CHECK (
        (EsIlimitado = 1 AND StockDisponible IS NULL) OR
        (EsIlimitado = 0 AND StockDisponible IS NOT NULL AND StockDisponible >= 0)
    ),
    CONSTRAINT CHK_Recompensas_Vigencia CHECK (FechaVigenciaDesde IS NULL OR FechaVigenciaHasta IS NULL OR FechaVigenciaDesde <= FechaVigenciaHasta)
);
GO

ALTER TABLE CanjesRecompensas ADD CONSTRAINT FK_CanjesRecompensas_Recompensas 
FOREIGN KEY (RecompensaId) REFERENCES Recompensas(RecompensaId);
GO

CREATE TABLE ContenidoEducativo (
    ContenidoId INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(200) NOT NULL,
    TipoContenidoId INT NOT NULL,
    CategoriaContenidoId INT NOT NULL,
    RecursoArchivoId INT NULL,
    EsExterno BIT NOT NULL DEFAULT 0,
    FuenteExterna NVARCHAR(500) NULL,
    FechaPublicacion DATE DEFAULT GETDATE(),
    FOREIGN KEY (TipoContenidoId) REFERENCES TiposContenido(TipoContenidoId),
    FOREIGN KEY (CategoriaContenidoId) REFERENCES CategoriasContenido(CategoriaContenidoId),
    FOREIGN KEY (RecursoArchivoId) REFERENCES Archivos(ArchivoId)
);
GO

CREATE TABLE UsuariosContenidoVisto (
    UsuarioContenidoVistoId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    ContenidoId INT NOT NULL,
    FechaVisionado DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (ContenidoId) REFERENCES ContenidoEducativo(ContenidoId) ON DELETE CASCADE,
    CONSTRAINT UQ_UsuariosContenidoVisto UNIQUE (UsuarioId, ContenidoId)
);
GO

CREATE TABLE Notificaciones (
    NotificacionId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    TipoNotificacionId INT NOT NULL,
    Mensaje NVARCHAR(500) NOT NULL,
    FechaEnvio DATETIME2 DEFAULT GETDATE(),
    Leida BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (TipoNotificacionId) REFERENCES TiposNotificacion(TipoNotificacionId)
);
GO

CREATE TABLE FeedbackUsuarios (
    FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    TipoFeedbackId INT NOT NULL,
    Mensaje NVARCHAR(1000) NOT NULL,
    Fecha DATETIME2 DEFAULT GETDATE(),
    EstadoFeedbackId INT NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (TipoFeedbackId) REFERENCES TiposFeedback(TipoFeedbackId),
    FOREIGN KEY (EstadoFeedbackId) REFERENCES EstadosFeedback(EstadoFeedbackId)
);
GO

-- 3. TABLAS DE ROLES Y PERMISOS
CREATE TABLE Roles (
    RolId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    FechaCreacion DATETIME2 DEFAULT GETDATE()
);
GO

CREATE TABLE Permisos (
    PermisoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NULL
);
GO

CREATE TABLE RolesPermisos (
    RolPermisoId INT IDENTITY(1,1) PRIMARY KEY,
    RolId INT NOT NULL,
    PermisoId INT NOT NULL,
    FOREIGN KEY (RolId) REFERENCES Roles(RolId) ON DELETE CASCADE,
    FOREIGN KEY (PermisoId) REFERENCES Permisos(PermisoId) ON DELETE CASCADE,
    CONSTRAINT UQ_RolesPermisos_Rol_Permiso UNIQUE (RolId, PermisoId)
);
GO

CREATE TABLE UsuariosRoles (
    UsuarioRolId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    RolId INT NOT NULL,
    FechaAsignacion DATETIME2 DEFAULT GETDATE(),
    AsignadoPor INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    FOREIGN KEY (RolId) REFERENCES Roles(RolId) ON DELETE CASCADE,
    CONSTRAINT UQ_UsuariosRoles_Usuario_Rol UNIQUE (UsuarioId, RolId)
);
GO

-- 4. VISTAS
CREATE VIEW MetricasImpacto AS
SELECT 
    u.UsuarioId,
    CAST(rr.FechaRegistro AS DATE) AS FechaCalculo,
    SUM(rr.PesoKg) AS ResiduosRegistradosTotalKg,
    SUM(rr.PesoKg * 0.8) AS KgCompostadosAprovechados,
    SUM(rr.PesoKg * 0.5) AS CO2EstimadoAhorradoKg
FROM Usuarios u
INNER JOIN RegistrosResiduos rr ON u.UsuarioId = rr.UsuarioId
GROUP BY u.UsuarioId, CAST(rr.FechaRegistro AS DATE);
GO

-- 5. ÍNDICES
CREATE INDEX IX_Usuarios_NivelIdActual ON Usuarios(NivelIdActual);
CREATE INDEX IX_RegistrosResiduos_UsuarioId ON RegistrosResiduos(UsuarioId);
CREATE INDEX IX_RegistrosResiduos_FechaRegistro ON RegistrosResiduos(FechaRegistro);
CREATE INDEX IX_CanjesRecompensas_UsuarioId ON CanjesRecompensas(UsuarioId);
CREATE INDEX IX_Notificaciones_UsuarioId_Leida ON Notificaciones(UsuarioId, Leida);
CREATE INDEX IX_PuntosHistoricos_UsuarioId_FechaCambio ON PuntosHistoricos(UsuarioId, FechaCambio);
CREATE INDEX IX_UsuariosContenidoVisto_UsuarioId ON UsuariosContenidoVisto(UsuarioId);
CREATE INDEX IX_PreferenciasUsuarios_UsuarioId ON PreferenciasUsuarios(UsuarioId);
GO

-- 6. DATOS INICIALES (CATÁLOGOS)
INSERT INTO TiposRecompensa (Nombre) VALUES ('Digital'), ('Tangible');
GO

INSERT INTO TiposNotificacion (Nombre) VALUES ('Recordatorio'), ('Logro'), ('RecompensaDisponible');
GO

INSERT INTO EstadosFeedback (Nombre) VALUES ('Pendiente'), ('Resuelto');
GO

INSERT INTO TiposFeedback (Nombre) VALUES ('Sugerencia'), ('Problema'), ('Idea');
GO

INSERT INTO CalidadesResiduo (Nombre, FactorBase) VALUES ('Alta', 15), ('Media', 12);
GO

INSERT INTO TiposContenido (Nombre) VALUES ('Guia practica'), ('Video'), ('Infografia'), ('Articulo');
GO

INSERT INTO CategoriasContenido (Nombre) VALUES ('Compostaje domestico'), ('Separacion de residuos'), ('Impacto ambiental');
GO

INSERT INTO TiposArchivo (Nombre) VALUES ('Imagen'), ('Video'), ('PDF'), ('EnlaceExterno');
GO

-- 7. INSERTAR NIVELES (ANTES DE CREAR USUARIOS)
INSERT INTO Niveles (NombreNivel, PuntosMinimoNecesario, PuntosMaximo, InsigniaArchivoId) VALUES
('Principiante', 0, 999, NULL),
('Aprendiz', 1000, 4999, NULL),
('Experto', 5000, 14999, NULL),
('Maestro Compostero', 15000, 999999, NULL);
GO

-- 8. INSERTAR TIPOS DE RESIDUO
INSERT INTO TiposResiduos (Nombre, CalidadResiduoId, AporteNutricional, RelacionCarbono, RelacionNitrogeno) VALUES
('Cascaras de fruta', 1, 'Alto en potasio y fosforo', 20, 1),
('Restos de verduras', 1, 'Alto en nitrogeno', 15, 1),
('Cascaras de huevo', 2, 'Aporte de calcio', NULL, NULL),
('Borra de cafe', 1, 'Alto en nitrogeno', 20, 1),
('Restos de pan', 2, 'Carbono estructural', 50, 1),
('Hojas secas', 2, 'Carbono estructural', 60, 1),
('Cascaras de citricos', 2, 'Moderado, puede acidificar', 25, 1),
('Restos de te', 1, 'Alto en nitrogeno', 20, 1);
GO

-- 9. INSERTAR RECOMPENSAS
INSERT INTO Recompensas (Nombre, Descripcion, TipoRecompensaId, CostoPuntos, StockDisponible, EsIlimitado, FechaVigenciaDesde, FechaVigenciaHasta, ImagenArchivoId) VALUES
('Insignia Verde', 'Insignia digital por compromiso ambiental', 1, 500, NULL, 1, '2025-01-01', NULL, NULL),
('Nivel Avanzado', 'Acceso a contenido exclusivo', 1, 2000, NULL, 1, '2025-01-01', NULL, NULL),
('Kit de Compostaje', 'Kit basico para compostaje domestico', 2, 5000, 50, 0, '2025-01-01', '2025-12-31', NULL),
('Semillas Organicas', 'Paquete de semillas de hortalizas', 2, 1500, 100, 0, '2025-01-01', '2025-12-31', NULL);
GO

-- 10. ROLES Y PERMISOS (TODO EN UN SOLO BLOQUE)
-- Insertar Roles
INSERT INTO Roles (Nombre, Descripcion) VALUES 
('Administrador', 'Rol con capacidad para crear, editar y eliminar contenido educativo'),
('Usuario', 'Rol básico para usuarios regulares del sistema');

-- Insertar Permisos
INSERT INTO Permisos (Nombre, Descripcion) VALUES 
('contenido.crear', 'Permite crear nuevo contenido educativo'),
('contenido.editar', 'Permite editar contenido educativo existente'),
('contenido.eliminar', 'Permite eliminar contenido educativo'),
('contenido.ver_todos', 'Permite ver todo el contenido educativo'),
('usuarios.ver_todos', 'Permite ver todos los usuarios'),
('usuarios.asignar_roles', 'Permite asignar roles a usuarios'),
('reportes.ver', 'Permite ver reportes y métricas');

-- Declarar variables
DECLARE @RolAdminId INT;
DECLARE @RolUsuarioId INT;

-- Obtener IDs de roles
SELECT @RolAdminId = RolId FROM Roles WHERE Nombre = 'Administrador';
SELECT @RolUsuarioId = RolId FROM Roles WHERE Nombre = 'Usuario';

-- Asignar todos los permisos al Administrador
INSERT INTO RolesPermisos (RolId, PermisoId)
SELECT @RolAdminId, PermisoId FROM Permisos;

-- Asignar permisos básicos al Usuario
INSERT INTO RolesPermisos (RolId, PermisoId)
SELECT @RolUsuarioId, PermisoId 
FROM Permisos 
WHERE Nombre IN ('contenido.ver_todos');

PRINT 'Roles y permisos creados exitosamente.';
GO

-- 11. CREAR USUARIO ADMINISTRADOR
DECLARE @RolAdminId INT;
DECLARE @AdminUserId INT;

SELECT @RolAdminId = RolId FROM Roles WHERE Nombre = 'Administrador';

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE CorreoElectronico = 'admin@ecocycle.com')
BEGIN
    INSERT INTO Usuarios (
        CorreoElectronico, 
        ContrasenaHash, 
        FechaRegistro, 
        NivelIdActual
    ) VALUES (
        'admin@ecocycle.com',
        CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'Admin123!'), 2),
        GETDATE(),
        (SELECT NivelId FROM Niveles WHERE NombreNivel = 'Maestro Compostero')
    );
    
    SET @AdminUserId = SCOPE_IDENTITY();
    
    INSERT INTO UsuariosRoles (UsuarioId, RolId, AsignadoPor)
    VALUES (@AdminUserId, @RolAdminId, NULL);
    
    PRINT 'Usuario administrador creado exitosamente.';
    PRINT 'Email: admin@ecocycle.com';
    PRINT 'Contraseña temporal: Admin123! (CAMBIAR EN PRODUCCIÓN)';
END
ELSE
BEGIN
    PRINT 'El usuario administrador ya existe.';
END
GO

-- 12. VISTA ADICIONAL Y PROCEDIMIENTOS
CREATE VIEW vw_ContenidoEducativoConPermisos AS
SELECT 
    c.*
FROM ContenidoEducativo c;
GO

CREATE FUNCTION fn_EsAdministrador (@UsuarioId INT)
RETURNS BIT
AS
BEGIN
    DECLARE @EsAdmin BIT = 0;
    
    IF EXISTS (
        SELECT 1 
        FROM UsuariosRoles ur
        INNER JOIN Roles r ON ur.RolId = r.RolId
        WHERE ur.UsuarioId = @UsuarioId AND r.Nombre = 'Administrador'
    )
    BEGIN
        SET @EsAdmin = 1;
    END
    
    RETURN @EsAdmin;
END
GO

CREATE PROCEDURE sp_AsignarRol
    @UsuarioId INT,
    @RolNombre NVARCHAR(50),
    @AsignadoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF dbo.fn_EsAdministrador(@AsignadoPor) = 0
    BEGIN
        RAISERROR('Solo los administradores pueden asignar roles.', 16, 1);
        RETURN;
    END
    
    DECLARE @RolId INT = (SELECT RolId FROM Roles WHERE Nombre = @RolNombre);
    
    IF @RolId IS NULL
    BEGIN
        RAISERROR('El rol especificado no existe.', 16, 1);
        RETURN;
    END
    
    INSERT INTO UsuariosRoles (UsuarioId, RolId, AsignadoPor)
    VALUES (@UsuarioId, @RolId, @AsignadoPor);
    
    PRINT 'Rol asignado exitosamente.';
END
GO

-- 13. PROCEDIMIENTOS PARA CONTENIDO EDUCATIVO
CREATE PROCEDURE sp_CrearContenidoEducativo
    @Titulo NVARCHAR(200),
    @TipoContenidoId INT,
    @CategoriaContenidoId INT,
    @RecursoArchivoId INT = NULL,
    @EsExterno BIT = 0,
    @FuenteExterna NVARCHAR(500) = NULL,
    @UsuarioId INT,
    @NuevoContenidoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF dbo.fn_EsAdministrador(@UsuarioId) = 0
    BEGIN
        RAISERROR('El usuario no tiene permisos para crear contenido educativo.', 16, 1);
        RETURN;
    END
    
    IF NOT EXISTS (SELECT 1 FROM TiposContenido WHERE TipoContenidoId = @TipoContenidoId)
    BEGIN
        RAISERROR('El tipo de contenido no existe.', 16, 1);
        RETURN;
    END
    
    IF NOT EXISTS (SELECT 1 FROM CategoriasContenido WHERE CategoriaContenidoId = @CategoriaContenidoId)
    BEGIN
        RAISERROR('La categoría de contenido no existe.', 16, 1);
        RETURN;
    END
    
    INSERT INTO ContenidoEducativo (
        Titulo, TipoContenidoId, CategoriaContenidoId, 
        RecursoArchivoId, EsExterno, FuenteExterna
    ) VALUES (
        @Titulo, @TipoContenidoId, @CategoriaContenidoId,
        @RecursoArchivoId, @EsExterno, @FuenteExterna
    );
    
    SET @NuevoContenidoId = SCOPE_IDENTITY();
    
    PRINT 'Contenido educativo creado exitosamente.';
END
GO

CREATE PROCEDURE sp_EditarContenidoEducativo
    @ContenidoId INT,
    @Titulo NVARCHAR(200),
    @TipoContenidoId INT,
    @CategoriaContenidoId INT,
    @RecursoArchivoId INT = NULL,
    @EsExterno BIT = 0,
    @FuenteExterna NVARCHAR(500) = NULL,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF dbo.fn_EsAdministrador(@UsuarioId) = 0
    BEGIN
        RAISERROR('El usuario no tiene permisos para editar contenido educativo.', 16, 1);
        RETURN;
    END
    
    IF NOT EXISTS (SELECT 1 FROM ContenidoEducativo WHERE ContenidoId = @ContenidoId)
    BEGIN
        RAISERROR('El contenido no existe.', 16, 1);
        RETURN;
    END
    
    UPDATE ContenidoEducativo
    SET 
        Titulo = @Titulo,
        TipoContenidoId = @TipoContenidoId,
        CategoriaContenidoId = @CategoriaContenidoId,
        RecursoArchivoId = @RecursoArchivoId,
        EsExterno = @EsExterno,
        FuenteExterna = @FuenteExterna
    WHERE ContenidoId = @ContenidoId;
    
    PRINT 'Contenido educativo actualizado exitosamente.';
END
GO

CREATE PROCEDURE sp_EliminarContenidoEducativo
    @ContenidoId INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF dbo.fn_EsAdministrador(@UsuarioId) = 0
    BEGIN
        RAISERROR('El usuario no tiene permisos para eliminar contenido educativo.', 16, 1);
        RETURN;
    END
    
    IF NOT EXISTS (SELECT 1 FROM ContenidoEducativo WHERE ContenidoId = @ContenidoId)
    BEGIN
        RAISERROR('El contenido no existe.', 16, 1);
        RETURN;
    END
    
    DELETE FROM ContenidoEducativo WHERE ContenidoId = @ContenidoId;
    
    PRINT 'Contenido educativo eliminado exitosamente.';
END
GO

-- FINAL

PRINT 'Base de datos Ecocycle creada exitosamente.';
PRINT '';
PRINT 'Credenciales de administrador:';
PRINT '  Email: admin@ecocycle.com';
PRINT '  Contraseña: Admin123!';
PRINT '==========================================';
GO

-- Mostrar resumen
SELECT 'RESUMEN DE TABLAS CREADAS' as Mensaje;
SELECT COUNT(*) as TotalTablas FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
GO

SELECT 'ROLES CREADOS' as Mensaje;
SELECT RolId, Nombre, Descripcion FROM Roles;
GO

SELECT 'USUARIO ADMINISTRADOR' as Mensaje;
SELECT UsuarioId, CorreoElectronico, NivelIdActual FROM Usuarios WHERE CorreoElectronico = 'admin@ecocycle.com';
GO