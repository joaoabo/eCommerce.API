using Ecommerce.API.Models;
using Ecommerce.API.Repositorios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private IClienteRepositorios _repositorios;

        public ClientesController()
        {
            _repositorios = new ClienteRepositorios();
        }

        [HttpGet]
        public IActionResult GetCliente()
        {
            return Ok(_repositorios.Get());
        }

        [HttpGet("{id}")]

        public IActionResult GetCliente(int id)
        {
            var cliente = _repositorios.Get(id);
            if(cliente == null)
            {
                return NotFound();
            }
            return Ok(cliente);

        }

        [HttpPost]
        public IActionResult Insert([FromBody]Clientes cliente)
        {
            _repositorios.Insert(cliente);
            return Ok(cliente);
        }

        [HttpPut]
        public IActionResult Update([FromBody] Clientes cliente)
        {
            try
            {
            _repositorios.Update(cliente);

            }catch(Exception ex)
            {
                return BadRequest("" + ex.Message);
            }

            return Ok(cliente);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _repositorios.Delete(id);
            return Ok();
        }
    }
}
