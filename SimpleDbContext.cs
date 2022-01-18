using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace simple.dbapp
{
    public class SimpleDbContext : DbContext
    {
        public SimpleDbContext()
        {
            //Blank constructor for tests
        }
        public SimpleDbContext(DbContextOptions<SimpleDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // enable seeing the likes of SQL query parameters in logs
            optionsBuilder.EnableSensitiveDataLogging();

            // need more error info? TODO make this a config option 
            optionsBuilder.EnableDetailedErrors();

            optionsBuilder.UseSqlServer("Server=tcp:localhost,1433;Initial Catalog=SimpleDb;Persist Security Info=False;User ID=sa;Password=SOMEPASSWORD;Encrypt=False;Max Pool Size=500;Pooling=True;", b =>
            {
                b.MigrationsAssembly("App");
                b.EnableRetryOnFailure(
                    maxRetryCount: 4,
                    maxRetryDelay: TimeSpan.FromSeconds(1),
                    errorNumbersToAdd: new int[] { }
                );
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            });

            optionsBuilder.ConfigureWarnings(w => w.Throw(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(
                p =>
                {
                    p.ToTable("People");
                    p.HasKey(u => u.Id).HasName("PK_People");
                    
                    p.HasOne(p => p.Vet)
                        .WithMany(v => v.Customers)
                        .HasForeignKey(p => p.VetId);

                    p.HasMany(e => e.Pets)
                        .WithOne(p => p.Owner)
                        .HasForeignKey(p => p.OwnerId);
                }
            );

            modelBuilder.Entity<Pet>(
                p =>
                {
                    p.ToTable("Pets");
                    p.HasKey(u => u.Id).HasName("PK_Pets");
                    p.HasOne(p => p.Owner)
                        .WithMany(v => v.Pets)
                        .HasForeignKey(p => p.OwnerId);
                }
            );

            modelBuilder.Entity<Vet>(
                p =>
                {
                    p.ToTable("Vets");
                    p.HasKey(u => u.Id).HasName("PK_Vets");
                    p.HasMany(v => v.Customers)
                        .WithOne(c => c.Vet)
                        .HasForeignKey(c => c.VetId);
                }
            );
        }

        public virtual DbSet<Pet> Pets { get; set; }
        public virtual DbSet<Vet> Vets { get; set; }
        public virtual DbSet<Person> People { get; set; }

    }
}