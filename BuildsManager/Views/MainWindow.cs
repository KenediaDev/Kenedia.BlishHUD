using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls.BuildPage;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly Panel _buildSection;
        private readonly Panel _selectionSection;
        private readonly BuildPage _build;
        private readonly TextBox _buildCodeBox;

        private Template _template;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data) : base(background, windowRegion, contentRegion)
        {
            _data = data;

            _selectionSection = new()
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 330,
                //BackgroundColor = Color.Yellow * 0.2F,
            };

            _buildSection = new()
            {
                Parent = this,
                Location = new(_selectionSection.Right + 15, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = ContentRegion.Width - 144,
                //BackgroundColor = Color.Green * 0.2F,
            };

            _buildCodeBox = new()
            {
                Parent = _buildSection,
                EnterPressedAction = (code) =>
                {
                    Template.BuildTemplate.LoadFromCode(code);
                    ApplyTemplate();
                }
            };

            _build = new BuildPage()
            {
                Parent = _buildSection,
                Location = new(0, _buildCodeBox.Bottom),
            };
            _build.BuildAdjusted += BuildAdjusted;
        }

        private void BuildAdjusted(object sender, EventArgs e)
        {
            _buildCodeBox.Text = Template?.BuildTemplate.ParseBuildCode();
        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if(_buildCodeBox != null) _buildCodeBox.Width = _buildSection.Width;
        }

        private void ApplyTemplate()
        {
            _build.Template = _template;
            _buildCodeBox.Text = Template?.BuildTemplate.ParseBuildCode();
            _build.ApplyTemplate();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _build?.Dispose();
            _buildSection?.Dispose();
            _selectionSection?.Dispose();
            _buildCodeBox?.Dispose();
        }
    }
}
