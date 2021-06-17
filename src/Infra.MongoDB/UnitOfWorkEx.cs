using System;

namespace Infra.MongoDB
{
    public static class UnitOfWorkEx
    {
        public static MongoGenericRepo<T> Repo<T>(this IUnitOfWork uow) where T : class
        {
            if (uow is not MongoUnitOfWork efUow)
            {
                throw new InvalidOperationException("uow must be EfUnitOfWork");
            }

            return efUow.GenericRepo<T>();
        }
    }
}
