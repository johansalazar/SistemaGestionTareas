# Sistema de Gestión de Tareas

Aplicación fullstack para registrar usuarios y tareas, asignarlas y seguir su estado.

- **Backend:** .NET 8 Web API + Entity Framework Core 8 (arquitectura hexagonal)
- **Base de datos:** SQL Server local, con migraciones y script SQL
- **Frontend:** Angular 21

```
backend/    Solución .NET (GestionTareas.sln)
frontend/   Proyecto Angular
Scripts/    Scripts SQL: crear la base, consultas JSON y datos de prueba
```

## Requisitos

- Visual Studio 2022 Community con .NET 8 SDK
- SQL Server local (instancia por defecto `localhost`, autenticación de Windows)
- Node.js 20.19+ o 22+
- Visual Studio Code

## Pasos para ejecutar el proyecto

### 1. Base de datos

La cadena de conexión está en `backend/src/GestionTareas.Api/appsettings.json`:

```
Server=localhost;Database=GestionTareasDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Si se usa otra instancia, cambiar `localhost` por `localhost\NOMBRE_INSTANCIA`.

No hay que crear nada a mano: al arrancar, la API aplica las migraciones y crea la base si no existe.
Si se prefiere hacerlo con script, ejecutar `Scripts/01_crear_base_de_datos.sql`.

**Datos de prueba (opcional):** `Scripts/03_datos_de_prueba.sql` crea 3 usuarios con 2 tareas cada uno (se ejecuta después de crear la base):

```bash
sqlcmd -S localhost -E -C -i Scripts/03_datos_de_prueba.sql
```

Usuarios: `ana@correo.com`, `beto@correo.com` y `carla@correo.com`, todos con contraseña `123456`.

### 2. Backend

1. Abrir `backend/GestionTareas.sln` en Visual Studio.
2. Dejar `GestionTareas.Api` como proyecto de inicio y ejecutar con el perfil `http` (F5).
3. Swagger queda en `http://localhost:5053/swagger`.

**Probar en Swagger:** ejecutar `POST /api/auth/login` (el ejemplo ya trae `ana@correo.com` / `123456`, usuario del script de datos de prueba), copiar el `token` de la respuesta y pegarlo en el botón **Authorize**. Después los demás endpoints ya se pueden ejecutar; cada uno trae un ejemplo de body listo para enviar.

Por consola: `dotnet run --project src/GestionTareas.Api --launch-profile http` desde `backend/`.

### 3. Frontend

```bash
cd frontend
npm install
npm start
```

Abrir `http://localhost:4200`. La primera vez, usar **Crear una cuenta** en la pantalla de acceso. La URL de la API está en `frontend/src/app/core/config.ts` (`http://localhost:5053/api`).
El backend debe estar encendido antes de usar la pantalla.

### Pantallas del frontend

Menú lateral con dos secciones. Cada CRUD tiene su pantalla de lista y su pantalla de formulario (el mismo formulario sirve para crear y editar):

| Ruta | Pantalla |
|---|---|
| `/login` | Iniciar sesión o crear cuenta |
| `/tareas` | Lista de tareas con filtros, paginación y cambio de estado |
| `/tareas/nueva` | Crear tarea |
| `/tareas/:id/editar` | Editar tarea y cambiar un campo de sus metadatos |
| `/usuarios` | Lista de usuarios |
| `/usuarios/nuevo` | Crear usuario |
| `/usuarios/:id/editar` | Editar usuario |

## Arquitectura

Hexagonal, en cuatro proyectos:

| Proyecto | Responsabilidad |
|---|---|
| `Dominio` | Entidades (`Usuario`, `Tarea`), enums y excepciones. No depende de nada. |
| `Aplicacion` | Casos de uso, DTOs y puertos (entrada: `ITareaServicio`, `IUsuarioServicio`; salida: repositorios). |
| `Infraestructura` | Adaptador de salida: EF Core, repositorios, migraciones, hash de contraseñas y emisión del JWT. |
| `Api` | Adaptador de entrada: controladores y middleware de errores. |

