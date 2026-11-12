/* Messaging (renter: messages.html, owner: owner-messages.html) — connected to PostgreSQL backend with offline fallback */
(async function () {
  if (RC.blocked) return;
  const db = RC.db, main = document.getElementById('main');
  const role = document.body.dataset.role || 'renter';
  const me = RC.requireRole(role);
  const other = role === 'renter' ? 'owner' : 'renter';

  const otherOf = c => {
    let u = role === 'renter' ? RC.owner(c.ownerId) : RC.renter(c.renterId);
    if (!u || !u.firstName) {
      const inq = (db.inquiries || []).find(q => String(q.id) === String(c.inquiryId));
      if (inq && inq.renterName) {
        const parts = inq.renterName.trim().split(' ');
        u = {
          id: c.renterId,
          firstName: parts[0] || 'Juan',
          lastName: parts.slice(1).join(' ') || 'Dela Cruz',
          email: inq.renterEmail || '',
          phone: inq.renterPhone || ''
        };
      } else {
        u = {
          id: role === 'renter' ? (c.ownerId || 1) : (c.renterId || 1),
          firstName: role === 'renter' ? 'Maria' : 'Juan',
          lastName: role === 'renter' ? 'Santos' : 'Dela Cruz',
          email: '',
          phone: ''
        };
      }
    }
    return u;
  };

  const st = { active: RC.qs('c'), q: '' };
  const pageTitle = role === 'renter' ? '<div class="d-flex justify-content-between align-items-end mb-3"><div><h1 class="h2 mb-1">Messages</h1><p class="text-muted mb-0">Your conversations with the property owner.</p></div></div>' : '';

  main.innerHTML = `<div class="${role === 'renter' ? 'container-xxl py-4' : ''}">${pageTitle}
    <div class="rc-chat" id="chat">
      <div class="rc-chat-list">
        <div class="head"><div class="input-icon"><i class="bi bi-search"></i><input class="form-control" id="cSearch" placeholder="Search conversations" aria-label="Search conversations"></div></div>
        <div class="rc-convos" id="convos" role="list"></div>
      </div>
      <section class="rc-chat-pane" id="pane" aria-label="Conversation"></section>
    </div></div>`;
  const chat = document.getElementById('chat');

  // Sync properties and inquiries with backend PostgreSQL database
  if (RC.syncProperties) await RC.syncProperties();
  if (RC.syncInquiries) await RC.syncInquiries(role, me ? me.id : null);

  function renderList() {
    const q = st.q.toLowerCase();
    const list = RC.convosFor(role, me.id).filter(c => {
      const u = otherOf(c), p = RC.prop(c.propertyId) || {};
      const msgs = (c.messages || []).map(m => m.text || '');
      return !q || [RC.fullName(u), p.name || '', ...msgs].join(' ').toLowerCase().includes(q);
    });

    document.getElementById('convos').innerHTML = list.length ? list.map(c => {
      const u = otherOf(c), p = RC.prop(c.propertyId) || { name: 'Rental Property' }, l = RC.lastMsg(c), un = (c.unread && c.unread[role]) || 0;
      const otherName = RC.fullName(u) || (role === 'renter' ? 'Property Owner' : 'Interested Renter');
      return `<button class="rc-convo ${c.id === st.active ? 'active' : ''} ${un ? 'unread' : ''}" data-c="${c.id}" role="listitem">${RC.avatar(u)}
        <span class="body"><span class="top"><span class="name">${RC.esc(otherName)}</span><span class="time">${l.time ? RC.timeAgo(l.time) : ''}</span></span>
        <span class="prop d-block">${RC.esc(p.name)}</span>
        <span class="d-flex justify-content-between gap-2 align-items-center"><span class="prev">${l.text ? (l.from === role ? 'You: ' : '') + RC.esc(l.text) : 'No messages yet'}</span>${un ? `<span class="rc-unread" aria-label="${un} unread">${un}</span>` : ''}</span></span></button>`;
    }).join('') : `<div class="rc-empty p-4"><i class="bi bi-chat-dots fs-2"></i><h2 class="h6 mt-3">${st.q ? 'No conversations found' : 'No messages yet'}</h2><p class="text-muted small">${st.q ? 'Try another search.' : 'Your conversations with the property owner or interested renters will appear here.'}</p>${role === 'renter' && !st.q ? '<a class="btn btn-primary btn-pill btn-sm" href="browse.html">Browse Rentals</a>' : ''}</div>`;
  }

  function renderPane() {
    const c = (db.conversations || []).find(x => String(x.id) === String(st.active) || String(x.inquiryId) === String(st.active));
    const pane = document.getElementById('pane');
    if (!c) {
      pane.innerHTML = `<div class="rc-chat-empty"><div><i class="bi bi-chat-square-text fs-1"></i><h2 class="h5 mt-2">No conversation selected</h2><p class="mb-3">Select a conversation from the left to start messaging.</p>${role === 'renter' ? '<a href="browse.html" class="btn btn-primary btn-pill">Browse Rentals</a>' : ''}</div></div>`;
      return;
    }

    const u = otherOf(c);
    const p = RC.prop(c.propertyId) || { name: 'Rental Property', id: c.propertyId || '' };
    const q = (db.inquiries || []).find(x => String(x.id) === String(c.inquiryId));
    const otherName = RC.fullName(u) || (role === 'renter' ? 'Property Owner' : 'Interested Renter');
    const otherFirst = u.firstName || otherName.split(' ')[0] || 'User';

    let lastDay = '';
    const log = (c.messages || []).map(m => {
      const d = RC.date(m.time, { weekday: 'short', month: 'short', day: 'numeric' }), sep = d !== lastDay ? `<div class="rc-day">${d}</div>` : '';
      lastDay = d;
      const isMe = m.from === role;
      return sep + `<div class="rc-msg ${isMe ? 'me' : 'them'}"><div class="bubble">${RC.esc(m.text)}</div><span class="stamp">${isMe ? 'You' : RC.esc(otherFirst)} · ${RC.time(m.time)}</span></div>`;
    }).join('');

    pane.innerHTML = `
      <header class="rc-chat-head">
        <button class="rc-icon-btn rc-chat-back" id="backBtn" aria-label="Back to conversations"><i class="bi bi-arrow-left"></i></button>
        ${RC.avatar(u)}
        <div class="who"><div class="fw-bold">${RC.esc(otherName)}</div><div class="small text-muted"><a class="text-muted text-decoration-underline" href="property.html?id=${p.id}">${RC.esc(p.name)}</a>${role === 'owner' && u.phone ? ' · ' + RC.esc(u.phone) : ''}</div></div>
        ${q ? RC.statusBadge(q.status) : ''}
        ${role === 'owner' && q ? `<a class="btn btn-light-soft btn-sm d-none d-sm-inline-flex" href="owner-inquiries.html?id=${q.id}">View inquiry</a>` : ''}
      </header>
      <div class="rc-chat-log" id="log" aria-live="polite">${log || `<div class="rc-chat-empty"><div><i class="bi bi-hand-wave fs-2"></i><p class="mt-2 mb-0">Say hello to ${RC.esc(otherFirst)} and ask about ${RC.esc(p.name)}.</p></div></div>`}</div>
      <form class="rc-chat-input" id="sendForm">
        <label class="visually-hidden" for="msgInput">Message</label>
        <textarea class="form-control" id="msgInput" rows="1" maxlength="2000" placeholder="Write a message…"></textarea>
        <button class="btn btn-primary" type="submit" aria-label="Send message"><i class="bi bi-send-fill"></i></button>
      </form>`;

    const lg = document.getElementById('log');
    if (lg) lg.scrollTop = lg.scrollHeight;
    const ta = document.getElementById('msgInput');
    if (ta) {
      ta.addEventListener('input', () => { ta.style.height = 'auto'; ta.style.height = Math.min(ta.scrollHeight, 140) + 'px'; });
      ta.addEventListener('keydown', e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); document.getElementById('sendForm').requestSubmit(); } });
    }

    document.getElementById('sendForm').addEventListener('submit', async e => {
      e.preventDefault();
      const text = ta.value.trim();
      if (!text) { ta.focus(); return; }

      // 1. Optimistic UI update
      c.messages = c.messages || [];
      c.messages.push({ from: role, text, time: RC.nowIso() });
      c.unread = c.unread || {};
      c.unread[other] = (c.unread[other] || 0) + 1;
      if (role === 'owner' && q && q.status === 'Pending') {
        q.status = 'Ongoing';
        RC.addNotif('renter', 'status', `Your inquiry about ${p.name} is now Ongoing`, 'my-inquiries.html');
      }
      RC.addNotif(other, 'message', `${RC.fullName(me)} sent a message about ${p.name}`, other === 'owner' ? `owner-messages.html?c=${c.id}` : `messages.html?c=${c.id}`);
      RC.save();
      renderList();
      renderPane();
      RC.renderChrome();

      // 2. Persist message to backend PostgreSQL database
      const API_BASE = (location.port === '5115') ? '' : 'http://localhost:5115';
      const inqId = Number(c.inquiryId || c.id);
      const senderId = Number(me.id) || (role === 'owner' ? 1 : 2);
      if (inqId > 0) {
        try {
          await fetch(`${API_BASE}/api/messages`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              inquiryId: inqId,
              senderId: senderId,
              body: text
            })
          });
        } catch (apiErr) {
          console.warn('Backend message save skipped:', apiErr);
        }
      }

      const inputAfter = document.getElementById('msgInput');
      if (inputAfter) inputAfter.focus();
    });

    const backBtn = document.getElementById('backBtn');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        chat.classList.remove('show-pane');
        st.active = null;
        history.replaceState(null, '', location.pathname);
        renderList();
      });
    }
  }

  function open(id) {
    st.active = id;
    const c = (db.conversations || []).find(x => String(x.id) === String(id) || String(x.inquiryId) === String(id));
    if (c && c.unread && c.unread[role]) {
      c.unread[role] = 0;
      RC.save();
      RC.renderChrome();
    }
    history.replaceState(null, '', `${location.pathname}?c=${id}`);
    chat.classList.add('show-pane');
    renderList();
    renderPane();
  }

  document.getElementById('convos').addEventListener('click', e => {
    const b = e.target.closest('[data-c]');
    if (b) open(b.dataset.c);
  });
  document.getElementById('cSearch').addEventListener('input', e => {
    st.q = e.target.value.trim();
    renderList();
  });

  const convos = RC.convosFor(role, me.id);
  const first = convos[0];
  if (st.active && (db.conversations || []).some(c => String(c.id) === String(st.active) || String(c.inquiryId) === String(st.active))) {
    open(st.active);
  } else {
    st.active = window.matchMedia('(min-width: 992px)').matches && first ? first.id : null;
    if (st.active) open(st.active);
    else { renderList(); renderPane(); }
  }
})();
