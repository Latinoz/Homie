using System.Linq;

namespace Homie.Areas.Series.Models {

    public interface IMovieRepository {

        IQueryable<Movies> Products { get; }
    }
}
