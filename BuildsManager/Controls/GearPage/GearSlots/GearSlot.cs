using Control = Blish_HUD.Controls.Control;
using Container = Blish_HUD.Controls.Container;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using MonoGame.Extended.BitmapFonts;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.Core.Controls;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class GearSlot : Control
    {
        protected readonly DetailedTexture Icon = new() { TextureRegion = new(37, 37, 54, 54) };
        private TemplateSlotType _slot = TemplateSlotType.None;

        protected int MaxTextLength = 52;
        protected Color StatColor = Color.White;
        protected Color UpgradeColor = Color.Orange;
        protected Color InfusionColor = new(153, 238, 221);
        protected Color ItemColor = Color.Gray;

        protected BitmapFont StatFont = Content.DefaultFont16;
        protected BitmapFont UpgradeFont = Content.DefaultFont18;
        protected BitmapFont InfusionFont = Content.DefaultFont12;

        protected TemplatePresenter TemplatePresenter;

        protected ItemTexture ItemTexture { get; } = new() { };

        public BaseItem Item { get => ItemTexture.Item; set => ItemTexture.Item = value; }

        public TemplateSlotType Slot { get => _slot; set => Common.SetProperty(ref _slot, value, ApplySlot); }

        public GearSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;
            Slot = gearSlot;
            Parent = parent;

            Size = new(380, 64);
            ClipsBounds = true;

            Menu = new();
            CreateSubMenus();

            TemplatePresenter.LoadedGearFromCode += SetItems;
            TemplatePresenter.TemplateChanged += SetItems;
        }

        public SelectionPanel SelectionPanel { get; set; }

        public List<GearSlot> SlotGroup { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Icon.Draw(this, spriteBatch, RelativeMousePosition);
            ItemTexture.Draw(this, spriteBatch, RelativeMousePosition, ItemColor);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            //ClickAction?.Invoke();
        }

        protected virtual void ApplySlot()
        {
            var assetIds = new Dictionary<TemplateSlotType, int>()
            {
                { TemplateSlotType.AquaBreather, 156308},
                { TemplateSlotType.Head, 156307},
                { TemplateSlotType.Shoulder, 156311},
                { TemplateSlotType.Chest, 156297},
                { TemplateSlotType.Hand, 156306},
                { TemplateSlotType.Leg, 156309},
                { TemplateSlotType.Foot, 156300},
                { TemplateSlotType.MainHand, 156316},
                { TemplateSlotType.OffHand, 156320},
                { TemplateSlotType.Aquatic, 156313},
                { TemplateSlotType.AltMainHand, 156316},
                { TemplateSlotType.AltOffHand, 156320},
                { TemplateSlotType.AltAquatic, 156313},
                { TemplateSlotType.Back, 156293},
                { TemplateSlotType.Amulet, 156310},
                { TemplateSlotType.Accessory_1, 156298},
                { TemplateSlotType.Accessory_2, 156299},
                { TemplateSlotType.Ring_1, 156301},
                { TemplateSlotType.Ring_2, 156302},
                { TemplateSlotType.PvpAmulet, 784322},
            };

            if (assetIds.TryGetValue(Slot, out int assetId))
            {
                Icon.Texture = AsyncTexture2D.FromAssetId(assetId);
            }

            RecalculateLayout();
        }

        protected string GetDisplayString(string s)
        {
            return s.Length > MaxTextLength ? s.Substring(0, MaxTextLength) + "..." : s;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            Icon.Bounds = new(0, 0, size, size);
            ItemTexture.Bounds = new(0, 0, size, size);
        }

        protected virtual void SetItems(object sender, EventArgs e)
        {

        }

        protected void CreateSubMenu(Func<string> menuGroupName, Func<string> menuGroupTooltip = null, Action menuGroupAction = null, List<(Func<string> text, Func<string> tooltip, Action action)> menuItems = null)
        {
            if (menuItems == null)
            {
                _ = Menu.AddMenuItem(new ContextMenuItem(menuGroupName, menuGroupAction, menuGroupTooltip));
                return;
            }

            var menuGroup = Menu.AddMenuItem(new ContextMenuItem(menuGroupName, menuGroupAction, menuGroupTooltip)).Submenu = new();

            foreach (var (text, tooltip, action) in menuItems ?? new())
            {
                _ = menuGroup.AddMenuItem(new ContextMenuItem(text, action, tooltip));
            }
        }

        protected virtual void CreateSubMenus()
        {

        }
    }
}
