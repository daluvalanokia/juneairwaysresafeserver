// ── AirwaysMSS Vehicle Registry & Three.js Scene Helpers ────────────────────
// Shared by Traffic/Index and Traffic3D/Index views.
// Requires Three.js r134 loaded before this script.

var VEHICLE_REGISTRY = [
    // ── Sedans ──────────────────────────────────────────────────────────────
    { type:'sedan', make:'Toyota',   model:'Camry',      size:'medium', icon:'🚗', colors:['#c0392b','#2c3e50','#bdc3c7','#e8d5b7','#16a085'] },
    { type:'sedan', make:'Honda',    model:'Civic',      size:'small',  icon:'🚗', colors:['#3498db','#e74c3c','#2ecc71','#ecf0f1','#9b59b6'] },
    { type:'sedan', make:'Ford',     model:'Fusion',     size:'medium', icon:'🚗', colors:['#2980b9','#7f8c8d','#c0392b','#f39c12','#1a1a2e'] },
    { type:'sedan', make:'Chevrolet',model:'Malibu',     size:'medium', icon:'🚗', colors:['#d35400','#8e44ad','#16a085','#bdc3c7','#2c2c54'] },
    { type:'sedan', make:'BMW',      model:'3 Series',   size:'medium', icon:'🚗', colors:['#2c2c2c','#f5f5f5','#a0522d','#4169e1','#708090'] },
    { type:'sedan', make:'Mercedes', model:'C-Class',    size:'medium', icon:'🚗', colors:['#1a1a2e','#c0c0c0','#000080','#8b0000','#f5f5f5'] },
    // ── SUVs ────────────────────────────────────────────────────────────────
    { type:'suv', make:'Ford',       model:'Explorer',   size:'large',  icon:'🚙', colors:['#1a1a2e','#4682b4','#8b4513','#696969','#006400'] },
    { type:'suv', make:'Chevrolet',  model:'Tahoe',      size:'large',  icon:'🚙', colors:['#1c1c1c','#f5f5dc','#556b2f','#8b0000','#4169e1'] },
    { type:'suv', make:'Toyota',     model:'RAV4',       size:'medium', icon:'🚙', colors:['#cc0000','#1a1a2e','#808080','#f0f0f0','#2e8b57'] },
    { type:'suv', make:'Honda',      model:'CR-V',       size:'medium', icon:'🚙', colors:['#b22222','#708090','#2f4f4f','#ffd700','#4682b4'] },
    { type:'suv', make:'Jeep',       model:'Wrangler',   size:'medium', icon:'🚙', colors:['#ff4500','#2f4f4f','#f5f5f5','#ffd700','#1a1a2e'] },
    { type:'suv', make:'Tesla',      model:'Model X',    size:'large',  icon:'🚙', colors:['#f5f5f5','#cc0000','#1a1a2e','#808080','#000000'] },
    // ── Trucks ──────────────────────────────────────────────────────────────
    { type:'truck', make:'Ford',     model:'F-150',      size:'large',  icon:'🛻', colors:['#1a1a2e','#cc0000','#696969','#f5f5dc','#006400'] },
    { type:'truck', make:'Chevrolet',model:'Silverado',  size:'large',  icon:'🛻', colors:['#c0392b','#2c3e50','#bdc3c7','#8b4513','#1abc9c'] },
    { type:'truck', make:'Ram',      model:'1500',       size:'large',  icon:'🛻', colors:['#1a1a2e','#cc0000','#808080','#f5f5f5','#8b4513'] },
    { type:'truck', make:'Toyota',   model:'Tacoma',     size:'medium', icon:'🛻', colors:['#cc0000','#696969','#f5f5dc','#2f4f4f','#1a1a2e'] },
    { type:'truck', make:'GMC',      model:'Sierra',     size:'large',  icon:'🛻', colors:['#1a1a2e','#8b0000','#bdc3c7','#5c4033','#2e8b57'] },
    // ── Motorcycles ─────────────────────────────────────────────────────────
    { type:'motorcycle', make:'Harley-Davidson', model:'Street Glide', size:'medium', icon:'🏍', colors:['#1a1a2e','#cc0000','#f5f5f5','#ffd700','#696969'] },
    { type:'motorcycle', make:'Honda',    model:'CBR600',    size:'small',  icon:'🏍', colors:['#cc0000','#1a1a2e','#f5f5f5','#ffa500','#0000cd'] },
    { type:'motorcycle', make:'Yamaha',   model:'R1',        size:'small',  icon:'🏍', colors:['#1a1a2e','#cc0000','#696969','#0000cd','#f5f5f5'] },
    { type:'motorcycle', make:'Kawasaki', model:'Ninja 400', size:'small',  icon:'🏍', colors:['#228b22','#1a1a2e','#ff4500','#f5f5f5','#696969'] },
    { type:'motorcycle', make:'Ducati',   model:'Monster',   size:'medium', icon:'🏍', colors:['#cc0000','#1a1a2e','#f5f5f5','#ffd700','#696969'] },
    // ── Vans ────────────────────────────────────────────────────────────────
    { type:'van', make:'Toyota',   model:'Sienna',   size:'large', icon:'🚐', colors:['#f5f5f5','#696969','#cc0000','#1a1a2e','#ffd700'] },
    { type:'van', make:'Honda',    model:'Odyssey',  size:'large', icon:'🚐', colors:['#c0c0c0','#1a1a2e','#cc0000','#f5f5f5','#696969'] },
    { type:'van', make:'Ford',     model:'Transit',  size:'large', icon:'🚐', colors:['#f5f5f5','#1a1a2e','#ffd700','#cc0000','#808080'] },
    { type:'van', make:'Mercedes', model:'Sprinter', size:'large', icon:'🚐', colors:['#f5f5f5','#c0c0c0','#1a1a2e','#808080','#696969'] },
];

