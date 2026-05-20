// EyewaysMergeSafeServer — site.js

document.addEventListener('DOMContentLoaded', function () {
    var toggle  = document.getElementById('sidebarToggle');
    var sidebar = document.querySelector('.mss-sidebar');
    var main    = document.querySelector('.mss-main');

    if (toggle && sidebar) {
        // Restore persisted state
        if (localStorage.getItem('sidebarCollapsed') === '1' && window.innerWidth >= 768) {
            sidebar.classList.add('collapsed');
            if (main) main.classList.add('sidebar-collapsed');
        }

        toggle.addEventListener('click', function () {
            if (window.innerWidth >= 768) {
                var isCollapsed = sidebar.classList.toggle('collapsed');
                if (main) main.classList.toggle('sidebar-collapsed', isCollapsed);
                localStorage.setItem('sidebarCollapsed', isCollapsed ? '1' : '0');
            } else {
                sidebar.classList.toggle('open');
            }
        });

        document.addEventListener('click', function (e) {
            if (window.innerWidth < 768 && sidebar.classList.contains('open')) {
                if (!sidebar.contains(e.target) && e.target !== toggle) {
                    sidebar.classList.remove('open');
                }
            }
        });
    }

    // Auto-dismiss success alerts after 4 s
    setTimeout(function () {
        document.querySelectorAll('.alert.alert-success').forEach(function (el) {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert.close();
        });
    }, 4000);

    // Bootstrap tooltips
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        new bootstrap.Tooltip(el);
    });
});

function startLiveClock(elementId) {
    function tick() {
        var el = document.getElementById(elementId);
        if (!el) return;
        el.textContent = new Date().toLocaleTimeString();
    }
    tick();
    setInterval(tick, 1000);
}
