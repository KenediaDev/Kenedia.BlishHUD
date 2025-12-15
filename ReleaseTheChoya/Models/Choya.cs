using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Res;

namespace Kenedia.Modules.ReleaseTheChoya.Models
{
    [DataContract]
    public class Choya : IDisposable
    {
        private SettingEntry<Choya> _setting;
        private bool _isDisposed;
        private Vector2 _travelDistance = new(4, 2);
        private Rectangle _bounds = new(50, 50, 96, 96);
        private Panel _panel;
        private FlowPanel _contentPanel;
        private ImageButton _deleteBtn;
        private Button _editBtn;
        private TextBox _textBox;
        private Dummy _dummy;
        private Label _label;
        private (Label, NumberBox) _stepsControls;
        private (Label, NumberBox, NumberBox) _travelDistanceControls;
        private Checkbox _canMoveControl;

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name
        {
            get;
            set => Common.SetProperty(field, value, v => field = v, UpdateSetting);
        } = "New Choya";

        [DataMember]
        public int Steps
        {
            get;
            set => Common.SetProperty(field, value, v => field = v, UpdateSetting);
        } = 360;

        [DataMember]
        public Vector2 TravelDistance
        {
            get => _travelDistance;
            set => Common.SetProperty(ref _travelDistance, value, UpdateSetting);
        }

        [DataMember]
        public Rectangle Bounds
        {
            get => _bounds;
            set => Common.SetProperty(ref _bounds, value, UpdateSetting);
        }

        [DataMember]
        public bool CanMove
        {
            get;
            set => Common.SetProperty(field, value, v => field = v, UpdateSetting);
        }

        public SettingCollection StaticChoya { get; private set; }

        public RollingChoya Control { get; set; }

        public ResizeableContainer Container { get; set; }

        public void ToggleEdit(bool? show = null)
        {
            if (!Container.CanChange || show == true)
            {
                Container.BackgroundColor = Color.Black * 0.5f;
                Container.BorderColor = ContentService.Colors.ColonialWhite;
                Container.BorderWidth = new(2);
                Container.CanChange = true;
                Container.CaptureInput = true;
            }
            else
            {
                Container.BackgroundColor = Color.Transparent;
                Container.BorderColor = Color.Transparent;
                Container.BorderWidth = new(2);
                Container.CanChange = false;
                Container.CaptureInput = false;
            }
        }

        public void Initialize(SettingCollection staticChoya, AsyncTexture2D choyaTexture, Blish_HUD.Controls.Container settingsPanel)
        {
            StaticChoya = staticChoya;
            if (string.IsNullOrEmpty(Id)) Id = Guid.NewGuid().ToString();

            _setting = !staticChoya.ContainsSetting(Id)
                ? staticChoya.DefineSetting(Id, new Choya()
                {
                    Id = Id,
                    Name = Name,
                    Bounds = Bounds,
                    Steps = Steps,
                    TravelDistance = TravelDistance,
                    CanMove = true,
                })
                : (SettingEntry<Choya>)staticChoya[Id];

            CreateSettingControls(settingsPanel);

            Container = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = _setting.Value.Bounds.Location,
                Size = _setting.Value.Bounds.Size,
                MaxSize = new(int.MaxValue),
                Visible = true,
                BasicTooltipText = Name,
                CaptureInput = false,
                CanChange = false,
            };

            Control = new()
            {
                ChoyaTexture = choyaTexture,
                Parent = Container,
                Size = new(Math.Min(_setting.Value.Bounds.Size.X, _setting.Value.Bounds.Size.Y)),
                Steps = Steps,
                TravelDistance = TravelDistance,
                CanMove = CanMove,
                CaptureInput = false,
                Enabled = false,
            };

            Container.Moved += UpdateBounds;
            Container.Resized += UpdateBounds;
        }

        public void Delete()
        {
            _setting.Value = null;
            _setting = null;
            StaticChoya.UndefineSetting(Id);
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Container?.Dispose();
            Control?.Dispose();

            _dummy?.Dispose();
            _editBtn?.Dispose();
            _deleteBtn?.Dispose();
            _textBox?.Dispose();
            _label?.Dispose();
            _contentPanel?.Dispose();
            _panel?.Dispose();
        }

