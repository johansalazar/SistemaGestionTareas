USE GestionTareasDB;
GO

/* ------------------------------------------------------------------
   1) Tareas por usuario, filtradas por estado y ordenadas por fecha de creación.
      Usa el índice IX_Tasks_Usuario_Estado_Fecha.
   ------------------------------------------------------------------ */
DECLARE @UsuarioId INT = 1;
DECLARE @Estado NVARCHAR(20) = N'Pending';

SELECT t.Id, t.Titulo, t.Estado, t.Prioridad, t.FechaEstimada, t.FechaCreacion, u.Nombre AS Usuario
FROM Tasks t
INNER JOIN Users u ON u.Id = t.UsuarioId
WHERE t.UsuarioId = @UsuarioId
  AND t.Estado = @Estado
ORDER BY t.FechaCreacion DESC;
GO

/* ------------------------------------------------------------------
   2) JSON en SQL Server (columna Metadatos)
   ------------------------------------------------------------------ */

-- 2.1 ISJSON: revisar que lo guardado sea JSON válido (debe devolver 0 filas inválidas).
SELECT Id, Titulo FROM Tasks WHERE Metadatos IS NOT NULL AND ISJSON(Metadatos) = 0;

-- 2.2 JSON_VALUE: leer un campo escalar de los metadatos.
SELECT Id, Titulo,
       JSON_VALUE(Metadatos, '$.origen')  AS Origen,
       JSON_VALUE(Metadatos, '$.cliente') AS Cliente
FROM Tasks;

-- 2.3 Filtrar tareas por un valor dentro del JSON (las que vienen del origen "correo").
SELECT Id, Titulo, Estado
FROM Tasks
WHERE JSON_VALUE(Metadatos, '$.origen') = N'correo'
ORDER BY FechaCreacion DESC;

-- 2.4 JSON_QUERY: devolver el JSON completo de los metadatos (objeto) tal cual.
SELECT Id, Titulo, JSON_QUERY(Metadatos, '$') AS MetadatosJson
FROM Tasks
WHERE Metadatos IS NOT NULL;

-- 2.5 OPENJSON: abrir los metadatos como filas (clave / valor) para ver qué trae cada tarea.
SELECT t.Id, m.[key] AS Clave, m.[value] AS Valor
FROM Tasks t
CROSS APPLY OPENJSON(t.Metadatos) m
WHERE t.Metadatos IS NOT NULL;

-- 2.6 JSON_MODIFY: actualizar un solo campo dentro del JSON sin reescribir todo el texto.
UPDATE Tasks
SET Metadatos = JSON_MODIFY(Metadatos, '$.cliente', N'Cliente actualizado')
WHERE Id = 1 AND Metadatos IS NOT NULL;
GO
