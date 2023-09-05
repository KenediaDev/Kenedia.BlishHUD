using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.OverflowTradingAssist.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class Data
    {

        [EnumeratorMember]
        public static HostedItems HostedItems { get; set; } = new();

        public IEnumerator<(string name, HostedItems map)> GetEnumerator()
        {
            var propertiesToEnumerate = GetType()
                .GetProperties()
                .Where(property => property.GetCustomAttribute<EnumeratorMemberAttribute>() != null);

            foreach (var property in propertiesToEnumerate)
            {
                yield return (property.Name, property.GetValue(this) as HostedItems);
            }
        }

    }
}
