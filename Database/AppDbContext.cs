using booking_api.Models;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels { get; set; }
}
