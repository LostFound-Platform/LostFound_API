using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class FBLADbContextFactory : IDesignTimeDbContextFactory<FBLADbContext>
    {
        // Create DBContext without DI
        public FBLADbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<FBLADbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyConnection"));

            return new FBLADbContext(optionsBuilder.Options);
        }
    }
}
