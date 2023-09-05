using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Attributes
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class JsonSemverVersionAttribute : Attribute
    {
    }
}
