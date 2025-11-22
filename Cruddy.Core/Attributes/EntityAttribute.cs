using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cruddy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EntityAttribute : Attribute
    {
        public Type TargetType { get; }

        public EntityAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}