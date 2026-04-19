// ============================================
// EcoCycle – Lógica página Aprender
// asp_presentacion/wwwroot/js/aprender.js
// ============================================

/**
 * Filtra las cards de artículos por categoría.
 * @param {HTMLElement} btn   - Botón de filtro clickeado
 * @param {string}      cat   - Categoría: 'todo' | 'basicos' | 'avanzado' | 'tips' | 'impacto'
 */
function filtrar(btn, cat) {
    // Actualizar estado activo de los botones
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    const cards = document.querySelectorAll('.article-card');
    const featured = document.querySelector('.featured-card');

    cards.forEach(card => {
        const match = cat === 'todo' || card.dataset.cat === cat;
        card.style.display = match ? '' : 'none';
    });

    // La card destacada ocupa toda la fila solo en "Todo"
    if (featured) {
        featured.style.gridColumn = cat === 'todo' ? '1 / -1' : '';
    }
}