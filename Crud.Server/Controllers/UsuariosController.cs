using Crud.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crud.Shared;
using Microsoft.EntityFrameworkCore;

namespace Crud.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly FacturacionContext _dbContext;
        public UsuariosController(FacturacionContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("CrearUsuario")]
        public async Task<IActionResult> Guardar(UsuarioDTO usuario)
        {
            var responseApi = new ResponseAPI<int>();
            try
            {
                var buscar = await _dbContext.Usuarios.FirstOrDefaultAsync(x => x.Cedula == usuario.Cedula || x.Email==usuario.Email);
                
                if (buscar==null)
                {
                    var dbUsuario = new Usuario
                    {
                        Cedula = usuario.Cedula,
                        Nombre = usuario.Nombre,
                        Apellido = usuario.Apellido,
                        Direccion = usuario.Direccion,
                        Password = usuario.Password,
                        Email = usuario.Email,
                        Telefono = usuario.Telefono,
                        FechaNacimiento = usuario.FechaNacimiento,
                        Activo = usuario.Activo,
                        IdRolPer = usuario.RolID
                    };

                    _dbContext.Usuarios.Add(dbUsuario);
                    await _dbContext.SaveChangesAsync();
                    if (dbUsuario.UsuarioId != 0)
                    {
                        responseApi.EsCorrecto = true;
                        responseApi.Valor = dbUsuario.UsuarioId;
                    }
                    else
                    {
                        responseApi.EsCorrecto = false;
                        responseApi.Mensaje = "No Creado";
                    }
                }
                else
                {
                    responseApi.EsCorrecto = false;
                    responseApi.Mensaje = "Uno de los datos ya esta registrado";
                }

            }
            catch (Exception ex)
            {
                responseApi.EsCorrecto = false;
                responseApi.Mensaje = ex.Message;
            }
            return Ok(responseApi);
        }

        [HttpGet]
        [Route("ListarUsuarios")]
        public async Task<IActionResult> ListaUsuarios()
        {
            var responseApi = new ResponseAPI<List<UsuarioDTO>>();
            var listaCli = new List<UsuarioDTO>();
            var listaUsers = await _dbContext.Usuarios.Where(p => p.Activo == true).ToArrayAsync();

            try
            {
                foreach (var usuDB in listaUsers)
                {

                    listaCli.Add(new UsuarioDTO
                    {
                        UsuarioID = usuDB.UsuarioId,
                        Cedula = usuDB.Cedula,
                        Nombre = usuDB.Nombre,
                        Apellido= usuDB.Apellido,
                        Activo = usuDB.Activo,
                        Direccion = usuDB.Direccion,
                        Email = usuDB.Email,
                        Password = usuDB.Password,
                        Telefono = usuDB.Telefono,
                        FechaNacimiento = usuDB.FechaNacimiento,
                        RolID = usuDB.IdRolPer
                    });
                }
                responseApi.EsCorrecto = true;
                responseApi.Valor = listaCli;
            }
            catch (Exception ex)
            {
                responseApi.EsCorrecto = false;
                responseApi.Mensaje = ex.Message;
            }
            return Ok(responseApi);
        }
    }
}
