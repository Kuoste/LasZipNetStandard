using Kuoste.LasZipNetStandard;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SampleProject
{
    internal class Program
    {
        

        static void Main(string[] args)
        {
            Console.WriteLine("Attributions:");
            Console.WriteLine($"Sample file contains open data from NLS Finland, lisenced under https://creativecommons.org/licenses/by/4.0/deed.en");
            Console.WriteLine($"Sample file contains point cloud data from file Q5232G1.laz. Accessed May 2023. Data is cropped for smaller file size.");
            Console.WriteLine("");

            Samples.PrintFirstAndLastCoordinates();

            Samples.ElevateCoordinates(100);
        }



    }
}
