using Ecommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Repositorios
{
    interface IClienteRepositorios
    {
        public List<Clientes> Get();
        public Clientes Get(int id);
        public void Insert(Clientes cliente);
        public void Update(Clientes cliente);
        public void Delete(int id);
    }
}
