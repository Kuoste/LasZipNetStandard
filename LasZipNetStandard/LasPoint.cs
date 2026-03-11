using System;
using System.Runtime.CompilerServices;

namespace Kuoste.LasZipNetStandard
{
    public class LasPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public ushort Intensity { get; set; }
        public byte ReturnNumber { get; set; }
        public byte NumberOfReturns { get; set; }
        public byte ScanDirectionFlag { get; set; }
        public byte EdgeOfFlightLine { get; set; }
        public byte Classification { get; set; }
        public sbyte ScanAngleRank { get; set; }
        public byte UserData { get; set; }
        public ushort PointSourceId { get; set; }
        public double GpsTime { get; set; }
        public ushort Red { get; set; }
        public ushort Green { get; set; }
        public ushort Blue { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void ConvertPoint(LasZipPointNative* pointStruct, ref LasPoint lasPoint, double scaleFactorX, double scaleFactorY, double scaleFactorZ, double offsetX, double offsetY, double offsetZ)
        {
            lasPoint.X = pointStruct->X * scaleFactorX + offsetX;
            lasPoint.Y = pointStruct->Y * scaleFactorY + offsetY;
            lasPoint.Z = pointStruct->Z * scaleFactorZ + offsetZ;
            lasPoint.Intensity = pointStruct->Intensity;
            lasPoint.ReturnNumber = pointStruct->ReturnNumber;
            lasPoint.NumberOfReturns = pointStruct->NumberOfReturns;
            lasPoint.ScanDirectionFlag = pointStruct->ScanDirectionFlag;
            lasPoint.EdgeOfFlightLine = pointStruct->EdgeOfFlightLine;
            lasPoint.Classification = pointStruct->Classification;
            lasPoint.ScanAngleRank = pointStruct->ScanAngleRank;
            lasPoint.UserData = pointStruct->UserData;
            lasPoint.PointSourceId = pointStruct->PointSourceID;
            lasPoint.GpsTime = pointStruct->GpsTime;
            lasPoint.Red = pointStruct->Rgb[0];
            lasPoint.Green = pointStruct->Rgb[1];
            lasPoint.Blue = pointStruct->Rgb[2];
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LasPoint);
        }

        public bool Equals(LasPoint? p)
        {
            return p != null &&
                p.X == this.X &&
                p.Y == this.Y &&
                p.Z == this.Z &&
                p.Intensity == this.Intensity &&
                p.ReturnNumber == this.ReturnNumber &&
                p.NumberOfReturns == this.NumberOfReturns &&
                p.ScanDirectionFlag == this.ScanDirectionFlag &&
                p.EdgeOfFlightLine == this.EdgeOfFlightLine &&
                p.Classification == this.Classification &&
                p.ScanAngleRank == this.ScanAngleRank &&
                p.UserData == this.UserData &&
                p.PointSourceId == this.PointSourceId &&
                p.GpsTime == this.GpsTime &&
                p.Red == this.Red &&
                p.Green == this.Green &&
                p.Blue == this.Blue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GpsTime, Z);
        }
    }
}
