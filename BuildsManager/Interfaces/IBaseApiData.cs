using Blish_HUD.Content;

namespace Kenedia.Modules.BuildsManager.Interfaces
{
    public interface IBaseApiData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public AsyncTexture2D Icon { get; }
    }
}
