using System;

namespace MangaTracker
{
    internal static class ConsoleHelpers
    {
        public static int LerInt(string msg, int min = int.MinValue, int max = int.MaxValue)
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

        public static int? LerIntOpcional(string msg)
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

        public static int? LerIntOpcionalComCancelamento(string msg, int min = int.MinValue, int max = int.MaxValue)
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

        public static string? LerTextoComCancelamento(string msg)
        {
            Console.Write(msg);
            string? entrada = Console.ReadLine();

            if (entrada is null) return null;

            entrada = entrada.Trim();

            if (entrada.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
                return null;

            return entrada;
        }

        public static void Pausa(string msg)
        {
            Console.WriteLine();
            Console.WriteLine(msg);
            Console.WriteLine("Pressione qualquer tecla...");
            Console.ReadKey(true);
        }

        public static void PausaErro(string msg)
        {
            Console.Clear();
            Console.WriteLine("=== Erro ===");
            Console.WriteLine();
            Console.WriteLine(msg);
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para voltar...");
            Console.ReadKey(true);
        }

        public static string Truncar(string texto, int max)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Length <= max ? texto : texto.Substring(0, Math.Max(0, max - 1)) + "…";
        }

        public static string PadDir(string texto, int largura)
        {
            texto ??= "";
            if (texto.Length >= largura) return texto;
            return texto + new string(' ', largura - texto.Length);
        }

        public static string PadEsq(string texto, int largura)
        {
            texto ??= "";
            if (texto.Length >= largura) return texto;
            return new string(' ', largura - texto.Length) + texto;
        }

        public static void EscreverLinha(char ch = '─', int tamanho = 80)
        {
            Console.WriteLine(new string(ch, tamanho));
        }
    }
}
