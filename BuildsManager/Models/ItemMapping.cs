using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class ItemMapping : IDisposable
    {
        private bool _isDisposed = false;

        public List<ItemMap> Nourishments = new();
        public List<ItemMap> Utilities = new();
        public List<ItemMap> PveRunes = new();
        public List<ItemMap> PvpRunes = new();
        public List<ItemMap> PveSigils = new();
        public List<ItemMap> PvpSigils = new();
        public List<ItemMap> Infusions = new();
        public List<ItemMap> Enrichments = new();
        public List<ItemMap> Trinkets = new();
        public List<ItemMap> Backs = new();
        public List<ItemMap> Weapons = new();
        public List<ItemMap> Armors = new();
        public List<ItemMap> PowerCores = new();
        public List<ItemMap> Relics = new();
        public List<ItemMap> PvpAmulets = new();
    
        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;

            Nourishments.Clear();
            Utilities.Clear();
            PveRunes.Clear();
            PvpRunes.Clear();
            PveSigils.Clear();
            PvpSigils.Clear();
            Infusions.Clear();
            Enrichments.Clear();
            Trinkets.Clear();
            Backs.Clear();
            Weapons.Clear();
            Armors.Clear();
            PowerCores.Clear();
            PvpAmulets.Clear();
        }
    }
}
