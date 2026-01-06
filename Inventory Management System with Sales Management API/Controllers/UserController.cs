using Inventory_Management_System_with_Sales_Management_API.Data;
using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace Inventory_Management_System_with_Sales_Management_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _config;
        public UserController(UserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> AddNewUser([FromBody] UserModel userModel)
        {
            if (userModel == null)
            {
                return BadRequest(new { message = "Invalid user data." });
            }
            var hasher = new PasswordHasher<UserModel>();
            var user = new UserModel
            {
                FullName = userModel.FullName,
                Email = userModel.Email,
                PasswordHash = hasher.HashPassword(null, userModel.PasswordHash),
                IsAdmin = false,
                IsActive = true
            };
            int rowsAffected = _userRepository.Insert(user);

            if (rowsAffected > 0)
            {
                return Ok(new { success = true, message = "User added successfully." });
            }

            return StatusCode(500, new { success = false, message = "Failed to add user." });
        }


        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null ||
                string.IsNullOrWhiteSpace(loginModel.Email) ||
                string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            // ✅ Get user by email
            var user = _userRepository.GetByEmail(loginModel.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            if (!user.IsActive)
                return Unauthorized(new { message = "Your account is not active." });

            // ✅ Verify password hash
            var hasher = new PasswordHasher<UserModel>();
            var result = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginModel.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password." });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                token,
                user = new
                {
                    user.UserID,
                    user.FullName,
                    user.Email,
                    user.IsAdmin,
                    user.IsActive,
                    user.CreatedAt
                }
            });
        }


        private string GenerateJwtToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpGet("Users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return Ok(users);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int id)
        {
            int rowsAffected = _userRepository.Delete(id);
            if (rowsAffected<=0)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult EditExistUser(int id, [FromBody] UpdateUserModel userModel)
        {
            if (id != userModel.UserID)
                return BadRequest("User ID mismatch");

            // 🔐 Handle password hashing ONLY
            if (!string.IsNullOrWhiteSpace(userModel.NewPassword))
            {
                if (userModel.NewPassword != userModel.ConfirmPassword)
                    return BadRequest("Passwords do not match");

                var hasher = new PasswordHasher<UserModel>();
                userModel.NewPassword = hasher.HashPassword(null, userModel.NewPassword);
            }
            else
            {
                // IMPORTANT: ensure repository does NOT update password
                userModel.NewPassword = null;
            }

            int rowsAffected = _userRepository.Update(userModel);

            if (rowsAffected <= 0)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            var user = _userRepository.GetByID(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        
    }
}
