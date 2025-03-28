using System.Text;
using Microsoft.EntityFrameworkCore;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Data
{
    public class TestDbContext : AppDbContext
    {
        public TestDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void SetInitialData(ModelBuilder modelBuilder)
        {
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

            ConfirmationService confirmationService = new ConfirmationService();

            modelBuilder.Entity<Confirmation>().HasData(
                new Confirmation
                {
                    ID = 1,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("UserFiveToken"))),
                    Expiration = new DateTime(3025, 2, 2),
                    Name = "User Five",
                    Email = "User5@gmail.com",
                    Password = passwordService.HashPassword("User5@gmail.com")
                },
                new Confirmation
                {
                    ID = 2,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("UserSixToken"))),
                    Expiration = new DateTime(1025, 2, 2),
                    Name = "User Six",
                    Email = "User6@gmail.com",
                    Password = passwordService.HashPassword("User6@gmail.com")
                }
            );

            modelBuilder.Entity<Recovery>().HasData(
                new Recovery
                {
                    ID = 1,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("UserZeroToken"))),
                    Expiration = new DateTime(3025, 2, 2),
                    AccountID = 3
                },
                new Recovery
                {
                    ID = 2,
                    Token = Convert.ToHexString(confirmationService.HashToken(Encoding.UTF8.GetBytes("UserOneToken"))),
                    Expiration = new DateTime(1025, 2, 2),
                    AccountID = 4
                }
            );

            TokenService tokenService = new TokenService();

            modelBuilder.Entity<Session>().HasData(
                new Session
                {
                    ID = 1,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("UserZeroToken"))),
                    Expiration = new DateTime(3025, 2, 28),
                    AccountID = 3
                },
                new Session
                {
                    ID = 2,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("UserOneToken"))),
                    Expiration = new DateTime(3025, 2, 28),
                    AccountID = 4
                },
                new Session
                {
                    ID = 3,
                    Token = Convert.ToBase64String(tokenService.HashToken(Encoding.UTF8.GetBytes("UserTwoToken"))),
                    Expiration = new DateTime(3025, 2, 28),
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

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    ID = 1,
                    Name = "Pagamento 1",
                    Description = "Descrição do evento",
                    Date = new DateTime(2025, 3, 15),
                    Type = EventType.Payment,
                    AccountID = 3
                }
            );
        }
    }
}
