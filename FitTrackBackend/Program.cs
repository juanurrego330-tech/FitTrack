using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

var conexion = builder.Configuration.GetConnectionString("CadenaPostgres");
builder.Services.AddDbContext<GimnasioContext>(options => options.UseNpgsql(conexion));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/ejercicios", async (GimnasioContext db) =>
{
    return await db.ejercicios.ToListAsync();
});

app.MapPost("/ejercicios", async (GimnasioContext db, Ejercicio nuevoEjercicio) =>
{
    db.ejercicios.Add(nuevoEjercicio);
    await db.SaveChangesAsync();
    return Results.Created($"/ejercicios/{nuevoEjercicio.id}", nuevoEjercicio);
});

app.MapDelete("/ejercicios/{id}", async (GimnasioContext db, int id) =>
{
    var ejercicio = await db.ejercicios.FindAsync(id);
    if (ejercicio is null) return Results.NotFound();
    db.ejercicios.Remove(ejercicio);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GimnasioContext>();
        // Verifica si hay migraciones pendientes en el código que no existan en Render
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error crítico: No se pudieron aplicar las migraciones en el arranque.");
    }
}
app.Run();