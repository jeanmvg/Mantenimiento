using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MantenimientoIndustrial.Data;
using MantenimientoIndustrial.Models;

namespace MantenimientoIndustrial.Controllers
{
    public class AlertasController : Controller
    {
        private readonly AppDbContext _context;

        public AlertasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Alertas
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Alertas.Include(a => a.Mantenimiento);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Alertas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alerta = await _context.Alertas
                .Include(a => a.Mantenimiento)
                .FirstOrDefaultAsync(m => m.AlertaID == id);
            if (alerta == null)
            {
                return NotFound();
            }

            return View(alerta);
        }

        // GET: Alertas/Create
        public IActionResult Create()
        {
            ViewData["MantenimientoID"] = new SelectList(_context.Mantenimientos, "MantenimientoID", "MantenimientoID");
            return View();
        }

        // POST: Alertas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlertaID,MantenimientoID,FechaAlerta,Mensaje,Estado")] Alerta alerta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alerta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MantenimientoID"] = new SelectList(_context.Mantenimientos, "MantenimientoID", "MantenimientoID", alerta.MantenimientoID);
            return View(alerta);
        }

        // GET: Alertas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alerta = await _context.Alertas.FindAsync(id);
            if (alerta == null)
            {
                return NotFound();
            }
            ViewData["MantenimientoID"] = new SelectList(_context.Mantenimientos, "MantenimientoID", "MantenimientoID", alerta.MantenimientoID);
            return View(alerta);
        }

        // POST: Alertas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlertaID,MantenimientoID,FechaAlerta,Mensaje,Estado")] Alerta alerta)
        {
            if (id != alerta.AlertaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alerta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertaExists(alerta.AlertaID))
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
            ViewData["MantenimientoID"] = new SelectList(_context.Mantenimientos, "MantenimientoID", "MantenimientoID", alerta.MantenimientoID);
            return View(alerta);
        }

        // GET: Alertas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alerta = await _context.Alertas
                .Include(a => a.Mantenimiento)
                .FirstOrDefaultAsync(m => m.AlertaID == id);
            if (alerta == null)
            {
                return NotFound();
            }

            return View(alerta);
        }

        // POST: Alertas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alerta = await _context.Alertas.FindAsync(id);
            if (alerta != null)
            {
                _context.Alertas.Remove(alerta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlertaExists(int id)
        {
            return _context.Alertas.Any(e => e.AlertaID == id);
        }
    }
}
