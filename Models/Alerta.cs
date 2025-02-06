namespace MantenimientoIndustrial.Models
{
    public class Alerta
    {
        public int AlertaID { get; set; } // Llave primaria
        public int MantenimientoID { get; set; } // Llave foránea
        public Mantenimiento Mantenimiento { get; set; }  
        public DateTime FechaAlerta { get; set; }
        public string Mensaje { get; set; } = "Detalle de la alerta"; // Valor por defecto
        public string Estado { get; set; } = "Pendiente"; // Valor por defecto
    }
}
