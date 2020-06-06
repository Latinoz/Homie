using Homie;
using Homie.Areas.Series.Models;
using Homie.Areas.Cigars.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Homie.Areas.Identity.Models;

namespace Homies.Data.Models {

    public class ApplicationDbContext : IdentityDbContext<User>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<MoviesModel> MoviesEF { get; set; }
        public DbSet<CigarsModel> CigarsEF { get; set; }
        public DbSet<Format> FormatsEF { get; set; }

    }
    
}
