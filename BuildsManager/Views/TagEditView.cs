using Blish_HUD;
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
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditView : View
    {
        private FlowPanel _tagsPanel;
        private TagEditPanel _tagEditPanel;
        private FilterBox _tagFilterBox;
        private ImageButton _tagAddButton;
        private List<TagSelectable> _tagSelectables = [];

        private FlowPanel _groupsPanel;
        private GroupEditPanel _groupEditPanel;
        private FilterBox _groupFilterBox;
        private ImageButton _groupAddButton;
        private List<GroupSelectable> _groupSelectables = [];
        private Panel _tagsParent;
        private Panel _groupParent;

        public TagEditView(TemplateTags templateTags, TagGroups tagGroups)
        {
            TemplateTags = templateTags;
            TagGroups = tagGroups;
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public TagSelectable SelectedTag { get; set; }

        public GroupSelectable SelectedGroup { get; set; }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            int width = buildPanel.Size.X;

            _tagsParent = new Panel()
            {
                Parent = buildPanel,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = width / 3 * 2,
            };

            _groupParent = new Panel()
            {
                Parent = buildPanel,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = width / 3,
                Location = new Point(_tagsParent.Right, _tagsParent.Top).Add(new(0, 0)),
            };

            BuildTagView(_tagsParent);
            BuildGroupView(_groupParent);

            buildPanel.Resized += BuildPanel_Resized;
        }

        private void BuildPanel_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            int width = e.CurrentSize.X;
            return;

            if (_tagsParent is not null)
            {
                _tagsParent.Width = (width - 0) / 2;
            }

            if (_groupParent is not null)
            {
                _groupParent.Width = (width - 0) / 2;
                _groupParent.Location = _tagsParent.Location.Add(new(0, 0));
            }
        }

        private void BuildGroupView(Blish_HUD.Controls.Container buildPanel)
        {
           var lbl = new Label()
            {
                Parent = buildPanel,
                Location = new(0, 0),
                SetLocalizedText = () => strings.Groups,
                Font = GameService.Content.DefaultFont18,
                Height = GameService.Content.DefaultFont18.LineHeight + 2,
                AutoSizeWidth = true,
            };

            var sep = new Separator()
            {
                Parent = buildPanel,
                Location = new Point(lbl.LocalBounds.X, lbl.LocalBounds.Bottom).Add(new(0, 2)),
                Width = buildPanel.Width - 25,
                Height = 2,
                Color = Color.White * 0.8F,
            };

            _groupFilterBox = new()
            {
                Parent = buildPanel,
                Location = new Point(sep.LocalBounds.X, sep.LocalBounds.Bottom).Add(new(0, 10)),
                Width = buildPanel.Width - 50,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                PerformFiltering = FilterGroups,
            };

            _groupAddButton = new()
            {
                Parent = buildPanel,
                Location = new(_groupFilterBox.Right + 2, _groupFilterBox.Top),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                Size = new Point(_groupFilterBox.Height),
                ClickAction = (m) => TagGroups.Add(new(_groupFilterBox.Text)),
                SetLocalizedTooltip = () => strings.AddGroup,
            };

            _groupsPanel = new()
            {
                Parent = buildPanel,
                Location = new(_groupFilterBox.Left, _groupFilterBox.Bottom + 5),
                ContentPadding = new(5, 5, 0, 0),
                Width = buildPanel.Width / 2,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                CanScroll = true,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ControlPadding = new(0, 2)
            };

            _groupEditPanel = new GroupEditPanel(TagGroups)
            {
                Location = new(_groupsPanel.Right + 5, _groupFilterBox.Bottom + 5),
                CanScroll = true,
                Parent = buildPanel,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                Width = buildPanel.Width - (_groupsPanel.Right + 27),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            foreach (var g in TagGroups)
            {
                AddGroup(g);
            }

            TagGroups.GroupAdded += TagGroups_TagAdded;
            TagGroups.GroupRemoved += TagGroups_TagRemoved;
            TagGroups.GroupChanged += TagGroups_TagChanged;

            buildPanel.Resized += GroupBuildPanel_Resized;

            var selectable = _groupsPanel.OfType<GroupSelectable>().FirstOrDefault(x => x.Visible);
            SetGroupToEdit(selectable.Group);
        }

        private void FilterGroups(string? obj = null)
        {
            obj ??= _groupFilterBox.Text ?? string.Empty;

            _groupsPanel.SuspendLayout();

            obj = obj.ToLowerInvariant();
            bool isEmpty = string.IsNullOrEmpty(obj);

            foreach (var g in _groupSelectables)
            {
                g.Visible = isEmpty || g.Group.Name.ToLowerInvariant().Contains(obj);
            }

            _groupsPanel.ResumeLayout();
            _groupsPanel.Invalidate();
            SortSelectables();
        }

        private void BuildTagView(Blish_HUD.Controls.Container buildPanel)
        {
            var lbl = new Label()
            {
                Parent = buildPanel,
                Location = new(50, 0),
                SetLocalizedText = () => strings.Tags,
                Font = GameService.Content.DefaultFont18,
                Height = GameService.Content.DefaultFont18.LineHeight + 2,
                AutoSizeWidth = true,
            };

            var sep = new Separator()
            {
                Parent = buildPanel,
                Location = new Point(lbl.LocalBounds.X, lbl.LocalBounds.Bottom).Add(new(0, 2)),
                Width = buildPanel.Width - 75,
                Height = 2,
                Color = Color.White * 0.8F,
            };

            _tagFilterBox = new()
            {
                Parent = buildPanel,
                Location = new Point(sep.LocalBounds.X, sep.LocalBounds.Bottom).Add(new(0, 10)),
                Width = buildPanel.Width - 75 - 27,
                SetLocalizedPlaceholder = () => strings_common.Search,
                FilteringOnTextChange = true,
                PerformFiltering = FilterTags,
            };

            _tagAddButton = new()
            {
                Parent = buildPanel,
                Location = new(_tagFilterBox.Right + 2, _tagFilterBox.Top),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                Size = new Point(_tagFilterBox.Height),
                ClickAction = (m) => TemplateTags.Add(new(_tagFilterBox.Text)),
                SetLocalizedTooltip = () => strings.AddTag,
            };

            _tagsPanel = new()
            {
                Parent = buildPanel,
                Location = new(50, _tagFilterBox.Bottom + 5),
                ContentPadding = new(5, 5, 0, 0),
                Width = (buildPanel.Width - 75) / 3,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                CanScroll = true,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ControlPadding = new(0, 2)
            };

            _tagEditPanel = new TagEditPanel(TagGroups)
            {
                Location = new(_tagsPanel.Right + 5, _tagFilterBox.Bottom + 5),
                CanScroll = true,
                Parent = buildPanel,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                Width = buildPanel.Width - 75 - (_tagsPanel.Width + 5),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            foreach (var g in TemplateTags)
            {
                AddTag(g);
            }

            TemplateTags.TagAdded += TemplateTags_TagAdded;
            TemplateTags.TagRemoved += TemplateTags_TagRemoved;
            TemplateTags.TagChanged += TemplateTags_TagChanged;

            buildPanel.Resized += TagBuildPanel_Resized;

            var selectable = _tagsPanel.OfType<TagSelectable>().FirstOrDefault(x => x.Visible);
            SetTagToEdit(selectable.Tag);
        }

        private void GroupBuildPanel_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            var b = e.CurrentSize;
            return;

            if (_groupFilterBox is not null)
                _groupFilterBox.Width = b.X - 75 - 27;

            if (_tagEditPanel is not null)
                _tagEditPanel.Width = b.X - 75 - (_tagsPanel.Width + 5);

            if (_tagAddButton is not null)
                _tagAddButton.Location = new(_tagFilterBox.Right + 2, _tagFilterBox.Top);
        }

        private void TagBuildPanel_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            var b = e.CurrentSize;
            return;

            if (_tagFilterBox is not null)
                _tagFilterBox.Width = b.X - 75 - 27;

            if (_tagEditPanel is not null)
                _tagEditPanel.Width = b.X - 75 - (_tagsPanel.Width + 5);

            if (_tagAddButton is not null)
                _tagAddButton.Location = new(_tagFilterBox.Right + 2, _tagFilterBox.Top);
        }

        private void FilterTags(string? obj = null)
        {
            obj ??= _tagFilterBox.Text ?? string.Empty;
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

        public void SetTagToEdit(TemplateTag? tag)
        {
            SelectedTag = _tagSelectables.FirstOrDefault(x => x.Tag == tag);

            if (_tagEditPanel is not null)
                _tagEditPanel.Tag = tag;

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

        private void TagGroups_TagChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterGroups();
        }

        private void TagGroups_TagRemoved(object sender, TagGroup e)
        {
            var group = _groupSelectables.FirstOrDefault(x => x.Group == e);

            if (group is not null)
            {
                _groupSelectables.Remove(group);
                group.Dispose();

                FilterGroups();
            }
        }

        private void TagGroups_TagAdded(object sender, TagGroup e)
        {
            AddGroup(e);
        }

        private void AddGroup(TagGroup g)
        {
            _groupSelectables.Add(new(g, _groupsPanel, TagGroups)
            {
                OnClickAction = SetGroupToEdit,
            });

            FilterGroups();
        }

        private void SetGroupToEdit(TagGroup group)
        {
            SelectedGroup = _groupSelectables.FirstOrDefault(x => x.Group == group);
            _groupEditPanel.Group = group;

            foreach (var g in _groupSelectables)
            {
                g.Selected = g == SelectedGroup;
            }
        }

        protected override void Unload()
        {
            base.Unload();

            TemplateTags.TagAdded -= TemplateTags_TagAdded;
            TemplateTags.TagRemoved -= TemplateTags_TagRemoved;
            TemplateTags.TagChanged -= TemplateTags_TagChanged;

            _tagsParent.Resized -= TagBuildPanel_Resized;
            _groupParent.Resized -= GroupBuildPanel_Resized;
        }
    }
}
