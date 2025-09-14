using Server.DTO;
using Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers
{
    [Authorize(Roles = "Admin")]
    //[Authorize(Roles = "Admin, Manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {        
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;       

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (string.IsNullOrEmpty(createRoleDto.RoleName))
            {
                return BadRequest("Role Name is required");
            }

            var roleExist = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);
            if (roleExist)
            {
                return BadRequest ("Role Already exists");
            }

            var roleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDto.RoleName));

            if (roleResult.Succeeded)
            {
                //return Ok("Role created"); 
                return Ok(new {message = "Role created"});
            }

            return BadRequest("Role creation failed");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                TotalUsers = _userManager.GetUsersInRoleAsync(r.Name!).Result.Count
            }).ToListAsync();

            return Ok(roles);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
            {
                return NotFound("Role doesn't exist");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Ok("Role deleted");
            }

            return BadRequest("Role deletion failed");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRoles(string ids)
        {
            var idList = ids.Split(',').ToList();
            var result = new IdentityResult();
            foreach (var id in idList)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role is null)
                {
                    return NotFound($"Role with id {id} doesn't exist");
                }

                result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest($"Role deletion failed for id {id}");
                }
            }

            if (result.Succeeded)
            {
                return Ok("Role deleted");
            }
            else
            {
                return BadRequest("Role deletion failed");
            }            
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto roleAssignDto)
        {
            var user = await _userManager.FindByIdAsync(roleAssignDto.UserId);
            if (user is null)
            {
                return NotFound("User not found");
            }

            var role = await _roleManager.FindByIdAsync(roleAssignDto.RoleId);
            if (role is null)
            {
                return NotFound("Role not found");
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (result.Succeeded)
            {
                return Ok("Role assigned");
            }
            else
            {
                var error = result.Errors.FirstOrDefault();
                return BadRequest (error!.Description);
            }
        }
    }
}