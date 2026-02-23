public class Manga
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titulo { get; set; } = "";

    public int? TotalCapitulos { get; set; } = null;

    // NOVO:
    public bool LancadoNoBrasil { get; set; } = false;
    public string? Editora { get; set; } = null;

    // NOVO: para facilitar filtro por editora
    public string? EditoraKey { get; set; } = null;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}