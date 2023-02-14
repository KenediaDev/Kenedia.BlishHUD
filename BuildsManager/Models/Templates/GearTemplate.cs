namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearTemplate
    {
        public GearTemplate()
        {
        }

        public GearTemplate(string code) : this()
        {
            LoadFromCode(code);
        }

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
