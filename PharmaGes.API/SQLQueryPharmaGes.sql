-- ============================================================
-- PharmaGes - Script de creación de base de datos
-- SQL Server
-- ============================================================

CREATE DATABASE PharmaGesDB;
GO

USE PharmaGesDB;
GO


-- ============================================================
-- ROLES
-- ============================================================
CREATE TABLE roles (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    nombre      NVARCHAR(50)  NOT NULL UNIQUE,
    es_activo   BIT           NOT NULL DEFAULT 1,
    creado_en   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Datos iniciales
INSERT INTO roles (nombre) VALUES ('Administrador'), ('Gerente'), ('Empleado');
GO

-- ============================================================
-- USUARIOS
-- ============================================================
CREATE TABLE usuarios (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    rol_id              INT           NOT NULL,
    nombre              NVARCHAR(100) NOT NULL,
    email               NVARCHAR(150) NOT NULL UNIQUE,
    contrasena_hash     NVARCHAR(255) NOT NULL,
    foto_url            NVARCHAR(500) NULL,
    es_activo           BIT           NOT NULL DEFAULT 1,
    intentos_fallidos   INT           NOT NULL DEFAULT 0,
    bloqueado_hasta     DATETIME2     NULL,
    creado_en           DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    actualizado_en      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_usuarios_roles FOREIGN KEY (rol_id) REFERENCES roles(id)
);

-- Usuario administrador inicial (contraseña: Admin123! — cambiar en producción)
-- Hash SHA256 de "Admin123!" en mayúsculas
INSERT INTO usuarios (rol_id, nombre, email, contrasena_hash)
VALUES (1, 'Administrador', 'admin@pharma.com',
        'hash_cambiar_en_produccion');
GO

-- ============================================================
-- SESIONES
-- ============================================================
CREATE TABLE sesiones (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    usuario_id  INT           NOT NULL,
    token       NVARCHAR(500) NOT NULL UNIQUE,
    expira_en   DATETIME2     NOT NULL,
    creado_en   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_sesiones_usuarios FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);
GO

-- ============================================================
-- MEDICAMENTOS
-- ============================================================
CREATE TABLE medicamentos (
    id                      INT IDENTITY(1,1) PRIMARY KEY,
    creado_por              INT             NOT NULL,
    codigo                  NVARCHAR(20)    NOT NULL UNIQUE,
    nombre                  NVARCHAR(150)   NOT NULL,
    descripcion             NVARCHAR(500)   NULL,
    stock                   INT             NOT NULL DEFAULT 0,
    stock_minimo            INT             NOT NULL DEFAULT 10,
    stock_maximo            INT             NOT NULL DEFAULT 100,
    precio_compra           DECIMAL(10,2)   NOT NULL DEFAULT 0,
    precio_venta            DECIMAL(10,2)   NOT NULL DEFAULT 0,
    fecha_caducidad         DATE            NULL,
    alerta_vencimiento_dias INT             NOT NULL DEFAULT 30,
    es_activo               BIT             NOT NULL DEFAULT 1,
    creado_en               DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    actualizado_en          DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_medicamentos_usuarios FOREIGN KEY (creado_por) REFERENCES usuarios(id),
    CONSTRAINT CHK_stock_minimo CHECK (stock_minimo >= 0),
    CONSTRAINT CHK_stock_maximo CHECK (stock_maximo >= stock_minimo),
    CONSTRAINT CHK_precio_compra CHECK (precio_compra >= 0),
    CONSTRAINT CHK_precio_venta CHECK (precio_venta >= 0)
);
GO

-- ============================================================
-- MOVIMIENTOS DE INVENTARIO
-- ============================================================
CREATE TABLE movimientos_inventario (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    medicamento_id  INT             NOT NULL,
    usuario_id      INT             NOT NULL,
    tipo            NVARCHAR(20)    NOT NULL, -- 'entrada', 'venta', 'ajuste', 'baja'
    cantidad        INT             NOT NULL,
    stock_anterior  INT             NOT NULL,
    stock_nuevo     INT             NOT NULL,
    motivo          NVARCHAR(300)   NULL,
    creado_en       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_movimientos_medicamentos FOREIGN KEY (medicamento_id) REFERENCES medicamentos(id),
    CONSTRAINT FK_movimientos_usuarios     FOREIGN KEY (usuario_id)     REFERENCES usuarios(id),
    CONSTRAINT CHK_tipo_movimiento CHECK (tipo IN ('entrada', 'venta', 'ajuste', 'baja'))
);
GO

-- ============================================================
-- FACTURAS
-- Nunca se eliminan — se anulan cambiando estado
-- ============================================================
CREATE TABLE facturas (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    usuario_id          INT             NOT NULL,
    numero_correlativo  NVARCHAR(20)    NOT NULL UNIQUE, -- Ej: F-00001
    estado              NVARCHAR(20)    NOT NULL DEFAULT 'activa', -- 'activa', 'anulada'
    subtotal            DECIMAL(10,2)   NOT NULL DEFAULT 0,
    total               DECIMAL(10,2)   NOT NULL DEFAULT 0,
    efectivo_recibido   DECIMAL(10,2)   NOT NULL DEFAULT 0,
    cambio              DECIMAL(10,2)   NOT NULL DEFAULT 0,
    notas               NVARCHAR(500)   NULL,
    creado_en           DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_facturas_usuarios FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    CONSTRAINT CHK_estado_factura CHECK (estado IN ('activa', 'anulada'))
);
GO

-- ============================================================
-- DETALLES DE FACTURA
-- Guarda nombre y código del medicamento al momento de la venta
-- para preservar el historial aunque el medicamento cambie
-- ============================================================
CREATE TABLE detalles_factura (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    factura_id          INT             NOT NULL,
    medicamento_id      INT             NOT NULL,
    medicamento_nombre  NVARCHAR(150)   NOT NULL,
    medicamento_codigo  NVARCHAR(20)    NOT NULL,
    cantidad            INT             NOT NULL,
    precio_unitario     DECIMAL(10,2)   NOT NULL,
    subtotal            AS (cantidad * precio_unitario) PERSISTED, -- columna calculada

    CONSTRAINT FK_detalles_facturas     FOREIGN KEY (factura_id)     REFERENCES facturas(id),
    CONSTRAINT FK_detalles_medicamentos FOREIGN KEY (medicamento_id) REFERENCES medicamentos(id),
    CONSTRAINT CHK_cantidad_detalle     CHECK (cantidad > 0),
    CONSTRAINT CHK_precio_detalle       CHECK (precio_unitario >= 0)
);
GO

-- ============================================================
-- ÍNDICES para mejorar consultas frecuentes
-- ============================================================
CREATE INDEX IX_medicamentos_codigo        ON medicamentos(codigo);
CREATE INDEX IX_medicamentos_nombre        ON medicamentos(nombre);
CREATE INDEX IX_medicamentos_caducidad     ON medicamentos(fecha_caducidad) WHERE es_activo = 1;
CREATE INDEX IX_medicamentos_stock         ON medicamentos(stock) WHERE es_activo = 1;
CREATE INDEX IX_facturas_correlativo       ON facturas(numero_correlativo);
CREATE INDEX IX_facturas_estado            ON facturas(estado);
CREATE INDEX IX_facturas_fecha            ON facturas(creado_en);
CREATE INDEX IX_movimientos_medicamento    ON movimientos_inventario(medicamento_id);
CREATE INDEX IX_sesiones_token             ON sesiones(token);
CREATE INDEX IX_sesiones_expira            ON sesiones(expira_en);
GO

-- ============================================================
-- FUNCIÓN: Generar número correlativo de factura
-- ============================================================
CREATE FUNCTION fn_siguiente_correlativo()
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @ultimo INT;
    SELECT @ultimo = ISNULL(MAX(CAST(SUBSTRING(numero_correlativo, 3, LEN(numero_correlativo)) AS INT)), 0)
    FROM facturas;
    RETURN 'F-' + RIGHT('00000' + CAST(@ultimo + 1 AS NVARCHAR), 5);
END;
GO

-- ============================================================
-- VISTA: Medicamentos con alerta de stock bajo
-- ============================================================
CREATE VIEW v_alertas_stock AS
SELECT
    id,
    codigo,
    nombre,
    stock,
    stock_minimo,
    stock_maximo,
    CASE
        WHEN stock = 0        THEN 'agotado'
        WHEN stock <= stock_minimo THEN 'bajo'
        ELSE 'ok'
    END AS estado_stock
FROM medicamentos
WHERE es_activo = 1 AND stock <= stock_minimo;
GO

-- ============================================================
-- VISTA: Medicamentos próximos a vencer
-- ============================================================
CREATE VIEW v_alertas_vencimiento AS
SELECT
    id,
    codigo,
    nombre,
    stock,
    fecha_caducidad,
    alerta_vencimiento_dias,
    DATEDIFF(day, SYSUTCDATETIME(), fecha_caducidad) AS dias_para_vencer
FROM medicamentos
WHERE
    es_activo = 1
    AND fecha_caducidad IS NOT NULL
    AND fecha_caducidad <= DATEADD(day, alerta_vencimiento_dias, SYSUTCDATETIME());
GO

-- ============================================================
-- VISTA: Dashboard - ventas del día
-- ============================================================
CREATE VIEW v_ventas_hoy AS
SELECT
    COUNT(*)        AS total_transacciones,
    SUM(total)      AS ventas_total,
    AVG(total)      AS promedio_venta
FROM facturas
WHERE
    estado = 'activa'
    AND CAST(creado_en AS DATE) = CAST(SYSUTCDATETIME() AS DATE);
GO

-- ============================================================
-- Actualizar Admin
-- ============================================================

USE PharmaGesDB;
GO
UPDATE usuarios 
SET contrasena_hash = '$2a$12$sOa2RnsHZOzpJd6KtxWxR.59QZtSWwlnKhWP/ngiN/Q4JGhB/WFqe'
WHERE email = 'admin@pharma.com';
-- La contraseña es: Admin123!

