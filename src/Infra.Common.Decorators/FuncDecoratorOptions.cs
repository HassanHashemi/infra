using Autofac;
using System;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class FuncDecoratorOptions
    {
        public Func<object, ILifetimeScope, Task> Handler { get; set; }
    }
}
