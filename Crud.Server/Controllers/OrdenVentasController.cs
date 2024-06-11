using Crud.Server.Models;
using Crud.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crud.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenVentasController : ControllerBase
    {
        private readonly FacturacionContext _dbContext;
        public OrdenVentasController(FacturacionContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("ListaOrdenesVentas")]
        public async Task<IActionResult> ListaOrdenesVentas()
        {
            var responseApi = new ResponseAPI<List<OrdenVentaDTO>>();
            var listOV = new List<OrdenVentaDTO>();
            var listaOVBD = await _dbContext.OrdenVenta.ToArrayAsync();
            try
            {
                foreach (var ovBD in listaOVBD)
                {
           
                    listOV.Add(new OrdenVentaDTO
                    {
                        OrdenID = ovBD.OrdenId,
                        ClienteID = ovBD.ClienteId,
                        Estado = ovBD.Estado,
                        FechaVenta = ovBD.FechaVenta,
                        TotalVenta = ovBD.TotalVenta,
                    });
                }
                responseApi.EsCorrecto = true;
                responseApi.Valor = listOV;

            }
            catch (Exception ex)
            {
                responseApi.EsCorrecto = false;
                responseApi.Mensaje = ex.Message;
            }
            return Ok(responseApi);
        }

        [HttpPost]
        [Route("GuardarOrdenVenta")]
        public async Task<IActionResult> GuardarOrdenVenta(OrdenVentaDTO orden)
        {
            var serverDateTime = await _dbContext.OrdenVenta
                    .FromSqlRaw("SELECT GETDATE() AS FechaVenta")
                    .Select(o => o.FechaVenta)
                    .FirstAsync();

            var responseApi = new ResponseAPI<int>();
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var dborden = new OrdenVentum
                    {
                        ClienteId = orden.ClienteID,
                        FechaVenta = serverDateTime,
                        TotalVenta = orden.TotalVenta,
                        Estado = orden.Estado,
                    };

                    _dbContext.OrdenVenta.Add(dborden);
                    await _dbContext.SaveChangesAsync();
                    // Verificar si se pudo obtener el ID de la orden
                    if (dborden.OrdenId != 0)
                    {
                        List<DetalleOrdenDTO> ordenes = orden.ListaOrdenes;

                        foreach (var item in ordenes)
                        {
                            var dbDetalle = new DetalleOrden
                            {
                                OrdenId = dborden.OrdenId,
                                Cantidad = item.Cantidad,
                                ProductoId = item.ProductoID,
                                PrecioUnitario = item.PrecioUnitario,
                                Subtotal = item.Subtotal,
                            };
                            _dbContext.DetalleOrdens.Add(dbDetalle);
                            var productToUpdate = await _dbContext.Productos.FirstOrDefaultAsync(x => x.ProductoId == dbDetalle.ProductoId);
                            productToUpdate.Stock = productToUpdate.Stock - dbDetalle.Cantidad;
                            await _dbContext.SaveChangesAsync();
                        }

                        responseApi.EsCorrecto = true;
                        responseApi.Valor = dborden.OrdenId;

                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        responseApi.EsCorrecto = false;
                        responseApi.Valor = 0;
                        responseApi.Mensaje = "Error al completar la compra, intente más tarde";
                    }

                }
                catch (Exception ex)
                {
                    // En caso de error, revertir la transacción
                    await transaction.RollbackAsync();
                    responseApi.EsCorrecto = false;
                    responseApi.Mensaje = ex.Message;
                }
            }
            return Ok(responseApi);
        }


        [HttpGet]
        [Route("DevolverOrdenVenta/{id}")]
        public async Task<IActionResult> DevolverOrdenVenta(int id)
        {
            var responseApi = new ResponseAPI<OrdenVentaDTO>();
            var ordenVenta = new OrdenVentaDTO();

            var ordenBD = await _dbContext.OrdenVenta.Where(x => x.OrdenId == id).ToArrayAsync();
            try
            {
                if (ordenBD.Length>0)
                {
                    foreach (var ordDB in ordenBD)
                    {
                        ordenVenta = (new OrdenVentaDTO
                        {
                            OrdenID = ordDB.OrdenId,
                            Estado = ordDB.Estado,
                            ClienteID = ordDB.ClienteId,
                            FechaVenta = ordDB.FechaVenta,
                            TotalVenta = ordDB.TotalVenta,
                        });
                    }
                    responseApi.EsCorrecto = true;
                    responseApi.Valor = ordenVenta;
                }
                else
                {
                    responseApi.EsCorrecto = false;
                    responseApi.Valor = null;
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
        [Route("ObtenerIdUltimaOrdenVenta")]
        public async Task<IActionResult> ObtenerUltimaOrden()
        {
            var responseApi = new ResponseAPI<int>();
            try
            {
                var ultimoDepartamento = await _dbContext.OrdenVenta
               .OrderByDescending(d => d.OrdenId)
               .FirstOrDefaultAsync();
                responseApi.EsCorrecto = true;
                responseApi.Valor = ultimoDepartamento!.OrdenId;
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
