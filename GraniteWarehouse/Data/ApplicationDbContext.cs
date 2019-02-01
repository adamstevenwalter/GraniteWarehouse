using System;
using System.Collections.Generic;
using System.Text;
using GraniteWarehouse.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GraniteWarehouse.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProductTypes> ProductTypes { get; set; }
        public DbSet<SpecialTags> SpecialTags { get; set; }
    }
}
