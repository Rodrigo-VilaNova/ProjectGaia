using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectGaia.Server.Migrations
{
    /// <inheritdoc />
    public partial class CurrentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AccessLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ErrorLogs_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Consumption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Invoices_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sessions_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "ID", "Email", "Name", "Password", "Status", "Type" },
                values: new object[,]
                {
                    { 1, "Admin0@gmail.com", "Admin Zero", new byte[] { 247, 29, 251, 37, 229, 79, 87, 173, 203, 118, 106, 6, 27, 122, 61, 92, 47, 129, 199, 213, 132, 30, 15, 48, 225, 44, 65, 100, 237, 61, 145, 253 }, 0, 1 },
                    { 2, "Admin1@gmail.com", "Admin One", new byte[] { 47, 71, 65, 19, 18, 3, 186, 233, 46, 141, 163, 36, 76, 229, 140, 187, 140, 197, 118, 181, 178, 58, 231, 52, 75, 160, 190, 23, 148, 68, 240, 76 }, 0, 1 },
                    { 3, "User0@gmail.com", "User Zero", new byte[] { 174, 189, 228, 229, 148, 10, 6, 101, 193, 44, 194, 80, 93, 44, 201, 123, 198, 131, 130, 47, 90, 195, 11, 203, 62, 147, 230, 151, 81, 26, 2, 6 }, 0, 0 },
                    { 4, "User1@gmail.com", "User One", new byte[] { 98, 67, 122, 103, 110, 207, 70, 228, 253, 179, 10, 45, 140, 113, 234, 30, 125, 201, 126, 31, 172, 234, 96, 222, 220, 174, 216, 30, 88, 42, 183, 192 }, 0, 0 },
                    { 5, "User2@gmail.com", "User Two", new byte[] { 102, 128, 196, 211, 221, 116, 187, 125, 117, 37, 99, 136, 180, 215, 228, 98, 85, 207, 81, 185, 127, 182, 55, 168, 2, 68, 191, 51, 64, 215, 80, 236 }, 0, 0 },
                    { 6, "User3@gmail.com", "User Three", new byte[] { 180, 219, 17, 148, 40, 54, 29, 85, 38, 187, 213, 122, 249, 23, 215, 100, 124, 107, 147, 25, 205, 67, 144, 19, 13, 169, 103, 180, 11, 115, 1, 176 }, 1, 0 },
                    { 7, "User4@gmail.com", "User Four", new byte[] { 72, 4, 248, 39, 87, 27, 96, 64, 217, 166, 186, 70, 149, 133, 141, 129, 64, 102, 178, 253, 129, 107, 142, 145, 89, 79, 126, 172, 169, 185, 102, 22 }, 1, 0 }
                });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "ID", "AccountID", "Consumption", "EmissionDate", "Price", "UploadDate" },
                values: new object[] { 1, 3, 2m, new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 3m, new DateTime(2025, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Sessions",
                columns: new[] { "ID", "AccountID", "Expiration", "Token" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2025, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "B+RuP8JimIAmX/6/iN/VJDJko2bOcJMMSIOuesKn384=" },
                    { 2, 4, new DateTime(2025, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "7f3YlmAyhNHm1MTXCSUiFNbRfb8+PNUSfj3Fb0vCrvY=" },
                    { 3, 5, new DateTime(2025, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "x1bTdbciDgLLY67E4by8tv9UaVOgNqkfyBFIIB9GIBM=" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_AccountID",
                table: "AccessLogs",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_AccountID",
                table: "ErrorLogs",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AccountID",
                table: "Invoices",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_AccountID",
                table: "Sessions",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Token",
                table: "Sessions",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLogs");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
