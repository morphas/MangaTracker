using System;
using System.Linq;
using MangaTracker.Models;
using MangaTracker.Services;
using static MangaTracker.ConsoleHelpers;

namespace MangaTracker
{
    internal class Program
    {
        static void Main()
        {
            IBibliotecaService service = new BibliotecaService();

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

                var user = service.ObterUsuarioLogado();
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

                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1": TelaCatalogo(service); break;
                    case "2": TelaMeuPerfil(service); break;
                    case "3": TelaUsuarios(service); break;
                    case "4": return;
                    default: PausaErro("Opção inválida."); break;
                }
            }
        }

        static void TelaMeuPerfil(IBibliotecaService service)
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

        static void TelaUsuarios(IBibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Usuários ===");

                var atual = service.ObterUsuarioLogado();
                Console.WriteLine($"Usuário atual: {(atual is null ? "(nenhum)" : atual.Nome)}");
                Console.WriteLine();

                Console.WriteLine("1 - Listar usuários");
                Console.WriteLine("2 - Trocar usuário atual");
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

        static void TelaListarUsuarios(IBibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Lista de usuários ===");
            Console.WriteLine();

            var usuarios = service.ListarUsuarios()
                .OrderBy(u => u.Nome, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var atual = service.ObterUsuarioLogado();

            for (int i = 0; i < usuarios.Count; i++)
            {
                string mark = (atual is not null && usuarios[i].Id == atual.Id) ? "  <== atual" : "";
                Console.WriteLine($"{i + 1}. {usuarios[i].Nome}{mark}");
            }

            Pausa("Voltar");
        }

        static void TelaTrocarUsuario(IBibliotecaService service)
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

                var atual = service.ObterUsuarioLogado();

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

        static void TelaCatalogo(IBibliotecaService service)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Catálogo de Mangás ===");
                Console.WriteLine("1 - Cadastrar novo mangá");
                Console.WriteLine("2 - Listar catálogo");
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

        // ✅ AQUI está o ajuste principal do seu erro
        static void TelaCadastrarNoCatalogo(IBibliotecaService service)
        {
            string? tituloDigitado = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Cadastrar mangá ===");
                Console.WriteLine("Digite /cancel para voltar ao menu.");
                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(tituloDigitado))
                    Console.WriteLine($"Título: {tituloDigitado}");

                if (string.IsNullOrWhiteSpace(tituloDigitado))
                {
                    string? titulo = LerTextoComCancelamento("Título: ");
                    if (titulo is null) return;
                    if (string.IsNullOrWhiteSpace(titulo)) { PausaErro("Título obrigatório."); continue; }

                    if (service.MangaExisteNoCatalogo(titulo.Trim())) { PausaErro("Já existe."); continue; }
                    tituloDigitado = titulo.Trim();
                    continue;
                }

                // ✅ NOVO: pergunta se é lançado no Brasil e a editora
                bool lancadoNoBrasil = LerConfirmacao("Lançado no Brasil? (s/n): ");

                string? editora = null;
                if (lancadoNoBrasil)
                {
                    editora = LerTexto("Editora: ");
                    if (string.IsNullOrWhiteSpace(editora))
                    {
                        PausaErro("Editora obrigatória quando é lançado no Brasil.");
                        continue;
                    }
                    editora = editora.Trim();
                }

                try
                {
                    // ✅ CHAMADA CERTA (3 parâmetros)
                    service.CadastrarNoCatalogo(tituloDigitado, lancadoNoBrasil, editora);
                    Pausa("Mangá cadastrado!");
                    return;
                }
                catch (Exception ex)
                {
                    PausaErro(ex.Message);
                    return;
                }
            }
        }

        static void TelaListarCatalogo(IBibliotecaService service)
        {
            Console.Clear();
            Console.WriteLine("=== Catálogo ===");

            var lista = service.ListarCatalogo()
                .OrderBy(m => m.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (lista.Count == 0)
            {
                Pausa("Vazio.");
                return;
            }

            foreach (var m in lista)
            {
                // Se você ainda não colocou os campos novos no Model Manga, remova essas linhas
                // e deixe só: Console.WriteLine($"- {m.Titulo}");
                Console.WriteLine($"- {m.Titulo}");
            }

            Pausa("Voltar");
        }

        static void TelaAdicionarNaMinhaLeitura(IBibliotecaService service)
        {
            Console.Clear();
            var catalogo = service.ListarCatalogo().OrderBy(m => m.Titulo).ToList();
            for (int i = 0; i < catalogo.Count; i++) Console.WriteLine($"{i + 1}. {catalogo[i].Titulo}");
            int escolha = LerInt("Escolha o número (ou 0): ", 0, catalogo.Count);
            if (escolha == 0) return;

            var manga = catalogo[escolha - 1];
            service.AdicionarNaMinhaLista(manga.Id, StatusLeitura.Lendo, 0);
            Pausa("Adicionado!");
        }

        static void TelaAtualizarProgresso(IBibliotecaService service)
        {
            var lista = service.ListarMinhaLista();
            if (lista.Count == 0) { Pausa("Lista vazia."); return; }
            Pausa("Use a API para atualizar o progresso com segurança.");
        }

        static void TelaListarMinhasLeituras(IBibliotecaService service)
        {
            var lista = service.ListarMinhaLista();
            Console.Clear();
            foreach (var item in lista) Console.WriteLine($"- {item.Manga.Titulo}: {item.Leitura.CapituloAtual}");
            Pausa("Voltar");
        }

        // =========================
        // Helpers simples (no Console)
        // =========================

        static bool LerConfirmacao(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                var s = Console.ReadLine()?.Trim().ToLower();

                if (s == "s" || s == "sim") return true;
                if (s == "n" || s == "nao" || s == "não") return false;

                PausaErro("Responda com 's' ou 'n'.");
            }
        }

        static string LerTexto(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s))
                    return s.Trim();

                PausaErro("Texto obrigatório.");
            }
        }
    }
}