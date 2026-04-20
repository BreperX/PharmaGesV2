// components.js — Sidebar + Header compartido para todas las páginas

// ── Timezone offset del cliente ───────────────────────────────
// JS getTimezoneOffset() devuelve minutos invertidos (Colombia = 300 = UTC-5)
// Se inyecta en cada request como header X-Timezone-Offset
window.TZ_OFFSET = new Date().getTimezoneOffset();

// ── apiFetch global — fetch con auth + timezone header ────────
const _API_BASE = 'https://localhost:7092/api';
window.apiFetch = function(url, opts = {}) {
  const token = localStorage.getItem('token');
  return fetch(_API_BASE + url, {
    ...opts,
    headers: {
      'Content-Type':       'application/json',
      'Authorization':      token ? 'Bearer ' + token : '',
      'X-Timezone-Offset':  String(window.TZ_OFFSET),
      ...(opts.headers || {})
    }
  });
};

(function () {
  const PAGES = [
    { href: 'dashboard.html',  icon: 'layout-dashboard', label: 'Dashboard', id: 'nav-dashboard' },
    { href: 'inventario.html', icon: 'package',          label: 'Inventario', id: 'nav-inventario' },
    { href: 'ventas.html',     icon: 'shopping-cart',    label: 'Ventas',     id: 'nav-ventas' },
    { href: 'reportes.html',   icon: 'bar-chart-2',      label: 'Reportes',   id: 'nav-reportes' },
    { href: 'usuarios.html',   icon: 'users',            label: 'Usuarios',   id: 'nav-usuarios' },
  ];

  const PERMISOS = {
    'Empleado':      ['dashboard.html', 'inventario.html', 'ventas.html'],
    'Gerente':       ['dashboard.html', 'inventario.html', 'ventas.html', 'reportes.html'],
    'Administrador': ['dashboard.html', 'inventario.html', 'ventas.html', 'reportes.html', 'usuarios.html']
  };

  const current = location.pathname.split('/').pop() || 'dashboard.html';
  const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
  const rol = usuario.rol || 'Empleado';

  function mostrarErrorPermisos() {
    const nombreCompleto = `${usuario.nombre || ''} ${usuario.apellido || ''}`.trim() || 'Usuario';
    
    document.body.innerHTML = `
      <div style="height: 100vh; display: flex; flex-direction: column; align-items: center; justify-content: center; background: #f8fafc; color: #1e293b; font-family: 'Inter', system-ui, sans-serif; text-align: center; padding: 20px;">
        <div style="font-size: 120px; color: #ef4444; line-height: 1; margin-bottom: 20px; filter: drop-shadow(0 0 10px rgba(239,68,68,0.2));">✕</div>
        <h1 style="font-size: 28px; font-weight: 800; margin-bottom: 12px; color: #0f172a;">Acceso restringido</h1>
        <p style="color: #64748b; margin-bottom: 32px; max-width: 500px; font-size: 1.1rem; line-height: 1.6;">
          Lo sentimos <strong>${nombreCompleto}</strong>, pero tu cuenta con rol de <strong>${rol}</strong> no tiene permisos para ver esta página.
        </p>
        <a href="dashboard.html" style="background: #2563eb; color: white; padding: 14px 28px; border-radius: 10px; text-decoration: none; font-weight: 600; box-shadow: 0 4px 12px rgba(37,99,235,0.2); transition: all 0.2s;">
          Volver al inicio
        </a>
      </div>
    `;
    throw new Error("Acceso denegado");
  }

  function buildSidebar() {
    const paginasPermitidas = PERMISOS[rol] || PERMISOS['Empleado'];
    
    const navItems = PAGES.map(p => {
      if (!paginasPermitidas.includes(p.href)) return '';
      
      return `
        <a class="nav-item${current === p.href ? ' active' : ''}" href="${p.href}" id="${p.id}">
          <span class="nav-icon"><i data-lucide="${p.icon}"></i></span>
          <span class="nav-label"> ${p.label}</span>
        </a>`;
    }).join('');

    return `
      <aside class="sidebar">
        <div class="sidebar-brand">
          <img src="images/iso.png" class="brand-logo" alt="PharmaGes">
          <span class="brand-name">PharmaGes</span>
          <button class="sidebar-toggle" id="sidebarToggle" title="Colapsar menú">
            <i data-lucide="menu" style="width:18px;height:18px"></i>
          </button>
        </div>
        <nav class="nav">
          ${navItems}
          <div class="nav-divider"></div>
          <a class="nav-item" href="#"><span class="nav-icon"><i data-lucide="settings"></i></span><span class="nav-label"> Configuración</span></a>
        </nav>
        <div class="sidebar-footer">
          <a class="nav-item" href="#"><span class="nav-icon"><i data-lucide="help-circle"></i></span><span class="nav-label"> Ayuda</span></a>
          <button class="nav-item" onclick="cerrarSesion()"><span class="nav-icon"><i data-lucide="log-out"></i></span><span class="nav-label"> Cerrar sesión</span></button>
        </div>
      </aside>`;
  }

  function buildHeader(title, subtitle) {
    return `
      <header class="header">
        <div class="header-left">
          <h1 id="pageTitle">${title || ''}</h1>
          <p id="pageSubtitle">${subtitle || ''}</p>
        </div>
        <div class="header-right">
          <div style="position:relative">
            <div class="notif-btn" onclick="toggleNotif()" id="notifBtn">
              <i data-lucide="bell" style="width:17px;height:17px"></i>
              <div class="notif-badge" id="notifBadge">0</div>
            </div>
            <div class="notif-panel" id="notifPanel">
              <div class="notif-header">
                <span>Notificaciones</span>
                <span id="notifCount" style="color:var(--text-muted);font-weight:400;font-size:0.78rem">0 alertas</span>
              </div>
              <div class="notif-list" id="notifList">
                <div class="notif-empty">Sin notificaciones nuevas</div>
              </div>
            </div>
          </div>
          <div class="user-info">
            <div class="user-avatar" id="userAvatar">-</div>
            <div>
              <div class="user-name" id="userName">Cargando...</div>
              <div class="user-role" id="userRole">...</div>
            </div>
          </div>
        </div>
      </header>`;
  }

  function init() {
    const paginasPermitidas = PERMISOS[rol] || PERMISOS['Empleado'];
    if (!paginasPermitidas.includes(current)) {
      mostrarErrorPermisos();
      return;
    }

    const body = document.body;
    const title    = body.dataset.title    || '';
    const subtitle = body.dataset.subtitle || '';

    body.insertAdjacentHTML('afterbegin', buildSidebar() + buildHeader(title, subtitle));

    const userNameEl = document.getElementById('userName');
    const userRoleEl = document.getElementById('userRole');
    const userAvatarEl = document.getElementById('userAvatar');

    if (userNameEl && usuario.nombre) {
      userNameEl.textContent = usuario.nombre;
      if (userAvatarEl) userAvatarEl.textContent = usuario.nombre.charAt(0).toUpperCase();
    }
    if (userRoleEl) userRoleEl.textContent = rol;

    const STORAGE_KEY = 'sidebar_collapsed';
    function applyState(collapsed) {
      const sidebar = document.querySelector('.sidebar');
      const header  = document.querySelector('.header');
      const mains   = document.querySelectorAll('.main');
      const w = collapsed ? 'var(--sidebar-collapsed-w)' : 'var(--sidebar-w)';
      if (sidebar) sidebar.classList.toggle('collapsed', collapsed);
      if (header)  header.style.left = w;
      mains.forEach(m => { if (!m.classList.contains('panel-open')) m.style.marginLeft = w; });
    }

    const collapsed = localStorage.getItem(STORAGE_KEY) === 'true';
    const toggleButton = document.getElementById('sidebarToggle');
    applyState(collapsed);

    if (toggleButton) {
      toggleButton.addEventListener('click', () => {
        const isCurrentlyCollapsed = document.querySelector('.sidebar').classList.contains('collapsed');
        localStorage.setItem(STORAGE_KEY, !isCurrentlyCollapsed);
        applyState(!isCurrentlyCollapsed);
      });
    }

    document.addEventListener('click', e => {
      if (!e.target.closest('#notifBtn') && !e.target.closest('#notifPanel'))
        document.getElementById('notifPanel')?.classList.remove('open');
    });

    if (typeof lucide !== 'undefined') lucide.createIcons();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();