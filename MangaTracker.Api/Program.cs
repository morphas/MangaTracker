using MangaTracker.Api.Data;
using MangaTracker.Api.Services;
using MangaTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// CONFIGURAÇÕES BÁSICAS
// ==========================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================
// CONNECTION STRING
// ==========================

var connectionString = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("ConnectionStrings:Default não configurada. Verifique appsettings.json ou variáveis do Render.");
}

builder.Services.AddDbContext<MangaTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==========================
// SERVICE PRINCIPAL
// ==========================

builder.Services.AddScoped<IBibliotecaService, BibliotecaServiceDb>();

var app = builder.Build();

// ==========================
// APLICA MIGRATIONS AUTOMATICAMENTE
// ==========================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MangaTrackerDbContext>();
    db.Database.Migrate();
}

// ==========================
// SWAGGER (DEV ONLY)
// ==========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================
// PIPELINE
// ==========================

app.UseHttpsRedirection();
app.UseAuthorization();

// Ativa site estático
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();