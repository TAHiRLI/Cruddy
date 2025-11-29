using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cruddy.Core.Configuration.FluentBuilders.Base;
using Cruddy.Core.Models;

namespace Cruddy.Core.Configuration.FluentBuilders
{
    public class RelationshipBuilder<TEntity, TRelated> : IRelationshipBuilder
    where TEntity : class
    where TRelated : class
    {
        private readonly RelationshipMetadata _metadata;
        private readonly EntityBuilder<TEntity> _entityBuilder;

        internal RelationshipBuilder(RelationshipMetadata metadata, EntityBuilder<TEntity> entityBuilder)
        {
            _metadata = metadata;
            _entityBuilder = entityBuilder;
        }
        RelationshipMetadata IRelationshipBuilder.Build()
        {
            return _metadata;
        }

        /// <summary>
        /// Specify the foreign key property on this entity
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> WithForeignKey(Expression<Func<TEntity, object>> foreignKeyProperty)
        {
            _metadata.ForeignKey = GetPropertyName(foreignKeyProperty);
            return this;
        }

        /// <summary>
        /// Specify the foreign key property name directly
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> WithForeignKey(string foreignKeyName)
        {
            _metadata.ForeignKey = foreignKeyName;
            return this;
        }

        /// <summary>
        /// Specify the inverse navigation property on the related entity
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> WithInverse(Expression<Func<TEntity, object>> inverseProperty)
        {
            _metadata.InverseProperty = GetPropertyName(inverseProperty);
            return this;
        }

        /// <summary>
        /// Specify the inverse navigation property name directly
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> WithInverse(string inversePropertyName)
        {
            _metadata.InverseProperty = inversePropertyName;
            return this;
        }

        /// <summary>
        /// Mark this relationship as required (non-nullable foreign key)
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> IsRequired()
        {
            _metadata.IsRequired = true;
            return this;
        }

        /// <summary>
        /// Set the display name for this relationship in the UI
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> HasDisplayName(string displayName)
        {
            _metadata.DisplayName = displayName;
            return this;
        }

        /// <summary>
        /// Set the description for this relationship
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> HasDescription(string description)
        {
            _metadata.Description = description;
            return this;
        }


        /// <summary>
        /// Show this relationship in list views
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> ShowInList()
        {
            _metadata.ShowInList = true;
            return this;
        }

        /// <summary>
        /// Show this relationship in form views
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> ShowInForm()
        {
            _metadata.ShowInForm = true;
            return this;
        }

        /// <summary>
        /// Hide this relationship from the UI
        /// </summary>
        public RelationshipBuilder<TEntity, TRelated> IsHidden()
        {
            _metadata.IsHidden = true;
            return this;
        }

        /// <summary>
        /// Return to entity builder for chaining
        /// </summary>
        public EntityBuilder<TEntity> And()
        {
            return _entityBuilder;
        }



        private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }
            throw new ArgumentException("Expression must be a member expression");
        }

        private static string GetPropertyName<TProperty>(Expression<Func<TRelated, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }
            throw new ArgumentException("Expression must be a member expression");
        }
    }
}