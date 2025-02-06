$(document).ready(function () {
    $('#dataTable').DataTable({
        paging: true, // Habilitar paginación
        searching: true, // Habilitar búsqueda
        ordering: true, // Habilitar ordenamiento
        lengthChange: true, // Permitir cambiar el número de filas
        pageLength: 10, // Número de filas por defecto
        dom: 'Bfrtip', // Controles de exportación
        buttons: [
            'copy', 'excel', 'csv', 'pdf'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.1/i18n/es-ES.json" // Traducción al español
        }
    });
});