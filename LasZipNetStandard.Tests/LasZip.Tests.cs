namespace Kuoste.LasZipNetStandard.Tests
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

        private static string GetOutputFilename() =>
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.laz");

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
            string outputFilename = GetOutputFilename();
            LasZip lasZip = new(out _);

            lasZip.OpenReader(_sSampleFilename);

            LaszipHeaderStruct h = lasZip.GetReaderHeader();
            lasZip.SetWriterHeader(h);

            lasZip.OpenWriter(outputFilename, true);

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            LasPoint p = new();

            List<LasPoint> points = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                if (i % 2 != 0)
                {
                    continue;
                }

                points.Add(p);

                lasZip.WritePoint(ref p, updateInventory: true);
            }

            lasZip.CloseWriter();
            lasZip.DestroyWriter();

            lasZip.CloseReader();
            lasZip.DestroyReader();

            LasZip lasZip2 = new(out _);
            lasZip2.OpenReader(outputFilename);

            h = lasZip2.GetReaderHeader();

            ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            Assert.Equal(points.Count, (int)ulPointCount);

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip2.ReadPoint(ref p);

                Assert.Equal(points[(int)i], p);
            }

            lasZip2.CloseReader();
            lasZip2.DestroyReader();
        }

        [Fact]
        public void WriteFileWithManualHeaderUpdate()
        {
            string outputFilename = GetOutputFilename();

            LasPoint p = new();
            List<LasPoint> points = new();
            LaszipHeaderStruct writerHeader;
            uint[] numberOfPointsByReturn = new uint[5];
            ulong[] extendedNumberOfPointsByReturn = new ulong[15];

            using (LasZip lasZip = new(out _))
            {
                lasZip.OpenReader(_sSampleFilename);

                LaszipHeaderStruct h = lasZip.GetReaderHeader();
                ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

                for (ulong i = 0; i < ulPointCount; i++)
                {
                    lasZip.ReadPoint(ref p);

                    if (i % 2 != 0)
                    {
                        continue;
                    }

                    points.Add(p);

                    int returnIndex = p.ReturnNumber - 1;
                    if (returnIndex >= 0)
                    {
                        if (returnIndex < numberOfPointsByReturn.Length)
                        {
                            numberOfPointsByReturn[returnIndex]++;
                        }

                        if (returnIndex < extendedNumberOfPointsByReturn.Length)
                        {
                            extendedNumberOfPointsByReturn[returnIndex]++;
                        }
                    }
                }

                lasZip.CloseReader();

                writerHeader = h;
                writerHeader.NumberOfPointRecords = (uint)points.Count;
                writerHeader.ExtendedNumberOfPointRecords = (ulong)points.Count;
                writerHeader.NumberOfPointsByReturn = numberOfPointsByReturn;
                writerHeader.ExtendedNumberOfPointsByReturn = extendedNumberOfPointsByReturn;

                lasZip.SetWriterHeader(writerHeader);
                lasZip.OpenWriter(outputFilename, true);

                for (int i = 0; i < points.Count; i++)
                {
                    LasPoint pointToWrite = points[i];
                    lasZip.WritePoint(ref pointToWrite);
                }
            }

            using (LasZip lasZip2 = new(out _))
            {
                lasZip2.OpenReader(outputFilename);

                LaszipHeaderStruct h = lasZip2.GetReaderHeader();

                ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

                Assert.Equal(points.Count, (int)ulPointCount);

                for (int i = 0; i < writerHeader.NumberOfPointsByReturn.Length; i++)
                {
                    Assert.Equal(writerHeader.NumberOfPointsByReturn[i], h.NumberOfPointsByReturn[i]);
                }

                for (ulong i = 0; i < ulPointCount; i++)
                {
                    lasZip2.ReadPoint(ref p);
                    Assert.Equal(points[(int)i], p);
                }
            }
        }

        [Fact]
        public void OpenReaderAfterDisposeThrowsObjectDisposedException()
        {
            LasZip lasZip = new(out _);

            lasZip.Dispose();

            Assert.Throws<ObjectDisposedException>(() => lasZip.OpenReader(_sSampleFilename));
        }

        [Fact]
        public void OpenWriterAfterDisposeThrowsObjectDisposedException()
        {
            LasZip lasZip = new(out _);

            lasZip.Dispose();

            Assert.Throws<ObjectDisposedException>(() => lasZip.OpenWriter(_sOutputFilename, true));
        }

        [Fact]
        public void GetReaderHeaderAfterDisposeThrowsObjectDisposedException()
        {
            LasZip lasZip = new(out _);

            lasZip.Dispose();

            Assert.Throws<ObjectDisposedException>(() => lasZip.GetReaderHeader());
        }

        [Fact]
        public void GetReaderHeaderWithoutOpenReaderThrowsInvalidOperationException()
        {
            LasZip lasZip = new(out _);

            Assert.Throws<InvalidOperationException>(() => lasZip.GetReaderHeader());

            lasZip.Dispose();
        }

        [Fact]
        public void ReadPointWithoutOpenReaderThrowsInvalidOperationException()
        {
            LasZip lasZip = new(out _);
            LasPoint point = new();

            Assert.Throws<InvalidOperationException>(() => lasZip.ReadPoint(ref point));

            lasZip.Dispose();
        }

        [Fact]
        public void WritePointWithoutOpenWriterThrowsInvalidOperationException()
        {
            LasZip lasZip = new(out _);
            LasPoint point = new();

            Assert.Throws<InvalidOperationException>(() => lasZip.WritePoint(ref point));

            lasZip.Dispose();
        }
    }
}