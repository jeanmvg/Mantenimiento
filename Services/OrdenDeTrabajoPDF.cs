using MantenimientoIndustrial.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace MantenimientoIndustrial.Services
{
    public class OrdenDeTrabajoPDF : IDocument
    {
        private readonly List<Componente> _componentes;
        private readonly string _descripcion;
        private readonly string _ubicacion;
        private readonly string _marca;
        private readonly string _rutaImagen;

        public OrdenDeTrabajoPDF(string descripcion, string ubicacion, string marca, string rutaImagen, List<Componente> componentes)
        {
            _descripcion = descripcion;
            _ubicacion = ubicacion;
            _marca = marca;
            _rutaImagen = rutaImagen;
            _componentes = componentes;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);

                page.Header().Text("Orden de Trabajo").FontSize(20).Bold().AlignCenter();

                page.Content().Stack(stack =>
                {
                stack.Spacing(10);

                stack.Item().Text($"Descripción: {_descripcion}");
                stack.Item().Text($"Ubicación: {_ubicacion}");
                stack.Item().Text($"Marca: {_marca}");

                // Agregar imagen (asegúrate de usar una ruta válida)
                stack.Item().Text("Texto");
                    stack.Item().Element(imagenPath =>
                    {
                        if (File.Exists("ruta/a/tu/imagen.jpg")) // Verifica si la imagen existe
                        {
                            imagenPath
                                .Height(150) // Define la altura máxima del contenedor
                                .Width(200)  // Define el ancho máximo del contenedor
                                .Image("ruta/a/tu/imagen.jpg");
                        }
                        else
                        {
                            imagenPath.AlignCenter().AlignMiddle()
                                      .Text("No hay imagen disponible")
                                      .FontSize(14)
                                      .Italic()
                                      .FontColor(Colors.Grey.Medium);
                        }
                    });

                    int itemIndex = 1; // Contador para los elementos
                    stack.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // Item
                            columns.RelativeColumn(); // Cantidad
                            columns.RelativeColumn(); // Componente
                            columns.RelativeColumn(); // Estado
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Item").Bold();
                            header.Cell().Text("Cantidad").Bold();
                            header.Cell().Text("Componente").Bold();
                            header.Cell().Text("Estado").Bold();
                        });

                        foreach (var componente in _componentes)
                        {
                            table.Cell().Text(itemIndex.ToString());
                            table.Cell().Text(componente.Cantidad.ToString());
                            table.Cell().Text(componente.Nombre);
                            table.Cell().Text(componente.Estado ?? "Bueno"); // Evita valores nulos
                            itemIndex++;
                        }
                    });
                });
            });
        }
    }
}
