using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class ItemMapping : IDisposable
    {
        private bool _isDisposed = false;

        public List<BasicItemMap> Nourishments = [];
        public List<BasicItemMap> Utilities = [];
        public List<BasicItemMap> PveRunes = [];
        public List<BasicItemMap> PvpRunes = [];
        public List<BasicItemMap> PveSigils = [];
        public List<BasicItemMap> PvpSigils = [];
        public List<BasicItemMap> Infusions = [];
        public List<BasicItemMap> Enrichments = [];
        public List<BasicItemMap> Trinkets = [];
        public List<BasicItemMap> Backs = [];
        public List<BasicItemMap> Weapons = [];
        public List<BasicItemMap> Armors = [];
        public List<BasicItemMap> PowerCores = [];
        public List<BasicItemMap> Relics = [];
        public List<BasicItemMap> PvpAmulets = [];
    
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
