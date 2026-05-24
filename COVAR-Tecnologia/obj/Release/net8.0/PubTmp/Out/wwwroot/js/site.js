// Lógica para Modo Oscuro/Claro
document.addEventListener("DOMContentLoaded", function () {
    const themeToggleBtn = document.getElementById('theme-toggle');
    const themeIcon = document.getElementById('theme-icon');
    const htmlElement = document.documentElement;

    // Inicializar icono según el tema actual
    const currentTheme = htmlElement.getAttribute('data-bs-theme');
    if (currentTheme === 'dark') {
        themeIcon.classList.replace('bi-sun-fill', 'bi-moon-stars-fill');
        themeIcon.parentElement.classList.replace('text-warning', 'text-light');
    }

    themeToggleBtn.addEventListener('click', function () {
        const currentTheme = htmlElement.getAttribute('data-bs-theme');
        let newTheme = 'light';

        if (currentTheme === 'light') {
            newTheme = 'dark';
            themeIcon.classList.replace('bi-sun-fill', 'bi-moon-stars-fill');
            themeToggleBtn.classList.replace('text-warning', 'text-light');
        } else {
            themeIcon.classList.replace('bi-moon-stars-fill', 'bi-sun-fill');
            themeToggleBtn.classList.replace('text-light', 'text-warning');
        }

        htmlElement.setAttribute('data-bs-theme', newTheme);
        localStorage.setItem('theme', newTheme);
    });
});
// Función para alternar visibilidad de contraseña
function togglePassword(button) {
    const input = button.previousElementSibling;
    const icon = button.querySelector('i');
    
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('bi-eye');
        icon.classList.add('bi-eye-slash');
    } else {
        input.type = 'password';
        icon.classList.remove('bi-eye-slash');
        icon.classList.add('bi-eye');
    }
}
