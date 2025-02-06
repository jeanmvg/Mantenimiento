using System.Collections.Generic;
using MantenimientoIndustrial.Models;

namespace MantenimientoIndustrial.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEquipos { get; set; }
        public int TotalMantenimientos { get; set; }
        public int MantenimientosPendientes { get; set; }
        public int AlertasActivas { get; set; }
        public List<Mantenimiento> Mantenimientos { get; set; }
    }
}
