// Función para abrir WhatsApp
window.openWhatsAppWindow = function(url) {
    console.log("Abriendo WhatsApp con URL:", url);
    window.open(url, '_blank');
    return true;
};