function vehicleGetRandom(typeFilter) {
    var pool = typeFilter
        ? VEHICLE_REGISTRY.filter(function(v) { return v.type === typeFilter; })
        : VEHICLE_REGISTRY;
    if (!pool.length) pool = VEHICLE_REGISTRY;
    return pool[Math.floor(Math.random() * pool.length)];
}

function vehicleHexInt(hex) {
    return parseInt((hex || '#888888').replace('#', ''), 16);
}

// ── Three.js mesh builders ──────────────────────────────────────────────────

function vehicleBuildMesh(vSpec) {
    var group  = new THREE.Group();
    var type   = vSpec.type || 'sedan';
    var cHex   = vSpec.colors[Math.floor(Math.random() * vSpec.colors.length)];
    var body   = new THREE.MeshLambertMaterial({ color: vehicleHexInt(cHex) });
    var dark   = new THREE.MeshLambertMaterial({ color: 0x0a0a0a });
    var glass  = new THREE.MeshLambertMaterial({ color: 0x1e3a5f, transparent: true, opacity: 0.72 });
    var chrome = new THREE.MeshLambertMaterial({ color: 0xcccccc });
    var light  = new THREE.MeshLambertMaterial({ color: 0xffffc0, emissive: 0xffffc0, emissiveIntensity: 0.35 });

    if (type === 'sedan') {
        var b  = new THREE.Mesh(new THREE.BoxGeometry(1.80, 0.42, 4.00), body);  b.position.y = 0.38;
        var cb = new THREE.Mesh(new THREE.BoxGeometry(1.52, 0.40, 2.10), body);  cb.position.set(0, 0.80, -0.15);
        var fg = new THREE.Mesh(new THREE.BoxGeometry(1.50, 0.38, 0.06), glass); fg.position.set(0, 0.80,  0.93); fg.rotation.x =  0.28;
        var rg = new THREE.Mesh(new THREE.BoxGeometry(1.50, 0.38, 0.06), glass); rg.position.set(0, 0.80, -1.23); rg.rotation.x = -0.28;
        var sl = new THREE.Mesh(new THREE.BoxGeometry(0.05, 0.32, 1.80), glass); sl.position.set(-0.755, 0.82, -0.10);
        var sr = sl.clone(); sr.position.x = 0.755;
        var hl = new THREE.Mesh(new THREE.BoxGeometry(0.28, 0.12, 0.06), light); hl.position.set(-0.58, 0.38,  2.03);
        var hr = hl.clone(); hr.position.x = 0.58;
        var gr = new THREE.Mesh(new THREE.BoxGeometry(0.90, 0.18, 0.04), dark);  gr.position.set(0, 0.25, 2.03);
        _vWheels(group, dark, chrome, 1.86, 0.28, 1.35);
        group.add(b, cb, fg, rg, sl, sr, hl, hr, gr);

    } else if (type === 'suv') {
        var b  = new THREE.Mesh(new THREE.BoxGeometry(2.00, 0.55, 4.50), body);  b.position.y = 0.48;
        var tp = new THREE.Mesh(new THREE.BoxGeometry(1.88, 0.58, 3.40), body);  tp.position.set(0, 1.02, -0.20);
        var fg = new THREE.Mesh(new THREE.BoxGeometry(1.85, 0.55, 0.06), glass); fg.position.set(0, 1.00,  1.53); fg.rotation.x =  0.22;
        var rg = new THREE.Mesh(new THREE.BoxGeometry(1.85, 0.55, 0.06), glass); rg.position.set(0, 1.00, -1.92); rg.rotation.x = -0.22;
        var sl = new THREE.Mesh(new THREE.BoxGeometry(0.05, 0.50, 2.90), glass); sl.position.set(-0.935, 1.02, -0.18);
        var sr = sl.clone(); sr.position.x = 0.935;
        var rl = new THREE.Mesh(new THREE.BoxGeometry(0.06, 0.06, 3.20), chrome); rl.position.set(-0.82, 1.35, -0.10);
        var rr = rl.clone(); rr.position.x = 0.82;
        var hl = new THREE.Mesh(new THREE.BoxGeometry(0.36, 0.18, 0.06), light); hl.position.set(-0.68, 0.55, 2.28);
        var hr = hl.clone(); hr.position.x = 0.68;
        _vWheels(group, dark, chrome, 2.06, 0.35, 1.55);
        group.add(b, tp, fg, rg, sl, sr, rl, rr, hl, hr);

    } else if (type === 'truck') {
        var cab  = new THREE.Mesh(new THREE.BoxGeometry(2.00, 1.10, 2.10), body); cab.position.set(0, 0.72, 1.35);
        var roof = new THREE.Mesh(new THREE.BoxGeometry(1.88, 0.52, 1.90), body); roof.position.set(0, 1.38, 1.35);
        var ws   = new THREE.Mesh(new THREE.BoxGeometry(1.85, 0.48, 0.06), glass); ws.position.set(0, 1.35, 2.34); ws.rotation.x = 0.22;
        var bf   = new THREE.Mesh(new THREE.BoxGeometry(1.96, 0.10, 2.80), body); bf.position.set(0, 0.22, -1.05);
        var bl   = new THREE.Mesh(new THREE.BoxGeometry(0.10, 0.45, 2.80), body); bl.position.set(-0.93, 0.49, -1.05);
        var br   = bl.clone(); br.position.x = 0.93;
        var bw   = new THREE.Mesh(new THREE.BoxGeometry(1.96, 0.55, 0.10), body); bw.position.set(0, 0.49, 0.25);
        var hl   = new THREE.Mesh(new THREE.BoxGeometry(0.30, 0.18, 0.06), light); hl.position.set(-0.70, 0.72, 2.44);
        var hr   = hl.clone(); hr.position.x = 0.70;
        _vWheels(group, dark, chrome, 2.06, 0.35, 1.75);
        group.add(cab, roof, ws, bf, bl, br, bw, hl, hr);

    } else if (type === 'motorcycle') {
        var bd  = new THREE.Mesh(new THREE.BoxGeometry(0.50, 0.45, 1.90), body);  bd.position.y = 0.62;
        var fr  = new THREE.Mesh(new THREE.BoxGeometry(0.46, 0.32, 0.50), body);  fr.position.set(0, 0.85, 0.85);
        var st  = new THREE.Mesh(new THREE.BoxGeometry(0.36, 0.10, 0.80), dark);  st.position.set(0, 0.90, -0.10);
        var hb  = new THREE.Mesh(new THREE.BoxGeometry(0.72, 0.05, 0.05), chrome); hb.position.set(0, 1.00, 0.60);
        var wg  = new THREE.CylinderGeometry(0.30, 0.30, 0.14, 10);
        var wf  = new THREE.Mesh(wg, dark); wf.rotation.z = Math.PI/2; wf.position.set(0, 0.32,  0.75);
        var wr  = new THREE.Mesh(wg, dark); wr.rotation.z = Math.PI/2; wr.position.set(0, 0.32, -0.75);
        var ex  = new THREE.Mesh(new THREE.CylinderGeometry(0.04, 0.05, 0.70, 6), chrome);
        ex.rotation.z = Math.PI/2; ex.position.set(0.28, 0.42, -0.50);
        group.add(bd, fr, st, hb, wf, wr, ex);

    } else if (type === 'van') {
        var bd  = new THREE.Mesh(new THREE.BoxGeometry(2.10, 1.65, 5.00), body); bd.position.y = 0.97;
        var fg  = new THREE.Mesh(new THREE.BoxGeometry(2.05, 0.80, 0.06), glass); fg.position.set(0, 1.20, 2.52); fg.rotation.x = 0.12;
        var sl  = new THREE.Mesh(new THREE.BoxGeometry(0.05, 0.50, 2.00), glass); sl.position.set(-1.03, 1.20, -0.50);
        var sr  = sl.clone(); sr.position.x = 1.03;
        var st  = new THREE.Mesh(new THREE.BoxGeometry(2.12, 0.22, 5.02), new THREE.MeshLambertMaterial({ color: 0x111122 })); st.position.y = 1.65;
        var hl  = new THREE.Mesh(new THREE.BoxGeometry(0.40, 0.20, 0.06), light); hl.position.set(-0.72, 0.90, 2.53);
        var hr  = hl.clone(); hr.position.x = 0.72;
        _vWheels(group, dark, chrome, 2.16, 0.38, 1.75);
        group.add(bd, fg, sl, sr, st, hl, hr);

    } else {
        var b = new THREE.Mesh(new THREE.BoxGeometry(1.8, 0.75, 3.2), body); b.position.y = 0.375;
        _vWheels(group, dark, chrome, 1.86, 0.28, 1.20);
        group.add(b);
    }

    group.traverse(function(c) { if (c.isMesh) c.castShadow = true; });
    return group;
}

function _vWheels(group, tireMat, rimMat, bW, r, wb) {
    var tg = new THREE.CylinderGeometry(r, r, 0.22, 12);
    var rg = new THREE.CylinderGeometry(r * 0.55, r * 0.55, 0.24, 8);
    [ [bW/2+0.02, wb], [-(bW/2+0.02), wb], [bW/2+0.02, -wb], [-(bW/2+0.02), -wb] ]
    .forEach(function(p) {
        var t  = new THREE.Mesh(tg, tireMat); t.rotation.z  = Math.PI/2; t.position.set(p[0], r, p[1]);
        var ri = new THREE.Mesh(rg, rimMat);  ri.rotation.z = Math.PI/2; ri.position.set(p[0], r, p[1]);
        group.add(t, ri);
    });
}

var FEED_EVENT_COLORS = {
    detection:'#22c55e', merge:'#3b82f6', speeding:'#f59e0b', conflict:'#ef4444', fault:'#a855f7'
};
