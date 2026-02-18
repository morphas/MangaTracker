using MangaTracker.Services;

var app = builder.Build();

// Bloco para carregar os dados do JSON assim que a API sobe
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IBibliotecaService>();
    service.CarregarDados();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
