using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Gw2Mumble;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Skill = Gw2Sharp.WebApi.V2.Models.Skill;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class SkillConnectionEditor : StandardWindow
    {
        private Dictionary<int, SkillConnection> _connections = new();
        private ProfessionType _profession = ProfessionType.Guardian;
        private readonly EditingControl _connectionEdit;
        private readonly SkillConnectionSelector _selector;
        private readonly Dropdown _specialization;

        public Dictionary<int, SkillConnection> Connections { get => _connections; set => Common.SetProperty(ref _connections, value, CreateUI); }

        public SkillConnectionEditor(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            _specialization = new()
            {
                Parent = this,
                Width = 300,
                ValueChangedAction = (v) => Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), v)
            };

            foreach (ProfessionType p in Enum.GetValues(typeof(ProfessionType)))
            {
                _specialization.Items.Add(p.ToString());
            }

            _selector = new(this)
            {
                Parent = this,
                Location = new(0, _specialization.Bottom + 10),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 300,
                OnClickAction = (cn, arg2) => _connectionEdit.SkillConnection = cn.SkillConnection
            };

            _connectionEdit = new(this)
            {
                Parent = this,
                Location = new(_selector.Right + 10, 0),
                BorderColor = Color.Black,
                BorderWidth = new(2),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (player != null)
            {
                Profession = player.Profession;
            }

            _specialization.SelectedItem = Profession.ToString();
        }

        public event EventHandler<ProfessionType> ProfessionChanged;

        public ProfessionType Profession { get => _profession; private set => Common.SetProperty(ref _profession, value, OnProfessionChanged); }

        private void OnProfessionChanged()
        {
            ProfessionChanged?.Invoke(this, Profession);
        }

        private void CreateUI()
        {
            _connectionEdit.Connections = Connections;
            _selector.Items = Connections;
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (_selector != null) _selector.Width = 300;
        }
    }
}
