using Blish_HUD.Gw2Mumble;
using Blish_HUD;
using Gw2Sharp.Models;
using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using System.Diagnostics;

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

        public InfusionCollection Infusions { get; } = new();

        public int Nourishment { get; set; }

        public int Utility { get; set; }

        public string ParseGearCode()
        {
            string code = "";

            code += Gear.ToCode(GearTemplateSlot.Head);
            code += Gear.ToCode(GearTemplateSlot.Shoulder);
            code += Gear.ToCode(GearTemplateSlot.Chest);
            code += Gear.ToCode(GearTemplateSlot.Hand);
            code += Gear.ToCode(GearTemplateSlot.Leg);
            code += Gear.ToCode(GearTemplateSlot.Foot);
            code += Gear.ToCode(GearTemplateSlot.MainHand);
            code += Gear.ToCode(GearTemplateSlot.OffHand);
            code += Gear.ToCode(GearTemplateSlot.Aquatic);
            code += Gear.ToCode(GearTemplateSlot.AltMainHand);
            code += Gear.ToCode(GearTemplateSlot.AltOffHand);
            code += Gear.ToCode(GearTemplateSlot.AltAquatic);
            code += Gear.ToCode(GearTemplateSlot.Back);
            code += Gear.ToCode(GearTemplateSlot.Amulet);
            code += Gear.ToCode(GearTemplateSlot.Ring_1);
            code += Gear.ToCode(GearTemplateSlot.Accessory_2);
            code += Gear.ToCode(GearTemplateSlot.Ring_1);
            code += Gear.ToCode(GearTemplateSlot.Ring_2);
            code += Gear.ToCode(GearTemplateSlot.AquaBreather);
            code += Gear.ToCode(GearTemplateSlot.PvpAmulet);
            code += Infusions.ToCode();
            code += $"[{Nourishment}]";
            code += $"[{Utility}]";

            return code;
        }

        public void LoadFromCode(string code)
        {
            string[] parts = code.Split(']');

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if((GearTemplateSlot)i is GearTemplateSlot.Nourishment)
                {
                    Nourishment = int.TryParse(parts[i], out int nourishment) ? nourishment : 0;
                }
                else if((GearTemplateSlot)i is GearTemplateSlot.Utility)
                {
                    Utility = int.TryParse(parts[i], out int utility) ? utility : 0;
                }
                else
                {
                    Gear.FromCode((GearTemplateSlot)i, parts[i]);
                }
            }
        }
    }
}
