using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public class JsonStorage
    {
        private readonly string _arquivo;
        private readonly JsonSerializerOptions _opts;

        public JsonStorage(string arquivo)
        {
            _arquivo = arquivo;

            _opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public DadosApp Carregar()
        {
            if (!File.Exists(_arquivo))
                return new DadosApp();

            string json = File.ReadAllText(_arquivo);
            var dados = JsonSerializer.Deserialize<DadosApp>(json, _opts);

            return dados ?? new DadosApp();
        }

        public void Salvar(DadosApp dados)
        {
            dados.SalvoEm = DateTime.Now;

            string json = JsonSerializer.Serialize(dados, _opts);

            string dir = Path.GetDirectoryName(_arquivo)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string tmp = _arquivo + ".tmp";
            string bak = _arquivo + ".bak";

            File.WriteAllText(tmp, json);

            if (File.Exists(_arquivo))
                File.Replace(tmp, _arquivo, bak, true);
            else
                File.Move(tmp, _arquivo);
        }

        public string CaminhoArquivo => _arquivo;
    }
}
