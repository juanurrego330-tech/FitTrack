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

app.MapGet("/ejercicios", async (int? usuarioId, GimnasioContext context) =>
{
    if (usuarioId == null)
    {
        return Results.BadRequest(new { mensaje = "Es necesario el usuarioId para cargar las rutinas." });
    }

    // Corregido: Filtra por el dueño del ejercicio
    var misEjercicios = await context.ejercicios
        .Where(e => e.usuario_id == usuarioId)
        .ToListAsync();

    return Results.Ok(misEjercicios);
});

app.MapPost("/ejercicios", async (Ejercicio nuevoEjercicio, GimnasioContext context) =>
{
    // Corregido: Valida que el JSON traiga el usuario_id de quien entrena
    if (nuevoEjercicio.usuario_id == null || nuevoEjercicio.usuario_id == 0)
    {
        return Results.BadRequest(new { mensaje = "El ejercicio debe tener un usuario asignado." });
    }

    context.ejercicios.Add(nuevoEjercicio);
    await context.SaveChangesAsync();

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

app.MapPost("/api/auth/register", async (UsuarioRegistroDto dto, GimnasioContext context) =>
{
    if (string.IsNullOrEmpty(dto.correo) || string.IsNullOrEmpty(dto.password))
    {
        return Results.BadRequest(new { mensaje = "El correo y la contraseña son obligatorios." });
    }

    bool existe = await context.usuarios.AnyAsync(u => u.correo == dto.correo);
    if (existe)
    {
        return Results.BadRequest(new { mensaje = "El correo ya está registrado." });
    }

    // Encriptamos la contraseña con BCrypt antes de guardarla
    string hash = BCrypt.Net.BCrypt.HashPassword(dto.password);

    var nuevoUsuario = new Usuario
    {
        nombre = dto.nombre,
        correo = dto.correo,
        password_hash = hash
    };

    context.usuarios.Add(nuevoUsuario);
    await context.SaveChangesAsync();

    return Results.Ok(new { mensaje = "Usuario registrado con éxito", id = nuevoUsuario.id });
});

app.MapPost("/api/auth/login", async (UsuarioLoginDto dto, GimnasioContext context) =>
{
    if (string.IsNullOrEmpty(dto.correo) || string.IsNullOrEmpty(dto.password))
    {
        return Results.BadRequest(new { mensaje = "El correo y la contraseña son obligatorios." });
    }

    var usuario = await context.usuarios.FirstOrDefaultAsync(u => u.correo == dto.correo);
    if (usuario == null)
    {
        return Results.BadRequest(new { mensaje = "Correo o contraseña incorrectos." });
    }

    // Verificamos si la contraseña coincide con el hash guardado
    bool passwordValido = BCrypt.Net.BCrypt.Verify(dto.password, usuario.password_hash);
    if (!passwordValido)
    {
        return Results.BadRequest(new { mensaje = "Correo o contraseña incorrectos." });
    }

    return Results.Ok(new { 
        mensaje = "Inicio de sesión exitoso", 
        usuarioId = usuario.id,
        nombre = usuario.nombre,
        correo = usuario.correo
    });
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