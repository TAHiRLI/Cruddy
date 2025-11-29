using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration.FluentBuilders.Base
{
    public interface IRelationshipBuilder
    {
        internal RelationshipMetadata Build();
    }
}