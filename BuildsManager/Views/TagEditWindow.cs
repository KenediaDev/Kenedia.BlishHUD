using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditWindow : StandardWindow
    {
        private readonly TemplateTags _templateTags;

        private FilterBox _tagFilter;
        private FlowPanel _tagPanel;
        private readonly ButtonImage _editTags;

        public TagEditWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, TemplateTags templateTags) : base(background, windowRegion, contentRegion)
        {
            _templateTags = templateTags;

            _templateTags.TagsChanged += TemplateTags_TagsChanged;

            _tagFilter = new()
            {
                Parent = this,
                Location = new(0, 0),
                Width = 530,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                FilteringOnEnter = true,
                EnterPressedAction = (txt) =>
                {
                    if (!string.IsNullOrEmpty(txt.Trim()))
                    {
                        var templateTag = _templateTags.Tags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower());

                        if (templateTag is null)
                        {
                            _templateTags.Add(new TemplateTag() { Name = txt });
                        }
                        else
                        {

                        }
                    }
                },
                TextChangedAction = (txt) => _editTags.Enabled = !string.IsNullOrEmpty(txt.Trim()) && _templateTags.Tags.FirstOrDefault(e => e.Name.ToLower() == txt.ToLower()) is null,
                PerformFiltering = (txt) =>
                {
                    string t = txt.ToLower();
                    bool any = string.IsNullOrEmpty(t);

                    foreach (var tag in _tagPanel.GetChildrenOfType<TagControl>())
                    {
                        tag.Visible = any || tag.Tag.Name.ToLower().Contains(t);
                    }

                    _tagPanel.Invalidate();
                }
            };

            _editTags = new()
            {
                Parent = this,
                Size = new(_tagFilter.Height),
                Location = new(_tagFilter.Right + 2, _tagFilter.Top),
                Texture = AsyncTexture2D.FromAssetId(255443),
                HoveredTexture = AsyncTexture2D.FromAssetId(255297),
                DisabledTexture = AsyncTexture2D.FromAssetId(255296),
                SetLocalizedTooltip = () => "Add Tag",
                Enabled = true,
                ClickAction = (b) => _templateTags.Add(new TemplateTag() { Name = string.IsNullOrEmpty(_tagFilter.Text) ? "New Tag" : _tagFilter.Text })
            };

            _tagPanel = new()
            {
                Parent = this,
                Location = new(0, _tagFilter.Bottom + 2),
                Width = _editTags.Right,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ShowBorder = false,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                BackgroundColor = Color.Black * 0.4F,
                ShowRightBorder = true,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5),
                CanScroll = true,
            };

            CreateTagControls();
        }

        protected override void OnResized(Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnResized(e);

            _tagFilter?.SetSize(Width - 50);
            _editTags?.SetLocation(new(_tagFilter.Right + 2, _tagFilter.Top));
            _tagPanel?.SetLocation(new(0, _tagFilter.Bottom + 2));
            _tagPanel?.SetSize(_editTags.Right);

            ResizeTagControls();
        }

        private void ResizeTagControls()
        {
            if (_tagPanel?.Children?.OfType<TagEditControl>() is var tags && tags?.Any() is true)
            {
                foreach (var tag in tags)
                {
                    tag.Width = _tagPanel.Width - 25;
                }
            }
        }

        private void TemplateTags_TagsChanged(object sender, Models.TemplateTag e)
        {
            CreateTagControls();
        }

        private void CreateTagControls()
        {
            _tagPanel?.ClearChildren();

            foreach (var tag in _templateTags.Tags)
            {
                _ = new TagEditControl()
                {
                    Parent = _tagPanel,
                    Tag = tag,
                    Width = _tagPanel.Width - 25,
                    TemplateTags = _templateTags,
                };
            }
        }
    }
}
