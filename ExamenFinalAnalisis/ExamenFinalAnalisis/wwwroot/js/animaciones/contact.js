// Validación y simulación de envío del formulario de contacto
document.addEventListener("DOMContentLoaded", function () {
    const btn = document.getElementById("btn-enviar");
    const successMsg = document.getElementById("form-success");

    btn.addEventListener("click", function () {
        const nombre = document.getElementById("nombre").value.trim();
        const email = document.getElementById("email").value.trim();
        const mensaje = document.getElementById("mensaje").value.trim();

        if (!nombre || !email || !mensaje) {
            alert("Por favor, completa todos los campos.");
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            alert("Por favor, ingresa un correo electrónico válido.");
            return;
        }

        // Simula envío exitoso
        successMsg.style.display = "block";
        document.getElementById("nombre").value = "";
        document.getElementById("email").value = "";
        document.getElementById("mensaje").value = "";

        setTimeout(() => {
            successMsg.style.display = "none";
        }, 4000);
    });
});