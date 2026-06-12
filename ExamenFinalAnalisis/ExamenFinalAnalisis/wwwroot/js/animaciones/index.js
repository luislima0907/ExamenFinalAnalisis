// Scroll suave al hacer click en el botón "Conocer más"
document.addEventListener("DOMContentLoaded", function () {
    const btn = document.querySelector(".hero .btn-primary");
    if (btn) {
        btn.addEventListener("click", function (e) {
            e.preventDefault();
            const target = document.querySelector("#info");
            if (target) {
                target.scrollIntoView({ behavior: "smooth" });
            }
        });
    }

    // Animación de entrada para las cards
    const cards = document.querySelectorAll(".card");
    cards.forEach((card, i) => {
        card.style.opacity = "0";
        card.style.transform = "translateY(20px)";
        card.style.transition = `opacity 0.4s ease ${i * 0.1}s, transform 0.4s ease ${i * 0.1}s`;
        setTimeout(() => {
            card.style.opacity = "1";
            card.style.transform = "translateY(0)";
        }, 100);
    });
});