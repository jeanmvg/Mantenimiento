using System;
using System.ComponentModel.DataAnnotations;
namespace MantenimientoIndustrial.Models
{
    public class Equipo
    {
        public int EquipoID { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(8, ErrorMessage = "El código no debe exceder los 8 caracteres.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no debe exceder los 200 caracteres.")]
        public string Nombre { get; set; }

        public string Ubicacion { get; set; }
        public string Marca { get; set; }
        public string? Modelo { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaIngreso { get; set; }
        public string Estado { get; set; }
        public string FotoRuta { get; set; }
        // Relación con Componentes
        public ICollection<Componente> Componentes { get; set; } 
        public Equipo()
        {
            Componentes = new List<Componente>();
        }
    }
}
