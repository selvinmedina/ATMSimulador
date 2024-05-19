CREATE DATABASE ATMSimulador;
GO
USE ATMSimulador;

-- Creación de la tabla de Usuarios
CREATE TABLE Usuarios (
    UsuarioId INT IDENTITY(1,1),
    NombreUsuario NVARCHAR(255) NOT NULL,
    Pin VARBINARY(256) NOT NULL
    CONSTRAINT PK_Usuarios PRIMARY KEY (UsuarioId)
);

-- Creación de la tabla de Cuentas
CREATE TABLE Cuentas (
    CuentaId INT IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    NumeroCuenta NVARCHAR(20) UNIQUE NOT NULL,
    Saldo DECIMAL(18,2) NOT NULL,
    Activa BIT NOT NULL,
    CONSTRAINT PK_Cuentas PRIMARY KEY (CuentaId),
    CONSTRAINT FK_Cuentas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);

-- Creación de la tabla de Transacciones
CREATE TABLE Transacciones (
    TransaccionId INT IDENTITY(1,1),
    CuentaId INT NOT NULL,
    TipoTransaccion NVARCHAR(50) NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    FechaTransaccion DATETIME NOT NULL,
    Estado NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Transacciones PRIMARY KEY (TransaccionId),
    CONSTRAINT FK_Transacciones_Cuentas FOREIGN KEY (CuentaId) REFERENCES Cuentas(CuentaId)
);

-- Creación de la tabla de Servicios
CREATE TABLE Servicios (
    ServicioId INT IDENTITY(1,1),
    NombreServicio NVARCHAR(255) NOT NULL,
    Descripcion NVARCHAR(255),
    CONSTRAINT PK_Servicios PRIMARY KEY (ServicioId)
);

-- Creación de la tabla de Pagos
CREATE TABLE Pagos (
    PagoId INT IDENTITY(1,1),
    ServicioId INT NOT NULL,
    CuentaId INT NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    FechaPago DATETIME NOT NULL,
    CONSTRAINT PK_Pagos PRIMARY KEY (PagoId),
    CONSTRAINT FK_Pagos_Servicios FOREIGN KEY (ServicioId) REFERENCES Servicios(ServicioId),
    CONSTRAINT FK_Pagos_Cuentas FOREIGN KEY (CuentaId) REFERENCES Cuentas(CuentaId)
);

-- Creación de la tabla de Auditoría
CREATE TABLE Auditoria (
    AuditoriaId INT IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    TipoActividad NVARCHAR(255) NOT NULL,
    FechaActividad DATETIME NOT NULL,
    Descripcion NVARCHAR(255),
    CONSTRAINT PK_Auditoria PRIMARY KEY (AuditoriaId),
    CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);