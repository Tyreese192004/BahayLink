/* Owner: My Properties — search, filter, change availability, edit, recent delete, permanent delete (Connected to Backend API) */
(function () {
  if (RC.blocked) return;
  const db = RC.db, main = document.getElementById('main'), me = RC.me();
  const API_BASE = (location.port === '5115') ? '' : 'http://localhost:5115';
  const st = { q: '', show: 'all', view: localStorage.getItem('owner_prop_view') || 'list' };

  main.innerHTML = `
    <div class="d-flex flex-wrap gap-3 justify-content-between align-items-center mb-4">
      <div class="d-flex flex-wrap gap-2 align-items-center">
        <div class="input-icon" style="min-width:min(280px,100%)"><i class="bi bi-search"></i><input class="form-control" id="pSearch" placeholder="Search your properties" aria-label="Search properties"></div>
        <div class="rc-tabs" id="pTabs"></div></div>
      <div class="d-flex gap-2 align-items-center">
        <div class="btn-group" role="group" aria-label="View toggle">
          <button type="button" class="btn btn-outline-dark btn-sm ${st.view === 'list' ? 'active' : ''}" data-view="list" title="List view (table)"><i class="bi bi-list-ul"></i></button>
          <button type="button" class="btn btn-outline-dark btn-sm ${st.view === 'grid' ? 'active' : ''}" data-view="grid" title="Grid view (cards)"><i class="bi bi-grid-fill"></i></button>
        </div>
        <a href="owner-property-form.html" class="btn btn-primary btn-pill"><i class="bi bi-plus-lg me-1"></i>Add Property</a>
      </div>
    </div>
    <div id="pContainer"></div>`;

  async function loadServerProperties() {
    try {
      const res = await fetch(`${API_BASE}/api/properties?includeDeleted=true`);
      if (res.ok) {
        const list = await res.json();
        if (Array.isArray(list)) {
          db.properties = list.map(p => ({
            id: p.id,
            ownerId: p.ownerId,
            name: p.name,
            type: p.type,
            description: p.description,
            location: p.location,
            city: p.city,
            price: p.price,
            bedrooms: p.bedrooms,
            bathrooms: p.bathrooms,
            area: p.area,
            available: p.available,
            status: p.status || (p.isDeleted ? 'Deleted' : 'Active'),
            isDeleted: !!p.isDeleted,
            amenities: p.amenities || [],
            requirements: p.requirements || [],
            images: p.images || [],
            listed: p.createdAt ? p.createdAt.slice(0, 10) : RC.nowIso().slice(0, 10)
          }));
          RC.save();
        }
      }
    } catch (e) {
      console.warn('API error loading properties, using local state:', e);
    }
  }

  function getOwnerProperties() {
    const ownerId = me ? (me.id || me.userId) : null;
    return (db.properties || []).filter(p => !ownerId || String(p.ownerId) === String(ownerId) || String(p.ownerId) === 'o1');
  }

  function renderList(list) {
    if (!list.length) return renderEmpty();

    return `<div class="rc-table-wrap">
      <div class="table-responsive">
        <table class="table rc-table rc-users-table align-middle">
          <thead>
            <tr>
              <th>Property</th>
              <th>Type</th>
              <th>Price</th>
              <th>Specs</th>
              <th>Status</th>
              <th>Inquiries</th>
              <th class="text-end">Actions</th>
            </tr>
          </thead>
          <tbody>
            ${list.map(p => {
              const isDeleted = p.isDeleted || p.status === 'Deleted';
              const iq = (db.inquiries || []).filter(x => String(x.propertyId) === String(p.id));
              const open = iq.filter(x => x.status !== 'Resolved').length;

              const statusCol = isDeleted
                ? '<span class="rc-badge danger">Deleted</span>'
                : (RC.owner_ ? RC.owner_.availSwitch(p) : (p.available ? '<span class="rc-badge active">• Available</span>' : '<span class="rc-badge muted">• Occupied</span>'));

              const actionCol = isDeleted ? `
                <div class="rc-user-icon-actions">
                  <button type="button" class="rc-user-icon text-primary" data-restore="${p.id}" title="Restore listing" aria-label="Restore"><i class="bi bi-arrow-counterclockwise"></i></button>
                  <button type="button" class="rc-user-icon rc-user-icon-danger" data-perm-del="${p.id}" title="Delete permanently" aria-label="Delete permanently"><i class="bi bi-trash3-fill"></i></button>
                </div>
              ` : `
                <div class="rc-user-icon-actions">
                  <a href="owner-property-form.html?id=${p.id}" class="rc-user-icon" title="Edit property" aria-label="Edit property"><i class="bi bi-pencil-square"></i></a>
                  <button type="button" class="rc-user-icon rc-user-icon-danger" data-del="${p.id}" title="Move to Recently Deleted" aria-label="Delete"><i class="bi bi-trash3"></i></button>
                </div>
              `;

              return `<tr class="${isDeleted ? 'opacity-75' : ''}">
                <td>
                  <div class="d-flex gap-3 align-items-center">
                    <img src="${RC.esc(RC.cover(p))}" class="thumb" alt="${RC.esc(p.name)}" style="width:56px;height:44px;border-radius:8px;object-fit:cover;flex-shrink:0;background:var(--rc-soft)" onerror="RC.imgFail(this)">
                    <div class="min-w-0">
                      <a href="property.html?id=${p.id}" class="fw-semibold text-truncate text-reset text-decoration-none d-block" style="max-width:260px">${RC.esc(p.name)}</a>
                      <div class="small text-muted text-truncate" style="max-width:260px"><i class="bi bi-geo-alt"></i> ${RC.esc(p.location)}</div>
                    </div>
                  </div>
                </td>
                <td class="text-nowrap"><span class="badge text-bg-light fw-medium"><i class="bi ${RC.TYPE_ICON[p.type] || 'bi-house'} me-1"></i>${p.type}</span></td>
                <td class="text-nowrap"><span class="fw-bold num">${RC.peso(p.price)}</span> <span class="text-muted small">/mo</span></td>
                <td class="text-nowrap small text-muted">${p.bedrooms} bed · ${p.bathrooms} bath · ${p.area} sqm</td>
                <td class="text-nowrap">${statusCol}</td>
                <td class="text-nowrap"><a href="owner-inquiries.html?property=${p.id}" class="small fw-semibold text-reset text-decoration-underline">${iq.length} inquir${iq.length === 1 ? 'y' : 'ies'}${open ? ` · <span style="color:var(--rc-primary)">${open} open</span>` : ''}</a></td>
                <td class="text-end">${actionCol}</td>
              </tr>`;
            }).join('')}
          </tbody>
        </table>
      </div>
    </div>`;
  }

  function renderGrid(list) {
    if (!list.length) return renderEmpty();

    return `<div class="row row-cols-1 row-cols-md-2 row-cols-xxl-3 g-4">
      ${list.map(p => {
        const isDeleted = p.isDeleted || p.status === 'Deleted';
        const iq = (db.inquiries || []).filter(x => String(x.propertyId) === String(p.id)), open = iq.filter(x => x.status !== 'Resolved').length;

        const badge = isDeleted
          ? '<span class="position-absolute top-0 start-0 m-2 rc-badge danger">Recently Deleted</span>'
          : `<span class="rc-card-badge">${RC.availBadge(p.available)}</span>`;

        const menuItems = isDeleted ? `
          <li><button class="dropdown-item text-primary" data-restore="${p.id}"><i class="bi bi-arrow-counterclockwise me-2"></i>Restore listing</button></li>
          <li><hr class="dropdown-divider"></li>
          <li><button class="dropdown-item text-danger" data-perm-del="${p.id}"><i class="bi bi-trash3-fill me-2"></i>Delete permanently</button></li>
        ` : `
          <li><a class="dropdown-item" href="owner-property-form.html?id=${p.id}"><i class="bi bi-pencil me-2"></i>Edit</a></li>
          <li><a class="dropdown-item" href="owner-inquiries.html?property=${p.id}"><i class="bi bi-envelope-paper me-2"></i>See inquiries</a></li>
          <li><hr class="dropdown-divider"></li>
          <li><button class="dropdown-item text-danger" data-del="${p.id}"><i class="bi bi-trash me-2"></i>Move to Recently Deleted</button></li>
        `;

        return `<div class="col"><article class="rc-panel p-0 overflow-hidden h-100 d-flex flex-column ${isDeleted ? 'opacity-75' : ''}">
          <div class="position-relative overflow-hidden w-100" style="height:210px;background:var(--rc-soft)">
            <img src="${RC.esc(RC.cover(p))}" alt="${RC.esc(p.name)}" class="w-100 h-100" style="object-fit:cover;display:block;" onerror="RC.imgFail(this)">
            ${badge}
            ${p.hidden ? '<span class="position-absolute top-0 end-0 m-2 rc-badge danger">Hidden by admin</span>' : ''}
          </div>
          <div class="p-3 d-flex flex-column flex-grow-1 gap-1">
            <div class="d-flex justify-content-between gap-2">
              <h2 class="h6 mb-0 text-truncate">${RC.esc(p.name)}</h2>
              <div class="dropdown">
                <button class="btn btn-sm btn-light-soft btn-icon" data-bs-toggle="dropdown" aria-label="Actions for ${RC.esc(p.name)}"><i class="bi bi-three-dots"></i></button>
                <ul class="dropdown-menu dropdown-menu-end">${menuItems}</ul>
              </div>
            </div>
            <div class="small text-muted"><i class="bi bi-geo-alt"></i> ${RC.esc(p.location)}</div>
            <div class="small text-muted">${p.type} · ${p.bedrooms} bed · ${p.bathrooms} bath · ${p.area} sqm</div>
            <div class="fw-bold num mt-1">${RC.peso(p.price)} <span class="text-muted fw-normal small">/ month</span></div>
            <div class="d-flex justify-content-between align-items-center mt-auto pt-3 border-top gap-2 flex-wrap">
              ${isDeleted ? '<span class="small text-muted"><i class="bi bi-info-circle me-1"></i>Cannot be viewed by renters</span>' : (RC.owner_ ? RC.owner_.availSwitch(p) : '')}
              ${isDeleted ? '' : `<a href="owner-inquiries.html?property=${p.id}" class="small fw-semibold">${iq.length} inquir${iq.length === 1 ? 'y' : 'ies'}${open ? ` · <span style="color:var(--rc-primary)">${open} open</span>` : ''}</a>`}
            </div>
          </div></article></div>`;
      }).join('')}
    </div>`;
  }

  function renderEmpty() {
    const allProps = getOwnerProperties();
    return `<div class="rc-empty bg-white py-5"><i class="bi bi-houses"></i><h2 class="h5 mt-3">${allProps.length ? 'No properties in this category' : 'You haven’t listed a property yet'}</h2><p class="text-muted">${st.show === 'deleted' ? 'No recently deleted properties.' : 'Add your first listing to start receiving inquiries.'}</p>${st.show !== 'deleted' ? '<a href="owner-property-form.html" class="btn btn-primary btn-pill">Add Property</a>' : ''}</div>`;
  }

  function render() {
    const allProps = getOwnerProperties(), q = st.q.toLowerCase();
    const activeProps = allProps.filter(p => !p.isDeleted && p.status !== 'Deleted');
    const deletedProps = allProps.filter(p => p.isDeleted || p.status === 'Deleted');

    document.getElementById('pTabs').innerHTML = [
      ['all', 'All', activeProps.length],
      ['on', 'Available', activeProps.filter(p => p.available).length],
      ['off', 'Occupied', activeProps.filter(p => !p.available).length],
      ['deleted', 'Recently Deleted', deletedProps.length]
    ].map(([k, l, n]) => `<button class="rc-chip ${st.show === k ? 'active' : ''}" data-show="${k}">${l} <span class="count num">${n}</span></button>`).join('');

    // Update view toggle button active states
    document.querySelectorAll('[data-view]').forEach(btn => {
      btn.classList.toggle('active', btn.dataset.view === st.view);
    });

    let list = [];
    if (st.show === 'deleted') {
      list = deletedProps;
    } else {
      list = activeProps.filter(p => (st.show === 'all' || (st.show === 'on') === p.available));
    }

    if (q) {
      list = list.filter(p => (p.name + p.location + (p.city || '')).toLowerCase().includes(q));
    }

    const container = document.getElementById('pContainer');
    if (container) {
      container.innerHTML = st.view === 'list' ? renderList(list) : renderGrid(list);
    }
  }

  main.addEventListener('click', async e => {
    // View mode switch (List vs Grid)
    const v = e.target.closest('[data-view]');
    if (v) {
      st.view = v.dataset.view;
      localStorage.setItem('owner_prop_view', st.view);
      render();
      return;
    }

    const s = e.target.closest('[data-show]');
    if (s) { st.show = s.dataset.show; render(); return; }

    // 1. Temporary Delete (Move to Recently Deleted)
    const d = e.target.closest('[data-del]');
    if (d) {
      const propId = d.dataset.del;
      const p = (db.properties || []).find(x => String(x.id) === String(propId));
      if (!p) return;

      if (await RC.confirm({
        title: 'Move to Recently Deleted?',
        text: `“${RC.esc(p.name)}” will be moved to Recently Deleted and hidden from renters. You can restore it anytime.`,
        confirm: 'Move to Recently Deleted',
        danger: true
      })) {
        try {
          await fetch(`${API_BASE}/api/properties/${propId}`, { method: 'DELETE' });
        } catch (err) {
          console.warn('API temporary delete property failed:', err);
        }
        p.isDeleted = true;
        p.status = 'Deleted';
        RC.log('listing', RC.fullName(me), `moved ${p.name} to Recently Deleted`);
        RC.save();
        render();
        RC.toast(`“${p.name}” moved to Recently Deleted.`);
      }
      return;
    }

    // 2. Restore from Recently Deleted
    const r = e.target.closest('[data-restore]');
    if (r) {
      const propId = r.dataset.restore;
      const p = (db.properties || []).find(x => String(x.id) === String(propId));
      if (!p) return;

      if (await RC.confirm({
        title: 'Restore Property?',
        text: `“${RC.esc(p.name)}” will be returned to active listings and visible to renters again.`,
        confirm: 'Restore Property',
        danger: false
      })) {
        try {
          await fetch(`${API_BASE}/api/properties/${propId}/restore`, { method: 'POST' });
        } catch (err) {
          console.warn('API restore property failed:', err);
        }
        p.isDeleted = false;
        p.status = 'Active';
        RC.log('listing', RC.fullName(me), `restored ${p.name}`);
        RC.save();
        render();
        RC.toast(`“${p.name}” restored to active listings!`);
      }
      return;
    }

    // 3. Permanent Delete (Hard delete from database)
    const pd = e.target.closest('[data-perm-del]');
    if (pd) {
      const propId = pd.dataset.permDel || pd.getAttribute('data-perm-del');
      const p = (db.properties || []).find(x => String(x.id) === String(propId));
      if (!p) return;

      if (await RC.confirm({
        title: 'Permanently Delete Property?',
        text: `“${RC.esc(p.name)}” will be permanently removed from the database and cannot be recovered.`,
        confirm: 'Delete Permanently',
        danger: true
      })) {
        try {
          await fetch(`${API_BASE}/api/properties/${propId}/permanent`, { method: 'DELETE' });
        } catch (err) {
          console.warn('API permanent delete failed:', err);
        }
        db.properties = (db.properties || []).filter(x => String(x.id) !== String(propId));
        RC.log('listing', RC.fullName(me), `permanently deleted ${p.name}`);
        RC.save();
        render();
        RC.toast('Property permanently deleted from database.');
      }
      return;
    }
  });

  document.getElementById('pSearch').addEventListener('input', e => { st.q = e.target.value.trim(); render(); });
  document.addEventListener('rc:refresh', render);

  // Initial load from backend API
  (async () => {
    await loadServerProperties();
    render();
  })();
})();
