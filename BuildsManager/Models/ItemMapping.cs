using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class ItemMapping : IDisposable
    {
        private bool _isDisposed = false;

        public List<BasicItemMap> Nourishments = new();
        public List<BasicItemMap> Utilities = new();
        public List<BasicItemMap> PveRunes = new();
        public List<BasicItemMap> PvpRunes = new();
        public List<BasicItemMap> PveSigils = new();
        public List<BasicItemMap> PvpSigils = new();
        public List<BasicItemMap> Infusions = new();
        public List<BasicItemMap> Enrichments = new();
        public List<BasicItemMap> Trinkets = new();
        public List<BasicItemMap> Backs = new();
        public List<BasicItemMap> Weapons = new();
        public List<BasicItemMap> Armors = new();
        public List<BasicItemMap> PowerCores = new();
        public List<BasicItemMap> Relics = new();
        public List<BasicItemMap> PvpAmulets = new();
    
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
