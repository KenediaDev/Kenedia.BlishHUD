using Kenedia.Modules.FashionManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.FashionManager.Services
{
    public class FashionTemplateFactory
    {
        public FashionTemplate CreateFashionTemplate()
        {
            return new FashionTemplate();
        }
    }
}
