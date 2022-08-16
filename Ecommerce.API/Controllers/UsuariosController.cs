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
    public class UsuariosController : ControllerBase
    {
        private IUsuarioRepositorios _repositorios;

        public UsuariosController()
        {
            _repositorios = new Usuariorepositorios();
        }

        [HttpGet]
        public IActionResult GetUsuarios()
        {
            return Ok(_repositorios.Get());
        }

        [HttpGet("{id}")]

        public IActionResult GetUsuario(int id)
        {
            var usuario = _repositorios.Get(id);
            if( usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);

        }

        [HttpPost]
        public IActionResult Insert([FromBody]Usuario usuario)
        {
            _repositorios.Insert(usuario);
            return Ok(usuario);
        }

        [HttpPut]
        public IActionResult Update([FromBody] Usuario usuario)
        {
            _repositorios.Update(usuario);

            return Ok(usuario);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _repositorios.Delete(id);
            return Ok();
        }
    }
}
