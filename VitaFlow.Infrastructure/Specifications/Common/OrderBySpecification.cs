using System;
using System.Linq.Expressions;
using VitaFlow.Infrastructure.Specifications.Base;

namespace VitaFlow.Infrastructure.Specifications.Common
{
    public class OrderBySpecification<T> : Specification<T>
    {
        public OrderBySpecification(Expression<Func<T, object>> orderBy)
        {
            ApplyOrderBy(orderBy);
        }
    }
}
