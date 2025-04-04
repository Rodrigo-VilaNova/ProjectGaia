using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProjectGaia.Server.Data
{
    /// <summary>
    /// Fábrica de contexto para criação de uma instância de <see cref="AppDbContext"/> durante o design-time.
    /// Esta classe é utilizada pelo EF Core para operações como migrações e scaffold da base de dados.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Cria uma instância de <see cref="AppDbContext"/> durante o design-time.
        /// Este método é utilizado pelo EF Core para configurar o contexto da base de dados,
        /// especialmente durante a execução de migrações ou outras operações de scaffolding.
        /// </summary>
        /// <param name="args">Argumentos passados (geralmente não utilizados em cenários de migração).</param>
        /// <returns>Uma instância configurada de <see cref="AppDbContext"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada se a string de conexão não for encontrada na configuração da aplicação.
        /// </exception>
        public AppDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
