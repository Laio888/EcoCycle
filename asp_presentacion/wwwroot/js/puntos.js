// ============================================
// EcoCycle – Lógica página Puntos y Ranking
// asp_presentacion/wwwroot/js/puntos.js
// ============================================

/**
 * Cambia el período activo del ranking (Este mes / Global).
 * Aquí puedes agregar una llamada fetch() al backend cuando
 * tengas el endpoint listo.
 * @param {HTMLElement} btn - Tab clickeado
 */
function switchPeriod(btn) {
    document.querySelectorAll('.period-tab').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    // TODO: cuando el backend esté listo, cargar datos por período:
    // const periodo = btn.textContent.trim(); // "Este mes" | "Global"
    // cargarRanking(periodo);
}

/**
 * Ejemplo de carga dinámica del ranking desde el backend.
 * Descomentar cuando el endpoint esté disponible.
 */
// async function cargarRanking(periodo) {
//     const res  = await fetch(`/api/ranking?periodo=${encodeURIComponent(periodo)}`);
//     const data = await res.json();
//     renderizarRanking(data);
// }