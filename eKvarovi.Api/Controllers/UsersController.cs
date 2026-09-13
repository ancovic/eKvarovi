using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(
    Policy = AuthorizationPolicies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public UsersController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users =
            await _context.AppUsers
                .Include(user =>
                    user.UserRoles)
                .ThenInclude(userRole =>
                    userRole.AppRole)
                .OrderBy(user =>
                    user.Email)
                .ToListAsync();

        return Ok(
            users
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUserById(
        int id)
    {
        var user =
            await _context.AppUsers
                .Include(user =>
                    user.UserRoles)
                .ThenInclude(userRole =>
                    userRole.AppRole)
                .FirstOrDefaultAsync(user =>
                    user.Id == id);

        if (user is null)
        {
            return NotFound(
                "Korisnik nije pronađen.");
        }

        return Ok(
            ToDto(user));
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<LookupDto>>> GetRoles()
    {
        var roles =
            await _context.AppRoles
                .OrderBy(role =>
                    role.Id)
                .Select(role =>
                    new LookupDto
                    {
                        Id = role.Id,
                        Name = role.DisplayName
                    })
                .ToListAsync();

        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(
        SaveUserDto request)
    {
        var validationMessage =
            await ValidateRequest(
                request,
                null,
                true);

        if (validationMessage is not null)
        {
            return BadRequest(
                validationMessage);
        }

        var user =
            new AppUser
            {
                Email =
                    request.Email
                        .Trim()
                        .ToLowerInvariant(),

                DisplayName =
                    request.DisplayName.Trim(),

                IsActive =
                    request.IsActive,

                EmployeeId =
                    request.EmployeeId,

                TechnicianId =
                    request.TechnicianId
            };

        var hasher =
            new PasswordHasher<AppUser>();

        user.PasswordHash =
            hasher.HashPassword(
                user,
                request.Password!);

        var roleIds =
            request.RoleIds
                .Distinct()
                .ToList();

        foreach (var roleId in roleIds)
        {
            user.UserRoles.Add(
                new AppUserRole
                {
                    AppRoleId =
                        roleId
                });
        }

        _context.AppUsers.Add(
            user);

        await _context.SaveChangesAsync();

        await _context.Entry(user)
            .Collection(item =>
                item.UserRoles)
            .Query()
            .Include(userRole =>
                userRole.AppRole)
            .LoadAsync();

        return CreatedAtAction(
            nameof(GetUserById),
            new
            {
                id = user.Id
            },
            ToDto(user));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        SaveUserDto request)
    {
        var user =
            await _context.AppUsers
                .Include(user =>
                    user.UserRoles)
                .FirstOrDefaultAsync(user =>
                    user.Id == id);

        if (user is null)
        {
            return NotFound(
                "Korisnik nije pronađen.");
        }

        var validationMessage =
            await ValidateRequest(
                request,
                id,
                false);

        if (validationMessage is not null)
        {
            return BadRequest(
                validationMessage);
        }

        user.Email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        user.DisplayName =
            request.DisplayName.Trim();

        user.IsActive =
            request.IsActive;

        user.EmployeeId =
            request.EmployeeId;

        user.TechnicianId =
            request.TechnicianId;

        if (!string.IsNullOrWhiteSpace(
                request.Password))
        {
            var hasher =
                new PasswordHasher<AppUser>();

            user.PasswordHash =
                hasher.HashPassword(
                    user,
                    request.Password);
        }

        var requestedRoleIds =
            request.RoleIds
                .Distinct()
                .ToHashSet();

        var rolesToRemove =
            user.UserRoles
                .Where(userRole =>
                    !requestedRoleIds.Contains(
                        userRole.AppRoleId))
                .ToList();

        _context.AppUserRoles.RemoveRange(
            rolesToRemove);

        var existingRoleIds =
            user.UserRoles
                .Select(userRole =>
                    userRole.AppRoleId)
                .ToHashSet();

        foreach (var roleId in requestedRoleIds)
        {
            if (!existingRoleIds.Contains(
                    roleId))
            {
                user.UserRoles.Add(
                    new AppUserRole
                    {
                        AppRoleId =
                            roleId
                    });
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("employees")]
    public async Task<ActionResult<List<LookupDto>>> GetEmployeesLookup()
    {
        var employees =
            await _context.Employees
                .Where(employee =>
                    employee.IsActive)
                .OrderBy(employee =>
                    employee.LastName)
                .ThenBy(employee =>
                    employee.FirstName)
                .Select(employee =>
                    new LookupDto
                    {
                        Id = employee.Id,
                        Name =
                            employee.FirstName +
                            " " +
                            employee.LastName
                    })
                .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("technicians")]
    public async Task<ActionResult<List<LookupDto>>> GetTechniciansLookup()
    {
        var technicians =
            await _context.Technicians
                .Where(technician =>
                    technician.IsActive)
                .OrderBy(technician =>
                    technician.LastName)
                .ThenBy(technician =>
                    technician.FirstName)
                .Select(technician =>
                    new LookupDto
                    {
                        Id = technician.Id,
                        Name =
                            technician.FirstName +
                            " " +
                            technician.LastName
                    })
                .ToListAsync();

        return Ok(technicians);
    }

    private async Task<string?> ValidateRequest(
        SaveUserDto request,
        int? currentUserId,
        bool passwordRequired)
    {
        if (string.IsNullOrWhiteSpace(
                request.Email))
        {
            return "Email je obavezan.";
        }

        if (string.IsNullOrWhiteSpace(
                request.DisplayName))
        {
            return "Naziv korisnika je obavezan.";
        }

        if (passwordRequired &&
            string.IsNullOrWhiteSpace(
                request.Password))
        {
            return "Lozinka je obavezna.";
        }

        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var emailExists =
            await _context.AppUsers
                .AnyAsync(user =>
                    user.Email == email &&
                    (!currentUserId.HasValue ||
                     user.Id !=
                        currentUserId.Value));

        if (emailExists)
        {
            return "Korisnik s tim emailom već postoji.";
        }

        var roleIds =
            request.RoleIds
                .Distinct()
                .ToList();

        if (roleIds.Count == 0)
        {
            return "Odaberi barem jednu ulogu.";
        }

        var roles =
            await _context.AppRoles
                .Where(role =>
                    roleIds.Contains(
                        role.Id))
                .ToListAsync();

        if (roles.Count !=
            roleIds.Count)
        {
            return "Jedna ili više odabranih uloga nisu valjane.";
        }

        var hasReporterRole =
            roles.Any(role =>
                role.Name == "Reporter");

        var hasTechnicianRole =
            roles.Any(role =>
                role.Name == "Technician");

        if (hasReporterRole &&
            !request.EmployeeId.HasValue)
        {
            return "Za ulogu Prijavitelj potrebno je povezati djelatnika.";
        }

        if (!hasReporterRole &&
            request.EmployeeId.HasValue)
        {
            return "Djelatnika je moguće povezati samo s korisnikom koji ima ulogu Prijavitelj.";
        }

        if (hasTechnicianRole &&
            !request.TechnicianId.HasValue)
        {
            return "Za ulogu Izvršitelj potrebno je povezati izvršitelja.";
        }

        if (!hasTechnicianRole &&
            request.TechnicianId.HasValue)
        {
            return "Izvršitelja je moguće povezati samo s korisnikom koji ima ulogu Izvršitelj.";
        }

        if (request.EmployeeId.HasValue)
        {
            var employeeExists =
                await _context.Employees
                    .AnyAsync(employee =>
                        employee.Id ==
                            request.EmployeeId.Value &&
                        employee.IsActive);

            if (!employeeExists)
            {
                return "Odabrani djelatnik nije aktivan ili ne postoji.";
            }

            var employeeAlreadyLinked =
                await _context.AppUsers
                    .AnyAsync(user =>
                        user.EmployeeId ==
                            request.EmployeeId.Value &&
                        (!currentUserId.HasValue ||
                         user.Id !=
                            currentUserId.Value));

            if (employeeAlreadyLinked)
            {
                return "Odabrani djelatnik već je povezan s korisničkim računom.";
            }
        }

        if (request.TechnicianId.HasValue)
        {
            var technicianExists =
                await _context.Technicians
                    .AnyAsync(technician =>
                        technician.Id ==
                            request.TechnicianId.Value &&
                        technician.IsActive);

            if (!technicianExists)
            {
                return "Odabrani izvršitelj nije aktivan ili ne postoji.";
            }

            var technicianAlreadyLinked =
                await _context.AppUsers
                    .AnyAsync(user =>
                        user.TechnicianId ==
                            request.TechnicianId.Value &&
                        (!currentUserId.HasValue ||
                         user.Id !=
                            currentUserId.Value));

            if (technicianAlreadyLinked)
            {
                return "Odabrani izvršitelj već je povezan s korisničkim računom.";
            }
        }

        return null;
    }

    private static UserDto ToDto(
        AppUser user)
    {
        return new UserDto
        {
            Id =
                user.Id,

            Email =
                user.Email,

            DisplayName =
                user.DisplayName,

            IsActive =
                user.IsActive,

            EmployeeId =
                user.EmployeeId,

            TechnicianId =
                user.TechnicianId,

            RoleIds =
                user.UserRoles
                    .Select(userRole =>
                        userRole.AppRoleId)
                    .OrderBy(id => id)
                    .ToList(),

            Roles =
                user.UserRoles
                    .Where(userRole =>
                        userRole.AppRole is not null)
                    .Select(userRole =>
                        userRole.AppRole!.DisplayName)
                    .OrderBy(name => name)
                    .ToList()
        };
    }
}