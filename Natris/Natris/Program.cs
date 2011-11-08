using System;

namespace Natris
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (myGame game = new myGame())
            {
                game.Run();
            }
        }
    }
#endif
}

