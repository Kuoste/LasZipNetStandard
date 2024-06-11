using Kuoste.LasZipNetStandard;
using System.Diagnostics;

namespace LasZipNetStandard.Tests
{
    public class LasZipTests
    {
        /// <summary>
        /// Attributions:
        /// Sample file contains open data from NLS Finland, lisenced under https://creativecommons.org/licenses/by/4.0/deed.en
        /// Data is from file Q5232G1.laz, accessed in May 2023. The file is cropped for a smaller size.
        /// </summary>
        const string _sSampleFilename = @"..\..\..\Sotkamo.laz";
        const string _sOutputFilename = @"..\..\..\out.laz";

        [Fact]
        public void ReadFile()
        {
            LasZip lasZip = new(out string Version);

            //Console.Write("Found LASzip DLL " + Version + Environment.NewLine + Environment.NewLine);
            //Stopwatch sw = Stopwatch.StartNew();

            lasZip.OpenReader(_sSampleFilename);

            LaszipHeaderStruct h = lasZip.GetReaderHeader();

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            //Console.WriteLine("Samples.PrintFirstAndLastCoordinates:");
            //Console.WriteLine($"File contains {ulPointCount} points.");
            //Console.WriteLine("First 10 points are:");

            LasPoint p = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                if (i == 10)
                {
                    Assert.True(p.X == 567178.27);
                    Assert.True(p.Y == 7112224.82);
                    Assert.True(p.Z == 141.47);
                    Assert.True(p.Classification == 4);
                    Assert.True(p.GpsTime == 308645205.76348633);
                    Assert.True(p.Intensity == 34711);
                    Assert.True(p.Red == 0);
                    Assert.True(p.Green == 0);
                    Assert.True(p.Blue == 0);
                    Assert.True(p.ScanAngleRank == 65);
                    Assert.True(p.ScanDirectionFlag == 0);
                    Assert.True(p.PointSourceId == 10);
                }
            }

            //sw.Stop();
            //Console.WriteLine($"Samples.ReadFile took {sw.Elapsed.TotalSeconds:N2} seconds.");

            lasZip.CloseReader();
            lasZip.DestroyReader();
        }

        [Fact]
        public void WriteFile()
        {
            //Stopwatch sw = Stopwatch.StartNew();

            LasZip lasZip = new(out _);

            lasZip.OpenReader(_sSampleFilename);

            LaszipHeaderStruct h = lasZip.GetReaderHeader();
            lasZip.SetWriterHeader(h);

            lasZip.OpenWriter(_sOutputFilename, true);

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);
            
            LasPoint p = new();

            Dictionary<int, LasPoint> points = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                points.Add(p.GetHashCode(), p);

                lasZip.WritePoint(ref p);
            }

            //sw.Stop();
            //Console.WriteLine($"Samples.ElevateCoordinates took {sw.Elapsed.TotalSeconds:N2} seconds.");

            lasZip.CloseWriter();
            lasZip.DestroyWriter();

            lasZip.CloseReader();
            lasZip.OpenReader(_sOutputFilename);

            h = lasZip.GetReaderHeader();

            ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                Assert.True(points.TryGetValue(p.GetHashCode(), out LasPoint? p2));

                Assert.True(p.Equals(p2));
            }

            lasZip.CloseReader();
            lasZip.DestroyReader();
        }
    }
}