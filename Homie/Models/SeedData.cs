using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Homie.Areas.Series.Models;

namespace Homies.Models {

    public static class SeedData {

        public static void EnsurePopulated(IApplicationBuilder app) {
            ApplicationDbContext context = app.ApplicationServices
                .GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            if (!context.Movies.Any()) {
                context.Movies.AddRange(
                    new Movies {
                        Name = "Kayak",
                        Season = 1,
                        Episode = 4
                    },
                    new Movies {
                        Name = "Lifejacket",
                        Season = 1,
                        Episode = 3
                    },
                    new Movies {
                        Name = "Soccer Ball",
                        Season = 1,
                        Episode = 5
                    },
                    new Movies {
                        Name = "Corner Flags",
                        Season = 1,
                        Episode = 7
                    }
                    
                );
                context.SaveChanges();
            }
        }
    }
}
