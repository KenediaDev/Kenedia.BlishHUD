using Blish_HUD.Gw2Mumble;
using Blish_HUD;
using Gw2Sharp.Models;
using System.ComponentModel;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearTemplate
    {
        private ProfessionType _profession;

        public GearTemplate()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            Profession = player != null ? player.Profession : ProfessionType.Guardian;
        }

        public GearTemplate(string code) : this()
        {
            LoadFromCode(code);
        }

        public event PropertyChangedEventHandler Changed;

        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, Changed); }

        public GearCollection Gear { get; } = new();

        public string ParseGearCode()
        {
            string code = "";

            code += Gear.ToCode(GearSlot.Head);
            code += Gear.ToCode(GearSlot.Shoulder);
            code += Gear.ToCode(GearSlot.Chest);
            code += Gear.ToCode(GearSlot.Hand);
            code += Gear.ToCode(GearSlot.Leg);
            code += Gear.ToCode(GearSlot.Foot);
            code += Gear.ToCode(GearSlot.MainHand);
            code += Gear.ToCode(GearSlot.OffHand);
            code += Gear.ToCode(GearSlot.Aquatic);
            code += Gear.ToCode(GearSlot.AltMainHand);
            code += Gear.ToCode(GearSlot.AltOffHand);
            code += Gear.ToCode(GearSlot.AltAquatic);
            code += Gear.ToCode(GearSlot.Back);
            code += Gear.ToCode(GearSlot.Amulet);
            code += Gear.ToCode(GearSlot.Accessory_1);
            code += Gear.ToCode(GearSlot.Accessory_2);
            code += Gear.ToCode(GearSlot.Ring_1);
            code += Gear.ToCode(GearSlot.Ring_2);

            return code;
        }

        public void LoadFromCode(string code)
        {
            string[] parts = code.Split(']');

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var gear = Gear[(GearSlot)i];
                gear.FromCode((GearSlot)i, parts[i]);
            }
        }
    }
}
