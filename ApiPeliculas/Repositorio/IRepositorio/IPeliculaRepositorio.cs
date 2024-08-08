using ApiPeliculas.Models;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IPeliculaRepositorio
    {
        //v1
        //ICollection<Pelicula> GetPeliculas();
        //v2
        ICollection<Pelicula> GetPeliculas(int pageNumber, int pageSize);
        int GetTotalPeliculas();
        ICollection<Pelicula> GetPeliculasEnCategoria(int catId);
        IEnumerable<Pelicula> BuscarPelicula(string nombre);

        Pelicula GetPelicula(int peliculaId);
        bool ExistePelicula(int id);
        bool ExistePelicula(string nombre);

        bool CrearPelicula(Pelicula pelicula);
        bool ActualizarPelicula(Pelicula pelicula);
        bool BorrarPelicula(Pelicula pelicula);
        bool Guardar();

    }
}
