using Ecommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Ecommerce.API.Repositorios
{
    public class Usuariorepositorios : IUsuarioRepositorios
    {
        private IDbConnection _connection;
        public Usuariorepositorios()
        {
            _connection = new SqlConnection("data source=GEMINI2019\\SQL2019; initial catalog=eCommerce; user id=sa; password=cdssql; multipleactiveresultsets=true");
        }
        public List<Usuario> Get()
        {
            //return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList(); // buscando apenas as informações do ususario sem as relações
            List<Usuario> usuarios = new List<Usuario>();
            string sql = "select * from Usuarios as U left join Contatos as C on C.UsuarioId = U.Id left join EnderecosEntrega EE on EE.UsuarioId = U.Id where U.Id = U.Id";
            _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(sql,
                (usuario, contato, enderecoEntrega) =>
                {
                    if (usuarios.SingleOrDefault(async => async.Id == usuario.Id) == null)
                    {
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = usuarios.SingleOrDefault(a => a.Id == usuario.Id);
                    }
                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                    return usuario;
                });
            return usuarios;
        }
        public Usuario Get(int id)
        {
            List<Usuario> usuarios = new List<Usuario>();
            string sql = "select * from Usuarios as U left join Contatos as C on C.UsuarioId = U.Id left join EnderecosEntrega EE on EE.UsuarioId = U.Id where U.Id = @Id";
            _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(sql,
                (usuario, contato, enderecoEntrega) =>
                {
                    if (usuarios.SingleOrDefault(async => async.Id == usuario.Id) == null)
                    {
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = usuarios.SingleOrDefault(a => a.Id == usuario.Id);
                    }
                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                    return usuario;
                }, new { Id = id });
            return usuarios.SingleOrDefault();
            //return _connection.Query<Usuario, Contato, Usuario>(
            //    "select * from Usuarios as U left join Contatos C on C.UsuarioId = U.Id where U.Id = @Id",
            //    (usuario, contato) =>
            //    {
            //        usuario.Contato = contato;
            //        return usuario;
            //    },
            //    new { Id = id }
            //    ).SingleOrDefault();
        }
        public void Insert(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string Sql = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                usuario.Id = _connection.Query<int>(Sql, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.usuarioId = usuario.Id;
                    string sqlContato = "INSERT INTO Contatos(UsuarioId, Telefone, Celular) VALUES (@UsuarioId, @Telefone, @Celular); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }
                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    //
                }
            }
            finally
            {
                _connection.Close();
            }
        }
        public void Update(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string Sql = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id";
                _connection.Execute(Sql, usuario, transaction);
                if (usuario.Contato != null)
                {
                    string sqlContato = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id ";
                    _connection.Execute(sqlContato, usuario.Contato, transaction);
                }
                string sqlDeletarEnderecoEntrega = "DELETE FROM EnderecoEntrega WHERE UsuarioId = @Id";
                _connection.Execute(sqlDeletarEnderecoEntrega, usuario, transaction);
                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    throw new Exception(ex.Message);
                }
            }
            finally
            {
                _connection.Close();
            }
        }
        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @id", new { Id = id });
        }
    }
}


