using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Kenedia.Modules.Characters
{
    public class OCR_TrainDisplay : Panel
    {
        private static readonly Random s_random = new();

        private MultilineTextBox textBox;

        readonly string _characterString;
        readonly char[] _characters = {
            'A',
            'a',
            'B',
            'b',
            'C',
            'c',
            'D',
            'd',
            'E',
            'e',
            'F',
            'f',
            'G',
            'g',
            'H',
            'h',
            'I',
            'i',
            'J',
            'j',
            'K',
            'k',
            'L',
            'l',
            'M',
            'm',
            'N',
            'n',
            'O',
            'o',
            'P',
            'p',
            'Q',
            'q',
            'R',
            'r',
            'S',
            's',
            'T',
            't',
            'U',
            'u',
            'V',
            'v',
            'W',
            'w',
            'X',
            'x',
            'Y',
            'y',
            'Z',
            'z',
            'Á',
            'á',
            'Â',
            'â',
            'Ä',
            'ä',
            'À',
            'à',
            'Æ',
            'æ',
            'Ç',
            'ç',
            'Ê',
            'ê',
            'É',
            'é',
            'Ë',
            'ë',
            'È',
            'è',
            'Ï',
            'ï',
            'Í',
            'í',
            'Î',
            'î',
            'Ñ',
            'ñ',
            'Œ',
            'œ',
            'Ô',
            'ô',
            'Ö',
            'ö',
            'Ó',
            'ó',
            'Ú',
            'ú',
            'Ü',
            'ü',
            'Û',
            'û',
            'Ù',
            'ù',
        };
        public string RandomString(int length)
        {
            return new string(Enumerable.Repeat(_characterString, length)
                .Select(s => s[s_random.Next(s.Length)]).ToArray());
        }

        public OCR_TrainDisplay()
        {
            Parent = GameService.Graphics.SpriteScreen;
            Width = GameService.Graphics.SpriteScreen.Width;
            Height = GameService.Graphics.SpriteScreen.Height;

            Visible = true;
            BackgroundColor = Color.White;
            _characterString = new string(_characters);
            _characterString += " ";

            string text = "";
            for (int i = 0; i < 50; i++)
            {
                text+= RandomString(s_random.Next(3, 20)) + " ";
            }

            textBox = new()
            {
                Parent = this,
                Text = text,
                Width = Graphics.SpriteScreen.Width,
                Height = 30
                //TextColor= Color.Black,
            };

            var flow = new FlowPanel()
            {
                Location = new(10, 150),
                ControlPadding = new(50, 50),
                Parent = this,
                Width = Graphics.SpriteScreen.Width - 20,
                Height = Graphics.SpriteScreen.Height,
            };

            _ = new Label()
            {
                Location = new(10, 30),
                Parent = flow,
                Text = text,
                WrapText= true,
                Width = Graphics.SpriteScreen.Width - 20,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont12,
                TextColor= Color.Black,
            };

            _ = new Label()
            {
                Location = new(10, 30),
                Parent = flow,
                Text = text,
                WrapText = true,
                Width = Graphics.SpriteScreen.Width - 20,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Black,
            };

            _ = new Label()
            {
                Location = new(10, 30),
                Parent = flow,
                Text = text,
                WrapText = true,
                Width = Graphics.SpriteScreen.Width - 20,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                TextColor = Color.Black,
            };

            string upper = "A   B   C   D   E   F   G   H   I   J   K   L   M   N   O   P   Q   R   S   T   U   V   W   X   Y   Z   Á   Â   Ä   À   Æ   Ç   Ê   É   Ë   È   Ï   Í   Î   Ñ   Œ   Ô   Ö   Ó   Ú   Ü   Û   Ù";
            string lower = "a   b   c   d   e   f   g   h   i   j   k   l   m   n   o   p   q   r   s   t   u   v   w   x   y   z   á   â   ä   à   æ   ç   ê   é   ë   è   ï   í   î   ñ   œ   ô   ö   ó   ú   ü   û   ù";
            string numbers = "1   2   3   4   5   6   7   8   9   0";

            _ = new Label()
            {
                Location = new(10, 30),
                Parent = flow,
                Text = string.Format("{1}{0}{0}{2}{0}{0}{3}", Environment.NewLine, upper, lower, numbers),
                WrapText = true,
                Width = Graphics.SpriteScreen.Width - 20,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont32,
                TextColor = Color.Black,
            };
        }
    }
}
