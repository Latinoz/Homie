using Homies.Models;
using System.Collections.Generic;
using System.Linq;

namespace Homie.Areas.Series.Models
{

    public class EFMoviesRepository : IMovieRepository {

        private ApplicationDbContext context;

        public EFMoviesRepository(ApplicationDbContext ctx) {
            context = ctx;
        }

        public IQueryable<Movies> Products => context.Movies;
    }
}
