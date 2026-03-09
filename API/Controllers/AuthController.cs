using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var userFromRepo = await _authRepository.Login(dto.EmployeeNumber, dto.Password);

            if(userFromRepo == null)
            {
                return Unauthorized("Número de nómina o contraseña incorrectos");
            }

            var token = _tokenService.Create(userFromRepo);

            return Ok(new
            {
                
                token = token,
                username = userFromRepo.Username,
            });
        }
    }
}