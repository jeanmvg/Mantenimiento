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
                FechaIngreso = DateTime.Today,
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
        public async Task<IActionResult> Create(Equipo equipo, int[] componentesSeleccionados, int[] cantidades, IFormFile Foto)
        {
            // En la creación, eliminamos los errores de ModelState relacionados con Foto y FotoRuta
            ModelState.Remove("Foto");
            ModelState.Remove("FotoRuta");

            if (ModelState.IsValid)
            {
                // Guardar la foto solo si se proporcionó
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
                    var componente = await _context.Componentes.FirstOrDefaultAsync(c => c.ComponenteID == componentesSeleccionados[i]);
                    if (componente != null)
                    {
                        componente.EquipoID = equipo.EquipoID;
                        componente.Cantidad = cantidades[i];
                        _context.Entry(componente).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar la lista de componentes para la vista
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

        // GET: Equipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Esperamos el resultado asíncrono para obtener el equipo
            var equipo = await _context.Equipos
                .Include(e => e.Componentes) // Si necesitas incluir componentes relacionados
                .FirstOrDefaultAsync(e => e.EquipoID == id);

            if (equipo == null)
            {
                return NotFound();
            }

            // Preparamos la lista de componentes para el select en la vista
            ViewBag.Componentes = new SelectList(_context.Componentes, "ComponenteID", "Nombre");
            return View(equipo);
        }

        // POST: Equipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Equipo equipo, int[] componentesSeleccionados, int[] cantidades, IFormFile Foto)
        {
            if (id != equipo.EquipoID)
            {
                return NotFound();
            }

            // En edición, si no se envía nueva foto, eliminamos los errores de ModelState de Foto y FotoRuta
            ModelState.Remove("Foto");
            ModelState.Remove("FotoRuta");

            if (!ModelState.IsValid)
            {
                // Recargar la lista de componentes para la vista en caso de error
                ViewBag.Componentes = await _context.Componentes
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ComponenteID.ToString(),
                        Text = c.Nombre
                    })
                    .ToListAsync();
                return View(equipo);
            }

            // Cargar la entidad existente de la base de datos.
            var equipoDB = await _context.Equipos.FirstOrDefaultAsync(e => e.EquipoID == id);
            if (equipoDB == null)
            {
                return NotFound();
            }

            // Actualizar manualmente las propiedades escalares
            equipoDB.Codigo = equipo.Codigo;
            equipoDB.Nombre = equipo.Nombre;
            equipoDB.Ubicacion = equipo.Ubicacion;
            equipoDB.Marca = equipo.Marca;
            equipoDB.Modelo = equipo.Modelo;
            equipoDB.FechaIngreso = equipo.FechaIngreso;
            equipoDB.Estado = equipo.Estado;

            // Manejo de la foto: si se envía una nueva, actualizar la ruta; de lo contrario, conservar la existente.
            if (Foto != null && Foto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Foto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Foto.CopyToAsync(fileStream);
                }
                equipoDB.FotoRuta = "/images/" + uniqueFileName;
            }

            // Forzar que la entidad principal se marque como modificada.
            _context.Entry(equipoDB).State = EntityState.Modified;

            // Actualizar los componentes asignados:
            // 1. Desasociar los componentes que actualmente tienen este Equipo asignado.
            var componentesActuales = await _context.Componentes.Where(c => c.EquipoID == id).ToListAsync();
            foreach (var comp in componentesActuales)
            {
                comp.EquipoID = null;
                _context.Entry(comp).State = EntityState.Modified;
            }

            // 2. Asignar los nuevos componentes según los arrays recibidos.
            if (componentesSeleccionados != null && cantidades != null)
            {
                int total = Math.Min(componentesSeleccionados.Length, cantidades.Length);
                for (int i = 0; i < total; i++)
                {
                    var comp = await _context.Componentes.FirstOrDefaultAsync(c => c.ComponenteID == componentesSeleccionados[i]);
                    if (comp != null)
                    {
                        comp.EquipoID = equipoDB.EquipoID;
                        comp.Cantidad = cantidades[i];
                        _context.Entry(comp).State = EntityState.Modified;
                    }
                }
            }

            try
            {
                int affected = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Filas afectadas: {affected}");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipoExists(equipoDB.EquipoID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
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
