# 💊 PharmaGesV2 - Sistema de Gestión Farmacéutica

Este es un sistema integral para la gestión de inventarios, ventas y administración de farmacias, desarrollado con **.NET (C#)** para el Backend y una interfaz moderna en **HTML/CSS/JS**.

## 🚀 Configuración del Proyecto

Para poner en marcha el sistema en tu entorno local, sigue estos pasos en orden:

### 1. Preparar la Base de Datos 🗄️
El proyecto incluye un script SQL que configura toda la estructura necesaria:
1. Abre **SQL Server Management Studio (SSMS)**.
2. Ejecuta el archivo `SQLQueryPharmaGes.sql` que se encuentra en la raíz del proyecto.
3. Esto creará la base de datos `PharmaGesDB`, las tablas, roles, vistas de alertas y un usuario administrador inicial.

### 2. Configuración del Backend (.NET Core) ⚙️
Por seguridad, el archivo de configuración real (`appsettings.json`) está excluido del repositorio. Debes crearlo manualmente siguiendo estos pasos:

1. Ve a la carpeta del proyecto Backend y localiza el archivo `appsettings.Example.json`.
2. Crea una copia de ese archivo y renómbrala a `appsettings.json`.
3. Edita el nuevo `appsettings.json` con tus credenciales:
   - **DefaultConnection**: Ajusta la cadena de conexión a tu instancia local de SQL Server.
   - **Jwt:Key**: Sustituye el valor por una cadena aleatoria segura de al menos 32 caracteres (256 bits).

### 3. Ejecución y Acceso 🏃‍♂️
Una vez configurada la base de datos y el archivo de configuración, puedes iniciar el sistema:

1. **Inicia la API:** Abre la solución en Visual Studio y dale a "Run" o usa la terminal en la carpeta del proyecto con el comando `dotnet run`.
2. **Abre el Frontend:** Navega a la carpeta del frontend y abre el archivo `login.html` en tu navegador preferido.
3. **Credenciales de acceso predefinidas:** Usa los siguientes datos (ya configurados con hash en el script SQL) para entrar por primera vez:
   - **Usuario:** `admin@pharma.com`
   - **Contraseña:** `Admin123!`
---
