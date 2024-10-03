using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;
using View = Blish_HUD.Graphics.UI.View;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagGroupView : View
    {
        private FlowPanel _groupsPanel;
        private GroupEditPanel _editPanel;
        private FilterBox _filterBox;
        private ImageButton _addButton;
        private List<GroupSelectable> _groupSelectables = [];

        public TagGroupView(TagGroups tagGroups)
        {
            TagGroups = tagGroups;
        }

        public GroupSelectable SelectedGroup { get; set; }

        public Blish_HUD.Controls.Container BuildPanel { get; private set; }

        public TagGroups TagGroups { get; }

        override protected void Build(Blish_HUD.Controls.Container buildPanel)
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
                PerformFiltering = FilterGroups,
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
                ClickAction = (m) => TagGroups.Add(new(_filterBox.Text)),
                SetLocalizedTooltip = () => "Add Group",
            };

            _groupsPanel = new()
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

            _editPanel = new GroupEditPanel(TagGroups)
            {
                Location = new(_groupsPanel.Right + 5, _filterBox.Bottom + 5),
                CanScroll = true,
                Parent = BuildPanel,
                BorderColor = Color.Black,
                BorderWidth = new Core.Structs.RectangleDimensions(2),
                BackgroundColor = Color.Black * 0.5F,
                Width = BuildPanel.Width - 75 - (_groupsPanel.Width + 5),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            foreach (var g in TagGroups)
            {
                AddGroup(g);
            }

            TagGroups.GroupAdded += TagGroups_TagAdded;
            TagGroups.GroupRemoved += TagGroups_TagRemoved;
            TagGroups.GroupChanged += TagGroups_TagChanged;

            BuildPanel.Resized += BuildPanel_Resized;
        }

        private void BuildPanel_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            var b = e.CurrentSize;

            if (_filterBox is not null)
                _filterBox.Width = b.X - 75 - 27;

            if (_editPanel is not null)
                _editPanel.Width = b.X - 75 - (_groupsPanel.Width + 5);

            if (_addButton is not null)
                _addButton.Location = new(_filterBox.Right + 2, _filterBox.Top);
        }

        private void FilterGroups(string? obj = null)
        {
            obj ??= _filterBox.Text ?? string.Empty;

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

        private void SortSelectables()
        {
            _groupsPanel.SortChildren<GroupSelectable>((a, b) => TemplateTagComparer.CompareGroups(a.Group, b.Group));
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
            _editPanel.Group = group;

            foreach (var g in _groupSelectables)
            {
                g.Selected = g == SelectedGroup;
            }
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

        protected override void Unload()
        {
            base.Unload();

            TagGroups.GroupAdded -= TagGroups_TagAdded;
            TagGroups.GroupRemoved -= TagGroups_TagRemoved;
            TagGroups.GroupChanged -= TagGroups_TagChanged;

            BuildPanel.Resized -= BuildPanel_Resized;
        }
    }
}
