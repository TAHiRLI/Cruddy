using System.Linq.Expressions;
using Cruddy.Core.Configuration.FluentBuilders.Base;
using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration.FluentBuilders
{
    public class ManyToManyRelationshipBuilder<TEntity, TRelated> : IRelationshipBuilder
    where TEntity : class 
    where TRelated : class
    {
        private readonly RelationshipMetadata _metadata;
        private readonly EntityBuilder<TEntity> _entityBuilder;

        internal ManyToManyRelationshipBuilder(RelationshipMetadata metadata, EntityBuilder<TEntity> entityBuilder)
        {
            _metadata = metadata;
            _entityBuilder = entityBuilder;
        }

        RelationshipMetadata IRelationshipBuilder.Build()
        {
            return _metadata;
        }

        /// <summary>
        /// Specify the join/junction table name
        /// </summary>
        public ManyToManyRelationshipBuilder<TEntity, TRelated> UsingJoinTable(string tableName)
        {
            _metadata.JoinTable = tableName;
            return this;
        }

        /// <summary>
        /// Specify the inverse navigation property on the related entity
        /// </summary>
        public ManyToManyRelationshipBuilder<TEntity, TRelated> WithInverse(
            Expression<Func<TRelated, ICollection<TEntity>?>> inverseProperty)
        {
            _metadata.InverseProperty = GetPropertyName(inverseProperty);
            return this;
        }

        /// <summary>
        /// Set the display name for this relationship in the UI
        /// </summary>
        public ManyToManyRelationshipBuilder<TEntity, TRelated> HasDisplayName(string displayName)
        {
            _metadata.DisplayName = displayName;
            return this;
        }

        /// <summary>
        /// Set the description for this relationship
        /// </summary>
        public ManyToManyRelationshipBuilder<TEntity, TRelated> HasDescription(string description)
        {
            _metadata.Description = description;
            return this;
        }


        /// <summary>
        /// Show this relationship in list views
        /// </summary>
        public ManyToManyRelationshipBuilder<TEntity, TRelated> ShowInList()
        {
            _metadata.ShowInList = true;
            return this;
        }

        /// <summary>
        /// Show this relationship in form views
        /// </summary>
        public ManyToManyRelationshipBuilder<TEntity, TRelated> ShowInForm()
        {
            _metadata.ShowInForm = true;
            return this;
        }

        /// <summary>
        /// Return to entity builder for chaining
        /// </summary>
        public EntityBuilder<TEntity> And()
        {
            return _entityBuilder;
        }

        private static string GetPropertyName<TProperty>(Expression<Func<TRelated, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            throw new ArgumentException("Expression must be a member expression");
        }

    }
}