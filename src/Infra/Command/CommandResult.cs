namespace Infra.Commands
{
    public class CommandResult
    {
        public CommandResult(bool success)
        {
            this.Status = success;
        }

        public bool Status { get; }

        public static CommandResult<Nothing> Empty => new CommandResult<Nothing>(true, Nothing.Instance);
    }
}
