using System;

namespace SkySlider
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SkySlider game = new SkySlider())
            {
                game.Run();
            }
        }
    }
#endif
}

