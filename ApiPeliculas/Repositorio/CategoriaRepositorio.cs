using ApiPeliculas.Data;
using ApiPeliculas.Models;
using ApiPeliculas.Repositorio.IRepositorio;

namespace ApiPeliculas.Repositorio
{
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly ApplicationDbContext _db;
        public CategoriaRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool ActualizarCategoria(Categoria Categoria)
        {
            Categoria.FechaCreacion = DateTime.Now;
            var categoriaExistente = _db.Categorias.Find(Categoria.Id);
            if (categoriaExistente != null)
            {
                _db.Entry(categoriaExistente).CurrentValues.SetValues(Categoria);
            }
            else 
            {
                _db.Categorias.Update(Categoria);
            }

            return Guardar();
        }

        public bool BorrarCategoria(Categoria Categoria)
        {
            _db.Categorias.Remove(Categoria);
            return Guardar();
        }

        public bool CrearCategoria(Categoria Categoria)
        {
            Categoria.FechaCreacion = DateTime.Now;
            _db.Categorias.Add(Categoria);
            return Guardar();
        }

        public bool ExisteCategoria(int CategoriaId)
        {
            return _db.Categorias.Any(c => c.Id == CategoriaId);
        }

        public bool ExisteCategoria(string Nombre)
        {
            bool valor = _db.Categorias.Any(c => c.NombreCategoria.ToLower().Trim() == Nombre.ToLower().Trim());
            return valor;
        }

        public Categoria GetCategoria(int CategoriaId)
        {
            return _db.Categorias.FirstOrDefault(c => c.Id == CategoriaId);
        }

        public ICollection<Categoria> GetCategorias()
        {
            return _db.Categorias.OrderBy(c => c.NombreCategoria).ToList();
        }

        public bool Guardar()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }
    }
}
