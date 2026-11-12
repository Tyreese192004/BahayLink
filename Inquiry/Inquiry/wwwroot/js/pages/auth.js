/* Login and Sign Up pages.
   One login page for every account type. The role comes from the account (RC.Auth), never from the user.
   Preview mode: see js/auth.js — passwords are validated for format only and are never stored. */
(function () {
  const db = RC.db, main = document.getElementById('main');
  const mode = document.body.dataset.active; // 'login' | 'signup'
  const LOGO = `<a class="rc-logo mb-4 text-decoration-none" href="index.html" aria-label="BahayLink Home"><svg class="rc-logo-mark" viewBox="0 0 32 32" aria-hidden="true"><path d="M16 3 3.5 13.2V29h9.2v-8.6h6.6V29h9.2V13.2z" fill="#222222"/><circle cx="16" cy="14.5" r="2.6" fill="#fff"/></svg><span>BahayLink</span></a>`;
  const media = (img, quote, who) => `<div class="rc-auth-media"><img src="${RC.esc(img || 'images/signup-house.jpg')}" alt="BahayLink" loading="eager" style="position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; object-position: center;"><div class="rc-auth-quote"><p class="rc-auth-quote-title">${quote}</p>${who ? `<p class="rc-auth-quote-sub mb-0 opacity-75">${who}</p>` : ''}</div></div>`;
  const pwField = (id, label, ac, extra = '') => `<div class="mb-3 rc-field"><label class="form-label" for="${id}">${label}</label>
    <div class="position-relative"><input class="form-control pe-5" id="${id}" type="password" autocomplete="${ac}"><button type="button" class="pw-toggle" aria-label="Show password"><i class="bi bi-eye"></i></button></div>${extra}<div class="invalid-feedback"></div></div>`;
  const $ = id => document.getElementById(id);
  const busy = (btn, on, label) => { btn.disabled = on; btn.innerHTML = on ? '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Please wait…' : label; };
  const alertBox = (t, kind = 'danger') => $('formAlert').innerHTML = t ? `<div class="alert alert-${kind} py-2 small d-flex gap-2 align-items-start" role="alert"><i class="bi ${kind === 'danger' ? 'bi-exclamation-circle' : kind === 'success' ? 'bi-check-circle' : 'bi-info-circle'} mt-1"></i><span>${t}</span></div>` : '';
  const previewNote = '';

  // already signed in → go to the right place
  const current = RC.me();
  if (current) {
    const next = RC.safeNext(RC.qs('next'), current.role);
    location.replace(next || RC.HOME[current.role]);
    return;
  }

  /* ---------- LOGIN ---------- */
  if (mode === 'login') {
    const email0 = RC.qs('email') || '';
    main.innerHTML = `<div class="rc-auth">
      ${media(db.IMG.auth, 'Because Everyone Deserves a Place to<br>Belong.', '')}
      <div class="rc-auth-form"><div class="inner">
        ${LOGO}
        <h1 class="h2 mb-1">Welcome back</h1><p class="text-muted mb-4">Log in to check your inquiries, messages and listings.</p>
        <div id="formAlert" aria-live="polite"></div>
        <form id="loginForm" novalidate>
          <div class="mb-3 rc-field"><label class="form-label" for="lEmail">Email address</label><input class="form-control" id="lEmail" type="email" autocomplete="username" placeholder="you@example.com" value="${RC.esc(email0)}"><div class="invalid-feedback"></div></div>
          ${pwField('lPass', 'Password', 'current-password')}
          <div class="d-flex justify-content-between align-items-center mb-4">
            <div class="form-check"><input class="form-check-input" type="checkbox" id="lRemember"><label class="form-check-label" for="lRemember">Remember me</label></div>
            <a href="#" id="forgotLink" class="fw-semibold text-decoration-underline small">Forgot password?</a></div>
          <button class="btn btn-gradient w-100 btn-lg" type="submit" id="lBtn">Log in</button>
        </form>
        <p class="text-center mt-4 mb-0">Don’t have an account? <a href="signup.html" class="fw-semibold text-decoration-underline">Create an account</a></p>
        ${previewNote}
      </div></div></div>`;

    if (RC.qs('out')) alertBox('You’ve been logged out.', 'secondary');
    if (RC.qs('registered')) alertBox('Your account is ready. Log in to continue.', 'success');
    if (RC.qs('next')) alertBox('Please log in to continue.', 'info');
    (email0 ? $('lPass') : $('lEmail')).focus();

    $('loginForm').addEventListener('submit', async e => {
      e.preventDefault();
      alertBox('');
      const email = $('lEmail').value.trim(), pw = $('lPass').value;
      let ok = RC.setInvalid($('lEmail'), !email ? 'Enter your email address.' : !RC.isEmail(email) ? 'That email address looks incomplete.' : '');
      ok = RC.setInvalid($('lPass'), !pw ? 'Enter your password.' : pw.length < 8 ? 'Passwords are at least 8 characters.' : '') && ok;
      if (!ok) return;
      busy($('lBtn'), true);
      const res = await RC.Auth.login({ email, password: pw, remember: $('lRemember').checked });
      if (!res.ok) { busy($('lBtn'), false, 'Log in'); alertBox(res.error); $('lPass').value = ''; $('lPass').focus(); return; }
      // the account's assigned role decides where to go
      location.href = RC.safeNext(RC.qs('next'), res.role) || RC.HOME[res.role];
    });

    $('forgotLink').addEventListener('click', e => {
      e.preventDefault();
      const m = RC.modal({ title: 'Reset your password', body: `<form id="fpForm" novalidate><p class="text-muted">Enter the email you signed up with and we’ll send you a link to reset your password.</p>
        <div class="mb-3 rc-field"><label class="form-label" for="fpEmail">Email address</label><input class="form-control" id="fpEmail" type="email" value="${RC.esc($('lEmail').value)}"><div class="invalid-feedback"></div></div>
        <button class="btn btn-dark w-100" type="submit">Send reset link</button>
        ${RC.AUTH_MODE === 'preview' ? '<p class="rc-demo-note mt-3 mb-0"><i class="bi bi-info-circle"></i>Preview build — the email is sent once the server is connected.</p>' : ''}</form>`, onShown: el => el.querySelector('#fpEmail').focus() });
      m.el.querySelector('#fpForm').addEventListener('submit', async ev => {
        ev.preventDefault(); const i = m.el.querySelector('#fpEmail');
        if (!RC.setInvalid(i, RC.isEmail(i.value.trim()) ? '' : 'Enter a valid email address.')) return;
        await RC.Auth.requestPasswordReset(i.value.trim());
        m.close(); RC.toast('If an account uses that email, a reset link is on its way.', 'info'); // same answer whether or not the email exists
      });
    });
    return;
  }

  /* ---------- SIGN UP ---------- */
  main.innerHTML = `<div class="rc-auth">
    ${media(db.IMG.ext5, 'Ask the owner directly. Keep every answer in one place.', 'Find your next home with BahayLink')}
    <div class="rc-auth-form"><div class="inner">
      ${LOGO}
      <h1 class="h2 mb-1">Create your renter account</h1><p class="text-muted mb-4">Sign up to explore rentals and send inquiries to the property owner.</p>
      <div id="formAlert" aria-live="polite"></div>
      <form id="suForm" novalidate>
        <div class="rc-name-row mb-3">
          <div class="rc-name-col-first rc-field">
            <label class="form-label" for="sFirst">First name</label>
            <input class="form-control" id="sFirst" autocomplete="given-name" placeholder="First name">
            <div class="invalid-feedback"></div>
          </div>
          <div class="rc-name-col-last rc-field">
            <label class="form-label" for="sLast">Last name</label>
            <input class="form-control" id="sLast" autocomplete="family-name" placeholder="Last name">
            <div class="invalid-feedback"></div>
          </div>
          <div class="rc-name-col-suffix rc-field">
            <label class="form-label text-nowrap" for="sSuffix">Suffix (Optional)</label>
            <select class="form-select" id="sSuffix" aria-label="Suffix (Optional)">
              <option value="">Select suffix</option>
              <option value="Jr.">Jr.</option>
              <option value="Sr.">Sr.</option>
              <option value="II">II</option>
              <option value="III">III</option>
              <option value="IV">IV</option>
              <option value="V">V</option>
            </select>
          </div>
        </div>
        <div class="mb-3 rc-field">
          <label class="form-label" for="sEmail">Email address</label>
          <input class="form-control" id="sEmail" type="email" autocomplete="email" placeholder="you@example.com">
          <div class="invalid-feedback"></div>
        </div>
        <div class="mb-3 rc-field">
          <label class="form-label" for="sPhone">Contact number</label>
          <input class="form-control" id="sPhone" type="tel" inputmode="tel" autocomplete="tel" maxlength="13" placeholder="09XXXXXXXXX" required>
          <div class="invalid-feedback"></div>
        </div>
        ${pwField('sPass', 'Password', 'new-password', '<div class="rc-meter mt-2"><span id="sMeter"></span></div><div class="small text-muted mt-1" id="sMeterText">Use 8 or more characters with a mix of letters, numbers and symbols.</div>')}
        ${pwField('sConfirm', 'Confirm password', 'new-password')}
        <div class="mb-4 rc-field">
          <p class="small text-muted mb-2">Please read and accept our <a href="#" id="openTermsLink" class="fw-semibold text-decoration-underline">Terms of Service</a> and <a href="#" data-policy="privacy" class="text-decoration-underline">Privacy Policy</a> before creating your account.</p>
          <div class="form-check d-none" id="sTermsWrap"><input class="form-check-input" type="checkbox" id="sTerms" checked><label class="form-check-label small" for="sTerms">I agree to the Terms of Service and Privacy Policy, and consent to the processing of my personal data under RA 10173.</label><div class="invalid-feedback"></div></div>
        </div>
        <button class="btn btn-gradient w-100 btn-lg" type="submit" id="sBtn">Create Account</button>
      </form>
      <p class="text-center mt-4 mb-0">Already have an account? <a href="login.html" class="fw-semibold text-decoration-underline">Log in</a></p>
    </div></div></div>`;

  // First Name & Last Name: allow all characters including numbers, dashes, and special characters

  // Restrict Contact Number: allows Philippine numbers starting with 09 (11 digits) or +639 (13 characters), without adding auto spaces
  const restrictPhoneInput = el => {
    if (!el) return;
    el.setAttribute('maxlength', '13');
    el.addEventListener('keydown', e => {
      if (e.ctrlKey || e.altKey || e.metaKey || e.key.length > 1) return;
      if (e.key === '+') {
        if (el.selectionStart === 0 && !el.value.includes('+')) return;
        e.preventDefault();
        return;
      }
      if (!/^\d$/.test(e.key)) {
        e.preventDefault();
        return;
      }
      const hasLeadingPlus = el.value.startsWith('+');
      const maxLen = hasLeadingPlus ? 13 : 11;
      if (el.value.length >= maxLen && el.selectionStart === el.selectionEnd) {
        e.preventDefault();
      }
    });
    el.addEventListener('input', () => {
      const v = el.value;
      const hasLeadingPlus = v.startsWith('+');
      const digits = v.replace(/\D/g, '');
      if (hasLeadingPlus) {
        el.value = ('+' + digits).slice(0, 13);
      } else {
        el.value = digits.slice(0, 11);
      }
    });
    el.addEventListener('paste', e => {
      e.preventDefault();
      const text = (e.clipboardData || window.clipboardData).getData('text') || '';
      const hasLeadingPlus = text.trim().startsWith('+');
      const digits = text.replace(/\D/g, '');
      if (hasLeadingPlus) {
        document.execCommand('insertText', false, ('+' + digits).slice(0, 13));
      } else {
        document.execCommand('insertText', false, digits.slice(0, 11));
      }
    });
  };

  restrictPhoneInput($('sPhone'));

  // Terms of Service modal — checkbox is hidden until user clicks "I Agree" inside the modal
  document.getElementById('openTermsLink').addEventListener('click', e => {
    e.preventDefault();
    const old = document.getElementById('rcTermsModal');
    if (old) old.remove();
    const modal = document.createElement('div');
    modal.id = 'rcTermsModal';
    modal.style.cssText = 'position:fixed;inset:0;z-index:9999;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.5);padding:16px;';
    modal.innerHTML = `
      <div style="background:#fff;border-radius:16px;max-width:560px;width:100%;max-height:80vh;display:flex;flex-direction:column;box-shadow:0 20px 60px rgba(0,0,0,0.25);">
        <div style="padding:24px 24px 0;border-bottom:1px solid #e5e7eb;padding-bottom:16px;display:flex;align-items:center;justify-content:space-between;">
          <h2 style="margin:0;font-size:1.2rem;font-weight:700;">Terms of Service</h2>
          <button id="rcTermsClose" type="button" aria-label="Close" style="background:none;border:none;font-size:1.4rem;cursor:pointer;color:#6b7280;line-height:1;">&times;</button>
        </div>
        <div style="overflow-y:auto;padding:20px 24px;flex:1;font-size:0.875rem;color:#374151;line-height:1.6;">
          <p><strong>Effective Date: November 2026</strong></p>
          <p>Welcome to <strong>BahayLink</strong>. By creating an account, you agree to the following terms:</p>
          <p><strong>1. Use of Service</strong><br>BahayLink is a web-based rental inquiry and communication platform. You may use it only for lawful purposes related to finding or listing rental properties.</p>
          <p><strong>2. Account Responsibility</strong><br>You are responsible for maintaining the confidentiality of your account credentials. BahayLink is not liable for any unauthorized access resulting from your failure to secure your account.</p>
          <p><strong>3. Inquiries</strong><br>Sending an inquiry is not a booking or payment. All rental agreements are made directly between the renter and the property owner.</p>
          <p><strong>4. Accuracy of Information</strong><br>You agree to provide accurate and truthful information when registering and communicating on the platform.</p>
          <p><strong>5. Privacy</strong><br>Your personal data is processed in accordance with our Privacy Policy and Republic Act 10173 (Data Privacy Act of 2012).</p>
          <p><strong>6. Prohibited Conduct</strong><br>You must not use BahayLink to post fraudulent listings, harass other users, or engage in any illegal activity.</p>
          <p><strong>7. Modifications</strong><br>BahayLink reserves the right to update these terms at any time. Continued use of the platform after changes constitutes your acceptance.</p>
          <p><strong>8. Contact</strong><br>For questions about these terms, contact us at <span style="font-weight:600;">bahaylink@gmail.com</span>.</p>
        </div>
        <div style="padding:16px 24px;border-top:1px solid #e5e7eb;display:flex;gap:12px;justify-content:flex-end;">
          <button id="rcTermsDecline" type="button" style="padding:10px 20px;border:1px solid #d1d5db;border-radius:8px;background:#fff;cursor:pointer;font-size:0.9rem;color:#374151;">Decline</button>
          <button id="rcTermsAgree" type="button" style="padding:10px 24px;border:none;border-radius:8px;background:#111;color:#fff;cursor:pointer;font-size:0.9rem;font-weight:600;">I Agree</button>
        </div>
      </div>`;
    document.body.appendChild(modal);
    const close = () => { modal.remove(); };
    document.getElementById('rcTermsClose').addEventListener('click', close);
    document.getElementById('rcTermsDecline').addEventListener('click', close);
    document.getElementById('rcTermsAgree').addEventListener('click', () => {
      close();
      const wrap = document.getElementById('sTermsWrap');
      if (wrap) { wrap.classList.remove('d-none'); }
      const cb = document.getElementById('sTerms');
      if (cb) { cb.checked = true; RC.setInvalid(cb, ''); }
    });
    modal.addEventListener('click', ev => { if (ev.target === modal) close(); });
  });

  const LEVELS = [['Too weak', '#C13515'], ['Weak', '#C13515'], ['Fair', '#D9822B'], ['Good', '#3B9C5A'], ['Strong', '#12774A']];
  $('sPass').addEventListener('input', () => {
    const v = $('sPass').value, s = RC.passwordScore(v);
    $('sMeter').style.width = v ? (s + 1) * 20 + '%' : 0; $('sMeter').style.background = LEVELS[s][1];
    $('sMeterText').textContent = v ? `Password strength: ${LEVELS[s][0]}` : 'Use 8 or more characters with a mix of letters, numbers and symbols.';
  });

  const rules = {
    sFirst: v => !v.trim() ? 'Enter your first name.' : v.trim().length < 2 ? 'First name must be at least 2 characters.' : '',
    sLast: v => !v.trim() ? 'Enter your last name.' : v.trim().length < 2 ? 'Last name must be at least 2 characters.' : '',
    sEmail: v => !RC.isEmail(v.trim()) ? 'Enter a valid email address.' : '',
    sPhone: v => {
      const val = v.trim();
      if (!val) return 'Enter your contact number.';
      if (!/^(09\d{9}|\+639\d{9})$/.test(val)) {
        return 'Please enter a valid Philippine mobile number (e.g. 09XXXXXXXXX or +639XXXXXXXXX).';
      }
      return '';
    },
    sPass: v => v.length < 8 ? 'Use at least 8 characters.' : RC.passwordScore(v) < 2 ? 'Add a number or symbol to make it stronger.' : '',
    sConfirm: v => !v ? 'Confirm your password.' : v !== $('sPass').value ? 'Passwords don’t match.' : ''
  };

  Object.keys(rules).forEach(id => $(id).addEventListener('blur', () => { if ($(id).value) RC.setInvalid($(id), rules[id]($(id).value)); }));

  // Modern Airbnb-style success popup with checkmark animation
  function showSuccessModal(email) {
    const old = document.getElementById('rcSuccessModal');
    if (old) old.remove();

    const backdrop = document.createElement('div');
    backdrop.id = 'rcSuccessModal';
    backdrop.className = 'rc-success-modal-backdrop';
    backdrop.setAttribute('role', 'dialog');
    backdrop.setAttribute('aria-modal', 'true');
    backdrop.innerHTML = `
      <div class="rc-success-modal-card">
        <div class="rc-success-icon-wrap">
          <svg class="rc-checkmark-svg" viewBox="0 0 52 52" aria-hidden="true">
            <circle class="rc-checkmark-circle" cx="26" cy="26" r="23" fill="none"/>
            <path class="rc-checkmark-check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
          </svg>
        </div>
        <h2 class="rc-success-title">Account Created Successfully!</h2>
        <p class="rc-success-desc">Welcome to BahayLink! Your account is ready. You can now explore rental properties and send inquiries to the property owner.</p>
        <button type="button" class="rc-success-btn" id="modalContinueBtn">Continue to Login <span aria-hidden="true">&rarr;</span></button>
      </div>
    `;

    document.body.appendChild(backdrop);

    const proceed = () => {
      location.href = `login.html?registered=1&email=${encodeURIComponent(email)}`;
    };

    const btn = backdrop.querySelector('#modalContinueBtn');
    btn.focus();
    btn.addEventListener('click', proceed);

    const onKey = e => {
      if (e.key === 'Enter' || e.key === 'Escape') {
        window.removeEventListener('keydown', onKey);
        proceed();
      }
    };
    window.addEventListener('keydown', onKey);
  }

  $('suForm').addEventListener('submit', async e => {
    e.preventDefault(); alertBox('');
    let ok = Object.keys(rules).map(id => RC.setInvalid($(id), rules[id]($(id).value))).every(Boolean);
    ok = RC.setInvalid($('sTerms'), $('sTerms').checked ? '' : 'Please accept the Terms and Privacy Policy to continue.') && ok;
    if (!ok) { document.querySelector('.is-invalid').focus(); return; }
    busy($('sBtn'), true);
    const email = $('sEmail').value.trim();
    const first = $('sFirst').value.trim();
    const last = $('sLast').value.trim();
    const suffix = $('sSuffix') ? $('sSuffix').value.trim() : '';
    const fullLastName = suffix ? `${last} ${suffix}` : last;

    const res = await RC.Auth.register({
      accountType: 'renter',
      firstName: first,
      lastName: fullLastName,
      suffix,
      email,
      phone: $('sPhone').value.trim(),
      password: $('sPass').value
    });

    if (!res.ok) {
      busy($('sBtn'), false, 'Create Account');
      if (res.field === 'email') RC.setInvalid($('sEmail'), res.error);
      else alertBox(res.error);
      return;
    }

    busy($('sBtn'), false, 'Create Account');
    showSuccessModal(email);
  });
})();
