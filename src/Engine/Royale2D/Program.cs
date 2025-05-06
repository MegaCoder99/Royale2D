using System.Diagnostics;

namespace Royale2D
{
    class Program
    {
        static void Main(string[] args)
        {
            //Tests.RunTests(); return;
            //LookupTables.Generate();return;
            //NetcodeSafetyTests.Test(); return;

            if (Debugger.IsAttached)
            {
                Console.WriteLine("DEBUGGING");
            }

            Game.Init();
            Game.StartGameLoop();
        }
    }
}