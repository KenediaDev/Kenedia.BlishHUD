using Blish_HUD.Content;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using AttributeType = Gw2Sharp.WebApi.V2.Models.AttributeType;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
{
    public class AttributeToggle : ImageToggle
    {
        private AttributeType _attribute;
        public AttributeToggle()
        {
            ImageColor = Color.Gray * 0.5F;
            ActiveColor = Color.White;
            TextureRectangle = new(4, 4, 24, 24);
        }

        public AttributeType Attribute { get => _attribute; set => Common.SetProperty(ref _attribute, value, OnAttributeChanged); }

        private void OnAttributeChanged()
        {
            Texture = _attribute switch
            {
                AttributeType.Power => AsyncTexture2D.FromAssetId(66722),
                AttributeType.Toughness => AsyncTexture2D.FromAssetId(156612),
                AttributeType.Vitality => AsyncTexture2D.FromAssetId(156613),
                AttributeType.Precision => AsyncTexture2D.FromAssetId(156609),
                AttributeType.CritDamage => AsyncTexture2D.FromAssetId(156602),
                AttributeType.ConditionDamage => AsyncTexture2D.FromAssetId(156600),
                AttributeType.ConditionDuration => AsyncTexture2D.FromAssetId(156601),
                AttributeType.BoonDuration => AsyncTexture2D.FromAssetId(156599),
                AttributeType.Healing => AsyncTexture2D.FromAssetId(156606),
                _ => AsyncTexture2D.FromAssetId(536054),
            };
        }
    }
}
