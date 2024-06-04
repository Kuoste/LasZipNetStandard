using Kuoste.LasZipNetStandard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProject
{
    internal static class Samples
    {
        const string _sSampleFilename = @"..\..\..\Sotkamo.laz";
        const string _sOutputFilename = @"..\..\..\out.laz";

        //const string _sSampleFilename = @"..\..\..\out.laz";
        //const string _sOutputFilename = @"..\..\..\out2.laz";

        internal static void PrintFirstAndLastCoordinates()
        {
            LasZip lasZip = new(out string Version);

            Console.Write("Found a dll with LASZip API version " + Version + Environment.NewLine + Environment.NewLine);

            lasZip.OpenReader(_sSampleFilename);

            LaszipHeaderStruct h = lasZip.GetReaderHeader();

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            Console.WriteLine("Samples.PrintFirstAndLastCoordinates:");
            Console.WriteLine($"File contains {ulPointCount} points.");
            Console.WriteLine("First 10 points are:");

            LasPoint p = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                if (i < 10)
                    PrintPoint(p);

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
                    PrintPoint(p);
            }

            lasZip.CloseReader();
        }

        internal static void ElevateCoordinates(int iElevateByMeters)
        {
            Console.WriteLine();
            Console.WriteLine("Samples.ElevateCoordinates:");
            Console.WriteLine($"Elevating points by {iElevateByMeters} meters and writing result to {Path.GetFullPath(_sOutputFilename)}");
            Stopwatch sw = Stopwatch.StartNew();

            LasZip lasZip = new(out _);

            lasZip.OpenReader(_sSampleFilename);

            LaszipHeaderStruct h = lasZip.GetReaderHeader();
            lasZip.SetWriterHeader(h);

            lasZip.OpenWriter(_sOutputFilename, true);

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            LasPoint p = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                p.Z += iElevateByMeters;

                lasZip.WritePoint(ref p);
            }

            sw.Stop();
            Console.WriteLine($"Samples.ElevateCoordinates took {sw.Elapsed.TotalSeconds:N2} seconds.");

            lasZip.CloseReader();
            lasZip.CloseWriter();
        }

        static void PrintPoint(LasPoint p)
        {
            Console.WriteLine($"x = {p.X:N2}  y = {p.Y:N2}  z = {p.Z:N2} class = {p.Classification}");
        }

    }
}
