namespace Snobol4W
{
    internal static class Program
    {
        internal static readonly object FinishLock = new();

                                [STAThread]
        private static int Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return 0;
        }
    }
}