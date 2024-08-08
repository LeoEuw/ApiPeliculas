using ApiPeliculas.Data;
using ApiPeliculas.Models;
using ApiPeliculas.Models.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _db;
        private string claveSecreta;
        private readonly UserManager<AppUsuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        public UsuarioRepositorio(ApplicationDbContext db, IConfiguration config, UserManager<AppUsuario> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _db = db;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public bool EsUnicoUsuario(string usuario)
        {
            var usuarioBd = _db.AppUsuario.FirstOrDefault(c => c.UserName == usuario);
            if (usuarioBd == null) 
            {
                return true;
            }

            return false;
        }

        public AppUsuario GetUsuario(string usuarioId)
        {
            return _db.AppUsuario.FirstOrDefault(c => c.Id == usuarioId);
        }

        public ICollection<AppUsuario> GetUsuarios()
        {
            return _db.AppUsuario.OrderBy(c => c.UserName).ToList();
        }

        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            var usuario = _db.AppUsuario.FirstOrDefault(
                u => u.UserName.ToLower() == usuarioLoginDto.NombreUsuario.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password);

            if (usuario == null || isValid == false) 
            {
                return new UsuarioLoginRespuestaDto() 
                {
                    Token = "",
                    Usuario = null
                };
            }

            var roles =await _userManager.GetRolesAsync(usuario);
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);
            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = _mapper.Map<UsuarioDatosDto>(usuario)
            };

            return usuarioLoginRespuestaDto;

        }

        public async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistroDto)
        {
            
            AppUsuario usuario = new AppUsuario() 
            {
               UserName = usuarioRegistroDto.NombreUsuario,
               Email = usuarioRegistroDto.NombreUsuario,
               NormalizedEmail = usuarioRegistroDto.NombreUsuario.ToUpper(),
               Nombre = usuarioRegistroDto.Nombre
            };

            try
            {
                var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                        await _roleManager.CreateAsync(new IdentityRole("Registrado"));
                    }

                    await _userManager.AddToRoleAsync(usuario, "Admin");
                    var usuarioRetornado = _db.AppUsuario.FirstOrDefault(u => u.UserName == usuarioRegistroDto.NombreUsuario);

                    return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return new UsuarioDatosDto();
        }

    }

}
