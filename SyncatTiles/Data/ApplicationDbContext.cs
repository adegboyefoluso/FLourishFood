using Microsoft.EntityFrameworkCore;
using SyncatTiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncatTiles.Data
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<ApplicationType> ApplicationType { get; set; }
        public DbSet<Product> Product { get; set; }
    }
}
