using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MantenimientoIndustrial.Models
{
    public class Componente
    {
        public int ComponenteID { get; set; } // Clave primaria
        public string Nombre { get; set; } = string.Empty; // Nombre del componente
        public int Cantidad { get; set; }
        // public string Descripcion { get; set; } = string.Empty; // Opcional, detalles adicionales
        public string Estado { get; set; } = "Bueno"; // Estado del componente asignado
        public string Observaciones { get; set; } = string.Empty; // Observaciones del componente

        // Relación con el equipo (opcional si es un componente general)
        public int? EquipoID { get; set; }
        public Equipo? Equipo { get; set; }
    }
}