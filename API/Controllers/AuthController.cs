using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthRepository authRepository, ITokenService tokenService)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
        }

        [HttpGet]
        [Route("Users")]
        public async Task<IActionResult> GetUsersList()
        {
            var result = await _authRepository.GetListUsers();

            return result.Any() ? Ok(result) : BadRequest("Sin datos");
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if(await _authRepository.UserExists(dto.EmployeeNumber))
            {
                return BadRequest("El número de nómina ya está registrado en el sistema");
            }

            var userToCreate = new User
            {
                Username = dto.EmployeeNumber
            };

            userToCreate.UserRoles.Add(new UserRole
            {
                RoleId = dto.RoleId
            });

            var createdUser = await _authRepository.Register(userToCreate, dto.EmployeeNumber);

            return Ok(new { message = "Usuario dado de alta" });
        }

        [Authorize]
        [HttpPut]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] UserEditDto dto)
        {
            try
            {
                var success = await _authRepository.UpdateUserAsync(dto.Id, dto.NewEmployeeNumber, dto.NewRoleId);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "No se encontró el usuario o no se realizaron cambios"
                    });
                }

                return Ok(new
                {
                    message = "Usuario actualizado correctamente"
                });
            }
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException?.Message ?? ex.Message;

                return BadRequest(new
                {
                    message = "Falló al guardar en la base de datos",
                    sqlError = realError
                });
            }
            catch (Exception ex)
            {               
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var userFromRepo = await _authRepository.Login(dto.EmployeeNumber, dto.Password);

                if (userFromRepo == null)
                {
                    return Unauthorized(new { message = "Número de nómina o contraseña incorrectos" });
                }

                var token = _tokenService.Create(userFromRepo);

                return Ok(new
                {
                    token = token,
                    username = userFromRepo.Username,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch]
        [Route("ToggleStatus/{username}")]
        public async Task<IActionResult> ToogleUserStatus(string username)
        {
            var result = await _authRepository.ToogleUserStatus(username);

            if (!result) return BadRequest("No se pudo actualizar el estado del usuario o el usuario no existe");

            return Ok(new
            {
                message = "Estado de usuario actualizado correctamente"
            });
        }
    }
}