namespace Kenedia.Modules.Characters.Controls
{
    public class FilterTag
    {
        public string Tag { get; set; }

        public bool Result { get; set; } = false;

        public static implicit operator string(FilterTag asyncTexture2D)
        {
            return asyncTexture2D.Tag;
        }
    }
}
