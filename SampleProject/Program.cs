using Kuoste.LasZipNetStandard;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SampleProject
{
    internal class Program
    {
        const string _sSampleFilename = @"..\..\..\Sotkamo.laz";
        const string _sOutputFilename = @"..\..\..\out.laz";

        static void Main(string[] args)
        {
            Console.WriteLine("Attributions:");
            Console.WriteLine($"Sample file contains open data from NLS Finland, lisenced under https://creativecommons.org/licenses/by/4.0/deed.en");
            Console.WriteLine($"Data is from file Q5232G1.laz, accessed in May 2023. The file is cropped for a smaller size.");
            Console.WriteLine("");

            Samples.ReadFile(_sSampleFilename);

            Samples.ElevateCoordinates(_sSampleFilename, _sOutputFilename, 100);
        }



    }
}
