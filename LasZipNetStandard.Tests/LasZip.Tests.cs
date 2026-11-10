using System.Runtime.InteropServices;

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

        const ulong _ulSamplePointCount = 21431;

        // Coordinates are stored as integers scaled by 0.01, so values read back can differ from
        // the written ones by floating point noise only.
        const double _dCoordinateTolerance = 1e-9;

        private static string GetOutputFilename(bool isCompressed = true) =>
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}" + (isCompressed ? ".laz" : ".las"));

        private static void DeleteIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        private static LasPoint GetSamplePoint10() => new()
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

        /// <summary>
        /// Minimal LAS 1.2 header for point data format 3 (GPS time + RGB) without VLRs.
        /// </summary>
        private static LaszipHeaderStruct CreateFormat3Header(double offsetX, double offsetY, double offsetZ)
        {
            return new LaszipHeaderStruct
            {
                ProjectId4 = new byte[8],
                VersionMajor = 1,
                VersionMinor = 2,
                SystemIdentifier = new byte[32],
                GeneratingSoftware = new byte[32],
                CreationDayOfYear = 1,
                CreationYear = 2026,
                HeaderSize = 227,
                OffsetToPointData = 227,
                PointDataFormat = 3,
                PointDataRecordLength = 34,
                NumberOfPointsByReturn = new uint[5],
                ScaleFactorX = 0.01,
                ScaleFactorY = 0.01,
                ScaleFactorZ = 0.01,
                OffsetX = offsetX,
                OffsetY = offsetY,
                OffsetZ = offsetZ,
                ExtendedNumberOfPointsByReturn = new ulong[15],
            };
        }

        /// <summary>
        /// Points exercising every field incl. bit fields at their limits and coordinates below the offset.
        /// </summary>
        private static List<LasPoint> CreateSyntheticPoints() => new()
        {
            new()
            {
                X = 1234.56, Y = 6789.01, Z = 100.25,
                Intensity = ushort.MaxValue, ReturnNumber = 1, NumberOfReturns = 7,
                ScanDirectionFlag = 1, EdgeOfFlightLine = 1, Classification = 31,
                ScanAngleRank = -90, UserData = byte.MaxValue, PointSourceId = ushort.MaxValue,
                GpsTime = 308645205.76348633, Red = ushort.MaxValue, Green = 1, Blue = 32768
            },
            new()
            {
                // Negative values relative to the offset (e.g. elevations below sea level)
                X = -0.07, Y = -1234.56, Z = -12.34,
                Intensity = 0, ReturnNumber = 7, NumberOfReturns = 7,
                ScanDirectionFlag = 0, EdgeOfFlightLine = 1, Classification = 0,
                ScanAngleRank = 90, UserData = 0, PointSourceId = 0,
                GpsTime = -1.5, Red = 0, Green = ushort.MaxValue, Blue = 0
            },
            new()
            {
                X = 0, Y = 0, Z = 0,
                Intensity = 1, ReturnNumber = 3, NumberOfReturns = 5,
                ScanDirectionFlag = 1, EdgeOfFlightLine = 0, Classification = 2,
                ScanAngleRank = 0, UserData = 7, PointSourceId = 12345,
                GpsTime = 0, Red = 100, Green = 200, Blue = ushort.MaxValue
            },
        };

        /// <summary>
        /// ReadPoint overwrites the given instance, so points stored for later comparison must be copied.
        /// </summary>
        private static LasPoint Copy(LasPoint p) => new()
        {
            X = p.X, Y = p.Y, Z = p.Z,
            Intensity = p.Intensity, ReturnNumber = p.ReturnNumber, NumberOfReturns = p.NumberOfReturns,
            ScanDirectionFlag = p.ScanDirectionFlag, EdgeOfFlightLine = p.EdgeOfFlightLine,
            Classification = p.Classification, ScanAngleRank = p.ScanAngleRank, UserData = p.UserData,
            PointSourceId = p.PointSourceId, GpsTime = p.GpsTime,
            Red = p.Red, Green = p.Green, Blue = p.Blue
        };

        private static void AssertPointEqual(LasPoint expected, LasPoint actual)
        {
            Assert.Equal(expected.X, actual.X, _dCoordinateTolerance);
            Assert.Equal(expected.Y, actual.Y, _dCoordinateTolerance);
            Assert.Equal(expected.Z, actual.Z, _dCoordinateTolerance);
            Assert.Equal(expected.Intensity, actual.Intensity);
            Assert.Equal(expected.ReturnNumber, actual.ReturnNumber);
            Assert.Equal(expected.NumberOfReturns, actual.NumberOfReturns);
            Assert.Equal(expected.ScanDirectionFlag, actual.ScanDirectionFlag);
            Assert.Equal(expected.EdgeOfFlightLine, actual.EdgeOfFlightLine);
            Assert.Equal(expected.Classification, actual.Classification);
            Assert.Equal(expected.ScanAngleRank, actual.ScanAngleRank);
            Assert.Equal(expected.UserData, actual.UserData);
            Assert.Equal(expected.PointSourceId, actual.PointSourceId);
            Assert.Equal(expected.GpsTime, actual.GpsTime);
            Assert.Equal(expected.Red, actual.Red);
            Assert.Equal(expected.Green, actual.Green);
            Assert.Equal(expected.Blue, actual.Blue);
        }

        [Fact]
        public void ReadFile()
        {
            using LasZip lasZip = new(out string Version);

            Assert.True(lasZip.OpenReader(_sSampleFilename));

            LaszipHeaderStruct h = lasZip.GetReaderHeader();

            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

            LasPoint pActual = new();
            LasPoint pExpected = GetSamplePoint10();

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
        public void GetVersionReturnsLasZipVersion()
        {
            using LasZip lasZip = new(out string version);

            Assert.StartsWith("3.5 ", version);
        }

        [Fact]
        public void GetReaderHeaderReturnsSampleFileHeader()
        {
            using LasZip lasZip = new(out _);
            Assert.True(lasZip.OpenReader(_sSampleFilename));

            LaszipHeaderStruct h = lasZip.GetReaderHeader();

            // Verifies the managed header struct layout matches the native one
            Assert.Equal(1, h.VersionMajor);
            Assert.Equal(4, h.VersionMinor);
            Assert.Equal(375, h.HeaderSize);
            Assert.Equal(2025u, h.OffsetToPointData);
            Assert.Equal(2u, h.NumberOfVariableLengthRecords);
            Assert.Equal(1, h.PointDataFormat);
            Assert.Equal(28, h.PointDataRecordLength);
            Assert.Equal((uint)_ulSamplePointCount, h.NumberOfPointRecords);
            Assert.Equal(_ulSamplePointCount, h.ExtendedNumberOfPointRecords);
            Assert.Equal(new uint[] { 18840, 1995, 509, 78, 9 }, h.NumberOfPointsByReturn);
            Assert.Equal(0.01, h.ScaleFactorX);
            Assert.Equal(0.01, h.ScaleFactorY);
            Assert.Equal(0.01, h.ScaleFactorZ);
            Assert.Equal(500000, h.OffsetX);
            Assert.Equal(7000000, h.OffsetY);
            Assert.Equal(0, h.OffsetZ);
            Assert.Equal(567000.2, h.MinX, _dCoordinateTolerance);
            Assert.Equal(567199.64, h.MaxX, _dCoordinateTolerance);
            Assert.Equal(7112100, h.MinY, _dCoordinateTolerance);
            Assert.Equal(7112300, h.MaxY, _dCoordinateTolerance);
            Assert.Equal(138.2, h.MinZ, _dCoordinateTolerance);
            Assert.Equal(168.32, h.MaxZ, _dCoordinateTolerance);
            Assert.Equal("PDAL", System.Text.Encoding.ASCII.GetString(h.SystemIdentifier).TrimEnd('\0'));
        }

        [Fact]
        public void ReadAllPointsMatchesHeader()
        {
            using LasZip lasZip = new(out _);
            Assert.True(lasZip.OpenReader(_sSampleFilename));

            LaszipHeaderStruct h = lasZip.GetReaderHeader();
            ulong ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);
            Assert.Equal(_ulSamplePointCount, ulPointCount);

            uint[] pointsByReturn = new uint[5];
            LasPoint p = new();

            for (ulong i = 0; i < ulPointCount; i++)
            {
                lasZip.ReadPoint(ref p);

                Assert.InRange(p.X, h.MinX - _dCoordinateTolerance, h.MaxX + _dCoordinateTolerance);
                Assert.InRange(p.Y, h.MinY - _dCoordinateTolerance, h.MaxY + _dCoordinateTolerance);
                Assert.InRange(p.Z, h.MinZ - _dCoordinateTolerance, h.MaxZ + _dCoordinateTolerance);
                Assert.InRange(p.ReturnNumber, (byte)1, p.NumberOfReturns);

                pointsByReturn[p.ReturnNumber - 1]++;
            }

            Assert.Equal(h.NumberOfPointsByReturn, pointsByReturn);
        }

        [Fact]
        public void ReadPointWithoutGetReaderHeaderReturnsScaledCoordinates()
        {
            using LasZip lasZip = new(out _);
            Assert.True(lasZip.OpenReader(_sSampleFilename));

            LasPoint p = new();
            for (int i = 0; i <= 10; i++)
            {
                lasZip.ReadPoint(ref p);
            }

            Assert.Equal(GetSamplePoint10(), p);
        }

        [Fact]
        public void OpenReaderWithMissingFileReturnsFalse()
        {
            using LasZip lasZip = new(out _);

            Assert.False(lasZip.OpenReader(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.laz")));

            LasPoint p = new();
            Assert.Throws<InvalidOperationException>(() => lasZip.ReadPoint(ref p));
        }

        [Fact]
        public void WriteFile()
        {
            string outputFilename = GetOutputFilename();

            try
            {
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

                    points.Add(Copy(p));

                    lasZip.WritePoint(ref p, updateInventory: true);
                }

                lasZip.CloseWriter();
                lasZip.DestroyWriter();

                lasZip.CloseReader();
                lasZip.DestroyReader();

                lasZip.Dispose();

                using LasZip lasZip2 = new(out _);
                lasZip2.OpenReader(outputFilename);

                h = lasZip2.GetReaderHeader();

                ulPointCount = Math.Max(h.NumberOfPointRecords, h.ExtendedNumberOfPointRecords);

                Assert.Equal(points.Count, (int)ulPointCount);

                LasPoint actual = new();
                for (ulong i = 0; i < ulPointCount; i++)
                {
                    lasZip2.ReadPoint(ref actual);

                    Assert.Equal(points[(int)i], actual);
                }

                lasZip2.CloseReader();
                lasZip2.DestroyReader();
            }
            finally
            {
                DeleteIfExists(outputFilename);
            }
        }

        [Fact]
        public void WriteFileWithManualHeaderUpdate()
        {
            string outputFilename = GetOutputFilename();

            try
            {
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

                        points.Add(Copy(p));

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

                    LasPoint actual = new();
                    for (ulong i = 0; i < ulPointCount; i++)
                    {
                        lasZip2.ReadPoint(ref actual);
                        Assert.Equal(points[(int)i], actual);
                    }
                }
            }
            finally
            {
                DeleteIfExists(outputFilename);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteAndReadSyntheticPointsRoundTripsAllFields(bool isCompressed)
        {
            string outputFilename = GetOutputFilename(isCompressed);
            List<LasPoint> points = CreateSyntheticPoints();

            try
            {
                using (LasZip writer = new(out _))
                {
                    writer.SetWriterHeader(CreateFormat3Header(0, 0, 0));
                    Assert.True(writer.OpenWriter(outputFilename, isCompressed));

                    foreach (LasPoint point in points)
                    {
                        LasPoint pointToWrite = point;
                        writer.WritePoint(ref pointToWrite, updateInventory: true);
                    }

                    writer.CloseWriter();
                }

                using LasZip reader = new(out _);
                Assert.True(reader.OpenReader(outputFilename));

                LaszipHeaderStruct h = reader.GetReaderHeader();
                Assert.Equal(3, h.PointDataFormat);
                Assert.Equal((uint)points.Count, h.NumberOfPointRecords);
                Assert.Equal(points.Min(p => p.X), h.MinX, _dCoordinateTolerance);
                Assert.Equal(points.Max(p => p.X), h.MaxX, _dCoordinateTolerance);
                Assert.Equal(points.Min(p => p.Z), h.MinZ, _dCoordinateTolerance);
                Assert.Equal(points.Max(p => p.Z), h.MaxZ, _dCoordinateTolerance);

                LasPoint actual = new();
                foreach (LasPoint expected in points)
                {
                    reader.ReadPoint(ref actual);
                    AssertPointEqual(expected, actual);
                }
            }
            finally
            {
                DeleteIfExists(outputFilename);
            }
        }

        [Theory]
        [InlineData(-12.34)]
        [InlineData(-0.01)]
        [InlineData(-0.004)]
        [InlineData(-0.006)]
        [InlineData(0.004)]
        [InlineData(0.006)]
        [InlineData(12.34)]
        public void WritePointRoundsCoordinatesToNearestScaledValue(double z)
        {
            string outputFilename = GetOutputFilename();

            try
            {
                using (LasZip writer = new(out _))
                {
                    writer.SetWriterHeader(CreateFormat3Header(0, 0, 0));
                    Assert.True(writer.OpenWriter(outputFilename, true));

                    LasPoint point = new() { Z = z, ReturnNumber = 1, NumberOfReturns = 1 };
                    writer.WritePoint(ref point, updateInventory: true);
                }

                using LasZip reader = new(out _);
                Assert.True(reader.OpenReader(outputFilename));

                LasPoint actual = new();
                reader.ReadPoint(ref actual);

                Assert.Equal(Math.Round(z, 2, MidpointRounding.AwayFromZero), actual.Z, _dCoordinateTolerance);
            }
            finally
            {
                DeleteIfExists(outputFilename);
            }
        }

        [Fact]
        public void HeaderStructLayoutMatchesNativeLaszipHeader()
        {
            // Offsets of laszip_header in laszip_api.h (LASzip 3.5) on 64-bit platforms.
            // If these drift, pointer fields are read from the wrong place and native code can crash.
            Assert.Equal(256, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.ExtendedNumberOfPointsByReturn)).ToInt32());
            Assert.Equal(376, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.MaxGpsTime)).ToInt32());
            Assert.Equal(392, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.TimeOffset)).ToInt32());
            Assert.Equal(396, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.UserDataInHeaderSize)).ToInt32());
            Assert.Equal(400, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.UserDataInHeader)).ToInt32());
            Assert.Equal(408, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.Vlrs)).ToInt32());
            Assert.Equal(416, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.UserDataAfterHeaderSize)).ToInt32());
            Assert.Equal(424, Marshal.OffsetOf<LaszipHeaderStruct>(nameof(LaszipHeaderStruct.UserDataAfterHeader)).ToInt32());
            Assert.Equal(432, Marshal.SizeOf<LaszipHeaderStruct>());
        }

        /// <summary>
        /// Creates a LAS 1.2 file with one format 0 point and the given bytes as user data inside the header.
        /// </summary>
        private static void WriteLasWithUserDataInHeader(string filename, byte[] userData)
        {
            ushort headerSize = (ushort)(227 + userData.Length);

            using BinaryWriter w = new(File.Create(filename));
            w.Write(System.Text.Encoding.ASCII.GetBytes("LASF"));
            w.Write((ushort)0);                  // file source id
            w.Write((ushort)0);                  // global encoding
            w.Write(new byte[16]);               // project guid
            w.Write((byte)1);                    // version major
            w.Write((byte)2);                    // version minor
            w.Write(new byte[32]);               // system identifier
            w.Write(new byte[32]);               // generating software
            w.Write((ushort)1);                  // creation day
            w.Write((ushort)2026);               // creation year
            w.Write(headerSize);
            w.Write((uint)headerSize);           // offset to point data
            w.Write(0u);                         // number of VLRs
            w.Write((byte)0);                    // point data format
            w.Write((ushort)20);                 // point data record length
            w.Write(1u);                         // number of points
            w.Write(1u); w.Write(0u); w.Write(0u); w.Write(0u); w.Write(0u);
            foreach (double d in new[] { 0.01, 0.01, 0.01, 0, 0, 0, 1, 1, 2, 2, 3, 3 })
            {
                w.Write(d);                      // scales, offsets, max/min x y z
            }
            w.Write(userData);

            w.Write(100); w.Write(200); w.Write(300);  // X Y Z
            w.Write((ushort)5);                        // intensity
            w.Write((byte)0b00001001);                 // return 1 of 1
            w.Write((byte)2);                          // classification
            w.Write((sbyte)0);                         // scan angle rank
            w.Write((byte)0);                          // user data
            w.Write((ushort)1);                        // point source id
        }

        [Fact]
        public void CopyHeaderWithUserDataInHeaderPreservesUserData()
        {
            string inputFilename = GetOutputFilename(isCompressed: false);
            string outputFilename = GetOutputFilename();
            byte[] userData = System.Text.Encoding.ASCII.GetBytes("USERDATA");

            try
            {
                WriteLasWithUserDataInHeader(inputFilename, userData);

                using (LasZip lasZip = new(out _))
                {
                    Assert.True(lasZip.OpenReader(inputFilename));
                    LaszipHeaderStruct h = lasZip.GetReaderHeader();
                    Assert.Equal((uint)userData.Length, h.UserDataInHeaderSize);

                    lasZip.SetWriterHeader(h);
                    Assert.True(lasZip.OpenWriter(outputFilename, true));

                    LasPoint p = new();
                    lasZip.ReadPoint(ref p);
                    lasZip.WritePoint(ref p, updateInventory: true);

                    lasZip.CloseWriter();
                    lasZip.DestroyWriter();
                    lasZip.CloseReader();
                    lasZip.DestroyReader();
                }

                using LasZip reader = new(out _);
                Assert.True(reader.OpenReader(outputFilename));

                LaszipHeaderStruct copied = reader.GetReaderHeader();
                Assert.Equal((uint)userData.Length, copied.UserDataInHeaderSize);

                byte[] copiedUserData = new byte[copied.UserDataInHeaderSize];
                Marshal.Copy(copied.UserDataInHeader, copiedUserData, 0, copiedUserData.Length);
                Assert.Equal(userData, copiedUserData);

                LasPoint actual = new();
                reader.ReadPoint(ref actual);
                Assert.Equal(3.0, actual.Z, _dCoordinateTolerance);
                Assert.Equal(2, actual.Classification);
            }
            finally
            {
                DeleteIfExists(inputFilename);
                DeleteIfExists(outputFilename);
            }
        }

        [Theory]
        [InlineData(1e12)]
        [InlineData(-1e12)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void WritePointWithCoordinateOutOfRangeThrowsAndWritesNothing(double x)
        {
            string outputFilename = GetOutputFilename();

            try
            {
                using (LasZip writer = new(out _))
                {
                    writer.SetWriterHeader(CreateFormat3Header(0, 0, 0));
                    Assert.True(writer.OpenWriter(outputFilename, true));

                    LasPoint invalid = new() { X = x, ReturnNumber = 1, NumberOfReturns = 1 };
                    ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(() => writer.WritePoint(ref invalid, updateInventory: true));
                    Assert.StartsWith("X coordinate", e.Message);

                    LasPoint valid = new() { X = 1, ReturnNumber = 1, NumberOfReturns = 1 };
                    writer.WritePoint(ref valid, updateInventory: true);
                }

                using LasZip reader = new(out _);
                Assert.True(reader.OpenReader(outputFilename));
                Assert.Equal(1u, reader.GetReaderHeader().NumberOfPointRecords);

                LasPoint actual = new();
                reader.ReadPoint(ref actual);
                Assert.Equal(1.0, actual.X, _dCoordinateTolerance);
            }
            finally
            {
                DeleteIfExists(outputFilename);
            }
        }

        [Fact]
        public void ReadPointFromTruncatedFileThrowsWithNativeErrorMessage()
        {
            string truncatedFilename = GetOutputFilename();

            try
            {
                byte[] bytes = File.ReadAllBytes(_sSampleFilename);
                File.WriteAllBytes(truncatedFilename, bytes.Take(bytes.Length / 2).ToArray());

                using LasZip lasZip = new(out _);
                Assert.True(lasZip.OpenReader(truncatedFilename));

                ulong ulPointCount = lasZip.GetReaderHeader().NumberOfPointRecords;
                LasPoint p = new();

                LasZipException e = Assert.Throws<LasZipException>(() =>
                {
                    for (ulong i = 0; i < ulPointCount; i++)
                    {
                        lasZip.ReadPoint(ref p);
                    }
                });

                // Base message followed by the reason reported by LASzip
                Assert.StartsWith("Failed to read LasZip point: ", e.Message);
                Assert.True(e.Message.Length > "Failed to read LasZip point: ".Length);
            }
            finally
            {
                DeleteIfExists(truncatedFilename);
            }
        }

        [Fact]
        public void DisposeTwiceDoesNotThrow()
        {
            LasZip lasZip = new(out _);
            Assert.True(lasZip.OpenReader(_sSampleFilename));

            lasZip.Dispose();
            lasZip.Dispose();
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
