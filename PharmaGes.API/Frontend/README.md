# Documentación del Frontend - PharmaGes

## Descripción General
Este proyecto es el frontend de la aplicación PharmaGes, un sistema de gestión farmacéutica. El frontend está desarrollado con HTML, CSS y JavaScript puro, sin frameworks adicionales, para mantener la simplicidad y el rendimiento.

## Estructura de Archivos

### Archivos Principales
- `index.html` - Página de inicio/login
- `dashboard.html` - Panel principal con estadísticas
- `inventario.html` - Gestión de medicamentos
- `usuarios.html` - Administración de usuarios
- `ventas.html` - Módulo de ventas con carrito
- `reportes.html` - Visualización de reportes

### Archivos de Estilos y Scripts
- `styles.css` - Estilos globales y específicos por página
- `components.js` - Funciones comunes para sidebar, header, etc.
- `sidebar.js` - Lógica específica de la barra lateral

### Librerías Externas
- **Lucide Icons**: Para iconos vectoriales (https://lucide.dev/)
- **jsPDF**: Para generación de PDFs (https://github.com/parallax/jsPDF)

## Funcionalidades Implementadas

### 1. Sistema de Autenticación
- **Archivo**: `login.html`
- **Funcionalidad**: Formulario de login con validación
- **Características**:
  - Almacenamiento de token JWT en localStorage
  - Redirección automática si ya está autenticado
  - Manejo de errores de autenticación

### 2. Dashboard
- **Archivo**: `dashboard.html`
- **Funcionalidad**: Vista general con estadísticas
- **Características**:
  - Cards con métricas principales (ventas, inventario, etc.)
  - Gráficos de tendencias (simulados)
  - Notificaciones de alertas (stock bajo, vencimientos)

### 3. Gestión de Inventario
- **Archivo**: `inventario.html`
- **Funcionalidad**: CRUD completo de medicamentos
- **Características**:
  - Tabla paginada con filtros
  - Panel lateral para agregar/editar medicamentos
  - Categorización automática por nombre
  - Estados de stock (OK, bajo, agotado)

### 4. Gestión de Usuarios
- **Archivo**: `usuarios.html`
- **Funcionalidad**: Administración de usuarios del sistema
- **Características**:
  - Lista de usuarios con roles
  - Funciones de activar/desactivar usuarios

### 5. Módulo de Ventas
- **Archivo**: `ventas.html`
- **Funcionalidad**: Proceso de venta con carrito
- **Características**:
  - Grid de productos con categorías
  - Carrito dinámico con controles de cantidad
  - Cálculo automático de totales y cambio
  - Modal de confirmación de venta
  - **Nueva**: Modal de generación de factura
    - Selector de tipo de factura (Tradicional/Electrónica)
    - Campos para datos del cliente (nombre, ID, email, dirección)
    - Campos para forma de pago (contado/crédito, medio de pago)
    - Lista de productos de la venta
    - Cálculos de subtotal, IVA y total
    - **Generación real de PDF** usando jsPDF
      - Factura Tradicional: Incluye todos los requisitos del Estatuto Tributario
      - Factura Electrónica: Incluye requisitos DIAN con CUFE, QR simulados
    - Descarga automática del PDF generado    - Diseño de factura estilo mockup con encabezado, tarjeta de cliente/vendedor, tabla de productos e información DIAN/IVA
### 6. Reportes
- **Archivo**: `reportes.html`
- **Funcionalidad**: Visualización de reportes
- **Características**:
  - Filtros por fecha y tipo
  - Tabla de resultados

## Estilos y Animaciones

### Diseño General
- **Paleta de colores**: Azul primario (#0B1F33), acentos en azul (#2563EB)
- **Tipografía**: Inter font family
- **Radio de bordes**: 12px para consistencia
- **Sombras**: Sistema de sombras sutiles para profundidad

### Animaciones Premium Agregadas
- **Transiciones suaves**: Todos los botones y cards tienen transiciones de 0.3s con cubic-bezier
- **Hover effects**:
  - Botones: Elevación con translateY(-2px) y sombra expandida
  - Cards: Elevación con translateY(-4px) y sombra
  - Productos: Elevación con translateY(-6px) y scale(1.02)
  - Estadísticas: Elevación con translateY(-8px)
- **Animaciones de entrada**: Filas de tabla con fadeInUp
- **Efectos especiales**: Botones primarios con pulso sutil

### Componentes Reutilizables
- **Botones**: Variantes primary, outline, danger
- **Modales**: Sistema de overlays con backdrop blur
- **Toasts**: Notificaciones temporales
- **Formularios**: Inputs consistentes con estados focus
- **Tablas**: Sistema de filas hover y paginación

## API Integration
- **Base URL**: `https://localhost:7092/api`
- **Autenticación**: Bearer token en headers
- **Endpoints utilizados**:
  - `POST /Auth/login` - Autenticación
  - `GET /Dashboard` - Estadísticas y alertas
  - `GET/POST/PUT/DELETE /Medicamentos` - Gestión de inventario
  - `GET/POST /Usuarios` - Gestión de usuarios
  - `POST /Ventas` - Registro de ventas

## Funciones JavaScript Comunes

### Gestión de Estado
- `localStorage` para token y datos de usuario
- Estado global para productos, carrito, etc.

### Utilidades
- `fmt(n)` - Formateo de números como moneda
- `showToast(msg, type)` - Notificaciones
- `apiFetch(url, opts)` - Fetch con autenticación

### Categorización Automática
- Función `categoriaDeProducto(p)` para asignar categorías basadas en nombre y descripción
- Emojis automáticos por tipo de producto

## Notas de Desarrollo

### Convenciones
- Nombres de variables en español para consistencia con el negocio
- Funciones nombradas descriptivamente
- Comentarios en español para claridad

### Limitaciones Actuales
- No se toca el backend (como solicitado)
- Funcionalidad de PDF es simulada visualmente
- Algunos datos están hardcodeados para demo

### Próximos Pasos
- Integración completa con backend para facturas
- Implementación real de generación de PDF
- Mejoras en UX basadas en feedback

## Responsabilidad del Desarrollador
Este frontend fue desarrollado por [Tu Nombre] como parte del sprint 1 del proyecto PharmaGes. El foco estuvo en crear una interfaz visual completa y funcional desde el punto de vista del usuario, dejando preparado el terreno para la integración completa con el backend en sprints posteriores.

La documentación incluye explicaciones breves pero completas para que cualquier desarrollador pueda entender rápidamente la estructura y continuar el desarrollo sin necesidad de explicaciones adicionales.