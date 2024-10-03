using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditView : View
    {
        private FlowPanel _tagsPanel;
        private TagEditPanel _editPanel;
        private FilterBox _filterBox;
        private ImageButton _addButton;
        private List<TagSelectable> _tagSelectables = [];

        public TagEditView(TemplateTags templateTags, TagGroups tagGroups)
        {
            TemplateTags = templateTags;
            TagGroups = tagGroups;
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public TagSelectable SelectedTag { get; set; }

        public Blish_HUD.Controls.Container BuildPanel { get; private set; }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            BuildPanel = buildPanel;

            _filterBox = new()
            {
                Parent = BuildPanel,
                Location = new(50, 0),
                Width = BuildPanel.Width - 75 - 27,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                PerformFiltering = FilterTags,
            };

            _addButton = new()
            {
                Parent = BuildPanel,
                Location = new(_filterBox.Right + 2, _filterBox.Top),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                Size = new Point(_filterBox.Height),
                ClickAction = (m) => TemplateTags.Add(new(_filterBox.Text)),
                SetLocalizedTooltip = () => strings.AddTag,
            };

            _tagsPanel = new()
            {
                Parent = BuildPanel,
                Location = new(50, _filterBox.Bottom + 5),
                ContentPadding = new(5, 5, 0, 0),
                Width = (BuildPanel.Width - 75) / 3,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                CanScroll = true,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ControlPadding = new(0, 2)
            };

            _editPanel = new TagEditPanel(TagGroups)
            {
                Location = new(_tagsPanel.Right + 5, _filterBox.Bottom + 5),
                CanScroll = true,
                Parent = BuildPanel,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                Width = BuildPanel.Width - 75 - (_tagsPanel.Width + 5),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            foreach (var g in TemplateTags)
            {
                AddTag(g);
            }

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagRemoved += TemplateTags_TagRemoved;
            TemplateTags.TagChanged += TemplateTags_TagChanged;

            BuildPanel.Resized += BuildPanel_Resized;
        }

        private void BuildPanel_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            var b = e.CurrentSize;

            if (_filterBox is not null)
                _filterBox.Width = b.X - 75 - 27;

            if (_editPanel is not null)
                _editPanel.Width = b.X - 75 - (_tagsPanel.Width + 5);

            if (_addButton is not null)
                _addButton.Location = new(_filterBox.Right + 2, _filterBox.Top);
        }

        private void FilterTags(string? obj = null)
        {
            obj ??= _filterBox.Text ?? string.Empty;
            _tagsPanel.SuspendLayout();

            obj = obj.ToLowerInvariant();

            foreach (var g in _tagSelectables)
            {
                g.Visible = g.Tag.Name.ToLowerInvariant().Contains(obj);
            }

            _tagsPanel.ResumeLayout();
            _tagsPanel.Invalidate();

            SortSelectables();
        }

        private void SortSelectables()
        {
            var comparerer = new TemplateTagComparer(TagGroups);
            _tagsPanel.SortChildren<TagSelectable>((a, b) => comparerer.Compare(a.Tag, b.Tag));
        }

        private void AddTag(TemplateTag g)
        {
            _tagSelectables.Add(new(g, _tagsPanel, TemplateTags)
            {
                OnClickAction = SetTagToEdit,
            });
        }

        public void SetTagToEdit(TemplateTag tag)
        {
            SelectedTag = _tagSelectables.FirstOrDefault(x => x.Tag == tag);
            _editPanel.Tag = tag;

            foreach (var g in _tagSelectables)
            {
                g.Selected = g == SelectedTag;
            }
        }

        private void TemplateTags_TagChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterTags();
        }

        private void TemplateTags_TagRemoved(object sender, TemplateTag e)
        {
            var group = _tagSelectables.FirstOrDefault(x => x.Tag == e);

            if (group is not null)
            {
                _tagSelectables.Remove(group);
                group.Dispose();

                FilterTags();
            }
        }

        private void TemplateTags_TagAdded(object sender, TemplateTag e)
        {
            AddTag(e);
            FilterTags();
        }

        protected override void Unload()
        {
            base.Unload();

            TemplateTags.TagAdded -= TemplateTags_TagAdded;
            TemplateTags.TagRemoved -= TemplateTags_TagRemoved;
            TemplateTags.TagChanged -= TemplateTags_TagChanged;

            BuildPanel.Resized -= BuildPanel_Resized;
        }
    }
}
