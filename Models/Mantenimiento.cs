using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MantenimientoIndustrial.Models
{
    public class Mantenimiento
    {
        public int MantenimientoID { get; set; }
        public int EquipoID { get; set; }
        public DateTime FechaInicial { get; set; } // Fecha del primer mantenimiento
        public string Frecuencia { get; set; } // Frecuencia: Mensual, Anual, etc.
        public DateTime FechaProgramada { get; set; }
        public DateTime? FechaRealizada { get; set; } // Nullable si puede no tener valor
        public string Descripcion { get; set; }
        public decimal Costo { get; set; }
        public string Estado { get; set; }

        // Relación con Equipos
        public Equipo Equipo { get; set; }

    }
}
