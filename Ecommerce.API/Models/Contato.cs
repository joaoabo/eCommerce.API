using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Models
{
    public class Contato
    {
        public int Id { get; set; }
        public int clienteId { get; set; }
        public string Telefone { get; set; }
        public string Celular { get; set; }
        public Clientes Cliente { get; set; }
    }
}
