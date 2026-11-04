/*
  Datos de prueba: 3 usuarios y 2 tareas por cada uno.
  Se ejecuta DESPUÉS de crear la base (con la API o con 01_crear_base_de_datos.sql).

  Todos los usuarios tienen la contraseña:  123456
  Correos: ana@correo.com, beto@correo.com, carla@correo.com

  Se puede correr varias veces: si el correo ya existe, no se vuelve a insertar.
*/
USE GestionTareasDB;
GO

-- Hash de la contraseña "123456" generado con el PasswordHasher de .NET.
DECLARE @Hash NVARCHAR(200) = N'AQAAAAIAAYagAAAAEOURELQCns3BT/ZP9YqnPJh+25wIgzgQTxxTJEF2jcdL8DvN3H7tolexluwu6BAC5g==';

-- 1) Usuarios
INSERT INTO Users (Nombre, Correo, ContrasenaHash, FechaCreacion)
SELECT v.Nombre, v.Correo, @Hash, SYSUTCDATETIME()
FROM (VALUES
    (N'Ana Pérez',   N'ana@correo.com'),
    (N'Beto Gómez',  N'beto@correo.com'),
    (N'Carla Ruiz',  N'carla@correo.com')
) AS v (Nombre, Correo)
WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.Correo = v.Correo);

-- 2) Tareas (dos por usuario). Solo se insertan si el usuario todavía no tiene tareas.
DECLARE @Ana   INT = (SELECT Id FROM Users WHERE Correo = N'ana@correo.com');
DECLARE @Beto  INT = (SELECT Id FROM Users WHERE Correo = N'beto@correo.com');
DECLARE @Carla INT = (SELECT Id FROM Users WHERE Correo = N'carla@correo.com');

IF NOT EXISTS (SELECT 1 FROM Tasks WHERE UsuarioId = @Ana)
    INSERT INTO Tasks (Titulo, Descripcion, UsuarioId, Estado, Prioridad, FechaEstimada, Etiquetas, Metadatos, FechaCreacion)
    VALUES
    (N'Diseñar el modelo de datos', N'Tablas de usuarios y tareas', @Ana, N'Done', N'Alta',
        DATEADD(DAY, 5, SYSUTCDATETIME()), N'base-de-datos,diseño',
        N'{"origen":"reunion","cliente":"ACME"}', DATEADD(DAY, -3, SYSUTCDATETIME())),
    (N'Crear los endpoints de usuarios', N'POST y GET de /api/users', @Ana, N'InProgress', N'Media',
        DATEADD(DAY, 7, SYSUTCDATETIME()), N'api,backend',
        N'{"origen":"correo","cliente":"ACME"}', DATEADD(DAY, -2, SYSUTCDATETIME()));

IF NOT EXISTS (SELECT 1 FROM Tasks WHERE UsuarioId = @Beto)
    INSERT INTO Tasks (Titulo, Descripcion, UsuarioId, Estado, Prioridad, FechaEstimada, Etiquetas, Metadatos, FechaCreacion)
    VALUES
    (N'Armar el formulario de tareas', N'Formulario reactivo en Angular', @Beto, N'Pending', N'Alta',
        DATEADD(DAY, 10, SYSUTCDATETIME()), N'frontend,angular',
        N'{"origen":"correo","cliente":"Globex"}', DATEADD(DAY, -2, SYSUTCDATETIME())),
    (N'Probar el cambio de estado', N'Validar que Pending no pase a Done', @Beto, N'Pending', N'Baja',
        NULL, N'pruebas',
        NULL, DATEADD(DAY, -1, SYSUTCDATETIME()));

IF NOT EXISTS (SELECT 1 FROM Tasks WHERE UsuarioId = @Carla)
    INSERT INTO Tasks (Titulo, Descripcion, UsuarioId, Estado, Prioridad, FechaEstimada, Etiquetas, Metadatos, FechaCreacion)
    VALUES
    (N'Escribir el README', N'Pasos para ejecutar el proyecto', @Carla, N'InProgress', N'Media',
        DATEADD(DAY, 3, SYSUTCDATETIME()), N'documentacion',
        N'{"origen":"reunion","cliente":"Globex"}', DATEADD(DAY, -1, SYSUTCDATETIME())),
    (N'Revisar los scripts SQL', N'Probar el script de creación y las consultas', @Carla, N'Pending', N'Media',
        DATEADD(DAY, 6, SYSUTCDATETIME()), N'base-de-datos,pruebas',
        N'{"origen":"web","cliente":"ACME"}', SYSUTCDATETIME());
GO

-- Verificación: debe mostrar 3 usuarios con 2 tareas cada uno.
SELECT u.Nombre, COUNT(t.Id) AS Tareas
FROM Users u
LEFT JOIN Tasks t ON t.UsuarioId = u.Id
GROUP BY u.Nombre
ORDER BY u.Nombre;
GO
