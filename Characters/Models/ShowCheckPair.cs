namespace Kenedia.Modules.Characters.Models
{
    public class ShowCheckPair
    {
        public ShowCheckPair(bool show, bool check)
        {
            Check = check;
            Show = show;
        }

        public bool Show { get; set; }

        public bool Check { get; set; }
    }
}
