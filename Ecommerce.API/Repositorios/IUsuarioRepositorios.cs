﻿using Ecommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Repositorios
{
    interface IUsuarioRepositorios
    {
        public List<Usuario> Get();
        public Usuario Get(int id);
        public void Insert(Usuario usuario);
        public void Update(Usuario usuario);
        public void Delete(int id);
    }
}
