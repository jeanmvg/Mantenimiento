using MantenimientoIndustrial.Data;
using MantenimientoIndustrial.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MantenimientoIndustrial.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context; // Declaración del campo _context

        // Inyección del contexto en el constructor
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            var totalEquipos = _context.Equipos.Count();
            var totalMantenimientos = _context.Mantenimientos.Count();
            var mantenimientosPendientes = _context.Mantenimientos.Count(m => m.Estado == "Pendiente");
            var alertasActivas = _context.Alertas.Count();

            var dashboardData = new DashboardViewModel
            {
                TotalEquipos = totalEquipos,
                TotalMantenimientos = totalMantenimientos,
                MantenimientosPendientes = mantenimientosPendientes,
                AlertasActivas = alertasActivas,
                Mantenimientos = _context.Mantenimientos
                    .Include(m => m.Equipo)
                    .ToList()
            };

            return View(dashboardData);
        }
    }
}