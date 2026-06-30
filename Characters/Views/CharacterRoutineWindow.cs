using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.ObjectModel;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StandardWindow = Kenedia.Modules.Core.Views.StandardWindow;

namespace Kenedia.Modules.Characters.Views
{
    public class CharacterRoutineWindow : StandardWindow
    {
        private readonly CharacterRoutineService _service;
        private readonly FlowPanel _contentPanel;
        private readonly CharacterRoutineSidebar _sidebar;
        private readonly CharacterRoutineDetailPanel _detailPanel;
        private bool _created;

        public CharacterRoutineWindow(
            AsyncTexture2D background,
            Rectangle windowRegion,
            Rectangle contentRegion,
            Settings settings,
            CharacterRoutineService service,
            TextureManager textureManager,
            CharacterSwapping characterSwapping,
            ObservableCollection<Character_Model> characterModels)
            : base(background, windowRegion, contentRegion)
        {
            _service = service;

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(5, 0),
            };

            _sidebar = new CharacterRoutineSidebar(_service)
            {
                Parent = _contentPanel,
            };

            _detailPanel = new CharacterRoutineDetailPanel(textureManager, _service, characterSwapping, settings, characterModels, contentRegion.Width - 210)
            {
                Parent = _contentPanel,
            };

            _created = true;
        }

        public CharacterRoutineService Service => _service;

        public void SwitchToNextRoutineStep()
        {
            _service.SwitchToNextIncompleteRoutineStep();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_created)
            {
                _detailPanel.Width = ContentRegion.Width - 210;
            }

            Width = Math.Max(Width, 530);
            Height = Math.Max(Height, 400);
        }

        protected override void DisposeControl()
        {
            _contentPanel?.Dispose();
            _sidebar?.Dispose();
            _detailPanel?.Dispose();

            base.DisposeControl();
        }
    }
}
