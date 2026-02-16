using System;
using System.Linq;
using MangaTracker.Models;
using MangaTracker.Services;
using System.Collections.Generic;


namespace MangaTracker
{
    internal class Program
    {
        static void Main()
        {
            var service = new BibliotecaService();

            try
            {
                service.CarregarDados();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Não foi possível carregar os dados salvos.");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey(true);
            }


            while (true)
            {
                Console.Clear();

                var user = service.UsuarioAtual();
                string nomeUser = user is null ? "(nenhum)" : user.Nome;

                Console.WriteLine("===== Manga Tracker =====");
                Console.WriteLine($"Usuário atual: {nomeUser}");
                Console.WriteLine();
                Console.WriteLine("1 - Catálogo de Mangás");
                Console.WriteLine("2 - Meu Perfil (Minhas leituras)");
                Console.WriteLine("3 - Usuários");
                Console.WriteLine("4 - Sair");
                Console.WriteLine();
                Console.Write("Escolha: ");

                string? op = Console.ReadLine();   // ✅ TEM QUE EXISTIR

                switch (op)                        // ✅ E O SWITCH USA ISSO
                {
                    case "1": TelaCatalogo(service); break;
                    case "2": TelaMeuPerfil(service); break;
                    case "3": TelaUsuarios(service); break;
                    case "4": return;
                    default: PausaErro("Opção inválida."); break;
                }
            }

        }

