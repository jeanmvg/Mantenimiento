using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MantenimientoIndustrial.Data;
using MantenimientoIndustrial.Models;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using QuestPDF.Helpers;
using MantenimientoIndustrial.Services;

namespace MantenimientoIndustrial.Controllers
{
    public class MantenimientosController : Controller
    {
        private readonly AppDbContext _context;

        public MantenimientosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Mantenimientos
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Mantenimientos.Include(m => m.Equipo);
            return View(await appDbContext.ToListAsync());
        }
        // GET: Importar Mantenimientos
        public IActionResult Importar()
        {
            return View();
        }

        // POST: Importar Mantenimientos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Importar(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un archivo.");
                return View();
            }

            if (!archivo.FileName.EndsWith(".xlsx"))
            {
                ModelState.AddModelError(string.Empty, "El archivo debe ser un archivo Excel con extensión .xlsx.");
                return View();
            }

            try
            {
                var mantenimientos = new List<Mantenimiento>();

                using (var stream = new MemoryStream())
                {
                    await archivo.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Saltar encabezados
                        {
                            var mantenimiento = new Mantenimiento
                            {
                                EquipoID = int.Parse(worksheet.Cells[row, 1].Text.Trim()),
                                FechaInicial = DateTime.Parse(worksheet.Cells[row, 2].Text.Trim()),
                                Frecuencia = worksheet.Cells[row, 3].Text.Trim(),
                                Descripcion = worksheet.Cells[row, 4].Text.Trim(),
                                Costo = decimal.Parse(worksheet.Cells[row, 5].Text.Trim()),
                                Estado = worksheet.Cells[row, 6].Text.Trim()
                            };

                            // Calcular la fecha programada basada en la frecuencia
                            switch (mantenimiento.Frecuencia)
                            {
                                case "Mensual":
                                    mantenimiento.FechaProgramada = mantenimiento.FechaInicial.AddMonths(1);
                                    break;
                                case "Bimestral":
                                    mantenimiento.FechaProgramada = mantenimiento.FechaInicial.AddMonths(2);
                                    break;
                                case "Trimestral":
                                    mantenimiento.FechaProgramada = mantenimiento.FechaInicial.AddMonths(3);
                                    break;
                                case "Anual":
                                    mantenimiento.FechaProgramada = mantenimiento.FechaInicial.AddYears(1);
                                    break;
                                default:
                                    ModelState.AddModelError(string.Empty, $"Frecuencia no válida en la fila {row}.");
                                    return View();
                            }

                            mantenimientos.Add(mantenimiento);
                        }
                    }
                }

                _context.Mantenimientos.AddRange(mantenimientos);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Importación exitosa.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al procesar el archivo: {ex.Message}");
                return View();
            }
        }
        
        // GET: Mantenimientos/Create
        public IActionResult Create()
        {
            ViewData["EquipoID"] = new SelectList(_context.Equipos, "EquipoID", "Nombre");
            return View();
        }

        // POST: Mantenimientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipoID,FechaProgramada,FechaRealizada,Descripcion,Costo,Estado")] Mantenimiento mantenimiento)
        {
            try
            {
                Console.WriteLine("POST Create ejecutado");

                var nuevoMantenimiento = new Mantenimiento
                {
                    EquipoID = mantenimiento.EquipoID,
                    FechaProgramada = mantenimiento.FechaProgramada,
                    FechaRealizada = mantenimiento.FechaRealizada,
                    Descripcion = mantenimiento.Descripcion,
                    Costo = mantenimiento.Costo,
                    Estado = mantenimiento.Estado
                };

                _context.Add(nuevoMantenimiento);
                await _context.SaveChangesAsync();
                Console.WriteLine("Mantenimiento guardado correctamente");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante SaveChangesAsync: {ex.Message}");
                ViewData["EquipoID"] = new SelectList(_context.Equipos, "EquipoID", "Nombre", mantenimiento.EquipoID);
                return View(mantenimiento);
            }
        }


        // GET: Mantenimientos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.Equipo)
                .FirstOrDefaultAsync(m => m.MantenimientoID == id);
            if (mantenimiento == null)
            {
                return NotFound();
            }

            return View(mantenimiento);
        }



        // GET: Mantenimientos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null)
            {
                return NotFound();
            }

            ViewData["EquipoID"] = new SelectList(_context.Equipos, "EquipoID", "Nombre", mantenimiento.EquipoID);
            return View(mantenimiento);
        }

        // POST: Mantenimientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MantenimientoID,EquipoID,FechaProgramada,FechaRealizada,Descripcion,Costo,Estado")] Mantenimiento mantenimiento)
        {
            if (id != mantenimiento.MantenimientoID)
            {
                return NotFound();
            }

            try
            {
                Console.WriteLine("POST Edit ejecutado");

                // Actualizar los campos directamente
                var mantenimientoExistente = await _context.Mantenimientos.FindAsync(id);
                if (mantenimientoExistente == null)
                {
                    return NotFound();
                }

                mantenimientoExistente.EquipoID = mantenimiento.EquipoID;
                mantenimientoExistente.FechaProgramada = mantenimiento.FechaProgramada;
                mantenimientoExistente.FechaRealizada = mantenimiento.FechaRealizada;
                mantenimientoExistente.Descripcion = mantenimiento.Descripcion;
                mantenimientoExistente.Costo = mantenimiento.Costo;
                mantenimientoExistente.Estado = mantenimiento.Estado;

                await _context.SaveChangesAsync();
                Console.WriteLine("Mantenimiento actualizado correctamente");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la actualización: {ex.Message}");
                ViewData["EquipoID"] = new SelectList(_context.Equipos, "EquipoID", "Nombre", mantenimiento.EquipoID);
                return View(mantenimiento);
            }
        }

        // GET: Mantenimientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mantenimiento = await _context.Mantenimientos
                .Include(m => m.Equipo)
                .FirstOrDefaultAsync(m => m.MantenimientoID == id);
            if (mantenimiento == null)
            {
                return NotFound();
            }

            return View(mantenimiento);
        }

        // POST: Mantenimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var mantenimiento = await _context.Mantenimientos.FindAsync(id);
                if (mantenimiento != null)
                {
                    _context.Mantenimientos.Remove(mantenimiento);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar mantenimiento: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al eliminar el mantenimiento.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MantenimientoExists(int id)
        {
            return _context.Mantenimientos.Any(e => e.MantenimientoID == id);
        }
    }
}
