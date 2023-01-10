using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DNIDb>(opt => opt.UseInMemoryDatabase("DNIList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/dni", async (DNIDb db) =>
    await db.DNIs.ToListAsync());

app.MapGet("/dni/{numero}", async (int numero, DNIDb db) =>
    await db.DNIs.FindAsync(numero)
        is DNI dni
            ? Results.Ok(dni)
            : Results.NotFound());

app.MapPost("/dni", async (DNI dni, DNIDb db) =>
{
    db.DNIs.Add(dni);
    await db.SaveChangesAsync();

    return Results.Created($"/dni/{dni.Numero}", dni);
});

app.MapPut("/dni/{numero}", async (int numero, DNI inputDNI, DNIDb db) =>
{
    var dni = await db.DNIs.FindAsync(numero);

    if (dni is null) return Results.NotFound();

    dni.Nombre = inputDNI.Nombre;
    dni.Apellido= inputDNI.Apellido;
    dni.Sexo = inputDNI.Sexo;
    dni.FechaNacimiento = inputDNI.FechaNacimiento;
    dni.Tramite= inputDNI.Tramite;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/dni/{numero}", async (int numero, DNIDb db) =>
{
    if (await db.DNIs.FindAsync(numero) is DNI dni)
    {
        db.DNIs.Remove(dni);
        await db.SaveChangesAsync();
        return Results.Ok(dni);
    }

    return Results.NotFound();
});

app.Run();

class DNI
{
    [Key]
    public int Numero { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public char? Sexo { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int Tramite { get; set; }
}

class DNIDb : DbContext
{
    public DNIDb(DbContextOptions<DNIDb> options)
        : base(options) { }

    public DbSet<DNI> DNIs => Set<DNI>();
}