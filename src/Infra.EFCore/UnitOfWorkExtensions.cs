using Domain;
using Infra.Eevents;
using Infra.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infra.EFCore
{
    public static class UowExtensions
    {
        public static IGenericRepository<T> Repo<T>(this IUnitOfWork uow) where T : class
        {
            if (!(uow is EfUnitOfWork efUow))
            {
                throw new ArgumentException("context must be EfUnitOfWork");
            }

            return new EfGenericRepo<T>(efUow.Context);
        }
    }
}
