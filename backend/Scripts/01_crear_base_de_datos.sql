/*
  Script de creación de la base de datos GestionTareasDB.
  Es el mismo esquema que generan las migraciones de EF Core (idempotente: se puede correr varias veces).
  Si prefieres no usar este script, basta con levantar la API: aplica las migraciones sola.
*/
IF DB_ID(N'GestionTareasDB') IS NULL
    CREATE DATABASE GestionTareasDB;
GO

USE GestionTareasDB;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104140418_CrearBaseDeDatos'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [ContrasenaHash] nvarchar(200) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104140418_CrearBaseDeDatos'
)
BEGIN
    CREATE TABLE [Tasks] (
        [Id] int NOT NULL IDENTITY,
        [Titulo] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [UsuarioId] int NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Prioridad] nvarchar(20) NOT NULL,
        [FechaEstimada] datetime2 NULL,
        [Etiquetas] nvarchar(500) NULL,
        [Metadatos] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Tasks] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Tasks_Metadatos_EsJson] CHECK ([Metadatos] IS NULL OR ISJSON([Metadatos]) = 1),
        CONSTRAINT [FK_Tasks_Users_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104140418_CrearBaseDeDatos'
)
BEGIN
    CREATE INDEX [IX_Tasks_Usuario_Estado_Fecha] ON [Tasks] ([UsuarioId], [Estado], [FechaCreacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104140418_CrearBaseDeDatos'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Users_Correo] ON [Users] ([Correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104140418_CrearBaseDeDatos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104140418_CrearBaseDeDatos', N'8.0.11');
END;
GO

COMMIT;
GO

