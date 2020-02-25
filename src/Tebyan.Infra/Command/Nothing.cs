namespace Infra.Commands
{
    public class Nothing
    {
        private Nothing()
        {
        }

        private static readonly Nothing _instance = new Nothing();

        public static Nothing Instance { get; } = _instance;
        public override string ToString() => string.Empty;
    }
}
