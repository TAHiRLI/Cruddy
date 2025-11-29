using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration.FluentBuilders.Base
{
    public interface IPropertyBuilder
    {
        internal PropertyMetadata Build();
    }
}