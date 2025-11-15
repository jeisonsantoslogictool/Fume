using fume.api.Helpers;
using fume.shared.DTOs;
using fume.shared.Enttities;
using fume.shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace fume.api.Controllers
{
    [ApiController]
    [Route("/api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly string _container;

        public AccountsController(IUserHelper userHelper, IConfiguration configuration)
        {
            _userHelper = userHelper;
            _configuration = configuration;
            _container = "users";
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> Put(User user)
        {
            try
            {
                // Validar que el Id no esté vacío
                if (string.IsNullOrWhiteSpace(user.Id))
                {
                    return BadRequest("El ID del usuario es requerido para actualizar");
                }

                if (user.Photo != null)
                {
                    var photoUser = Convert.ToBase64String(user.Photo);

                }

                // Buscar usuario por ID en lugar de email para permitir cambio de email
                var currentUser = await _userHelper.GetUserByIdAsync(user.Id);
                if (currentUser == null)
                {
                    return NotFound($"Usuario no encontrado con ID: {user.Id}");
                }

                // Verificar si el email cambió y si ya existe
                if (currentUser.Email != user.Email)
                {
                    var existingUser = await _userHelper.GetUserAsync(user.Email!);
                    if (existingUser != null)
                    {
                        return BadRequest("El correo electrónico ya está en uso por otro usuario");
                    }
                    currentUser.Email = user.Email;
                    currentUser.UserName = user.Email; // UserName también debe actualizarse
                }

                currentUser.Document = user.Document;
                currentUser.FirstName = user.FirstName;
                currentUser.LastName = user.LastName;
                currentUser.Address = user.Address;
                currentUser.PhoneNumber = user.PhoneNumber;
                currentUser.Photo = user.Photo;
                currentUser.CityId = user.CityId;

                var result = await _userHelper.UpdateUserAsync(currentUser);
                if (result.Succeeded)
                {
                    return NoContent();
                }

                return BadRequest(result.Errors.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("User/{email}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Get(string email)
        {
            return Ok(await _userHelper.GetUserAsync(email));
        }


        [HttpPost("CreateUser")]
        public async Task<ActionResult> CreateUser([FromBody] UserDTO model)
        {
            User user = model;
            var result = await _userHelper.AddUserAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());
                return Ok(BuildToken(user));
            }

            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO model)
        {
            var result = await _userHelper.LoginAsync(model);
            if(result.Succeeded)
            {
                var user = await _userHelper.GetUserAsync(model.Email);
                return Ok(BuildToken(user));
            }
            return BadRequest("Email o contraseña incorrectos.");
        }

        [HttpGet("GetAll")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> GetAll()
        {
            var users = await _userHelper.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _userHelper.GetUserAsync(id);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var result = await _userHelper.DeleteUserAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPut("ChangeUserType/{email}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> ChangeUserType(string email, [FromBody] UserType userType)
        {
            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var currentRole = user.UserType.ToString();
            var newRole = userType.ToString();

            await _userHelper.RemoveUserFromRoleAsync(user, currentRole);
            await _userHelper.AddUserToRoleAsync(user, newRole);

            user.UserType = userType;
            var result = await _userHelper.UpdateUserAsync(user);

            if (result.Succeeded)
            {
                return Ok(user);
            }

            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var user = await _userHelper.GetUserAsync(model.Email);
            if (user == null)
            {
                return Ok(new { Token = "", Message = "Si el correo existe, recibirás instrucciones para restablecer tu contraseña." });
            }

            var token = await _userHelper.GeneratePasswordResetTokenAsync(user);

            // TODO: Enviar email con el token
            // Por ahora, retornamos el token directamente (en producción esto debería ser un email)
            return Ok(new { Token = token, Message = "Token generado correctamente" });
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            var user = await _userHelper.GetUserAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return Ok("Contraseña restablecida correctamente");
            }

            return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Error al restablecer la contraseña");
        }
        private TokenDTO BuildToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Role, user.UserType.ToString()),
                new Claim("Document", user.Document),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Address", user.Address),
                new Claim("Photo", Convert.ToBase64String(user.Photo ?? new byte[0])),
                new Claim("CityId", user.CityId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(30);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }

}

