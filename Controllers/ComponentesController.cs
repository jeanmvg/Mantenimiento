using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MantenimientoIndustrial.Data;
using MantenimientoIndustrial.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OfficeOpenXml;

namespace MantenimientoIndustrial.Controllers
{
    public class ComponentesController : Controller
    {
        private readonly AppDbContext _context;

        public ComponentesController(AppDbContext context)
        {
            _context = context;
        }

        // Listar componentes
        public async Task<IActionResult> Index()
        {
            var componentes = await _context.Componentes.ToListAsync();
            return View(componentes);
        }

        // Crear componente (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Crear componente (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Componente componente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(componente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(componente);
        }

        // Editar componente (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componentes.FindAsync(id);
            if (componente == null)
            {
                return NotFound();
            }
            return View(componente);
        }

        // Editar componente (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComponenteID,Nombre,Cantidad,EquipoID,Estado,Observaciones")] Componente componente)
        {
            if (id != componente.ComponenteID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(componente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComponenteExists(componente.ComponenteID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(componente);
        }

        // Eliminar componente (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componentes
                .FirstOrDefaultAsync(c => c.ComponenteID == id);
            if (componente == null)
            {
                return NotFound();
            }
            return View(componente);
        }

        // Eliminar componente (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var componente = await _context.Componentes.FindAsync(id);
            if (componente != null)
            {
                _context.Componentes.Remove(componente);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Importar Componentes (POST)
        [HttpPost]
        public async Task<IActionResult> ImportarDesdeExcel(IFormFile archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                ModelState.AddModelError("", "Por favor, selecciona un archivo.");
                return RedirectToAction(nameof(Index));
            }

            using (var stream = new System.IO.MemoryStream())
            {
                await archivoExcel.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Obtener la primera hoja
                    int rowCount = worksheet.Dimension.Rows;

                    var componentes = new List<Componente>();

                    // Leer los datos del archivo Excel (se asume que la fila 1 tiene encabezados)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var nombre = worksheet.Cells[row, 1].Text;
                        var cantidadStr = worksheet.Cells[row, 2].Text;

                        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(cantidadStr))
                            continue;

                        if (int.TryParse(cantidadStr, out int cantidad))
                        {
                            var componente = new Componente
                            {
                                Nombre = nombre,
                                Cantidad = cantidad
                            };
                            componentes.Add(componente);
                        }
                    }

                    // Guardar los componentes en la base de datos
                    _context.Componentes.AddRange(componentes);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Obtener componentes disponibles (retorna JSON)
        [HttpGet]
        public async Task<JsonResult> ObtenerComponentes()
        {
            var componentes = await _context.Componentes
                .Select(c => new { c.ComponenteID, c.Nombre, c.Cantidad })
                .ToListAsync();

            return Json(componentes);
        }

        // Método helper para verificar existencia
        private bool ComponenteExists(int id)
        {
            return _context.Componentes.Any(e => e.ComponenteID == id);
        }
    }
}
