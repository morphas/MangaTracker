using MangaTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona as ferramentas do sistema
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra o serviço que guarda seus mangás
builder.Services.AddSingleton<IBibliotecaService, BibliotecaService>();

var app = builder.Build();

// Carrega seus dados (JSON) assim que o app liga
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IBibliotecaService>();
    service.CarregarDados();
}

// Configura o Swagger para testes
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// --- PEÇAS DO SEU SITE ---
app.UseDefaultFiles(); // Ativa o index.html como página principal
app.UseStaticFiles();  // Dá permissão para ler a pasta wwwroot
// -------------------------

app.MapControllers();

app.Run();