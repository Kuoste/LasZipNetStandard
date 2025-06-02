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
        const string _sSampleFilename = @"../../../Sotkamo.laz";
        const string _sOutputFilename = @"../../../out.laz";

        [Fact]
        public void ReadFile()
        {
            LasZip lasZip = new(out string Version);

            lasZip.OpenReader(_sSampleFilename);

            LaszipHeaderStruct h = lasZip.GetReaderHeader();

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            LasPoint pActual = new();
            LasPoint pExpected = new()
            {
                X = 567178.27,
                Y = 7112224.82,
                Z = 141.47,
                Classification = 4,
                GpsTime = 308645205.76348633,
                Intensity = 34711,
                NumberOfReturns = 3,
                ReturnNumber = 2,
                UserData = 2,
                Red = 0,
                Green = 0,
                Blue = 0,
                ScanAngleRank = 65,
                ScanDirectionFlag = 0,
                PointSourceId = 10
            };

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref pActual);

                if (i == 10)
                {
                    Assert.Equal(pExpected, pActual);
                    break;
                }
            }

            lasZip.CloseReader();
            lasZip.DestroyReader();
        }

        [Fact]
        public void WriteFile()
        {
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

            lasZip.CloseWriter();
            lasZip.DestroyWriter();

            lasZip.CloseReader();
            lasZip.OpenReader(_sOutputFilename);

            h = lasZip.GetReaderHeader();

            ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                Assert.True(points.TryGetValue(p.GetHashCode(), out LasPoint? pExpected));

                Assert.Equal(pExpected, p);
            }

            lasZip.CloseReader();
            lasZip.DestroyReader();
        }
    }
}