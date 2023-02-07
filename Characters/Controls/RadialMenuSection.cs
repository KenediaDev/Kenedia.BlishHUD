using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Drawing;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class RadialMenuSection
    {
        public Triangle Triangle { get; set; }

        public Vector2 IconPos { get; set; }

        public Rectangle Rectangle { get; set; }

        public Rectangle IconRectangle { get; set; }

        public List<PointF> Lines { get; set; }

        public Character_Model Character { get; set; }
    }
}
