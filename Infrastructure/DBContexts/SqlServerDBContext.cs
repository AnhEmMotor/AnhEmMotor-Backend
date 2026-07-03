using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DBContexts;

public class SqlServerDBContext(DbContextOptions<SqlServerDBContext> options) : ApplicationDBContext(options)
{
}
