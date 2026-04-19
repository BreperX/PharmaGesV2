// components.js — Sidebar + Header compartido para todas las páginas

(function () {
  const PAGES = [
    { href: 'dashboard.html',  icon: 'layout-dashboard', label: 'Dashboard' },
    { href: 'inventario.html', icon: 'package',           label: 'Inventario' },
    { href: 'ventas.html',     icon: 'shopping-cart',     label: 'Ventas' },
    { href: 'reportes.html',   icon: 'bar-chart-2',       label: 'Reportes' },
    { href: 'usuarios.html',   icon: 'users',             label: 'Usuarios', id: 'nav-usuarios' },
  ];

  const current = location.pathname.split('/').pop() || 'dashboard.html';

  function buildSidebar() {
    const navItems = PAGES.map(p => `
      <a class="nav-item${current === p.href ? ' active' : ''}" href="${p.href}"${p.id ? ` id="${p.id}"` : ''}>
        <span class="nav-icon"><i data-lucide="${p.icon}"></i></span>
        <span class="nav-label"> ${p.label}</span>
      </a>`).join('');

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
            <div class="user-avatar" id="userAvatar">A</div>
            <div>
              <div class="user-name" id="userName">Admin</div>
              <div class="user-role" id="userRole">Administrador</div>
            </div>
          </div>
        </div>
      </header>`;
  }

  function init() {
    // Leer atributos del <body> para título/subtítulo
    const body = document.body;
    const title    = body.dataset.title    || '';
    const subtitle = body.dataset.subtitle || '';

    body.insertAdjacentHTML('afterbegin', buildSidebar() + buildHeader(title, subtitle));

    // Sidebar colapsable
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
    if (toggleButton) toggleButton.title = collapsed ? 'Abrir menú' : 'Colapsar menú';
    applyState(collapsed);

    if (toggleButton) {
      toggleButton.addEventListener('click', () => {
        const next = !document.querySelector('.sidebar').classList.contains('collapsed');
        localStorage.setItem(STORAGE_KEY, next);
        applyState(next);
        toggleButton.title = next ? 'Abrir menú' : 'Colapsar menú';
      });
    }

    // Ocultar nav-usuarios si no es admin
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    if (usuario.rol !== 'Administrador') {
      const navU = document.getElementById('nav-usuarios');
      if (navU) navU.style.display = 'none';
    }

    // Notif panel cerrar al click fuera
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