        private void UpdateBounds(object sender, EventArgs e)
        {
            Bounds = new Rectangle(Container.Left + Container.BorderWidth.Left, Container.Top + Container.BorderWidth.Top, Container.Width - Container.BorderWidth.Horizontal, Container.Height - Container.BorderWidth.Vertical);
            Control.Size = new(Math.Min(Bounds.Size.X, Bounds.Size.Y));
            //Control.Location = Bounds.Location;
            //Control.MovementBounds = Bounds;
            Control.ResetPosition();

            UpdateSetting();
        }

        private void UpdateSetting()
        {
            if (_setting is not null)
            {
                _setting.Value = new()
                {
                    Id = Id,
                    Name = Name,
                    Bounds = Bounds,
                    Steps = Steps,
                    TravelDistance = TravelDistance,
                    CanMove = CanMove,
                };

                _panel.Title = $"{Name} {Bounds}";
                Container.BasicTooltipText = $"{Name}";
                Control.Steps = Steps;
                Control.TravelDistance = TravelDistance;
                Control.CanMove = CanMove;
            }
        }

        private void CreateSettingControls(Blish_HUD.Controls.Container parent)
        {
            _panel = new Panel()
            {
                Parent = parent,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,                
                Title = $"{Name} {Bounds}",
                CanCollapse = true,
            };

            _contentPanel = new FlowPanel()
            {
                Parent = _panel,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.LeftToRight,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ContentPadding = new(0, 8, 0, 8),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                ControlPadding = new(3),
                BackgroundColor = Color.Black * 0.4f,
            };

            _textBox = new TextBox()
            {
                Parent = _contentPanel,
                Width = 245,
                Height = 24,
                Text = Name,
                TextChangedAction = (t) => Name = t,
            };

            _dummy = new Dummy()
            {
                Parent = _contentPanel,
                Width = 115,
            };

            _editBtn = new Button()
            {
                Parent = _contentPanel,
                Size = new(100, 24),
                SetLocalizedText = () => strings_common.Edit,
                ClickAction = () => ToggleEdit(),
            };

            _deleteBtn = new ImageButton()
            {
                Parent = _contentPanel,
                Size = new(24, 24),
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                TextureRectangle = new Rectangle(5, 5, 24, 24),
                ClickAction = (m) => Delete(),
            };

            _stepsControls = CreateNumberSetting(_contentPanel, () => "Rotating Speed", null);
            _stepsControls.Item2.Value = Steps;
            _stepsControls.Item2.MaxValue = 360000;
            _stepsControls.Item2.MinValue = 0;
            _stepsControls.Item2.ValueChangedAction = (v) => Steps = v;

            _travelDistanceControls = CreateDoubleNumberSetting(_contentPanel, () => "Travel Distance", null);
            _travelDistanceControls.Item2.Value = (int)TravelDistance.X;
            _travelDistanceControls.Item3.Value = (int)TravelDistance.Y;
            _travelDistanceControls.Item2.ValueChangedAction = (v) => TravelDistance = new(v, TravelDistance.Y);
            _travelDistanceControls.Item3.ValueChangedAction = (v) => TravelDistance = new(TravelDistance.X, v);

            _canMoveControl = new Checkbox()
            {
                Parent = _contentPanel,
                Width = 245,
                SetLocalizedText = () => "Can Move",
                Checked = CanMove,
                CheckedChangedAction = (b) => CanMove = b,
            };
        }

        private (Label, NumberBox) CreateNumberSetting(Blish_HUD.Controls.Container parent, Func<string> setLocalizedText, Func<string> setLocalizedTooltip)
        {
            var p = new Panel()
            {
                Parent = parent,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                ContentPadding = new(2),
            };

            var label = new Label()
            {
                Parent = p,
                Width = 225,
                Height = 20,
                SetLocalizedText = setLocalizedText,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var num = new NumberBox()
            {
                Location = new(250, 0),
                Width = 120,
                Parent = p,
                MinValue = 0,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            return (label, num);
        }

        private (Label, NumberBox, NumberBox) CreateDoubleNumberSetting(Blish_HUD.Controls.Container parent, Func<string> setLocalizedText, Func<string> setLocalizedTooltip)
        {
            var p = new Panel()
            {
                Parent = parent,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            var label = new Label()
            {
                Parent = p,
                Width = 225,
                Height = 20,
                SetLocalizedText = setLocalizedText,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var num = new NumberBox()
            {
                Location = new(250, 0),
                Width = 120,
                Parent = p,
                MinValue = -50,
                MaxValue = 50,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var num2 = new NumberBox()
            {
                Location = new(num.Right + 5, 0),
                Width = 120,
                Parent = p,
                MinValue = -50,
                MaxValue = 50,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            return (label, num, num2);
        }
    }
}
