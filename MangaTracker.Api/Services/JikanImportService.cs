using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MangaTracker.Services;

namespace MangaTracker.Api.Services
{
    public record JikanImportResult(int Importados, int Ignorados, int Erros, IReadOnlyList<string> Mensagens);

    public interface IJikanImportService
    {
        Task<JikanImportResult> ImportarPaginaAsync(int page = 1, int limit = 25, CancellationToken ct = default);
    }

    /// <summary>Importa mangás da API Jikan (MyAnimeList) para o catálogo. LancadoNoBrasil = false.</summary>
    public class JikanImportService : IJikanImportService
    {
        private const string JikanBase = "https://api.jikan.moe/v4";
        private readonly HttpClient _http;
        private readonly IBibliotecaService _biblioteca;

        public JikanImportService(HttpClient http, IBibliotecaService biblioteca)
        {
            _http = http;
            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "MangaTracker/1.0");
            _biblioteca = biblioteca;
        }

        public async Task<JikanImportResult> ImportarPaginaAsync(int page = 1, int limit = 25, CancellationToken ct = default)
        {
            var mensagens = new List<string>();
            var importados = 0;
            var ignorados = 0;
            var erros = 0;

            limit = Math.Clamp(limit, 1, 25);
            var url = $"{JikanBase}/manga?page={page}&limit={limit}&order_by=popularity";

            try
            {
                var response = await _http.GetFromJsonAsync<JikanMangaResponse>(url, ct);
                if (response?.Data == null || response.Data.Count == 0)
                {
                    mensagens.Add("Nenhum mangá retornado pela API.");
                    return new JikanImportResult(0, 0, 0, mensagens);
                }

                foreach (var item in response.Data)
                {
                    try
                    {
                        if (_biblioteca.BuscarMangaPorMalId(item.MalId) != null)
                        {
                            ignorados++;
                            continue;
                        }

                        var titulo = item.Title ?? item.TitleEnglish ?? $"MalId_{item.MalId}";
                        var capa = item.Images?.Jpg?.LargeImageUrl ?? item.Images?.Jpg?.ImageUrl;
                        var ano = item.Published?.Prop?.From?.Year;
                        var autor = item.Authors?.FirstOrDefault()?.Name;
                        var demografia = item.Demographics?.FirstOrDefault()?.Name;
                        var generos = item.Genres?
                .Select(g => g.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OfType<string>()
                .ToList();

                        _biblioteca.CadastrarNoCatalogoDeJikan(
                            item.MalId,
                            titulo,
                            capa,
                            item.Synopsis,
                            demografia,
                            autor,
                            ano,
                            item.Chapters,
                            generos
                        );
                        importados++;
                    }
                    catch (Exception ex)
                    {
                        erros++;
                        mensagens.Add($"[MalId {item.MalId}] {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                mensagens.Add($"Erro ao chamar Jikan: {ex.Message}");
            }

            return new JikanImportResult(importados, ignorados, erros, mensagens);
        }

        // DTOs da API Jikan v4
        private class JikanMangaResponse
        {
            [JsonPropertyName("data")]
            public List<JikanMangaItem>? Data { get; set; }
        }

        private class JikanMangaItem
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("title_english")]
            public string? TitleEnglish { get; set; }

            [JsonPropertyName("synopsis")]
            public string? Synopsis { get; set; }

            [JsonPropertyName("chapters")]
            public int? Chapters { get; set; }

            [JsonPropertyName("images")]
            public JikanImages? Images { get; set; }

            [JsonPropertyName("published")]
            public JikanPublished? Published { get; set; }

            [JsonPropertyName("authors")]
            public List<JikanNameUrl>? Authors { get; set; }

            [JsonPropertyName("demographics")]
            public List<JikanNameUrl>? Demographics { get; set; }

            [JsonPropertyName("genres")]
            public List<JikanNameUrl>? Genres { get; set; }
        }

        private class JikanImages
        {
            [JsonPropertyName("jpg")]
            public JikanJpg? Jpg { get; set; }
        }

        private class JikanJpg
        {
            [JsonPropertyName("image_url")]
            public string? ImageUrl { get; set; }

            [JsonPropertyName("large_image_url")]
            public string? LargeImageUrl { get; set; }
        }

        private class JikanPublished
        {
            [JsonPropertyName("prop")]
            public JikanProp? Prop { get; set; }
        }

        private class JikanProp
        {
            [JsonPropertyName("from")]
            public JikanDate? From { get; set; }
        }

        private class JikanDate
        {
            [JsonPropertyName("year")]
            public int? Year { get; set; }
        }

        private class JikanNameUrl
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }
    }
}
