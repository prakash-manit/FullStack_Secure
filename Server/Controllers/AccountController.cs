using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Server.DTO;
using Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Server.Controllers
{
    [ApiController]
     [Authorize]
    [Route("api/[controller]")]
    //api/account
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            //_roleManager = roleManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
           var user = new AppUser{
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email
           };

           var result = await _userManager.CreateAsync(user, registerDto.Password);

           if (!result.Succeeded)
           {
            return BadRequest(result.Errors);
           }

           if (registerDto.Roles is null)
           {
            await _userManager.AddToRoleAsync(user, "User");
           }
           else
           {
                foreach(var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
           }

           return Ok(new AuthResponseDto{
            IsSuccess = true,
            Message = "Account Created!!"
           });
        }

        //api/account/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login (LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
            {
                return Unauthorized( new AuthResponseDto{
                    IsSuccess = false,
                    Message = "User not found with this email"
                });
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result) {
                return Unauthorized(new AuthResponseDto{
                    IsSuccess =  false,
                    Message = "Invalid Password"
                });
            }

            var token  = GenerateToken(user);
            return Ok(new AuthResponseDto{
                Token = token,
                IsSuccess = true,
                Message = "Login Successful!!"
            });
        }

        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSetting = _configuration.GetSection("JwtSetting");
            var key = Encoding.ASCII.GetBytes(jwtSetting["securityKey"]!);
            
            var roles = _userManager.GetRolesAsync(user).Result;
            List<Claim> claims = [
                new (JwtRegisteredClaimNames.Email, user.Email?? ""),
                new (JwtRegisteredClaimNames.Name, user.FullName?? ""),
                new (JwtRegisteredClaimNames.NameId, user.Id?? ""),
                new (JwtRegisteredClaimNames.Aud, jwtSetting["validAudience"]!),
                new (JwtRegisteredClaimNames.Iss, jwtSetting["validIssuer"]!)
                ];

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token  = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        //api/account/detail
        [HttpGet("detail")]       
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user is null) //same as user == null
            {
                return NotFound(new AuthResponseDto{
                    IsSuccess = false,
                    Message = "User Not Found"

                });
            }
            
            return Ok(new UserDetailDto{
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = [..await _userManager.GetRolesAsync(user)],
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userManager.Users.Select(u=> new UserDetailDto{
                Id  = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Roles = _userManager.GetRolesAsync(u).Result.ToArray()
            }).ToListAsync();

            return Ok(users);
        }
    }
}