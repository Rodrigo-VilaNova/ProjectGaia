using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<Recovery> Recoveries { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Event> Events { get; set; }
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

            modelBuilder.Entity<Recovery>().ToTable("Recoveries");
            modelBuilder.Entity<Recovery>().Property(c => c.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Recovery>().HasIndex(c => c.AccountID);
            modelBuilder.Entity<Recovery>().HasIndex(c => c.Token).IsUnique();
            modelBuilder.Entity<Recovery>().HasIndex(c => c.Expiration);
            modelBuilder.Entity<Recovery>().HasIndex(c => c.AccountID).IsUnique();

            modelBuilder.Entity<Session>().ToTable("Sessions");
            modelBuilder.Entity<Session>().Property(s => s.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Session>().HasIndex(s => s.AccountID);
            modelBuilder.Entity<Session>().HasIndex(s => s.Token).IsUnique();
            modelBuilder.Entity<Session>().HasIndex(s => s.Expiration);
            modelBuilder.Entity<Session>().HasOne(s => s.Account).WithMany(a => a.Sessions);

            modelBuilder.Entity<Invoice>().ToTable("Invoices");
            modelBuilder.Entity<Invoice>().Property(i => i.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Invoice>().HasIndex(i => i.AccountID);
            modelBuilder.Entity<Invoice>().HasOne(i => i.Account).WithMany(a => a.Invoices);
            modelBuilder.Entity<Invoice>().Property(i => i.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Invoice>().Property(i => i.Consumption).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Event>().ToTable("Events");
            modelBuilder.Entity<Event>().Property(e => e.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<Event>().HasIndex(e => e.AccountID);
            modelBuilder.Entity<Event>().HasOne(e => e.Account).WithMany(a => a.Events);
            modelBuilder.Entity<Event>().HasIndex(e => new { e.AccountID, e.Date });

            modelBuilder.Entity<AccessLog>().ToTable("AccessLogs");
            modelBuilder.Entity<AccessLog>().Property(a => a.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<AccessLog>().HasIndex(a => a.AccountID);
            modelBuilder.Entity<AccessLog>().HasOne(a => a.Account).WithMany(a => a.AccessLogs);

            modelBuilder.Entity<ErrorLog>().ToTable("ErrorLogs");
            modelBuilder.Entity<ErrorLog>().Property(e => e.ID).ValueGeneratedOnAdd();
            modelBuilder.Entity<ErrorLog>().HasIndex(e => e.AccountID);
            modelBuilder.Entity<ErrorLog>().HasOne(e => e.Account).WithMany(a => a.ErrorLogs);

            SetInitialData(modelBuilder);
        }

        protected virtual void SetInitialData(ModelBuilder modelBuilder)
        {
            PasswordService passwordService = new PasswordService();

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    ID = 1,
                    Name = "DemoUser0@example.com",
                    Email = "DemoUser0@example.com",
                    Password = passwordService.HashPassword("DemoUser0@example.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                },
                new Account
                {
                    ID = 2,
                    Name = "DemoUser1@example.com",
                    Email = "DemoUser1@example.com",
                    Password = passwordService.HashPassword("DemoUser1@example.com"),
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                }
            );

            ConfirmationService confirmationService = new ConfirmationService();

            modelBuilder.Entity<Confirmation>().HasData(
                new Confirmation
                {
                    ID = 1,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("DemoUser2Token"))),
                    Expiration = new DateTime(3025, 2, 2),
                    Name = "DemoUser2@example.com",
                    Email = "DemoUser2@example.com",
                    Password = passwordService.HashPassword("DemoUser2@example.com")
                },
                new Confirmation
                {
                    ID = 2,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("DemoUser3Token"))),
                    Expiration = new DateTime(1025, 2, 2),
                    Name = "DemoUser3@example.com",
                    Email = "DemoUser3@example.com",
                    Password = passwordService.HashPassword("DemoUser3@example.com")
                }
            );

            modelBuilder.Entity<Recovery>().HasData(
                new Recovery
                {
                    ID = 1,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("DemoUser0Token"))),
                    Expiration = new DateTime(3025, 2, 2),
                    AccountID = 1
                },
                new Recovery
                {
                    ID = 2,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("DemoUser1Token"))),
                    Expiration = new DateTime(1025, 2, 2),
                    AccountID = 2
                }
            );

            TokenService tokenService = new TokenService();

            modelBuilder.Entity<Session>().HasData(
                new Session
                {
                    ID = 1,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("DemoUser0Token"))),
                    Expiration = new DateTime(3025, 2, 28),
                    AccountID = 1
                }
            );

            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    ID = 1,
                    Price = 7,
                    Consumption = 50,
                    EmissionDate = new DateTime(2025, 1, 16),
                    UploadDate = new DateTime(2025, 1, 18),
                    AccountID = 1
                }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    ID = 1,
                    Name = "Payment 1",
                    Description = "Payment to the energy provider",
                    Date = new DateTime(2025, 4, 15),
                    Type = EventType.Payment,
                    AccountID = 1
                }
            );
        }
    }
}
