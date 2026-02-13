using System;
using MangaTracker.Models;
using MangaTracker.Services;
using System.Linq;


namespace MangaTracker
{
    internal class Program
    {
        static void Main()
        {
            var service = new BibliotecaService();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("===== Manga Tracker =====");
                Console.WriteLine("1 - Cadastrar mangá");
                Console.WriteLine("2 - Adicionar mangá para minha leitura");
                Console.WriteLine("3 - Atualizar progresso de leitura");
                Console.WriteLine("4 - Listar catálogo");
                Console.WriteLine("5 - Listar minhas leituras");
                Console.WriteLine("6 - Sair");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1": TelaCadastrarNoCatalogo(service); break;
                    case "2": TelaAdicionarNaMinhaLeitura(service); break;
                    case "3": TelaAtualizarProgresso(service); break;
                    case "4": TelaListarCatalogo(service); break;
                    case "5": TelaListarMinhasLeituras(service); break;
                    case "6": return;
                    default: Pausa("Opção inválida."); break;
                }


            }
        }

        static void TelaCadastrarNoCatalogo(BibliotecaService service)
        {
            string? tituloDigitado = null;
            bool? finalizado = null;
            int? total = null;
            string? erro = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Cadastrar mangá ===");
                Console.WriteLine("Digite /cancel para voltar ao menu.");
                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(erro))
                {
                    Console.WriteLine($"Erro: {erro}");
                    Console.WriteLine();
                    erro = null;
                }

               

                if (!string.IsNullOrWhiteSpace(tituloDigitado))
                    Console.WriteLine($"Título: {tituloDigitado}");

                if (finalizado.HasValue)
                    Console.WriteLine($"Finalizado: {(finalizado.Value ? "Sim" : "Não")}");

                if (total.HasValue)
                    Console.WriteLine($"Total de capítulos: {total.Value}");

                Console.WriteLine();

                // 1) Título
                if (string.IsNullOrWhiteSpace(tituloDigitado))
                {
                    string? titulo = LerTextoComCancelamento("Título: ");
                    if (titulo is null)
                        return; // volta pro menu principal


                    if (string.IsNullOrWhiteSpace(titulo))
                    {
                        Pausa("Título é obrigatório.");
                        continue;
                    }

                    string t = titulo.Trim();

                    if (service.MangaExisteNoCatalogo(t))
                    {
                        Pausa("Esse mangá já existe no catálogo.");
                        continue;
                    }

                    tituloDigitado = t;
                    continue;
                }

                // 2) Finalizado?
                if (!finalizado.HasValue)
                {
                    string? entrada = LerTextoComCancelamento("Esse mangá está finalizado? (S/N ou /cancel): ");
                    if (entrada is null)
                        return; // volta pro menu

                    string v = entrada.Trim().ToUpperInvariant();

                    if (v == "S" || v == "SIM")
                        finalizado = true;
                    else if (v == "N" || v == "NAO" || v == "NÃO")
                        finalizado = false;
                    else
                    {
                        PausaErro("Digite S para Sim ou N para Não.");
                        continue;
                    }

                    continue;
                }



                // 3) Total (obrigatório se finalizado)
                if (!total.HasValue && finalizado.Value)
                {
                    total = LerInt("Total de capítulos (obrigatório): ", min: 1);
                    // se digitou errado, LerInt repete sem perder o resumo
                }
                else if (!finalizado.Value)
                {
                    // opcional: se Enter, fica null e tudo bem
                    total = LerIntOpcional("Total de capítulos (Enter para pular): ");
                }

                // 4) Cadastrar
                service.CadastrarNoCatalogo(tituloDigitado, total);
                Pausa("Mangá cadastrado!");
                return;
            }
        }


        static void TelaAdicionarNaMinhaLeitura(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Adicionar à minha leitura ===");
                Console.WriteLine("1 - Ver catálogo");
                Console.WriteLine("2 - Adicionar digitando o título");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        TelaVerCatalogoSomente(service);
                        break;

                    case "2":
                        TelaAdicionarPorTitulo(service);
                        break;

                    case "0":
                        return;

                    default:
                        PausaErro("Opção inválida.");
                        break;
                }
            }
        }

        static void TelaVerCatalogoSomente(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Catálogo (somente visualizar) ===");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();

                var catalogo = service.ListarCatalogo();
                if (catalogo.Count == 0)
                {
                    PausaErro("O catálogo está vazio.");
                    return;
                }

                for (int i = 0; i < catalogo.Count; i++)
                {
                    string total = catalogo[i].TotalCapitulos.HasValue ? catalogo[i].TotalCapitulos.Value.ToString() : "?";
                    Console.WriteLine($"{i + 1}. {catalogo[i].Titulo} (Total: {total})");
                }

                Console.WriteLine();
                Console.Write("Digite 0 para voltar: ");
                string? op = Console.ReadLine();

                if (op == "0")
                    return;
            }
        }

        static void TelaAdicionarPorTitulo(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Adicionar por título ===");
                Console.WriteLine("Digite /cancel para voltar");
                Console.WriteLine();

                string? titulo = LerTextoComCancelamento("Título do mangá: ");
                if (titulo is null)
                    return;

                if (string.IsNullOrWhiteSpace(titulo))
                {
                    PausaErro("Título é obrigatório.");
                    continue;
                }

                var manga = service.BuscarMangaPorTitulo(titulo);
                if (manga is null)
                {
                    PausaErro("Mangá não cadastrado. Tente novamente.");
                    continue;
                }

                if (service.EstaNaMinhaLista(manga.Id))
                {
                    PausaErro("Esse mangá já está na sua lista de leitura.");
                    return;
                }

                // Agora escolhe status e capítulo atual
                StatusLeitura status = LerStatus();
                int? capAtual = null;

                if (status == StatusLeitura.PretendoLer)
                {
                    capAtual = 0;
                }
                else if (status == StatusLeitura.Lendo)
                {
                    capAtual = LerInt("Capítulo atual (último lido): ", min: 0);

                    if (manga.TotalCapitulos.HasValue && capAtual > manga.TotalCapitulos.Value)
                    {
                        PausaErro("Capítulo atual não pode ser maior que o total do catálogo.");
                        continue;
                    }
                }
                else if (status == StatusLeitura.Concluido)
                {
                    if (!manga.TotalCapitulos.HasValue)
                        capAtual = LerInt("Você concluiu em qual capítulo? ", min: 1);
                }

                try
                {
                    service.AdicionarNaMinhaLista(manga.Id, status, capAtual);
                    Pausa("Adicionado à sua lista!");
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    PausaErro($"Erro: {ex.Message}");
                    return;
                }
            }
        }


        static void TelaAtualizarProgresso(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Atualizar progresso de leitura ===");

                var lista = service.ListarMinhaLista();
                if (lista.Count == 0)
                {
                    Pausa("Sua lista de leitura está vazia.");
                    return;
                }

                for (int i = 0; i < lista.Count; i++)
                {
                    var item = lista[i];
                    string total = item.Manga.TotalCapitulos.HasValue ? item.Manga.TotalCapitulos.Value.ToString() : "?";
                    Console.WriteLine($"{i + 1}. {item.Manga.Titulo} ({item.Leitura.Status}) - {item.Leitura.CapituloAtual}/{total}");
                }

                int idx = LerInt($"Escolha o número do mangá (1 a {lista.Count}): ", 1, lista.Count);
                var selecionado = lista[idx - 1];

                Console.WriteLine();
                Console.WriteLine($"Selecionado: {selecionado.Manga.Titulo}");

                int cap = LerInt("Novo capítulo atual (último lido): ", min: 0);

                // Se o catálogo tem total, não deixa passar
                if (selecionado.Manga.TotalCapitulos.HasValue && cap > selecionado.Manga.TotalCapitulos.Value)
                {
                    Pausa("Capítulo atual não pode ser maior que o total do catálogo.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Quer mudar o status?");
                Console.WriteLine("0 - Não mudar");
                Console.WriteLine("1 - Pretendo ler");
                Console.WriteLine("2 - Lendo");
                Console.WriteLine("3 - Concluído");
                Console.Write("Escolha: ");
                string? s = Console.ReadLine();

                StatusLeitura? novoStatus = s switch
                {
                    "1" => StatusLeitura.PretendoLer,
                    "2" => StatusLeitura.Lendo,
                    "3" => StatusLeitura.Concluido,
                    _ => null
                };

                service.AtualizarLeitura(selecionado.Manga.Id, cap, novoStatus);
                Pausa("Progresso atualizado!");
                return;
            }
        }


        static void TelaListarCatalogo(BibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Catálogo ===");

            var lista = service.ListarCatalogo();

            if (lista.Count == 0)
            {
                Pausa("Nenhum mangá cadastrado no catálogo.");
                return;
            }

            foreach (var m in lista)
            {
                string total = m.TotalCapitulos.HasValue ? m.TotalCapitulos.Value.ToString() : "?";
                Console.WriteLine($"- {m.Titulo} | Total: {total}");
            }

            Pausa("Voltar");
        }


        static void TelaListarMinhasLeituras(BibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Minhas leituras ===");
            Console.WriteLine("1 - Mostrar todas");
            Console.WriteLine("2 - Filtrar por status");
            Console.Write("Escolha: ");
            string? op = Console.ReadLine();

            var lista = op == "2"
                ? service.ListarMinhaListaPorStatus(LerStatus())
                : service.ListarMinhaLista();

            Console.Clear();
            Console.WriteLine("=== Minhas leituras ===");

            if (lista.Count == 0)
            {
                Pausa("Nenhum mangá na sua lista de leitura.");
                return;
            }

            foreach (var item in lista)
            {
                string total = item.Manga.TotalCapitulos.HasValue ? item.Manga.TotalCapitulos.Value.ToString() : "?";
                Console.WriteLine($"- {item.Manga.Titulo} | {item.Leitura.Status} | {item.Leitura.CapituloAtual}/{total}");
            }

            Pausa("Voltar");
        }


        static StatusLeitura LerStatus()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Status:");
                Console.WriteLine("1 - Pretendo ler");
                Console.WriteLine("2 - Lendo");
                Console.WriteLine("3 - Concluído");
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                if (op == "1") return StatusLeitura.PretendoLer;
                if (op == "2") return StatusLeitura.Lendo;
                if (op == "3") return StatusLeitura.Concluido;

                Pausa("Opção inválida.");
                Console.Clear();
            }
        }

        static int LerInt(string msg, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(msg);
                string? entrada = Console.ReadLine();

                if (int.TryParse(entrada, out int v) && v >= min && v <= max)
                    return v;

                Pausa("Número inválido.");
                Console.Clear();
            }
        }

        static int? LerIntOpcional(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                string? entrada = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(entrada))
                    return null;

                if (int.TryParse(entrada, out int v) && v > 0)
                    return v;

                Pausa("Número inválido (ou aperte Enter para pular).");
                Console.Clear();
            }
        }

        static bool? TentarLerSimNao(string msg)
        {
            Console.Write(msg);
            string? entrada = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entrada))
                return null;

            string v = entrada.Trim().ToUpperInvariant();

            if (v == "S" || v == "SIM") return true;
            if (v == "N" || v == "NAO" || v == "NÃO") return false;

            return null;
        }

        static string? LerTextoComCancelamento(string msg)
        {
            Console.Write(msg);
            string? entrada = Console.ReadLine();

            if (entrada is null) return null;

            entrada = entrada.Trim();

            if (entrada.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
                return null;

            return entrada;
        }


        static void Pausa(string msg)
        {
            Console.WriteLine();
            Console.WriteLine(msg);
            Console.WriteLine("Pressione qualquer tecla...");
            Console.ReadKey();
        }

        static void PausaErro(string msg)
        {
            Console.Clear();
            Console.WriteLine("=== Erro ===");
            Console.WriteLine();
            Console.WriteLine(msg);
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para voltar...");
            Console.ReadKey(true);
        }

    }
}
