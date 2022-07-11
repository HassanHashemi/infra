using System;
using System.Threading.Tasks;
using Domain;

namespace Infra
{
    public class Test : AggregateRoot<int> 
    {
        public Test(IUnitOfWork uow)
        {
            uow.Save(new Test(null));

        }
    }

    public interface IUnitOfWork
    {
        Task<int> Save<T>(AggregateRoot<T> root);
        Task<int> Delete(AggregateRoot root);
        Task<int> Delete<T>(AggregateRoot<T> root);
    }
}
