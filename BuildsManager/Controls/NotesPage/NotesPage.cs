using Blish_HUD.Controls;
using Kenedia.Modules.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Controls.NotesPage
{
    public class NotesPage : Container
    {
        private readonly MultilineTextBox _noteField;
        private TexturesService _texturesService;

        public NotesPage(TexturesService texturesService)
        {
            _texturesService = texturesService;

            _noteField = new()
            {
                Parent = this,
                HideBackground = false,
            };
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if(_noteField != null) _noteField.Size = Size;
        }
    }
}
