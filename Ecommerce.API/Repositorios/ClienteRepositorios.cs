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
    public class ClienteRepositorios : IClienteRepositorios
    {
        private IDbConnection _connection;
        public ClienteRepositorios()
        {
            _connection = new SqlConnection("data source=LUGANENHUM; initial catalog=Thanus; user id=sa; password=cdssql; multipleactiveresultsets=true");
        }
        public List<Clientes> Get()
        {
            //return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList(); // buscando apenas as informações do ususario sem as relações
            List<Clientes> clientes = new List<Clientes>();
            string sql = @"select Cl.*, C.*, EE.*, D.* 
                           from clientes as Cl 
                           left join Contatos C on C.ClientesId = Cl.Id 
                           left join EnderecosEntrega EE on EE.ClientesId = Cl.Id 
                           left join ClientesDepartamentos Cd on Cd.ClientesId = Cl.Id 
                           left join Departamentos D on Cd.DepartamentoId = D.Id
                           where d.id is not null or cd.id is not null or ee.id is not null or c.id is not null";

            _connection.Query<Clientes, Contato, EnderecoEntrega, Departamento, Clientes>(sql,
                (cliente, contato, enderecoEntrega, departamento) =>
                {
                    if (clientes.SingleOrDefault(async => async.Id == cliente.Id) == null)
                    {
                        cliente.Departamentos = new List<Departamento>();
                        cliente.EnderecosEntrega = new List<EnderecoEntrega>();
                        cliente.Contato = contato;
                        clientes.Add(cliente);
                    }
                    else
                    {
                        cliente = clientes.SingleOrDefault(a => a.Id == cliente.Id);
                    }

                    if (cliente.EnderecosEntrega.SingleOrDefault(a => a.Id == enderecoEntrega.Id) == null)
                    {
                        cliente.EnderecosEntrega.Add(enderecoEntrega);
                    }

                    if (cliente.Departamentos.SingleOrDefault(a => a.Id == departamento.Id) == null)
                    {
                        cliente.Departamentos.Add(departamento);
                    }
                    return cliente;
                });
            return clientes;
        }


        public Clientes Get(int id)
        {
            List<Clientes> clientes = new List<Clientes>();
            string sql = @"select Cl.*, C.*, EE.*, D.* 
                           from Clientes as Cl 
                           left join Contatos as C on C.ClientesId = Cl.Id 
                           left join EnderecosEntrega EE on EE.ClientesId = Cl.Id 
                           left join CLientesDepartamentos Cd on Cd.ClientesId = Cl.Id
                           left join Departamentos D on Cd.DepartamentoId = D.Id
                           where Cl.Id = @Id";
            _connection.Query<Clientes, Contato, EnderecoEntrega, Departamento, Clientes>(sql,
                (cliente, contato, enderecoEntrega, Departamento) =>
                {
                    if (clientes.SingleOrDefault(async => async.Id == cliente.Id) == null)
                    {
                        cliente.Departamentos = new List<Departamento>();
                        cliente.EnderecosEntrega = new List<EnderecoEntrega>();
                        cliente.Contato = contato;
                        clientes.Add(cliente);
                    }
                    else
                    {
                        cliente = clientes.SingleOrDefault(a => a.Id == cliente.Id);
                    }
                    cliente.EnderecosEntrega.Add(enderecoEntrega);
                    cliente.Departamentos.Add(Departamento);
                    return cliente;
                }, new { Id = id });
            return clientes.SingleOrDefault();
        }


        public void Insert(Clientes cliente)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string Sql = "INSERT INTO Clientes(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                cliente.Id = _connection.Query<int>(Sql, cliente, transaction).Single();

                if (cliente.Contato != null)
                {
                    cliente.Contato.clienteId = cliente.Id;
                    string sqlContato = "INSERT INTO Contatos(ClientesId, Telefone, Celular) VALUES (@ClientesId, @Telefone, @Celular); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    cliente.Contato.Id = _connection.Query<int>(sqlContato, cliente.Contato, transaction).Single();
                }

                if (cliente.EnderecosEntrega != null && cliente.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in cliente.EnderecosEntrega)
                    {
                        enderecoEntrega.ClienteId = cliente.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(ClientesId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@ClientesId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }
                
                if (cliente.Departamentos != null && cliente.Departamentos.Count > 0)
                {
                    foreach ( var departamentos in cliente.Departamentos)
                    {
                        departamentos.Id = cliente.Id;
                        string sqlDepartamento = "INSERT INTO Departamentos(Id, Nome) VALUES (@Id, @Nome); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        departamentos.Id = _connection.Query<int>(sqlDepartamento, departamentos, transaction).Single();
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



        public void Update(Clientes cliente)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string Sql = "UPDATE Cliente SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id";
                _connection.Execute(Sql, cliente, transaction);
                if (cliente.Contato != null)
                {
                    string sqlContato = "UPDATE Contatos SET ClientesId = @ClientesId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id ";
                    _connection.Execute(sqlContato, cliente.Contato, transaction);
                }

                string sqlDeletarEnderecoEntrega = "DELETE FROM EnderecoEntrega WHERE ClientesId = @Id";
                _connection.Execute(sqlDeletarEnderecoEntrega, cliente, transaction);
                if (cliente.EnderecosEntrega != null && cliente.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in cliente.EnderecosEntrega)
                    {
                        enderecoEntrega.ClienteId = cliente.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(ClientesId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@ClientesId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                string sqlDeletarClienteDepartamentos = "DELETE FROM ClienteDepartamentos WHERE ClienteId = @Id";
                _connection.Execute(sqlDeletarClienteDepartamentos, cliente, transaction);
                if (cliente.EnderecosEntrega != null && cliente.EnderecosEntrega.Count > 0)
                {
                    foreach (var Departamentos in cliente.EnderecosEntrega)
                    {
                        Departamentos.ClienteId = cliente.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(ClientesId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@ClientesId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        Departamentos.Id = _connection.Query<int>(sqlEndereco, Departamentos, transaction).Single();
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
            _connection.Execute("DELETE FROM Cliente WHERE Id = @id", new { Id = id });
        }
    }
}


