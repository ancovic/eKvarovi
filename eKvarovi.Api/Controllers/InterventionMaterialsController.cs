using System.Security.Claims;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/interventions/{interventionId:int}/materials")]
[Authorize]
public class InterventionMaterialsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public InterventionMaterialsController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<InterventionMaterialDto>>> GetInterventionMaterials(
        int interventionId)
    {
        var intervention =
            await _context.Interventions
                .Include(item =>
                    item.WorkAssignment)
                .FirstOrDefaultAsync(item =>
                    item.Id ==
                        interventionId);

        if (intervention is null)
        {
            return NotFound(
                "Intervencija nije pronađena.");
        }

        if (!CanView(
                intervention.WorkAssignment))
        {
            return Forbid();
        }

        var materials =
            await _context.InterventionMaterials
                .Include(item =>
                    item.Material)
                    .ThenInclude(material =>
                        material!.MaterialUnit)
                .Where(item =>
                    item.InterventionId ==
                        interventionId)
                .OrderBy(item =>
                    item.Material!.Name)
                .ToListAsync();

        return Ok(
            materials
                .Select(ToDto)
                .ToList());
    }

    [HttpPost]
    public async Task<ActionResult<InterventionMaterialDto>> AddMaterial(
        int interventionId,
        SaveInterventionMaterialDto request)
    {
        if (request.MaterialId <= 0)
        {
            return BadRequest(
                "Materijal je obavezan.");
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(
                "Količina materijala mora biti veća od nule.");
        }

        var intervention =
            await _context.Interventions
                .Include(item =>
                    item.WorkAssignment)
                .FirstOrDefaultAsync(item =>
                    item.Id ==
                        interventionId);

        if (intervention is null)
        {
            return NotFound(
                "Intervencija nije pronađena.");
        }

        if (!CanEdit(
                intervention))
        {
            return Forbid();
        }

        if (intervention.FinishedAt.HasValue)
        {
            return BadRequest(
                "Materijal nije moguće mijenjati na završenoj intervenciji.");
        }

        if (intervention.WorkAssignment is null ||
            !intervention.WorkAssignment.IsActive)
        {
            return BadRequest(
                "Materijal je moguće evidentirati samo na aktivnom radnom nalogu.");
        }

        var material =
            await _context.Materials
                .Include(item =>
                    item.MaterialUnit)
                .FirstOrDefaultAsync(item =>
                    item.Id ==
                        request.MaterialId);

        if (material is null)
        {
            return BadRequest(
                "Materijal nije pronađen.");
        }

        if (!material.IsActive)
        {
            return BadRequest(
                "Neaktivni materijal nije moguće evidentirati.");
        }

        var exists =
            await _context.InterventionMaterials
                .AnyAsync(item =>
                    item.InterventionId ==
                        interventionId &&
                    item.MaterialId ==
                        material.Id);

        if (exists)
        {
            return BadRequest(
                "Materijal je već evidentiran na ovoj intervenciji.");
        }

        var interventionMaterial =
            new InterventionMaterial
            {
                InterventionId =
                    interventionId,

                Intervention =
                    intervention,

                MaterialId =
                    material.Id,

                Material =
                    material,

                Quantity =
                    request.Quantity
            };

        _context.InterventionMaterials.Add(
            interventionMaterial);

        await _context.SaveChangesAsync();

        return Ok(
            ToDto(interventionMaterial));
    }

    [HttpPut("{materialId:int}")]
    public async Task<IActionResult> UpdateMaterialQuantity(
        int interventionId,
        int materialId,
        UpdateInterventionMaterialDto request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(
                "Količina materijala mora biti veća od nule.");
        }

        var intervention =
            await _context.Interventions
                .Include(item =>
                    item.WorkAssignment)
                .FirstOrDefaultAsync(item =>
                    item.Id ==
                        interventionId);

        if (intervention is null)
        {
            return NotFound(
                "Intervencija nije pronađena.");
        }

        if (!CanEdit(
                intervention))
        {
            return Forbid();
        }

        if (intervention.FinishedAt.HasValue)
        {
            return BadRequest(
                "Materijal nije moguće mijenjati na završenoj intervenciji.");
        }

        if (intervention.WorkAssignment is null ||
            !intervention.WorkAssignment.IsActive)
        {
            return BadRequest(
                "Materijal je moguće mijenjati samo na aktivnom radnom nalogu.");
        }

        var interventionMaterial =
            await _context.InterventionMaterials
                .FirstOrDefaultAsync(item =>
                    item.InterventionId ==
                        interventionId &&
                    item.MaterialId ==
                        materialId);

        if (interventionMaterial is null)
        {
            return NotFound(
                "Materijal nije evidentiran na ovoj intervenciji.");
        }

        interventionMaterial.Quantity =
            request.Quantity;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CanView(
        WorkAssignment? assignment)
    {
        if (assignment is null)
        {
            return false;
        }

        if (User.IsInRole("Admin") ||
            User.IsInRole("Manager"))
        {
            return true;
        }

        if (User.IsInRole("Technician"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            return
                int.TryParse(
                    technicianIdValue,
                    out var technicianId) &&
                assignment.TechnicianId ==
                    technicianId;
        }

        return false;
    }

    private bool CanEdit(
        Intervention intervention)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (!User.IsInRole("Technician") ||
            intervention.WorkAssignment is null)
        {
            return false;
        }

        var technicianIdValue =
            User.FindFirstValue(
                AppClaimTypes.TechnicianId);

        return
            int.TryParse(
                technicianIdValue,
                out var technicianId) &&
            intervention.WorkAssignment.TechnicianId ==
                technicianId;
    }

    private static InterventionMaterialDto ToDto(
        InterventionMaterial item)
    {
        return new InterventionMaterialDto
        {
            InterventionId =
                item.InterventionId,

            MaterialId =
                item.MaterialId,

            MaterialName =
                item.Material?.Name
                ?? string.Empty,

            Quantity =
                item.Quantity,

            MaterialUnitName =
                item.Material?.MaterialUnit?.Name
                ?? string.Empty
        };
    }
}