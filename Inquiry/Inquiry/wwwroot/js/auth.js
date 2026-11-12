/* =========================================================
   BahayLink — authentication layer
   Direct REST API Provider (Simple Login / Registration)
   ========================================================= */
(function (RC) {
  RC.AUTH_MODE = 'api'; // Connected to ASP.NET Core API

  const SKEY = 'bahaylink-session-v1';
  RC.HOME = { renter: 'index.html', owner: 'owner-dashboard.html' };
  RC.roleName = r => ({ renter: 'Renter', owner: 'Property Owner/Administrator' }[r] || 'Guest');
  RC.isLocalDev = () => location.protocol === 'file:' || ['localhost', '127.0.0.1', '[::1]'].includes(location.hostname);

  // Validate PH mobile numbers and store them in one format (09XXXXXXXXX).
  RC.normalizePHMobile = value => {
    const raw = String(value || '').trim();
    if (!/^[+0-9()\s-]+$/.test(raw)) return '';
    const clean = raw.replace(/[()\s-]/g, '');
    const normalized = /^\+639\d{9}$/.test(clean) ? '0' + clean.slice(3) : clean;
    return /^09\d{9}$/.test(normalized) ? normalized : '';
  };

  /* Session record holds { userId, role, user } */
  const store = {
    read() {
      try {
        const raw = sessionStorage.getItem(SKEY) || localStorage.getItem(SKEY);
        if (!raw) return null;
        return JSON.parse(raw);
      } catch (e) { return null; }
    },
    write(s, remember) {
      try {
        localStorage.removeItem(SKEY);
        sessionStorage.removeItem(SKEY);
        (remember ? localStorage : sessionStorage).setItem(SKEY, JSON.stringify(s));
      } catch (e) { }
    },
    clear() {
      try {
        localStorage.removeItem(SKEY);
        sessionStorage.removeItem(SKEY);
      } catch (e) { }
    }
  };

  /* ---------- API Auth Provider (Connects to backend /api/auth) ---------- */
  const ApiAuthProvider = {
    mode: 'api',
    _session: null,

    async _post(url, body) {
      // Connect to ASP.NET Core API server at http://localhost:5115
      const baseUrl = (location.port === '5115') ? '' : 'http://localhost:5115';
      try {
        const r = await fetch(baseUrl + url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(body || {})
        });
        const data = await r.json().catch(() => ({}));
        return { ok: r.ok, data };
      } catch (err) {
        return {
          ok: false,
          data: { message: 'Hindi makakonekta sa API server (http://localhost:5115). Siguraduhing tumatakbo ang backend.' }
        };
      }
    },

    // POST /api/auth/login  { email, password, rememberMe }
    async login({ email, password, remember }) {
      const { ok, data } = await this._post('/api/auth/login', { email, password, rememberMe: !!remember });
      if (!ok) return { ok: false, error: data.message || 'Email or password is incorrect.' };

      this._session = { userId: data.user.id, role: data.role, user: data.user };
      store.write(this._session, !!remember);
      return { ok: true, role: data.role, user: data.user };
    },

    // POST /api/auth/register  { firstName, lastName, email, phone, password }
    async register(form) {
      const phone = RC.normalizePHMobile(form.phone);
      if (!phone) return { ok: false, field: 'phone', error: 'Please enter a valid Philippine mobile number (e.g. 09XXXXXXXXX or +639XXXXXXXXX).' };

      const { ok, data } = await this._post('/api/auth/register', { ...form, phone, accountType: 'renter' });
      return ok
        ? { ok: true, role: data.role }
        : { ok: false, field: data.field, error: data.message || 'We couldn’t create your account.' };
    },

    async requestPasswordReset(email) {
      await this._post('/api/auth/forgot-password', { email });
      return { ok: true };
    },

    async logout() {
      await this._post('/api/auth/logout');
      this._session = null;
      store.clear();
    },

    session() {
      return this._session || store.read();
    }
  };

  RC.Auth = ApiAuthProvider;

  /* ---------- helpers used by the rest of the frontend ---------- */
  RC.session = () => RC.Auth.session();
  RC.signOut = () => RC.Auth.logout();
  RC.me = function () {
    const s = RC.session();
    if (!s) return null;
    const u = s.user || RC.owner(s.userId) || RC.renter(s.userId) || { id: s.userId, firstName: 'User', lastName: '' };
    RC.recordLastActive(u.id || s.userId);
    return Object.assign({ role: s.role }, u);
  };

  /* Page guard — Redirects to login if not authenticated */
  RC.requireRole = function (role) {
    const me = RC.me();
    const page = location.pathname.split('/').pop() + location.search;
    if (!me) {
      RC.blocked = true;
      location.replace('login.html?next=' + encodeURIComponent(page));
      return null;
    }
    if (role && role !== '*' && role !== 'any') {
      const allowed = role.split(',').map(r => r.trim());
      if (!allowed.includes(me.role)) {
        RC.blocked = true;
        location.replace(RC.HOME[me.role] + '?denied=1');
        return null;
      }
    }
    return me;
  };

  RC.safeNext = function (next, role) {
    if (!next || !/^[a-z-]+\.html(\?[\w=&%.-]*)?$/i.test(next)) return null;
    if (next.startsWith('admin-') && role !== 'owner') return null;
    if (next.startsWith('owner-') && role !== 'owner') return null;
    return next;
  };
})(window.RC);
