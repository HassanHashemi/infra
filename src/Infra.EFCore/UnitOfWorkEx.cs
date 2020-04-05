using System;

namespace Infra.EFCore
{
    public static class UnitOfWorkEx
    {
        public static IGenericRepository<T> Repo<T>(this IUnitOfWork uow) where T : class
        {
            if (!(uow is EfUnitOfWork efUow))
            {
                throw new InvalidOperationException("uow must be EfUnitOfWork");
            }

            return efUow.GenericRepo<T>();
        }
    }
}
