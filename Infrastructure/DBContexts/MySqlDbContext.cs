using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DBContexts;

public class MySqlDbContext(DbContextOptions<MySqlDbContext> options) : ApplicationDBContext(options)
{
}
