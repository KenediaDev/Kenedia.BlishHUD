using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kenedia.Modules.Core.Utility
{
    public static class TextUtil
    {
        private static string WrapTextSegment(BitmapFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            var sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").Width;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    _ = sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    _ = sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public static string WrapText(BitmapFont spriteFont, string text, float maxLineWidth)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : string.Join("\n", text.Split('\n').Select(s => WrapTextSegment(spriteFont, s, maxLineWidth)));
        }
    }
}
