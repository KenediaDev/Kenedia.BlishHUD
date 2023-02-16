using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class GuardianSpecifics
    {
        private Template _template;

        private Dictionary<int, DetailedTexture> _fivePages = new()
        {
            { 0, new (1636723)},
            { 1, new (1636724)},
            { 2, new (1636725)},
            { 3, new (1636726)},
            { 4, new (1636727)},
            { 5, new (1636728)},
        };
        private Dictionary<int, DetailedTexture> _eightPages = new()
        {
            { 0, new (1636729)},
            { 1, new (1636730)},
            { 2, new (1636731)},
            { 3, new (1636732)},
            { 4, new (1636733)},
            { 5, new (1636734)},
            { 6, new (1636735)},
            { 7, new (1636736)},
            { 8, new (1636737)},
        };

        public GuardianSpecifics()
        {

        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public void Paint(Blish_HUD.Controls.Control control, SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        public void RecalculateLayout()
        {

        }

        private void ApplyTemplate()
        {
            RecalculateLayout();
        }
    }
}
