using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class BuildSelection : BaseSelection
    {
        private readonly List<DetailedTexture> _specIcons = new();

        private readonly Dropdown _sortBehavior;

        public BuildSelection()
        {
            _sortBehavior = new()
            {
                Parent = this,
                Location = new(0, 30),
            };
            _sortBehavior.Items.Add("Sort by Profession");
            _sortBehavior.Items.Add("Sort by Name");
            _sortBehavior.Items.Add("Sort by Game Mode");

            Search.Location = new(2, 60);
            SelectionContent.Location = new(0, Search.Bottom + 5);

            int i = 0;
            int size = 30;
            Point start = new(0, 0);
            foreach (var prof in BuildsManager.Data.Professions.Values)
            {
                int j = 0;
                _specIcons.Add(new DetailedTexture(prof.Icon)
                {
                    Bounds = new(start.X + (i * (size + 5)), start.Y + (j * size), size, size),
                    DrawColor = Color.White * 0.7F,
                    HoverDrawColor = ColorExtension.GetProfessionColor(prof.Id),
                });

                //foreach (var spec in prof.Specializations.Values)
                //{                    
                //    if (spec.Elite)
                //    {
                //        j++;
                //        _specIcons.Add(new DetailedTexture(spec.ProfessionIconBig.Texture.ToGrayScaledPalettable())
                //        {
                //            Bounds = new(i * 25, j * 25, 25, 25),
                //            DrawColor = ColorExtension.Guardian * 0.7F,
                //            HoverDrawColor = ColorExtension.Guardian,
                //        });
                //    }
                //}

                i++;
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            foreach (var specIcon in _specIcons)
            {
                specIcon.Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _sortBehavior?.SetLocation(Search.Left);
            _sortBehavior?.SetSize(Search.Width);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _sortBehavior?.Dispose();
        }
    }
}
