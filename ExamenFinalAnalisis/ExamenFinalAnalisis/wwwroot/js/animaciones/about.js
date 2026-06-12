// Animación de entrada para los bloques de About
document.addEventListener("DOMContentLoaded", function () {
    const blocks = document.querySelectorAll(".about-block");
    blocks.forEach((block, i) => {
        block.style.opacity = "0";
        block.style.transform = "translateY(20px)";
        block.style.transition = `opacity 0.4s ease ${i * 0.15}s, transform 0.4s ease ${i * 0.15}s`;
        setTimeout(() => {
            block.style.opacity = "1";
            block.style.transform = "translateY(0)";
        }, 100);
    });
});