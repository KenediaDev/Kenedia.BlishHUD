using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Blish_HUD.Content;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class TradeRank
    {
        public static TradeRank OneMillion = new(10000000000, new(56, 211, 141), "Eligible for 1 Mil. Trade Rank", 250, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{1000000}.png"));
        public static TradeRank FiveHundredThousand = new(5000000000, new(22, 234, 215), "Eligible for 500k Trade Rank", 175, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{500000}.png"));
        public static TradeRank OneHundredThousand = new(1000000000, new(17, 171, 205), "Eligible for 100k Trade Rank", 100, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{100000}.png"));
        public static TradeRank FiftyThousand = new(500000000, new(127, 186, 246), "Eligible for 50k Trade Rank", 50, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{50000}.png"));
        public static TradeRank TwentyFiveThousand = new(250000000, new(206, 233, 250), "Eligible for 25k Trade Rank", 25, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{25000}.png"));
        public static TradeRank FiveThousand = new(50000000, new(185, 192, 236), "Eligible for 5k Trade Rank", 10, OverflowTradingAssist.ModuleInstance.ContentsManager.GetTexture($@"textures\{5000}.png"));
        public static TradeRank NoRank = new(0, new(255, 255, 255), "Eligible for no Trade Rank", 0, AsyncTexture2D.FromAssetId(154982));

        public TradeRank(double threshold, Color color, string name, int trades, AsyncTexture2D texture)
        {
            Threshold = threshold;
            Color = color;
            Name = name;
            Trades = trades;
            Icon = texture;
        }

        public double Threshold { get; set; }

        public Color Color { get; set; }

        public string Name { get; set; }

        public int Trades { get; set; }

        public AsyncTexture2D Icon { get; set; }

        public static List<TradeRank> Ranks = new()
        {
            OneMillion,
            FiveHundredThousand,
            OneHundredThousand,
            FiftyThousand,
            TwentyFiveThousand,
            FiveThousand,
            NoRank,
        };
    }
}
