using System;

namespace VitaFlow.Infrastructure.Data.Base
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
