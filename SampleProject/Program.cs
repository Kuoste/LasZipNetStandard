using Kuoste.LasZipNetStandard;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SampleProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LasZip lasZip = new();

            lasZip.OpenReader(@"..\..\..\Sotkamo.laz");

            LasHeader h = lasZip.GetHeader();

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            Console.WriteLine($"File contains {ulPointCount} points.");
            Console.WriteLine("First 10 points are:");

            LasPoint p = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                if (i < 10)
                    PrintCoordinates(p);

                if (i == 10)
                {
                    Debug.Assert(p.X == 567178.27);
                    Debug.Assert(p.Y == 7112224.82);
                    Debug.Assert(p.Z == 141.47);

                    Debug.Assert(p.Classification == 4);
                    Debug.Assert(p.GpsTime == 308645205.76348633);
                    Debug.Assert(p.Intensity == 34711);

                    Debug.Assert(p.Red == 0);
                    Debug.Assert(p.Green == 0);
                    Debug.Assert(p.Blue == 0);

                    Debug.Assert(p.ScanAngleRank == 65);
                    Debug.Assert(p.ScanDirectionFlag == 0);
                    Debug.Assert(p.PointSourceId == 10);
                }


                if (i == ulPointCount - 10)
                    Console.WriteLine("Last 10 points are:");

                if (i >= ulPointCount - 10)
                    PrintCoordinates(p);
            }

            lasZip.CloseReader();
        }

        static void PrintCoordinates(LasPoint p)
        {
            Console.WriteLine($"x = {p.X:N2}  y = {p.Y:N2}  z = {p.Z:N2} class = {p.Classification}");
        }

    }
}
