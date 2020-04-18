using System;

namespace Infra
{
    public interface IUser
    {
        Guid Id { get; }
        string UserName { get; }
        string FullName { get; }
        string Phone { get; }
    }
}
