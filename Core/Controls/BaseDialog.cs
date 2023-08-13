using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.Core.Controls
{
    public enum DialogResult
    {
        None = 0,
        Cancel,
        OK,
        Yes,
        No,
    };

    public class ButtonDefinition
    {
        public string Title { get; set; }
        public DialogResult Result { get; set; }

        public ButtonDefinition(string title, DialogResult result)
        {
            Title = title;
            Result = result;
        }
    }

    public class BaseDialog : FramedContainer
    {
        private readonly Panel _modalBackground;
        private readonly AsyncTexture2D _backgroundImage = AsyncTexture2D.FromAssetId(156003);
        private readonly AsyncTexture2D _alertImage = AsyncTexture2D.FromAssetId(222246);

        public string Title { get; private set; }

        public string Message { get; private set; }

        private BitmapFont TitleFont { get; set; } = GameService.Content.DefaultFont32;

        private BitmapFont Font { get; set; } = GameService.Content.DefaultFont16;

        private readonly (Button Button, DialogResult Result)[] _buttons = new[]
        {
            (new Button() {Text = "OK", SelectedTint = true, }, DialogResult.OK),
            (new Button() {Text = "Cancel", SelectedTint = true, }, DialogResult.Cancel)
        };

        private readonly FlowPanel _buttonPanel = new()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.AutoSize,
            HeightSizingMode = SizingMode.AutoSize,
        };

        private DialogResult _dialogResult;
        private readonly EventWaitHandle _waitHandle = new(false, EventResetMode.ManualReset);

        private Rectangle _titleBounds;
        private Rectangle _alertBounds;
        private Rectangle _titleTextBounds;
        private Rectangle _messageTextBounds;
        private string _message;

        public ButtonDefinition[] Buttons { get; }

        public int SelectedButtonIndex { get; private set; }

        public bool AutoSize { get; set; } = true;

        public Color? ModalColor { get => _modalBackground.BackgroundColor; set => _modalBackground.BackgroundColor = value; }

        public int DesiredWidth { get; set; } = 300;

        public BaseDialog(string title, string message, ButtonDefinition[] buttons = null)
        {
            Title = title;
            Message = message;
            Buttons = buttons;

            if (buttons is not null)
            {
                _buttons = buttons.Select(x =>
                {
                    return (new Button()
                    {
                        Text = x.Title
                    }, x.Result);
                }).ToArray();
            }

            _modalBackground = new Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = int.MaxValue - 1,
                BackgroundColor = Color.White * 0.2f,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Visible = false,
            };

            BackgroundImage = _backgroundImage;
            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = int.MaxValue;
            BorderColor = Color.Black;
            BorderWidth = new(3);
            ContentPadding = new(5);
            Height = 115;
            Width = 300;
            Visible = false;

            _buttonPanel.Parent = this;
            _buttonPanel.Resized += ButtonPanel_Resized;

            BuildButtons();
            SelectButton();

            GameService.Input.Keyboard.KeyPressed += Keyboard_KeyPressed;
            Parent.Resized += Parent_Resized;
        }

        private void ButtonPanel_Resized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void Parent_Resized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void SelectButton(int direction = 0)
        {
            switch (direction)
            {
                case -1:
                    if (SelectedButtonIndex > 0)
                    {
                        SelectedButtonIndex--;
                    }

                    break;
                case 1:
                    if (SelectedButtonIndex < _buttons.Length - 1)
                    {
                        SelectedButtonIndex++;
                    }

                    break;
            }

            _buttons.ToList().ForEach(b => b.Button.Selected = false);
            _buttons[SelectedButtonIndex].Button.Selected = true;
        }

        private void BuildButtons()
        {
            foreach (var button in _buttons)
            {
                button.Button.Parent = _buttonPanel;
                button.Button.ClickAction = () =>
                {
                    _dialogResult = _buttons.Where(b => b == button).First().Result;
                    _ = _waitHandle.Set();
                };
            }
        }

        private void Keyboard_KeyPressed(object sender, Blish_HUD.Input.KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case Microsoft.Xna.Framework.Input.Keys.Escape:
                    _dialogResult = DialogResult.None;
                    _ = _waitHandle.Set();
                    break;

                case Microsoft.Xna.Framework.Input.Keys.Enter:
                case Microsoft.Xna.Framework.Input.Keys.Space:
                    _dialogResult = _buttons[SelectedButtonIndex].Result;
                    _ = _waitHandle.Set();
                    break;

                case Microsoft.Xna.Framework.Input.Keys.Left:
                    SelectButton(-1);
                    break;

                case Microsoft.Xna.Framework.Input.Keys.Right:
                    SelectButton(1);
                    break;
            }
        }

        public async Task<DialogResult> ShowDialog(CancellationToken cancellationToken = default)
        {
            return await ShowDialog(TimeSpan.FromMinutes(5), cancellationToken);
        }

        public async Task<DialogResult> ShowDialog(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            Show();

            bool waitResult = await _waitHandle.WaitOneAsync(timeout, cancellationToken);

            if (!waitResult)
            {
                _dialogResult = DialogResult.None;
            }

            Hide();

            return _dialogResult;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Parent is not null) Location = new((Parent.Width - Width) / 2, (Parent.Height - Height) / 2);
            TextureRectangle = new(30, 30, Math.Min(Width, _backgroundImage.Width - 60), Math.Min(Height, _backgroundImage.Height - 60));

            _titleBounds = new(ContentRegion.Left, ContentRegion.Top, ContentRegion.Width, TitleFont.LineHeight + 2);
            _alertBounds = new(_titleBounds.Left, _titleBounds.Top, _titleBounds.Height, _titleBounds.Height);
            _titleTextBounds = new(_titleBounds.Left + _alertBounds.Width + 10, _alertBounds.Top, _titleBounds.Width - (_alertBounds.Width * 2), _titleBounds.Height);
            _messageTextBounds = new(_titleBounds.Left, _titleBounds.Bottom + 5, _titleBounds.Width, _buttonPanel.Top - _titleBounds.Bottom + 5);

            if (AutoSize)
            {
                _message = DrawUtil.WrapText(Font, Message, ContentRegion.Width);
                Height = BorderWidth.Vertical + TitleFont.LineHeight + 2 + 5 + (int)Font.GetStringRectangle(_message).Height + 15 + _buttonPanel.Height + ContentPadding.Vertical;

                Width = Math.Max(DesiredWidth, ContentPadding.Horizontal + (_alertBounds.Width * 2) + 10 + (int)TitleFont.GetStringRectangle(Title).Width);
            }

            _buttonPanel.Location = new((ContentRegion.Width - _buttonPanel.Width) / 2, ContentRegion.Height - _buttonPanel.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, Title, TitleFont, _titleTextBounds, ContentService.Colors.ColonialWhite, false, HorizontalAlignment.Center, VerticalAlignment.Top);

            spriteBatch.DrawStringOnCtrl(this, _message, Font, _messageTextBounds, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Top);

            spriteBatch.DrawOnCtrl(this, _alertImage, _alertBounds, _alertImage.Bounds);
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            _modalBackground?.Hide();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _modalBackground?.Show();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _modalBackground?.Dispose();
            GameService.Input.Keyboard.KeyPressed -= Keyboard_KeyPressed;
        }
    }
}
