#if GRIFFIN
using System;

namespace Pinwheel.Griffin
{
    [ExcludeFromDoc]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class GDisplayName : Attribute
    {
        public string DisplayName { get; set; }

        public GDisplayName(string name)
        {
            DisplayName = name;
        }
    }
}
#endif