        static void TelaMeuPerfil(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Meu Perfil ===");
                Console.WriteLine("1 - Adicionar mangá para minhas leituras");
                Console.WriteLine("2 - Atualizar progresso de leitura");
                Console.WriteLine("3 - Listar minhas leituras");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1": TelaAdicionarNaMinhaLeitura(service); break;
                    case "2": TelaAtualizarProgresso(service); break;
                    case "3": TelaListarMinhasLeituras(service); break;
                    case "0": return;
                    default: PausaErro("Opção inválida."); break;
                }
            }
        }

        static void TelaUsuarios(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Usuários ===");

                var atual = service.UsuarioAtual();
                Console.WriteLine($"Usuário atual: {(atual is null ? "(nenhum)" : atual.Nome)}");
                Console.WriteLine();

                Console.WriteLine("1 - Listar usuários");
                Console.WriteLine("2 - Criar novo usuário");
                Console.WriteLine("3 - Trocar usuário atual");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        TelaListarUsuarios(service);
                        break;

                    case "2":
                        TelaCriarUsuario(service);
                        break;

                    case "3":
                        TelaTrocarUsuario(service);
                        break;

                    case "0":
                        return;

                    default:
                        PausaErro("Opção inválida.");
                        break;
                }
            }
        }

        static void TelaListarUsuarios(BibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Lista de usuários ===");
            Console.WriteLine();

            var usuarios = service.ListarUsuarios()
                .OrderBy(u => u.Nome, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var atual = service.UsuarioAtual();

            for (int i = 0; i < usuarios.Count; i++)
            {
                string mark = (atual is not null && usuarios[i].Id == atual.Id) ? "  <== atual" : "";
                Console.WriteLine($"{i + 1}. {usuarios[i].Nome}{mark}");
            }

            Pausa("Voltar");
        }

        static void TelaCriarUsuario(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Criar novo usuário ===");
                Console.WriteLine("Digite /cancel para voltar");
                Console.WriteLine();

                string? nome = LerTextoComCancelamento("Nome do usuário: ");
                if (nome is null) return;

                if (string.IsNullOrWhiteSpace(nome))
                {
                    PausaErro("Nome é obrigatório.");
                    continue;
                }

                try
                {
                    service.CriarUsuario(nome);
                    service.SalvarDados();
                    Pausa("Usuário criado!");
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    PausaErro(ex.Message);
                }
            }
        }

        static void TelaTrocarUsuario(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Trocar usuário atual ===");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();

                var usuarios = service.ListarUsuarios()
                    .OrderBy(u => u.Nome, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                var atual = service.UsuarioAtual();

                for (int i = 0; i < usuarios.Count; i++)
                {
                    string mark = (atual is not null && usuarios[i].Id == atual.Id) ? "  <== atual" : "";
                    Console.WriteLine($"{i + 1}. {usuarios[i].Nome}{mark}");
                }

                Console.WriteLine();
                int escolha = LerInt($"Escolha o número (1 a {usuarios.Count}) ou 0: ", 0, usuarios.Count);

                if (escolha == 0)
                    return;

                var escolhido = usuarios[escolha - 1];

                if (!service.DefinirUsuarioAtual(escolhido.Id))
                {
                    PausaErro("Não foi possível definir esse usuário.");
                    continue;
                }

                service.SalvarDados();
                Pausa($"Usuário atual agora é: {escolhido.Nome}");
                return;
            }
        }

        static void TelaMinhaLeitura(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Minha Leitura ===");
                Console.WriteLine("1 - Adicionar Mangá à Minha Lista");
                Console.WriteLine("2 - Atualizar Progresso de Leitura");
                Console.WriteLine("3 - Ver Minhas Leituras");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1": TelaAdicionarNaMinhaLeitura(service); break;
                    case "2": TelaAtualizarProgresso(service); break;
                    case "3": TelaListarMinhasLeituras(service); break;
                    case "0": return;
                    default: PausaErro("Opção inválida."); break;
                }
            }
        }

        // =========================
        // CATÁLOGO (MENU)
        // =========================
        static void TelaCatalogo(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Catálogo de Mangás ===");
                Console.WriteLine("1 - Cadastrar novo mangá");
                Console.WriteLine("2 - Editar mangá existente");
                Console.WriteLine("3 - Listar catálogo");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        TelaCadastrarNoCatalogo(service);
                        break;

                    case "2":
                        if (service.ListarCatalogo().Count == 0)
                            Pausa("Nenhum mangá cadastrado para editar.");
                        else
                            TelaEditarMangaDoCatalogo(service);
                        break;

                    case "3":
                        TelaListarCatalogo(service);
                        break;

                    case "0":
                        return;

                    default:
                        PausaErro("Opção inválida.");
                        break;
                }
            }
        }

        static void TelaCadastrarNoCatalogo(BibliotecaService service)
        {
            string? tituloDigitado = null;
            bool? finalizado = null;
            int? total = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Cadastrar mangá ===");
                Console.WriteLine("Digite /cancel para voltar ao menu.");
                Console.WriteLine();

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
                    if (titulo is null) return;

                    if (string.IsNullOrWhiteSpace(titulo))
                    {
                        PausaErro("Título é obrigatório.");
                        continue;
                    }

                    string t = titulo.Trim();

                    if (service.MangaExisteNoCatalogo(t))
                    {
                        PausaErro("Esse mangá já existe no catálogo.");
                        continue;
                    }

                    tituloDigitado = t;
                    continue;
                }

                // 2) Finalizado?
                if (!finalizado.HasValue)
                {
                    string? entrada = LerTextoComCancelamento("Esse mangá está finalizado? (S/N ou /cancel): ");
                    if (entrada is null) return;

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
                }
                else if (!finalizado.Value)
                {
                    total = LerIntOpcional("Total de capítulos (Enter para pular): ");
                }

                // 4) Cadastrar
                service.CadastrarNoCatalogo(tituloDigitado, total);                
                Pausa("Mangá cadastrado!");
                return;
            }
        }

        static void TelaListarCatalogo(BibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Catálogo ===");
            Console.WriteLine();

            var lista = CatalogoOrdenado(service);

            if (lista.Count == 0)
            {
                Pausa("Nenhum mangá cadastrado no catálogo.");
                return;
            }

            foreach (var m in lista)
            {
                string total = m.TotalCapitulos.HasValue ? m.TotalCapitulos.Value.ToString() : "?";
                Console.WriteLine($"- {m.Titulo} | Capítulos Totais: {total}");
            }

            Pausa("Voltar");
        }

        static void TelaEditarMangaDoCatalogo(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Editar mangá do catálogo ===");
                Console.WriteLine("1 - Mostrar lista e escolher");
                Console.WriteLine("2 - Digitar o nome do mangá");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? op = Console.ReadLine();

                if (op == "0") return;

                if (service.ListarCatalogo().Count == 0)
                {
                    Pausa("Catálogo vazio.");
                    return;
                }

                if (op == "1")
                {
                    // Escolher pela lista
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("=== Escolher pela lista ===");
                        Console.WriteLine("0 - Voltar");
                        Console.WriteLine();

                        var catalogo = CatalogoOrdenado(service);

                        for (int i = 0; i < catalogo.Count; i++)
                        {
                            string total = catalogo[i].TotalCapitulos.HasValue ? catalogo[i].TotalCapitulos.Value.ToString() : "?";
                            Console.WriteLine($"{i + 1}. {catalogo[i].Titulo} (Capítulos Totais: {total})");
                        }

                        Console.WriteLine();
                        Console.Write("Escolha o número do mangá (ou 0): ");
                        string? entrada = Console.ReadLine();

                        if (entrada == "0") break;

                        if (!int.TryParse(entrada, out int idx) || idx < 1 || idx > catalogo.Count)
                        {
                            PausaErro("Opção inválida.");
                            continue;
                        }

                        var manga = catalogo[idx - 1];
                        TelaEditarMangaSelecionado(service, manga.Id);
                        break;
                    }
                }
                else if (op == "2")
                {
                    // Digitar o nome
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("=== Buscar por nome ===");
                        Console.WriteLine("Digite /cancel para voltar");
                        Console.WriteLine();

                        string? titulo = LerTextoComCancelamento("Nome do mangá: ");
                        if (titulo is null) break;

                        if (string.IsNullOrWhiteSpace(titulo))
                        {
                            PausaErro("Título é obrigatório.");
                            continue;
                        }

                        var manga = service.BuscarMangaPorTitulo(titulo);
                        if (manga is null)
                        {
                            PausaErro("Mangá não encontrado no catálogo.");
                            continue;
                        }

                        TelaEditarMangaSelecionado(service, manga.Id);
                        break;
                    }
                }
                else
                {
                    PausaErro("Opção inválida.");
                }
            }
        }

        static void TelaEditarMangaSelecionado(BibliotecaService service, Guid mangaId)
        {
            var manga = service.BuscarMangaPorId(mangaId);
            if (manga is null)
            {
                PausaErro("Mangá não encontrado.");
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Editar mangá ===");
                Console.WriteLine($"Título: {manga.Titulo}");
                Console.WriteLine($"Total: {(manga.TotalCapitulos.HasValue ? manga.TotalCapitulos.Value.ToString() : "?")}");
                Console.WriteLine();
                Console.WriteLine("1 - Alterar título");
                Console.WriteLine("2 - Alterar total de capítulos");
                Console.WriteLine("3 - Remover total de capítulos (voltar para ?)");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha: ");
                string? acao = Console.ReadLine();

                if (acao == "0") return;

                if (acao == "1")
                {
                    Console.WriteLine();
                    Console.WriteLine("Digite /cancel para voltar");
                    string? novoTitulo = LerTextoComCancelamento("Novo título: ");
                    if (novoTitulo is null) continue;

                    string t = novoTitulo.Trim();
                    if (string.IsNullOrWhiteSpace(t))
                    {
                        PausaErro("Título não pode ser vazio.");
                        continue;
                    }

                    var existente = service.BuscarMangaPorTitulo(t);
                    if (existente is not null && existente.Id != manga.Id)
                    {
                        PausaErro("Já existe um mangá com esse título no catálogo.");
                        continue;
                    }

                    manga.Titulo = t;                    
                    Pausa("Título atualizado!");
                }
                else if (acao == "2")
                {
                    Console.WriteLine();
                    int novoTotal = LerInt("Novo total de capítulos: ", min: 1);
                    service.DefinirTotalCapitulos(manga.Id, novoTotal);
                    // Atualiza a referência do objeto após o service alterar
                    manga = service.BuscarMangaPorId(mangaId) ?? manga;                
                    Pausa("Total atualizado!");
                }
                else if (acao == "3")
                {
                    service.DefinirTotalCapitulos(manga.Id, null);                    
                    Pausa("Total removido (agora fica ?).");
                }
                else
                {
                    PausaErro("Opção inválida.");
                }
            }
        }

        // =========================
        // MINHA LEITURA
        // =========================
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
                Console.WriteLine("=== Catálogo ===");
                Console.WriteLine("Digite o número para adicionar à sua lista");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();

                var catalogo = CatalogoOrdenado(service);
                if (catalogo.Count == 0)
                {
                    PausaErro("O catálogo está vazio.");
                    return;
                }

                for (int i = 0; i < catalogo.Count; i++)
                {
                    string total = catalogo[i].TotalCapitulos.HasValue
                        ? catalogo[i].TotalCapitulos.Value.ToString()
                        : "?";

                    bool jaAdicionado = service.EstaNaMinhaLista(catalogo[i].Id);

                    string tag = jaAdicionado ? "  [Já adicionado]" : "";

                    Console.WriteLine($"{i + 1}. {catalogo[i].Titulo} (Capítulos Totais: {total}){tag}");
                }


                Console.WriteLine();
                int escolha = LerInt("Escolha o número (ou 0 para voltar): ", min: 0, max: catalogo.Count);

                if (escolha == 0)
                    return;

                var manga = catalogo[escolha - 1];

                if (service.EstaNaMinhaLista(manga.Id))
                {
                    PausaErro("Esse mangá já está na sua lista de leitura.");
                    continue;
                }

                // Escolhe status e capítulo
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
                }
            }
        }


        static void TelaAdicionarPorTitulo(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Adicionar (buscar por nome) ===");
                Console.WriteLine("Digite parte do nome (ex: nar, one, bla)");
                Console.WriteLine("Digite /cancel para voltar");
                Console.WriteLine();

                string? termo = LerTextoComCancelamento("Buscar: ");
                if (termo is null)
                    return;

                if (string.IsNullOrWhiteSpace(termo))
                {
                    PausaErro("Digite pelo menos 1 letra.");
                    continue;
                }

                var resultados = BuscarCatalogoPorTrecho(service, termo);

                if (resultados.Count == 0)
                {
                    PausaErro("Nenhum mangá encontrado com esse termo.");
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine($"Encontrados: {resultados.Count}");
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();

                for (int i = 0; i < resultados.Count; i++)
                {
                    string total = resultados[i].TotalCapitulos.HasValue
                        ? resultados[i].TotalCapitulos.Value.ToString()
                        : "?";

                    bool jaAdicionado = service.EstaNaMinhaLista(resultados[i].Id);
                    string tag = jaAdicionado ? "  [Já adicionado]" : "";

                    Console.WriteLine($"{i + 1}. {resultados[i].Titulo} (Capítulos Totais: {total}){tag}");
                }

                Console.WriteLine();
                int escolha = LerInt($"Escolha o número (1 a {resultados.Count}) ou 0: ", min: 0, max: resultados.Count);

                if (escolha == 0)
                    continue;

                var manga = resultados[escolha - 1];

                if (service.EstaNaMinhaLista(manga.Id))
                {
                    PausaErro("Esse mangá já está na sua lista de leitura.");
                    continue;
                }

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
                }
            }
        }


        static void TelaAtualizarProgresso(BibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Atualizar progresso de leitura ===");
                Console.WriteLine();

                var lista = service.ListarMinhaLista()
                    .OrderBy(x => x.Manga.Titulo, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                if (lista.Count == 0)
                {
                    Pausa("Sua lista de leitura está vazia.");
                    return;
                }

                for (int i = 0; i < lista.Count; i++)
                {
                    var linha = lista[i];
                    string total = linha.Manga.TotalCapitulos.HasValue ? linha.Manga.TotalCapitulos.Value.ToString() : "?";
                    Console.WriteLine($"{i + 1}. {linha.Manga.Titulo} ({linha.Leitura.Status}) - {linha.Leitura.CapituloAtual}/{total}");
                }

                Console.WriteLine();
                Console.WriteLine("0 - Voltar");
                int idx = LerInt($"Escolha o número do mangá (1 a {lista.Count}) ou 0: ", 0, lista.Count);

                if (idx == 0)
                    return;

                var selecionado = lista[idx - 1];

                Console.WriteLine();
                Console.WriteLine($"Selecionado: {selecionado.Manga.Titulo}");
                Console.WriteLine("Digite /cancel para voltar");
                Console.WriteLine();

                int? cap = LerIntOpcionalComCancelamento("Novo capítulo atual (último lido): ", min: 0);
                if (cap is null)
                    return;

                int capFinal = cap.Value;

                if (selecionado.Manga.TotalCapitulos.HasValue && capFinal > selecionado.Manga.TotalCapitulos.Value)
                {
                    PausaErro("Capítulo atual não pode ser maior que o total do catálogo.");
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

                service.AtualizarLeitura(selecionado.Manga.Id, capFinal, novoStatus);
                Pausa("Progresso atualizado!");
                return;
            }
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

            lista = lista
                .OrderBy(x => x.Manga.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            Console.Clear();
            Console.WriteLine("=== Minhas leituras ===");
            Console.WriteLine();

            if (lista.Count == 0)
            {
                Pausa("Nenhum mangá na sua lista de leitura.");
                return;
            }

            foreach (var linha in lista)
            {
                // Calcula larguras (limites para não estourar)
                int colTitulo = Math.Min(35, Math.Max(15, lista.Max(x => x.Manga.Titulo.Length)));
                int colStatus = 10;
                int colCap = 12;
                int colBarra = 27; // [████░░...] + " 100%"

                Console.Clear();
                Console.WriteLine("=== Minhas leituras ===");
                Console.WriteLine();

                EscreverLinha('─', colTitulo + colStatus + colCap + colBarra + 10);
                Console.Write(PadDir("Título", colTitulo));
                Console.Write("  ");
                Console.Write(PadDir("Status", colStatus));
                Console.Write("  ");
                Console.Write(PadDir("Capítulos", colCap));
                Console.Write("  ");
                Console.WriteLine("Progresso");
                EscreverLinha('─', colTitulo + colStatus + colCap + colBarra + 10);

                foreach (var item in lista)
                {
                    string titulo = PadDir(Truncar(item.Manga.Titulo, colTitulo), colTitulo);

                    string totalTxt = item.Manga.TotalCapitulos.HasValue
                        ? item.Manga.TotalCapitulos.Value.ToString()
                        : "?";

                    string caps = $"{item.Leitura.CapituloAtual}/{totalTxt}";
                    caps = PadDir(caps, colCap);

                    Console.Write(titulo);
                    Console.Write("  ");

                    EscreverStatusColorido(item.Leitura.Status, colStatus);
                    Console.Write("  ");

                    Console.Write(caps);
                    Console.Write("  ");

                    if (item.Manga.TotalCapitulos.HasValue)
                        EscreverBarraProgresso(item.Leitura.CapituloAtual, item.Manga.TotalCapitulos.Value, 20);
                    else
                        Console.Write("[????????????????????]   ?%");

                    Console.WriteLine();
                }

                EscreverLinha('─', colTitulo + colStatus + colCap + colBarra + 10);

            }


            Pausa("Voltar");
        }

        static void TelaRelatorios(BibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Relatórios ===");
            Console.WriteLine();

            var catalogo = service.ListarCatalogo();
            var minhaLista = service.ListarMinhaLista();

            int totalCatalogo = catalogo.Count;
            int totalMinhaLista = minhaLista.Count;

            int pretendo = minhaLista.Count(x => x.Leitura.Status == StatusLeitura.PretendoLer);
            int lendo = minhaLista.Count(x => x.Leitura.Status == StatusLeitura.Lendo);
            int concluido = minhaLista.Count(x => x.Leitura.Status == StatusLeitura.Concluido);

            double pctConcluido = totalMinhaLista == 0 ? 0 : (concluido * 100.0) / totalMinhaLista;

            Console.WriteLine($"Mangás no catálogo: {totalCatalogo}");
            Console.WriteLine($"Na minha lista: {totalMinhaLista}");
            Console.WriteLine();
            Console.WriteLine($"Pretendo ler: {pretendo}");
            Console.WriteLine($"Lendo: {lendo}");
            Console.WriteLine($"Concluídos: {concluido}");
            Console.WriteLine($"% concluído (na sua lista): {pctConcluido:0.0}%");

            Console.WriteLine();
            Console.WriteLine("Top 5 mais avançados (por capítulo atual):");

            var top = minhaLista
                .OrderByDescending(x => x.Leitura.CapituloAtual)
                .ThenBy(x => x.Manga.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .Take(5)
                .ToList();

            if (top.Count == 0)
            {
                Console.WriteLine("- (sem dados)");
            }
            else
            {
                foreach (var item in top)
                {
                    string total = item.Manga.TotalCapitulos.HasValue ? item.Manga.TotalCapitulos.Value.ToString() : "?";
                    Console.WriteLine($"- {item.Manga.Titulo} | {item.Leitura.Status} | {item.Leitura.CapituloAtual}/{total}");
                }
            }

            Pausa("Voltar");
        }


        // =========================
        // HELPERS
        // =========================
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

                PausaErro("Opção inválida.");
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

                PausaErro("Número inválido.");
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

                PausaErro("Número inválido (ou aperte Enter para pular).");
            }
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
            Console.ReadKey(true);
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

        static System.Collections.Generic.List<Manga> CatalogoOrdenado(BibliotecaService service)
        {
            return service.ListarCatalogo()
                .OrderBy(m => m.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        static string Truncar(string texto, int max)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Length <= max ? texto : texto.Substring(0, Math.Max(0, max - 1)) + "…";
        }

        static string PadDir(string texto, int largura)
        {
            texto ??= "";
            if (texto.Length >= largura) return texto;
            return texto + new string(' ', largura - texto.Length);
        }

        static string PadEsq(string texto, int largura)
        {
            texto ??= "";
            if (texto.Length >= largura) return texto;
            return new string(' ', largura - texto.Length) + texto;
        }

        static void EscreverStatusColorido(StatusLeitura status, int largura)
        {
            var corAntiga = Console.ForegroundColor;

            Console.ForegroundColor = status switch
            {
                StatusLeitura.PretendoLer => ConsoleColor.DarkYellow,
                StatusLeitura.Lendo => ConsoleColor.Cyan,
                StatusLeitura.Concluido => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            Console.Write(PadDir(status.ToString(), largura));
            Console.ForegroundColor = corAntiga;
        }

        static void EscreverBarraProgresso(int atual, int total, int larguraBarra = 20)
        {
            if (total <= 0)
            {
                Console.Write(PadDir("?", larguraBarra + 7));
                return;
            }

            atual = Math.Clamp(atual, 0, total);
            double pct = (atual * 1.0) / total;

            int preenchido = (int)Math.Round(pct * larguraBarra);
            preenchido = Math.Clamp(preenchido, 0, larguraBarra);

            string barra = "[" + new string('█', preenchido) + new string('░', larguraBarra - preenchido) + "]";
            string pctTxt = $"{pct * 100:0}%";

            Console.Write($"{barra} {PadEsq(pctTxt, 4)}");
        }

        static void EscreverLinha(char ch = '─', int tamanho = 80)
        {
            Console.WriteLine(new string(ch, tamanho));
        }

        static System.Collections.Generic.List<Manga> BuscarCatalogoPorTrecho(BibliotecaService service, string trecho)
        {
            trecho = (trecho ?? "").Trim();

            return service.ListarCatalogo()
                .Where(m => m.Titulo.Contains(trecho, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(m => m.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        static int? LerIntOpcionalComCancelamento(string msg, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(msg);
                string? entrada = Console.ReadLine();

                if (entrada is null) return null;

                entrada = entrada.Trim();

                if (entrada.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
                    return null;

                if (int.TryParse(entrada, out int v) && v >= min && v <= max)
                    return v;

                PausaErro("Número inválido (ou digite /cancel para voltar).");
            }
        }


    }
}
