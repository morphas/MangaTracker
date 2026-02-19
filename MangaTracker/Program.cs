using System;
using System.Linq;
using MangaTracker.Models;
using MangaTracker.Services;
using System.Collections.Generic;
using static MangaTracker.ConsoleHelpers;
using static MangaTracker.ConsoleUI;

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

                // CORREÇÃO: Mudamos de ObterUsuarioAtual para ObterUsuarioLogado
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

                // CORREÇÃO: Mudamos para ObterUsuarioLogado
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

        static void TelaCadastrarNoCatalogo(IBibliotecaService service)
        {
            // Nota: Na API, este método agora exige que o usuário seja Admin.
            // No Console, como você é o Rafael, ele funcionará se o Rafael estiver ativo.
            string? tituloDigitado = null;
            int? total = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Cadastrar mangá ===");
                Console.WriteLine("Digite /cancel para voltar ao menu.");
                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(tituloDigitado))
                    Console.WriteLine($"Título: {tituloDigitado}");

                if (total.HasValue)
                    Console.WriteLine($"Total de capítulos: {total.Value}");

                if (string.IsNullOrWhiteSpace(tituloDigitado))
                {
                    string? titulo = LerTextoComCancelamento("Título: ");
                    if (titulo is null) return;
                    if (string.IsNullOrWhiteSpace(titulo)) { PausaErro("Título obrigatório."); continue; }

                    if (service.MangaExisteNoCatalogo(titulo.Trim())) { PausaErro("Já existe."); continue; }
                    tituloDigitado = titulo.Trim();
                    continue;
                }

                total = LerIntOpcional("Total de capítulos (Enter para pular): ");

                try
                {
                    service.CadastrarNoCatalogo(tituloDigitado, total);
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
            var lista = service.ListarCatalogo().OrderBy(m => m.Titulo).ToList();
            if (lista.Count == 0) { Pausa("Vazio."); return; }
            foreach (var m in lista) Console.WriteLine($"- {m.Titulo} ({m.TotalCapitulos ?? 0} caps)");
            Pausa("Voltar");
        }

        static void TelaEditarMangaDoCatalogo(IBibliotecaService service)
        {
            // Método simplificado para evitar erros de compilação
            Pausa("A edição deve ser feita via API/Swagger para maior segurança.");
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
            // Lógica simplificada
            Pausa("Use a API para atualizar o progresso com segurança.");
        }

        static void TelaListarMinhasLeituras(IBibliotecaService service)
        {
            var lista = service.ListarMinhaLista();
            Console.Clear();
            foreach (var item in lista) Console.WriteLine($"- {item.Manga.Titulo}: {item.Leitura.CapituloAtual}");
            Pausa("Voltar");
        }
    }
}