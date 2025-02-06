using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MantenimientoIndustrial.Data;
using MantenimientoIndustrial.Models;
using OfficeOpenXml;

namespace MantenimientoIndustrial.Controllers
{
    public class EquiposController : Controller
    {
        private readonly AppDbContext _context;

        public EquiposController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Equipos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Equipos.ToListAsync());
        }

        // GET: Importar
        public IActionResult Importar()
        {
            return View();
        }

        // POST: Importar Equipos
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
                var equipos = new List<Equipo>();

                using (var stream = new MemoryStream())
                {
                    await archivo.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        if (package.Workbook.Worksheets.Count == 0)
                        {
                            ModelState.AddModelError(string.Empty, "El archivo Excel no contiene hojas.");
                            return View();
                        }

                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var equipo = new Equipo
                            {
                                Codigo = worksheet.Cells[row, 1].Text.Trim(),
                                Nombre = worksheet.Cells[row, 2].Text.Trim(),
                                Ubicacion = worksheet.Cells[row, 3].Text.Trim(),
                                Marca = worksheet.Cells[row, 4].Text.Trim(),
                                Modelo = worksheet.Cells[row, 5].Text.Trim(),
                                FechaIngreso = DateTime.Parse(worksheet.Cells[row, 6].Text.Trim()),
                                Estado = worksheet.Cells[row, 7].Text.Trim()
                            };

                            equipos.Add(equipo);
                        }
                    }
                }

                _context.Equipos.AddRange(equipos);
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

        // GET: Equipos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Equipos
                .Include(e => e.Componentes)
                .FirstOrDefaultAsync(m => m.EquipoID == id);
            if (equipo == null)
            {
                return NotFound();
            }

            return View(equipo);
        }

        // GET: Equipos/Create
        public IActionResult Create()
        {
            var equipo = new Equipo
            {
                Componentes = new List<Componente>()
            };

            ViewBag.Componentes = _context.Componentes
                .Where(c => c.EquipoID == null)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.ComponenteID.ToString(),
                    Text = c.Nombre
                }).ToList();

            return View(equipo);
        }

        // POST: Equipos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Ubicacion,Marca,Modelo,FechaIngreso,Estado")] Equipo equipo, int[] componentesSeleccionados, int[] cantidades, IFormFile Foto)
        {
            if (ModelState.IsValid)
            {
                // Guardar la foto
                if (Foto != null && Foto.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Foto.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Foto.CopyToAsync(fileStream);
                    }

                    equipo.FotoRuta = "/images/" + uniqueFileName;
                }

                _context.Equipos.Add(equipo);
                await _context.SaveChangesAsync();

                // Asignar componentes seleccionados
                for (int i = 0; i < componentesSeleccionados.Length; i++)
                {
                    var componente = _context.Componentes.FirstOrDefault(c => c.ComponenteID == componentesSeleccionados[i]);
                    if (componente != null)
                    {
                        componente.EquipoID = equipo.EquipoID;
                        componente.Cantidad = cantidades[i];
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Componentes = _context.Componentes
                .Where(c => c.EquipoID == null)
                .Select(c => new SelectListItem
                {
                    Value = c.ComponenteID.ToString(),
                    Text = c.Nombre
                }).ToList();

            return View(equipo);
        }

        // GET: Equipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Equipos
                .Include(e => e.Componentes) // Incluye componentes relacionados
                .FirstOrDefaultAsync(e => e.EquipoID == id);

            if (equipo == null)
            {
                return NotFound();
            }

            // Cargar lista de componentes disponibles
            ViewBag.Componentes = _context.Componentes
                .Where(c => c.EquipoID == null || c.EquipoID == equipo.EquipoID)
                .OrderBy(c => c.Nombre) // Ordenar alfabéticamente
                .Select(c => new SelectListItem
                {
                    Value = c.ComponenteID.ToString(),
                    Text = c.Nombre
                }).ToList();

            return View(equipo);
        }

        // POST: Equipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipoID,Codigo,Nombre,Ubicacion,Marca,Modelo,FechaIngreso,Estado")] Equipo equipo, int[] componentesSeleccionados, int[] cantidades, IFormFile Foto)
        {
            if (id != equipo.EquipoID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Manejo de la foto
                    if (Foto != null && Foto.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Foto.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Foto.CopyToAsync(fileStream);
                        }

                        equipo.FotoRuta = "/images/" + uniqueFileName;
                    }

                    _context.Update(equipo);

                    // Actualizar componentes asignados
                    var componentesActuales = _context.Componentes.Where(c => c.EquipoID == id).ToList();
                    foreach (var componente in componentesActuales)
                    {
                        componente.EquipoID = null; // Eliminar la relación actual
                    }

                    for (int i = 0; i < componentesSeleccionados.Length; i++)
                    {
                        var componente = _context.Componentes.FirstOrDefault(c => c.ComponenteID == componentesSeleccionados[i]);
                        if (componente != null)
                        {
                            componente.EquipoID = equipo.EquipoID;
                            componente.Cantidad = cantidades[i];
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipoExists(equipo.EquipoID))
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

            // Recargar la lista de componentes
            ViewBag.Componentes = _context.Componentes
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.ComponenteID.ToString(),
                    Text = c.Nombre
                }).ToList();

            return View(equipo);
        }

        // GET: Equipos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Equipos.FirstOrDefaultAsync(m => m.EquipoID == id);
            if (equipo == null)
            {
                return NotFound();
            }

            return View(equipo);
        }

        // POST: Equipos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo != null)
            {
                if (!string.IsNullOrEmpty(equipo.FotoRuta))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", equipo.FotoRuta.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Equipos.Remove(equipo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.EquipoID == id);
        }
    }
}
