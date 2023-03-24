namespace Kenedia.Modules.Characters.Models
{
    public class ShowCheckPair
    {
        public ShowCheckPair(bool show, bool check, bool tooltip)
        {
            Check = check;
            Show = show;
            ShowTooltip = tooltip;
        }

        public bool Show { get; set; }

        public bool Check { get; set; }

        public bool ShowTooltip { get; set; }
    }
}
