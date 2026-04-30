# EcoCycle

-- BASE DE DATOS ECOCYCLE
CREATE DATABASE Ecocycle;
GO

USE Ecocycle;
GO

-- TABLAS CATALOGO (dominios)
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

-- TABLA: Archivos (debe ir antes que Niveles por la FK)
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

-- TABLA: Niveles
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

-- TABLA: Usuarios
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

-- TABLA: PreferenciasUsuarios
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

-- TABLA: TiposResiduos
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

-- TABLA: RegistrosResiduos
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

-- TABLA: CanjesRecompensas (debe ir antes que PuntosHistoricos por la FK)
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

-- TABLA: PuntosHistoricos
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

-- TABLA: Recompensas
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

-- Agregar FK de CanjesRecompensas a Recompensas
ALTER TABLE CanjesRecompensas ADD CONSTRAINT FK_CanjesRecompensas_Recompensas 
FOREIGN KEY (RecompensaId) REFERENCES Recompensas(RecompensaId);
GO

-- TABLA: ContenidoEducativo
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

-- TABLA: UsuariosContenidoVisto
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

-- TABLA: Notificaciones
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

-- TABLA: FeedbackUsuarios
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

-- VISTA PARA METRICAS DE IMPACTO
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

-- INDICES
CREATE INDEX IX_Usuarios_NivelIdActual ON Usuarios(NivelIdActual);
CREATE INDEX IX_RegistrosResiduos_UsuarioId ON RegistrosResiduos(UsuarioId);
CREATE INDEX IX_RegistrosResiduos_FechaRegistro ON RegistrosResiduos(FechaRegistro);
CREATE INDEX IX_CanjesRecompensas_UsuarioId ON CanjesRecompensas(UsuarioId);
CREATE INDEX IX_Notificaciones_UsuarioId_Leida ON Notificaciones(UsuarioId, Leida);
CREATE INDEX IX_PuntosHistoricos_UsuarioId_FechaCambio ON PuntosHistoricos(UsuarioId, FechaCambio);
CREATE INDEX IX_UsuariosContenidoVisto_UsuarioId ON UsuariosContenidoVisto(UsuarioId);
CREATE INDEX IX_PreferenciasUsuarios_UsuarioId ON PreferenciasUsuarios(UsuarioId);
GO

-- DATOS INICIALES (catalogos)
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

-- Insertar niveles (sin InsigniaArchivoId por ahora)
INSERT INTO Niveles (NombreNivel, PuntosMinimoNecesario, PuntosMaximo, InsigniaArchivoId) VALUES
('Principiante', 0, 999, NULL),
('Aprendiz', 1000, 4999, NULL),
('Experto', 5000, 14999, NULL),
('Maestro Compostero', 15000, 999999, NULL);
GO

-- Insertar tipos de residuo
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

-- Insertar recompensas
INSERT INTO Recompensas (Nombre, Descripcion, TipoRecompensaId, CostoPuntos, StockDisponible, EsIlimitado, FechaVigenciaDesde, FechaVigenciaHasta, ImagenArchivoId) VALUES
('Insignia Verde', 'Insignia digital por compromiso ambiental', 1, 500, NULL, 1, '2025-01-01', NULL, NULL),
('Nivel Avanzado', 'Acceso a contenido exclusivo', 1, 2000, NULL, 1, '2025-01-01', NULL, NULL),
('Kit de Compostaje', 'Kit basico para compostaje domestico', 2, 5000, 50, 0, '2025-01-01', '2025-12-31', NULL),
('Semillas Organicas', 'Paquete de semillas de hortalizas', 2, 1500, 100, 0, '2025-01-01', '2025-12-31', NULL);
GO

PRINT 'Base de datos Ecocycle creada exitosamente.';
GO
