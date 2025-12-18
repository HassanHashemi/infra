using System;
using System.Threading;
using System.Threading.Tasks;
using Infra;
using Infra.Commands;
using Infra.EFCore;

namespace Sample01;

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;

    public TestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        var test = new Test(command.Title);
        _unitOfWork.Repo<Test>().Add(test);
        await _unitOfWork.Save(test);
        return test.Id.ToString();
    }
}