## Base de datos

- `Users`: `Id`, `Nombre`, `Correo` (único), `ContrasenaHash`, `FechaCreacion`.
- `Tasks`: `Id`, `Titulo`, `Descripcion`, `UsuarioId` (FK), `Estado`, `Prioridad`, `FechaEstimada`, `Etiquetas` (texto separado por comas), `Metadatos` (JSON), `FechaCreacion`.
- Índices: único por correo, e `IX_Tasks_Usuario_Estado_Fecha` para la consulta de tareas por usuario, estado y fecha.

### JSON en SQL Server

- `Metadatos` es `NVARCHAR(MAX)` con un `CHECK (ISJSON(Metadatos) = 1)`.
- La API recibe y devuelve los metadatos como objeto JSON.
- El filtro por metadato usa `JSON_VALUE`, y el cambio de un solo campo usa `JSON_MODIFY`. Ejemplos de `ISJSON`, `JSON_VALUE`, `JSON_QUERY`, `OPENJSON` y `JSON_MODIFY` en `Scripts/02_consultas.sql`.

```sql
SELECT Id, Titulo, JSON_VALUE(Metadatos, '$.origen') AS Origen
FROM Tasks
WHERE JSON_VALUE(Metadatos, '$.origen') = N'correo';
```

## Autenticación

JWT. Todos los endpoints exigen el header `Authorization: Bearer <token>`, excepto `POST /api/auth/login` y `POST /api/users` (registro, necesario para crear la primera cuenta).

```bash
# 1. registrar
curl -X POST http://localhost:5053/api/users -H "Content-Type: application/json"      -d '{"nombre":"Ana","correo":"ana@correo.com","contrasena":"secreta1"}'
# 2. login -> devuelve { token, expira, usuario }
curl -X POST http://localhost:5053/api/auth/login -H "Content-Type: application/json"      -d '{"correo":"ana@correo.com","contrasena":"secreta1"}'
```

En Swagger, el botón **Authorize** acepta el token. Las contraseñas se guardan con hash (`PasswordHasher` de .NET), nunca en claro. La clave de firma y la vigencia (`MinutosExpiracion`, 60 por defecto) están en la sección `Jwt` de `appsettings.json`; cuando el token vence, el frontend cierra la sesión y manda al login; la clave incluida es solo de desarrollo y debe cambiarse en cualquier otro ambiente.

