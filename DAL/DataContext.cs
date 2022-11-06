using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DataContext : DbContext
    {
        //передача options в родительский класс
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Console.WriteLine("dc " + Guid.NewGuid());

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(f => f.Email)
                .IsUnique();

            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
            modelBuilder.Entity<Post>().ToTable(nameof(Posts));
        }
        //использование пакета npgsql для миграций. Указываем билдеру, что миграции будут находить в сборке Api
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("Api"));
        //добавляем сущность User в качестве DbSet'a
        public DbSet<User> Users => Set<User>();
        //добавляем сессии
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
        public DbSet<Post> Posts => Set<Post>();
    }
}
