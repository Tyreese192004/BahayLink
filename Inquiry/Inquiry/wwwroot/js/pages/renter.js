/* Renter pages: dashboard (renter-dashboard.html) and My Inquiries (my-inquiries.html) */
(function () {
  if (RC.blocked) return;
  const db = RC.db, main = document.getElementById('main');
  const page = document.body.dataset.active;
  const me = RC.requireRole('renter');
  const mine = () => db.inquiries.filter(q => q.renterId === me.id).sort((a, b) => b.date.localeCompare(a.date));
  const convoFor = q => db.conversations.find(c => c.inquiryId === q.id) || db.conversations.find(c => c.propertyId === q.propertyId && c.renterId === q.renterId);

  function inquiryModal(q) {
    const p = RC.prop(q.propertyId) || { name: 'Removed listing', images: [], location: '' }, o = RC.owner(p.ownerId), c = convoFor(q);
    const reply = c && c.messages.find(m => m.from === 'owner');
    RC.modal({ title: 'Inquiry details', body: `
      <div class="d-flex gap-3 mb-3">${RC.img(RC.cover(p), '', 'rounded-3', 'style="width:110px;height:84px;object-fit:cover"')}
        <div class="min-w-0"><div class="fw-bold">${RC.esc(p.name)}</div><div class="small text-muted">${RC.esc(p.location)}</div><div class="mt-1">${RC.statusBadge(q.status)}</div></div></div>
      <div class="small text-muted mb-1">Your message · ${RC.date(q.date)} at ${RC.time(q.date)}</div>
      <div class="p-3 rounded-3 mb-4" style="background:var(--rc-soft)">${RC.esc(q.message)}</div>
      <h3 class="h6 mb-3">Progress</h3>
      <ol class="rc-timeline">
        <li class="done"><div class="fw-semibold">Inquiry sent</div><div class="small text-muted">${RC.date(q.date)}</div></li>
        <li class="${reply ? 'done' : ''}"><div class="fw-semibold">${o ? RC.esc(o.firstName) : 'Owner'} replied</div><div class="small text-muted">${reply ? RC.date(reply.time) : 'Waiting for a reply · usually ' + (o ? o.responseTime : '')}</div></li>
        <li class="${q.status === 'Resolved' ? 'done' : ''}"><div class="fw-semibold">Resolved</div><div class="small text-muted">${q.status === 'Resolved' ? 'Your question has been answered' : 'Marked by the owner once your question is answered'}</div></li>
      </ol>`,
      footer: `<a class="btn btn-light-soft" href="property.html?id=${p.id}">View property</a>${c ? `<a class="btn btn-dark" href="messages.html?c=${c.id}"><i class="bi bi-chat-dots me-1"></i>Open conversation</a>` : ''}` });
  }
  main.addEventListener('click', e => { const v = e.target.closest('[data-view]'); if (v) inquiryModal(db.inquiries.find(q => q.id === v.dataset.view)); });

  /* ---------- dashboard ---------- */
  if (page === 'dashboard') {
    location.replace('index.html');
    return;
  }

  /* ---------- My Inquiries ---------- */
  const st = { status: RC.qs('status') || '', q: '' };
  main.innerHTML = `<div class="container-xxl py-4">
    <div class="d-flex justify-content-between align-items-end flex-wrap gap-3 mb-4"><div><h1 class="h2 mb-1">My Inquiries</h1><p class="text-muted mb-0">Track every question you’ve sent to the property owner.</p></div>
      <a href="browse.html" class="btn btn-dark btn-pill"><i class="bi bi-plus-lg me-1"></i>New inquiry</a></div>
    <div class="d-flex flex-wrap gap-3 justify-content-between align-items-center mb-4">
      <div class="rc-tabs" id="tabs" role="group" aria-label="Filter by status"></div>
      <div class="input-icon" style="min-width:min(280px,100%)"><i class="bi bi-search"></i><input class="form-control" id="iqSearch" placeholder="Search by property or message" aria-label="Search inquiries"></div></div>
    <div id="iqList" class="d-grid gap-3" aria-live="polite"></div></div>`;
  function render() {
    const list = mine(), q = st.q.toLowerCase();
    document.getElementById('tabs').innerHTML = ['', 'Pending', 'Ongoing', 'Resolved'].map(s => `<button class="rc-chip ${st.status === s ? 'active' : ''}" data-status="${s}" aria-pressed="${st.status === s}">${s || 'All'} <span class="count num">${s ? list.filter(x => x.status === s).length : list.length}</span></button>`).join('');
    const shown = list.filter(x => (!st.status || x.status === st.status) && (!q || ((RC.prop(x.propertyId) || {}).name + ' ' + x.message).toLowerCase().includes(q)));
    document.getElementById('iqList').innerHTML = shown.length ? shown.map(x => {
      const p = RC.prop(x.propertyId) || { name: 'Removed listing', images: [], location: '' }, c = convoFor(x);
      return `<article class="rc-inq">${RC.img(RC.cover(p), p.name)}
        <div class="rc-inq-body"><div class="d-flex justify-content-between gap-2 flex-wrap"><h2 class="h6 mb-0">${RC.esc(p.name)}</h2>${RC.statusBadge(x.status)}</div>
          <div class="small text-muted"><i class="bi bi-geo-alt"></i> ${RC.esc(p.location)} · Sent ${RC.date(x.date)}</div>
          <p class="mb-2 text-truncate">${RC.esc(x.message)}</p>
          <div class="d-flex gap-2 flex-wrap mt-auto"><button class="btn btn-dark btn-sm btn-pill" data-view="${x.id}">View Details</button>
            ${c ? `<a class="btn btn-light-soft btn-sm btn-pill" href="messages.html?c=${c.id}"><i class="bi bi-chat-dots me-1"></i>Messages${c.unread.renter ? ` <span class="rc-unread d-inline-grid ms-1">${c.unread.renter}</span>` : ''}</a>` : ''}</div></div></article>`;
    }).join('') : `<div class="rc-empty"><i class="bi bi-envelope-open"></i><h2 class="h5 mt-3">${list.length ? 'No inquiries match' : 'No inquiries yet'}</h2><p class="text-muted">${list.length ? 'Try another status or search term.' : 'Once you inquire about a rental property, your inquiries will appear here.'}</p><a href="browse.html" class="btn btn-dark btn-pill">Browse rentals</a></div>`;
  }
  main.addEventListener('click', e => { const t = e.target.closest('[data-status]'); if (t) { st.status = t.dataset.status; render(); } });
  document.getElementById('iqSearch').addEventListener('input', e => { st.q = e.target.value.trim(); render(); });
  render();
})();
