using ApiPeliculas.Models;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface ICategoriaRepositorio
    {
        ICollection<Categoria> GetCategorias();
        Categoria GetCategoria(int CategoriaId);
        bool ExisteCategoria(int CategoriaId);
        bool ExisteCategoria(string NombreCategoria);
        bool CrearCategoria(Categoria Categoria);
        bool ActualizarCategoria(Categoria Categoria);
        bool BorrarCategoria(Categoria Categoria);
        bool Guardar();

    }
}
