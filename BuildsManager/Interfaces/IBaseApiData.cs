using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.DataModels.Items;

namespace Kenedia.Modules.BuildsManager.Interfaces
{
    public interface IBaseApiData : IDataMember
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public AsyncTexture2D Icon { get; }
    }
}
