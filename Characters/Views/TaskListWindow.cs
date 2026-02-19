using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StandardWindow = Kenedia.Modules.Core.Views.StandardWindow;

namespace Kenedia.Modules.Characters.Views
{
    public class TaskListWindow : StandardWindow
    {
        private readonly TaskListService _service;
        private readonly FlowPanel _contentPanel;
        private readonly TaskListSidebar _sidebar;
        private readonly TaskListDetailPanel _detailPanel;
        private bool _created;

        public TaskListWindow(
            AsyncTexture2D background,
            Rectangle windowRegion,
            Rectangle contentRegion,
            Settings settings,
            TaskListService service,
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

            _sidebar = new TaskListSidebar(_service)
            {
                Parent = _contentPanel,
            };

            _detailPanel = new TaskListDetailPanel(_service, settings, characterModels, contentRegion.Width - 210)
            {
                Parent = _contentPanel,
            };

            _created = true;
        }

        public TaskListService Service => _service;

        public void SwitchToNextCharacter()
        {
            _service.SwitchToNextIncompleteEntry();
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
        }

        protected override void DisposeControl()
        {
            _contentPanel?.Dispose();
            _sidebar?.Dispose();
            _detailPanel?.Dispose();
            _service?.Dispose();

            base.DisposeControl();
        }
    }
}
