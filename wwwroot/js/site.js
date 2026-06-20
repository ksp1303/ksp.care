// KSP Care — Shared site JavaScript

document.addEventListener('DOMContentLoaded', function() {

    // ── Form loading states ──────────────────────────────────
    // Disable submit buttons after click to prevent double submissions
    document.querySelectorAll('form').forEach(function(form) {
        form.addEventListener('submit', function() {
            var btn = form.querySelector('button[type="submit"]');
            if (btn && !btn.disabled) {
                btn.disabled = true;
                btn.style.opacity = '0.6';
                btn.style.cursor = 'wait';
                var originalHTML = btn.innerHTML;
                btn.innerHTML = '<i class="bi bi-arrow-repeat animate-spin"></i> ' + originalHTML;
                // Re-enable after 5s (safety net for failed requests)
                setTimeout(function() {
                    btn.disabled = false;
                    btn.style.opacity = '1';
                    btn.style.cursor = 'pointer';
                    btn.innerHTML = originalHTML;
                }, 5000);
            }
        });
    });

    // ── Auto-dismiss alerts after 4 seconds ──────────────────
    var alerts = document.querySelectorAll('.auto-dismiss');
    alerts.forEach(function(el) {
        setTimeout(function() {
            el.style.transition = 'opacity 0.5s, transform 0.5s';
            el.style.opacity = '0';
            el.style.transform = 'translateY(-10px)';
            setTimeout(function() { el.remove(); }, 500);
        }, 4000);
    });
});
