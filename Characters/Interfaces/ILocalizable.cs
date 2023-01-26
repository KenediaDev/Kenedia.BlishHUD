using System;

namespace Kenedia.Modules.Characters.Interfaces
{
    interface ILocalizable
    {
        void OnLanguageChanged(object s = null, EventArgs e = null);        
    }
}
