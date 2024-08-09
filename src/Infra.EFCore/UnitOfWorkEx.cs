using System;

namespace Infra.EFCore
{
    public static class UnitOfWorkEx
    {
        public static IGenericRepository<T> Repo<T>(this IUnitOfWork uow) where T : class
        {
            int maxUnwrappTry = 10;

            while (uow.GetType() != typeof(EfUnitOfWork) && maxUnwrappTry > 0)
            {
                uow = uow.Unwrap();
                maxUnwrappTry--;
            }

            var efUow = uow as EfUnitOfWork;

            if (efUow is null)
            {
                throw new InvalidOperationException("EfUnitOfWork not found");
            }

            return efUow.GenericRepo<T>();
        }
    }
}
