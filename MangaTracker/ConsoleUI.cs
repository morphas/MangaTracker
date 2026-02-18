using System;
using MangaTracker.Models;

namespace MangaTracker
{
    internal static class ConsoleUI
    {
        public static void EscreverStatusColorido(StatusLeitura status, int largura)
        {
            var corAntiga = Console.ForegroundColor;

            Console.ForegroundColor = status switch
            {
                StatusLeitura.PretendoLer => ConsoleColor.DarkYellow,
                StatusLeitura.Lendo => ConsoleColor.Cyan,
                StatusLeitura.Concluido => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            Console.Write(ConsoleHelpers.PadDir(status.ToString(), largura));
            Console.ForegroundColor = corAntiga;
        }

        public static void EscreverBarraProgresso(int atual, int total, int larguraBarra = 20)
        {
            if (total <= 0)
            {
                Console.Write(ConsoleHelpers.PadDir("?", larguraBarra + 7));
                return;
            }

            atual = Math.Clamp(atual, 0, total);
            double pct = (atual * 1.0) / total;

            int preenchido = (int)Math.Round(pct * larguraBarra);
            preenchido = Math.Clamp(preenchido, 0, larguraBarra);

            string barra = "[" + new string('█', preenchido) + new string('░', larguraBarra - preenchido) + "]";
            string pctTxt = $"{pct * 100:0}%";

            Console.Write($"{barra} {ConsoleHelpers.PadEsq(pctTxt, 4)}");
        }
    }
}
