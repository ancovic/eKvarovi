using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public MaterialsController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(
        Policy = AuthorizationPolicies.Management)]
    public async Task<ActionResult<List<MaterialDto>>> GetMaterials(
        [FromQuery] bool? isActive)
    {
        var query =
            _context.Materials
                .Include(material =>
                    material.MaterialUnit)
                .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(material =>
                material.IsActive ==
                    isActive.Value);
        }

        var materials =
            await query
                .OrderBy(material =>
                    material.Name)
                .ToListAsync();

        return Ok(
            materials
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("lookup")]
    [Authorize]
    public async Task<ActionResult<List<MaterialDto>>> GetMaterialLookup()
    {
        if (!User.IsInRole("Admin") &&
            !User.IsInRole("Manager") &&
            !User.IsInRole("Technician"))
        {
            return Forbid();
        }

        var materials =
            await _context.Materials
                .Include(material =>
                    material.MaterialUnit)
                .Where(material =>
                    material.IsActive)
                .OrderBy(material =>
                    material.Name)
                .ToListAsync();

        return Ok(
            materials
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("{id:int}")]
    [Authorize(
        Policy = AuthorizationPolicies.Management)]
    public async Task<ActionResult<MaterialDto>> GetMaterialById(
        int id)
    {
        var material =
            await _context.Materials
                .Include(item =>
                    item.MaterialUnit)
                .FirstOrDefaultAsync(item =>
                    item.Id == id);

        if (material is null)
        {
            return NotFound();
        }

        return Ok(ToDto(material));
    }

    [HttpPost]
    [Authorize(
        Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<MaterialDto>> CreateMaterial(
        SaveMaterialDto request)
    {
        if (string.IsNullOrWhiteSpace(
                request.Name))
        {
            return BadRequest(
                "Naziv materijala je obavezan.");
        }

        var unit =
            await _context.MaterialUnits
                .FirstOrDefaultAsync(unit =>
                    unit.Id ==
                        request.MaterialUnitId);

        if (unit is null)
        {
            return BadRequest(
                "Jedinica mjere nije pronađena.");
        }

        var name =
            request.Name.Trim();

        var exists =
            await _context.Materials
                .AnyAsync(material =>
                    material.Name == name);

        if (exists)
        {
            return BadRequest(
                "Materijal s tim nazivom već postoji.");
        }

        var material =
            new Material
            {
                Name = name,

                Description =
                    string.IsNullOrWhiteSpace(
                        request.Description)
                        ? null
                        : request.Description.Trim(),

                IsActive =
                    request.IsActive,

                MaterialUnitId =
                    unit.Id,

                MaterialUnit =
                    unit
            };

        _context.Materials.Add(
            material);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMaterialById),
            new { id = material.Id },
            ToDto(material));
    }

    [HttpPut("{id:int}")]
    [Authorize(
        Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> UpdateMaterial(
        int id,
        SaveMaterialDto request)
    {
        var material =
            await _context.Materials
                .FirstOrDefaultAsync(
                    material =>
                        material.Id == id);

        if (material is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(
                request.Name))
        {
            return BadRequest(
                "Naziv materijala je obavezan.");
        }

        var unit =
            await _context.MaterialUnits
                .FirstOrDefaultAsync(unit =>
                    unit.Id ==
                        request.MaterialUnitId);

        if (unit is null)
        {
            return BadRequest(
                "Jedinica mjere nije pronađena.");
        }

        var name =
            request.Name.Trim();

        var duplicate =
            await _context.Materials
                .AnyAsync(item =>
                    item.Id != id &&
                    item.Name == name);

        if (duplicate)
        {
            return BadRequest(
                "Materijal s tim nazivom već postoji.");
        }

        material.Name = name;

        material.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        material.IsActive =
            request.IsActive;

        material.MaterialUnitId =
            unit.Id;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> DeleteMaterial(
    int id)
    {
        var material =
            await _context.Materials
                .FirstOrDefaultAsync(material =>
                    material.Id == id);

        if (material is null)
        {
            return NotFound(
                "Materijal nije pronađen.");
        }

        var isUsed =
            await _context.InterventionMaterials
                .AnyAsync(item =>
                    item.MaterialId == id);

        if (isUsed)
        {
            return BadRequest(
                "Materijal koji je korišten u intervenciji nije moguće izbrisati. Deaktiviraj ga.");
        }

        _context.Materials.Remove(
            material);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static MaterialDto ToDto(
        Material material)
    {
        return new MaterialDto
        {
            Id =
                material.Id,

            Name =
                material.Name,

            Description =
                material.Description,

            IsActive =
                material.IsActive,

            MaterialUnitId =
                material.MaterialUnitId,

            MaterialUnitName =
                material.MaterialUnit?.Name
                ?? string.Empty
        };
    }
}