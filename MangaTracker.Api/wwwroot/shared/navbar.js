// wwwroot/shared/navbar.js
(function () {
    const NAVBAR_STYLE_ID = "mt-navbar-style";

    function getUsuarioLogado() {
        try { return JSON.parse(localStorage.getItem("usuarioLogado")); }
        catch { return null; }
    }

    function logout() {
        localStorage.removeItem("usuarioLogado");
        window.location.href = "index.html";
    }

    function injectCssOnce() {
        if (document.getElementById(NAVBAR_STYLE_ID)) return;

        const style = document.createElement("style");
        style.id = NAVBAR_STYLE_ID;
        style.textContent = `
      /* ====== NAVBAR (compartilhada) ====== */
      .navbar {
        background: #000;
        padding: 16px 50px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-bottom: 3px solid #e50914;
      }

      .logo {
        font-size: 1.5em;
        font-weight: 800;
        text-decoration: none;
        color: white;
        letter-spacing: .5px;
      }
      .logo span { color: #e50914; }

      .menu {
        display: flex;
        gap: 14px;
        align-items: center;
      }

      .menu a {
        color: white;
        text-decoration: none;
        font-weight: 700;
        padding: 10px 14px;
        border-radius: 10px;
        border: 1px solid transparent;
      }
      .menu a:hover {
        background: #151515;
        border-color: #2a2a2a;
      }

      .btn-primary {
        background: #e50914;
        border-color: #e50914 !important;
      }
      .btn-primary:hover {
        background: #ff1a1a;
        border-color: #ff1a1a !important;
      }

      /* Dropdown do usuário */
      .userbox {
        position: relative;
        display: inline-flex;
        align-items: center;
      }

      .userbtn {
        background: transparent;
        color: white;
        border: 1px solid #2a2a2a;
        padding: 10px 14px;
        border-radius: 10px;
        cursor: pointer;
        font-weight: 800;
      }
      .userbtn:hover { background: #151515; }

      .badge-admin {
        font-size: 0.75em;
        padding: 2px 8px;
        border-radius: 999px;
        border: 1px solid #e50914;
        color: #e50914;
        margin-left: 8px;
      }

      .dropdown {
        position: absolute;
        right: 0;
        top: 48px;
        width: 260px;
        background: #0f0f0f;
        border: 1px solid #2a2a2a;
        border-radius: 14px;
        padding: 8px;
        display: none;
        box-shadow: 0 14px 30px rgba(0,0,0,0.6);
        z-index: 9999;
      }

      .dropdown a,
      .dropdown button {
        width: 100%;
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 10px 12px;
        border-radius: 10px;
        text-decoration: none;
        background: transparent;
        border: none;
        color: #ddd;
        cursor: pointer;
        font-size: 0.95em;
        box-sizing: border-box;
      }

      .dropdown a:hover,
      .dropdown button:hover { background: #1b1b1b; }

      .dropdown .sep {
        height: 1px;
        background: #242424;
        margin: 6px 0;
      }

      @media (max-width: 980px) {
        .navbar { padding: 14px 18px; }
      }
    `;
        document.head.appendChild(style);
    }

    function mountNavbar(hostId = "navbarHost") {
        injectCssOnce();

        const host = document.getElementById(hostId);
        if (!host) return;

        // HTML base da navbar
        host.innerHTML = `
      <nav class="navbar">
        <a href="index.html" class="logo">MANGA<span>TRACKER</span></a>
        <div class="menu" id="menuArea"></div>
      </nav>
    `;

        const area = host.querySelector("#menuArea");
        const u = getUsuarioLogado();

        if (!u) {
            area.innerHTML = `
        <a href="login.html">Entrar</a>
        <a class="btn-primary" href="cadastro.html">Criar Conta</a>
      `;
            return;
        }

        area.innerHTML = `
      <div class="userbox">
        <button class="userbtn" id="userBtn">
          Olá, ${u.nome}
          ${u.isAdmin ? `<span class="badge-admin">admin</span>` : ``}
          <span style="opacity:.8; margin-left:8px;">▾</span>
        </button>

        <div class="dropdown" id="userDropdown">
          <a href="minha-lista.html">📚 Minha Lista <span>›</span></a>
          <a href="#" id="perfilBtn">👤 Perfil (em breve) <span>›</span></a>

          ${u.isAdmin ? `
            <div class="sep"></div>
            <a href="admin.html">🛠️ Admin (Catálogo) <span>›</span></a>
            
          ` : ``}

          <div class="sep"></div>
          <button id="logoutBtn">🚪 Sair <span>›</span></button>
        </div>
      </div>
    `;

        const btn = host.querySelector("#userBtn");
        const dd = host.querySelector("#userDropdown");
        const logoutBtn = host.querySelector("#logoutBtn");
        const perfilBtn = host.querySelector("#perfilBtn");

        btn.addEventListener("click", (e) => {
            e.preventDefault();
            dd.style.display = (dd.style.display === "block") ? "none" : "block";
        });

        logoutBtn.addEventListener("click", (e) => {
            e.preventDefault();
            logout();
        });

        perfilBtn.addEventListener("click", (e) => {
            e.preventDefault();
            alert("Perfil (em breve). Vamos fazer depois 🙂");
        });

        document.addEventListener("click", (e) => {
            if (!e.target.closest(".userbox")) dd.style.display = "none";
        });
    }

    // expõe globalmente
    window.mountNavbar = mountNavbar;
})();