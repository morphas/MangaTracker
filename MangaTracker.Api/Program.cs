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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("X-User-Id", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-User-Id",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o GUID do usuário logado"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "X-User-Id"
                }
            },
            new string[] {}
        }
    });
});

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

    // 🔥 CRIA ADMIN INICIAL SE NÃO EXISTIR
    var service = scope.ServiceProvider.GetRequiredService<IBibliotecaService>();
    service.CarregarDados();
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