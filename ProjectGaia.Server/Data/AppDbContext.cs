namespace ProjectGaia.Server.Data
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ProjectGaia.Server.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Account>().HasIndex(a => a.Email).IsUnique();

            modelBuilder.Entity<Session>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Session>().HasIndex(a => a.Token).IsUnique();
            modelBuilder.Entity<Session>().HasOne(a => a.Account).WithMany(a => a.Sessions);

            modelBuilder.Entity<AccessLog>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<AccessLog>().HasOne(a => a.Account).WithMany(a => a.AccessLogs);

            modelBuilder.Entity<ErrorLog>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<ErrorLog>().HasOne(a => a.Account).WithMany(a => a.ErrorLogs);


            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    ID = 1,
                    Name = "Patient Zero",
                    Email = "patientzero@gmail.com",
                    Password = [],
                    Type = AccountType.Admin,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 2,
                    Name = "Patient One",
                    Email = "patientone@gmail.com",
                    Password = [],
                    Type = AccountType.Admin,
                    Status = AccountStatus.Blocked
                },
                new Account
                {
                    ID = 3,
                    Name = "User Zero",
                    Email = "userzero@gmail.com",
                    Password = [],
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 4,
                    Name = "User One",
                    Email = "userone@gmail.com",
                    Password = [],
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 5,
                    Name = "User Two",
                    Email = "usertwo@gmail.com",
                    Password = [],
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 6,
                    Name = "User Three",
                    Email = "userthree@gmail.com",
                    Password = [],
                    Type = AccountType.User,
                    Status = AccountStatus.Blocked
                },
                new Account
                {
                    ID = 7,
                    Name = "User Four",
                    Email = "userfour@gmail.com",
                    Password = [],
                    Type = AccountType.User,
                    Status = AccountStatus.Blocked
                }
            );
        }
    }

}
