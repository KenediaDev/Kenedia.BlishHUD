using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using System.ComponentModel;

namespace Kenedia.Modules.BuildsManager.Controls.NotesPage
{
    public class RotationPage : Blish_HUD.Controls.Container
    {
        private readonly Blish_HUD.Controls.MultilineTextBox _noteField;
        private readonly bool created = false;
        private TexturesService _texturesService;

        private Template _template;

        public RotationPage(TexturesService texturesService)
        {
            _texturesService = texturesService;

            _noteField = new()
            {
                Parent = this,
                HideBackground = false,
            };

            created = true;
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        private void ApplyTemplate()
        {

        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!created) return;

            if (_noteField != null) _noteField.Size = Size;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if (_template != null) _template.Changed -= TemplateChanged;
        }
    }
}
