using System.Text;
using Humanizer.Bytes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Confirmation> Confirmations { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<Account>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Account>().HasIndex(a => a.Email).IsUnique();

            modelBuilder.Entity<Confirmation>().ToTable("Confirmations");
            modelBuilder.Entity<Confirmation>().Property(c => c.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Confirmation>().HasIndex(c => c.Token).IsUnique();
            modelBuilder.Entity<Confirmation>().HasIndex(c => c.Expiration);
            modelBuilder.Entity<Confirmation>().HasIndex(c => c.Email).IsUnique();

            modelBuilder.Entity<Session>().ToTable("Sessions");
            modelBuilder.Entity<Session>().Property(s => s.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Session>().HasIndex(s => s.Token).IsUnique();
            modelBuilder.Entity<Session>().HasIndex(s => s.Expiration);
            modelBuilder.Entity<Session>().HasOne(s => s.Account).WithMany(a => a.Sessions);

            modelBuilder.Entity<Invoice>().ToTable("Invoices");
            modelBuilder.Entity<Invoice>().Property(i => i.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Invoice>().HasIndex(i => i.AccountID);
            modelBuilder.Entity<Invoice>().HasOne(i => i.Account).WithMany(a => a.Invoices);
            modelBuilder.Entity<Invoice>().Property(i => i.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Invoice>().Property(i => i.Consumption).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AccessLog>().ToTable("AccessLogs");
            modelBuilder.Entity<AccessLog>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<AccessLog>().HasOne(a => a.Account).WithMany(a => a.AccessLogs);

            modelBuilder.Entity<ErrorLog>().ToTable("ErrorLogs");
            modelBuilder.Entity<ErrorLog>().Property(e => e.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<ErrorLog>().HasOne(e => e.Account).WithMany(a => a.ErrorLogs);

            PasswordService passwordService = new PasswordService();

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    ID = 1,
                    Name = "Admin Zero",
                    Email = "Admin0@gmail.com",
                    Password = passwordService.HashPassword("Admin0@gmail.com"),
                    Type = AccountType.Admin,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 2,
                    Name = "Admin One",
                    Email = "Admin1@gmail.com",
                    Password = passwordService.HashPassword("Admin1@gmail.com"),
                    Type = AccountType.Admin,
                    Status = AccountStatus.Active,
                },
                new Account
                {
                    ID = 3,
                    Name = "User Zero",
                    Email = "User0@gmail.com",
                    Password = passwordService.HashPassword("User0@gmail.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 4,
                    Name = "User One",
                    Email = "User1@gmail.com",
                    Password = passwordService.HashPassword("User1@gmail.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 5,
                    Name = "User Two",
                    Email = "User2@gmail.com",
                    Password = passwordService.HashPassword("User2@gmail.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 6,
                    Name = "User Three",
                    Email = "User3@gmail.com",
                    Password = passwordService.HashPassword("User3@gmail.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Blocked
                },
                new Account
                {
                    ID = 7,
                    Name = "User Four",
                    Email = "User4@gmail.com",
                    Password = passwordService.HashPassword("User4@gmail.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Blocked
                }
            );

            TokenService tokenService = new TokenService();

            modelBuilder.Entity<Session>().HasData(
                new Session
                {
                    ID = 1,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("UserZeroToken"))),
                    Expiration = new DateTime(2025, 2, 28),
                    AccountID = 3
                },
                new Session
                {
                    ID = 2,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("UserOneToken"))),
                    Expiration = new DateTime(2025, 2, 28),
                    AccountID = 4
                },
                new Session
                {
                    ID = 3,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("UserTwoToken"))),
                    Expiration = new DateTime(2025, 2, 28),
                    AccountID = 5
                }
            );

            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    ID = 1,
                    Price = 3,
                    Consumption = 2,
                    EmissionDate = new DateTime(2025, 1, 16),
                    UploadDate = new DateTime(2025, 1, 18),
                    AccountID = 3
                }
            );
        }
    }
}
