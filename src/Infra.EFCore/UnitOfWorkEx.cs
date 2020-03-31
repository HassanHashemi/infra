using Domain;
using Infra.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infra.EFCore
{
    public static class UnitOfWorkEx
    {
        public static IGenericRepository<T> Repo<T>(this IUnitOfWork uow)
        {
            if (!(uow is EfUnitOfWork efUow))
            {
                throw new InvalidOperationException("uow must be EfUnitOfWork");
            }

            return efUow.Repo<T>();
        }
    }
}
