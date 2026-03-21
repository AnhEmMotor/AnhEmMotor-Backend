using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DBContexts;

public class PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options) : ApplicationDBContext(options)
{
}
