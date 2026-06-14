using Microsoft.EntityFrameworkCore;

public class GimnasioContext : DbContext
{
    public GimnasioContext(DbContextOptions<GimnasioContext> options) : base(options)
    {
    }
    public DbSet<Ejercicio> ejercicios { get; set; }
}