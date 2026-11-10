namespace Kuoste.LasZipNetStandard.Tests
{
    public class LasPointTests
    {
        private static LasPoint CreatePoint() => new()
        {
            X = 567178.27, Y = 7112224.82, Z = 141.47,
            Intensity = 34711, ReturnNumber = 2, NumberOfReturns = 3,
            ScanDirectionFlag = 1, EdgeOfFlightLine = 1, Classification = 4,
            ScanAngleRank = -65, UserData = 2, PointSourceId = 10,
            GpsTime = 308645205.76348633, Red = 1, Green = 2, Blue = 3
        };

        public static TheoryData<string, Action<LasPoint>> SingleFieldChanges => new()
        {
            { nameof(LasPoint.X), p => p.X += 0.01 },
            { nameof(LasPoint.Y), p => p.Y += 0.01 },
            { nameof(LasPoint.Z), p => p.Z += 0.01 },
            { nameof(LasPoint.Intensity), p => p.Intensity++ },
            { nameof(LasPoint.ReturnNumber), p => p.ReturnNumber++ },
            { nameof(LasPoint.NumberOfReturns), p => p.NumberOfReturns++ },
            { nameof(LasPoint.ScanDirectionFlag), p => p.ScanDirectionFlag = 0 },
            { nameof(LasPoint.EdgeOfFlightLine), p => p.EdgeOfFlightLine = 0 },
            { nameof(LasPoint.Classification), p => p.Classification++ },
            { nameof(LasPoint.ScanAngleRank), p => p.ScanAngleRank++ },
            { nameof(LasPoint.UserData), p => p.UserData++ },
            { nameof(LasPoint.PointSourceId), p => p.PointSourceId++ },
            { nameof(LasPoint.GpsTime), p => p.GpsTime += 0.000001 },
            { nameof(LasPoint.Red), p => p.Red++ },
            { nameof(LasPoint.Green), p => p.Green++ },
            { nameof(LasPoint.Blue), p => p.Blue++ },
        };

        [Fact]
        public void EqualPointsAreEqualAndHaveSameHashCode()
        {
            LasPoint a = CreatePoint();
            LasPoint b = CreatePoint();

            Assert.NotSame(a, b);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(SingleFieldChanges))]
        public void PointsDifferingInOneFieldAreNotEqual(string field, Action<LasPoint> change)
        {
            LasPoint a = CreatePoint();
            LasPoint b = CreatePoint();

            change(b);

            Assert.False(a.Equals(b), $"Points differing in {field} should not be equal");
        }

        [Fact]
        public void PointIsNotEqualToNullOrOtherType()
        {
            LasPoint a = CreatePoint();

            Assert.False(a.Equals((LasPoint?)null));
            Assert.False(a.Equals((object?)null));
            Assert.False(a.Equals("not a point"));
        }

        [Fact]
        public void ReturnsOfSamePulseAreDistinctInHashSet()
        {
            // Returns of one laser pulse share the GPS time and can share the elevation
            LasPoint first = CreatePoint();
            LasPoint second = CreatePoint();
            second.X += 0.5;
            second.ReturnNumber = 3;

            HashSet<LasPoint> set = new() { first, second, CreatePoint() };

            Assert.Equal(2, set.Count);
            Assert.Contains(CreatePoint(), set);
        }
    }
}
