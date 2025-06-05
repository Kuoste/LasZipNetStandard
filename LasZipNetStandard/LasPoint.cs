using System;
using System.Runtime.InteropServices;

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

        internal static void ConvertPoint(LasZipPointStruct pointStruct, ref LasPoint lasPoint)
        {
            lasPoint.X = pointStruct.X;
            lasPoint.Y = pointStruct.Y;
            lasPoint.Z = pointStruct.Z;
            lasPoint.Intensity = pointStruct.Intensity;
            lasPoint.ReturnNumber = pointStruct.ReturnNumber;
            lasPoint.NumberOfReturns = pointStruct.NumberOfReturns;
            lasPoint.ScanDirectionFlag = pointStruct.ScanDirectionFlag;
            lasPoint.EdgeOfFlightLine = pointStruct.EdgeOfFlightLine;
            lasPoint.Classification = pointStruct.Classification;
            lasPoint.ScanAngleRank = pointStruct.ScanAngleRank;
            lasPoint.UserData = pointStruct.UserData;
            lasPoint.PointSourceId = pointStruct.PointSourceID;
            lasPoint.GpsTime = pointStruct.GpsTime;
            lasPoint.Red = pointStruct.Rgb[0];
            lasPoint.Green = pointStruct.Rgb[1];
            lasPoint.Blue = pointStruct.Rgb[2];
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

    [StructLayout(LayoutKind.Sequential)]
    public struct LasZipPointStruct
    {
        public int X;
        public int Y;
        public int Z;
        public ushort Intensity;
        private byte bitField1; // ReturnNumber : 3, NumberOfReturns : 3, ScanDirectionFlag : 1, EdgeOfFlightLine : 1
        private byte bitField2; // Classification : 5, SyntheticFlag : 1, KeypointFlag : 1, WithheldFlag : 1
        public sbyte ScanAngleRank;
        public byte UserData;
        public ushort PointSourceID;

        // LAS 1.4 only
        public short ExtendedScanAngle;
        private byte bitField3; // ExtendedPointType : 2, ExtendedScannerChannel : 2, ExtendedClassificationFlags : 4
        public byte ExtendedClassification;
        private byte bitField4; // ExtendedReturnNumber : 4, ExtendedNumberOfReturns : 4

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] Dummy;

        public double GpsTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Rgb;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)]
        public byte[] WavePacket;

        public int NumExtraBytes;

        public IntPtr ExtraBytes;  // Use IntPtr instead of byte* for safe pointer handling

        public byte ReturnNumber
        {
            get => (byte)(bitField1 & 0b00000111);
            set => bitField1 = (byte)((bitField1 & 0b11111000) | (value & 0b00000111));
        }

        public byte NumberOfReturns
        {
            get => (byte)((bitField1 >> 3) & 0b00000111);
            set => bitField1 = (byte)((bitField1 & 0b11000111) | ((value & 0b00000111) << 3));
        }

        public byte ScanDirectionFlag
        {
            get => (byte)((bitField1 >> 6) & 0b00000001);
            set => bitField1 = (byte)((bitField1 & 0b10111111) | ((value & 0b00000001) << 6));
        }

        public byte EdgeOfFlightLine
        {
            get => (byte)((bitField1 >> 7) & 0b00000001);
            set => bitField1 = (byte)((bitField1 & 0b01111111) | ((value & 0b00000001) << 7));
        }

        public byte Classification
        {
            get => (byte)(bitField2 & 0b00011111);
            set => bitField2 = (byte)((bitField2 & 0b11100000) | (value & 0b00011111));
        }

        public byte SyntheticFlag
        {
            get => (byte)((bitField2 >> 5) & 0b00000001);
            set => bitField2 = (byte)((bitField2 & 0b11011111) | ((value & 0b00000001) << 5));
        }

        public byte KeypointFlag
        {
            get => (byte)((bitField2 >> 6) & 0b00000001);
            set => bitField2 = (byte)((bitField2 & 0b10111111) | ((value & 0b00000001) << 6));
        }

        public byte WithheldFlag
        {
            get => (byte)((bitField2 >> 7) & 0b00000001);
            set => bitField2 = (byte)((bitField2 & 0b01111111) | ((value & 0b00000001) << 7));
        }

        public byte ExtendedPointType
        {
            get => (byte)(bitField3 & 0b00000011);
            set => bitField3 = (byte)((bitField3 & 0b11111100) | (value & 0b00000011));
        }

        public byte ExtendedScannerChannel
        {
            get => (byte)((bitField3 >> 2) & 0b00000011);
            set => bitField3 = (byte)((bitField3 & 0b11110011) | ((value & 0b00000011) << 2));
        }

        public byte ExtendedClassificationFlags
        {
            get => (byte)((bitField3 >> 4) & 0b00001111);
            set => bitField3 = (byte)((bitField3 & 0b00001111) | ((value & 0b00001111) << 4));
        }

        public byte ExtendedReturnNumber
        {
            get => (byte)(bitField4 & 0b00001111);
            set => bitField4 = (byte)((bitField4 & 0b11110000) | (value & 0b00001111));
        }

        public byte ExtendedNumberOfReturns
        {
            get => (byte)((bitField4 >> 4) & 0b00001111);
            set => bitField4 = (byte)((bitField4 & 0b00001111) | ((value & 0b00001111) << 4));
        }

        internal static LasZipPointStruct ConvertPoint(LasPoint lasPoint)
        {
            return new LasZipPointStruct()
            {
                X = (int)(lasPoint.X + 0.5),
                Y = (int)(lasPoint.Y + 0.5),
                Z = (int)(lasPoint.Z + 0.5),
                Intensity = lasPoint.Intensity,
                ReturnNumber = lasPoint.ReturnNumber,
                NumberOfReturns = lasPoint.NumberOfReturns,
                ScanDirectionFlag = lasPoint.ScanDirectionFlag,
                EdgeOfFlightLine = lasPoint.EdgeOfFlightLine,
                Classification = lasPoint.Classification,
                ScanAngleRank = lasPoint.ScanAngleRank,
                UserData = lasPoint.UserData,
                PointSourceID = lasPoint.PointSourceId,
                GpsTime = lasPoint.GpsTime,
                Rgb = new ushort[] { lasPoint.Red, lasPoint.Green, lasPoint.Blue, 0 }
            };
        }
    }
}
