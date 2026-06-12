// Marca el link activo en el navbar según la URL actual
document.addEventListener("DOMContentLoaded", function () {
    const links = document.querySelectorAll(".nav-links a");
    const currentPath = window.location.pathname.toLowerCase();

    links.forEach(link => {
        const href = link.getAttribute("href").toLowerCase();
        if (href === currentPath || (currentPath === "/" && href.includes("index"))) {
            link.style.color = "#e94560";
            link.style.fontWeight = "600";
        }
    });
});