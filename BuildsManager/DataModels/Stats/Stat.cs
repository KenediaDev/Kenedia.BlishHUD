using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using APIStat = Gw2Sharp.WebApi.V2.Models.Itemstat;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{
    [DataContract]
    public class Stat : IDisposable, IDataMember
    {
        private readonly Dictionary<int, List<int>> _textures = new()
        {
            { 1, new List<int> { 583, 616 } },
            { 2, new List<int> { 584, 161 } },
            { 3, new List<int> { 585, 154 } },
            { 4, new List<int> { 586, 162 } },
            { 5, new List<int> { 588, 559 } },
            { 6, new List<int> { 591 } },
            { 7, new List<int> { 592 } },
            { 8, new List<int> { 656, 155 } },
            { 9, new List<int> { 657, 158 } },
            { 10, new List<int> { 658, 159 } },
            { 11, new List<int> { 659, 605 } },
            { 12, new List<int> { 660 } },
            { 13, new List<int> { 690, 700 } },
            { 14, new List<int> { 1035, 686 } },
            { 15, new List<int> { 1037, 156 } },
            { 16, new List<int> { 1038, 160 } },
            { 17, new List<int> { 1064, 1067 } },
            { 18, new List<int> { 1066, 1026 } },
            { 19, new List<int> { 1097, 153 } },
            { 20, new List<int> { 1232, 1109, 1271, 1098 } },
            { 21, new List<int> { 1114, 754 } },
            { 22, new List<int> { 1115, 1085, 1229, 1262 } },
            { 23, new List<int> { 1119, 157 } },
            { 24, new List<int> { 1125, 1131, 1227, 1267 } },
            { 25, new List<int> { 1128, 753 } },
            { 26, new List<int> { 1130, 1153, 1224, 1268 } },
            { 27, new List<int> { 1134, 1123, 1226, 1265 } },
            { 28, new List<int> { 1139, 1118, 1228, 1264 } },
            { 29, new List<int> { 1145, 1111, 1231, 1263 } },
            { 30, new List<int> { 1162, 1270, 1140, 1225 } },
            { 31, new List<int> { 1163, 799 } },
            { 32, new List<int> { 1220, 1222, 1230, 1269 } },
            { 33, new List<int> { 1329, 1344, 1366, 1379 } },
            { 34, new List<int> { 1337, 1364, 1374, 1378 } },
            { 35, new List<int> { 1345, 1363, 1367, 1377 } },
            { 36, new List<int> { 1430, 628 } },
            { 37, new List<int> { 1436, 1032 } },
            { 38, new List<int> { 1486, 1484, 1549, 1559 } },
            { 39, new List<int> { 1538, 1566, 1539, 1556 } },
            { 40, new List<int> { 1694, 1686, 1706, 1717 } },
            { 41, new List<int> { 1697, 1681, 1687, 1691 } },
            { 42, new List<int> { 581 } },
        };
        private bool _isDisposed;

        private AsyncTexture2D _icon;

        public Stat()
        {
        }

        public Stat(APIStat stat)
        {
            Apply(stat);
        }

        public string DisplayAttributes => Attributes.ToString(0);

        [DataMember]
        public StatAttributes Attributes { get; set; } = new();

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public byte MappedId { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public int AssetId { get; set; }

        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon is not null) return _icon;

                _icon = BuildsManager.ModuleInstance.ContentsManager.GetTexture($@"textures\equipment_stats\{AssetId}.png") ?? AsyncTexture2D.FromAssetId(156021);
                return _icon;
            }
        }

        public void Apply(APIStat stat)
        {
            Name = stat.Name;
            Id = stat.Id;

            foreach (var att in stat.Attributes)
            {
                Attributes[att.Attribute.ToEnum()] = new(att);
            }

            AssetId = _textures?.FirstOrDefault(e => e.Value.Contains(stat.Id)).Key ?? 0;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _icon = null;
        }
    }
}
