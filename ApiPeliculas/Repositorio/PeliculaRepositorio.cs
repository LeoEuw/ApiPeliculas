using ApiPeliculas.Data;
using ApiPeliculas.Models;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio
{
    public class PeliculaRepositorio : IPeliculaRepositorio
    {
        private readonly ApplicationDbContext _db;
        public PeliculaRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool ActualizarPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            var peliculaExistente = _db.Pelicula.Find(pelicula.Id);
            if (peliculaExistente != null)
            {
                _db.Entry(peliculaExistente).CurrentValues.SetValues(pelicula);
            }
            else
            {
                _db.Pelicula.Update(pelicula);
            }
            

            return Guardar();
        }

        public bool BorrarPelicula(Pelicula pelicula)
        {
            _db.Pelicula.Remove(pelicula);
            return Guardar();
        }

        public IEnumerable<Pelicula> BuscarPelicula(string nombre)
        {
            IQueryable<Pelicula> query = _db.Pelicula;
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
            }
            return query.ToList();
        }

        public bool CrearPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            _db.Pelicula.Add(pelicula);
            return Guardar();
        }

        public bool ExistePelicula(int id)
        {
            return _db.Pelicula.Any(c => c.Id == id);
        }

        public bool ExistePelicula(string nombre)
        {
            bool valor = _db.Pelicula.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public Pelicula GetPelicula(int peliculaId)
        {
            return _db.Pelicula.FirstOrDefault(c => c.Id == peliculaId);
        }

        //v1
        //public ICollection<Pelicula> GetPeliculas()
        //{
        //    return _db.Pelicula.OrderBy(c => c.Nombre).ToList();
        //}

        //v2
        public ICollection<Pelicula> GetPeliculas(int pageNumber, int pageSize)
        {
            return _db.Pelicula.OrderBy(c => c.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        public int GetTotalPeliculas()
        {
            return _db.Pelicula.Count();
        }

        public ICollection<Pelicula> GetPeliculasEnCategoria(int catId)
        {
            return _db.Pelicula.Include(ca => ca.Categoria).Where(ca => ca.CategoriaId == catId).ToList();
        }

        public bool Guardar()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }
    }
}