## API

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/login` | Inicia sesión y devuelve el token (401 si las credenciales no coinciden) |
| POST | `/api/users` | Crea un usuario (`nombre`, `correo`, `contrasena`). 409 si el correo ya existe |
| GET | `/api/users` | Lista usuarios paginados |
| GET | `/api/users/{id}` | Devuelve un usuario |
| PUT | `/api/users/{id}` | Edita nombre y correo |
| DELETE | `/api/users/{id}` | Elimina el usuario (409 si tiene tareas asignadas) |
| POST | `/api/tasks` | Crea una tarea (inicia en `Pending`) |
| GET | `/api/tasks` | Lista tareas paginadas, más recientes primero |
| GET | `/api/tasks/{id}` | Devuelve una tarea |
| PUT | `/api/tasks/{id}` | Edita los datos de la tarea (no el estado) |
| PUT | `/api/tasks/{id}/status` | Cambia el estado |
| PATCH | `/api/tasks/{id}/metadatos` | Cambia un solo campo del JSON de metadatos |
| DELETE | `/api/tasks/{id}` | Elimina la tarea |

**Paginación:** los listados reciben `pagina` (desde 1) y `tamano` (1 a 100, por defecto 10) y responden:

```json
{ "items": [], "total": 25, "pagina": 1, "tamano": 10, "totalPaginas": 3 }
```

**Filtros de `GET /api/tasks`** (opcionales): `usuarioId`, `estado`, `prioridad`, `etiqueta`, `metadatoClave` + `metadatoValor`.

**Crear tarea:**

```json
{
  "titulo": "Revisar API",
  "usuarioId": 1,
  "prioridad": "Alta",
  "fechaEstimada": "2026-12-01",
  "etiquetas": ["api", "urgente"],
  "metadatos": { "origen": "correo", "cliente": "ACME" }
}
```

**Actualizar un campo del JSON** (`PATCH /api/tasks/{id}/metadatos`): se ejecuta en SQL Server con `JSON_MODIFY`, sin reescribir el resto del documento. Si no se envía `valor`, la clave se elimina:

```json
{ "clave": "cliente", "valor": "Nuevo cliente" }
```

Errores: 400 (validación o regla de negocio), 401 (sin token o credenciales inválidas), 404 (no existe), 409 (conflicto), 500 (interno). Responden JSON con `title` y `detail`.

## Reglas de negocio

- El título es obligatorio y la tarea debe tener un usuario existente.
- No se puede pasar de `Pending` a `Done` directamente.
- Transiciones permitidas: `Pending → InProgress`, `InProgress → Done | Pending`, `Done → InProgress`.

## Migraciones

Desde `backend/`:

```bash
dotnet ef migrations add NombreMigracion -p src/GestionTareas.Infraestructura -s src/GestionTareas.Api -o Persistencia/Migraciones
dotnet ef database update -p src/GestionTareas.Infraestructura -s src/GestionTareas.Api
```

## Problemas comunes

- **La API no conecta a SQL Server:** revisar que el servicio esté encendido y el `Server=` de la cadena de conexión.
- **El frontend dice que no se pudo conectar:** la API no está corriendo o el puerto no coincide con `core/config.ts`.
- **Error de CORS:** el frontend corre en un puerto distinto de 4200; agregarlo en `CorsOrigenes` de `appsettings.json`.

## Decisiones técnicas

- La regla de estados vive en la entidad (`Tarea.CambiarEstado`), no en el controlador.
- Un solo middleware traduce las excepciones del dominio a códigos HTTP.
- El JSON se valida en el dominio y también en la base (`ISJSON`).
- Los enums viajan como texto (`Pending`, `Alta`).
- La autenticación usa JWT; el hash de contraseñas y el token son puertos de salida (`IHasherContrasena`, `ITokenGenerador`), así que el dominio y la aplicación no dependen de ninguna librería de seguridad.
- La paginación se hace en SQL con `Skip/Take`, así no se traen todos los registros a memoria.
- Un usuario con tareas no se puede eliminar (FK con `Restrict`); la API responde 409 en vez de borrar en cascada.
- Las migraciones se aplican al arrancar para que la prueba levante sin pasos extra.
- Las etiquetas se guardan como texto separado por comas en la misma tabla (en vez de una tabla aparte) para mantener el modelo simple; el filtro por etiqueta busca dentro de ese texto.
- Los metadatos se guardan como JSON en `NVARCHAR(MAX)` porque su estructura es libre; los datos que se consultan siempre (estado, prioridad, fecha) son columnas normales con índice.

## Funcionalidades pendientes

- **Pruebas automatizadas:** no hay pruebas unitarias (reglas del dominio y servicios) ni de integración de la API.
- **Permisos por usuario:** cualquier usuario autenticado puede ver y editar todas las tareas y usuarios; no hay roles ni restricción por dueño.
- **Renovación de sesión:** el token dura 60 minutos y no hay refresh token; al vencer hay que iniciar sesión de nuevo.
- **Etiquetas normalizadas:** las etiquetas no son una tabla propia, así que no hay catálogo ni conteo por etiqueta.
- **Configuración por ambiente:** la clave del JWT y la cadena de conexión están en `appsettings.json` (solo para desarrollo); falta moverlas a variables de entorno o a un gestor de secretos.
- **Despliegue:** no hay Docker ni pipeline de integración continua.
- **Pruebas del frontend:** el frontend no tiene pruebas automatizadas (ni unitarias ni de extremo a extremo).
