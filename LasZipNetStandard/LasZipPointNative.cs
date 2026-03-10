using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kuoste.LasZipNetStandard
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct LasZipPointNative
    {
        public int X;
        public int Y;
        public int Z;
        public ushort Intensity;
        private byte bitField1;
        private byte bitField2;
        public sbyte ScanAngleRank;
        public byte UserData;
        public ushort PointSourceID;
        public short ExtendedScanAngle;
        private byte bitField3;
        public byte ExtendedClassification;
        private byte bitField4;
        public fixed byte Dummy[7];
        public double GpsTime;
        public fixed ushort Rgb[4];
        public fixed byte WavePacket[29];
        public int NumExtraBytes;
        public IntPtr ExtraBytes;

        public byte ReturnNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(bitField1 & 0b00000111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField1 = (byte)((bitField1 & 0b11111000) | (value & 0b00000111));
        }

        public byte NumberOfReturns
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField1 >> 3) & 0b00000111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField1 = (byte)((bitField1 & 0b11000111) | ((value & 0b00000111) << 3));
        }

        public byte ScanDirectionFlag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField1 >> 6) & 0b00000001);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField1 = (byte)((bitField1 & 0b10111111) | ((value & 0b00000001) << 6));
        }

        public byte EdgeOfFlightLine
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField1 >> 7) & 0b00000001);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField1 = (byte)((bitField1 & 0b01111111) | ((value & 0b00000001) << 7));
        }

        public byte Classification
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(bitField2 & 0b00011111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField2 = (byte)((bitField2 & 0b11100000) | (value & 0b00011111));
        }

        public byte SyntheticFlag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField2 >> 5) & 0b00000001);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField2 = (byte)((bitField2 & 0b11011111) | ((value & 0b00000001) << 5));
        }

        public byte KeypointFlag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField2 >> 6) & 0b00000001);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField2 = (byte)((bitField2 & 0b10111111) | ((value & 0b00000001) << 6));
        }

        public byte WithheldFlag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField2 >> 7) & 0b00000001);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField2 = (byte)((bitField2 & 0b01111111) | ((value & 0b00000001) << 7));
        }

        public byte ExtendedPointType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(bitField3 & 0b00000011);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField3 = (byte)((bitField3 & 0b11111100) | (value & 0b00000011));
        }

        public byte ExtendedScannerChannel
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField3 >> 2) & 0b00000011);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField3 = (byte)((bitField3 & 0b11110011) | ((value & 0b00000011) << 2));
        }

        public byte ExtendedClassificationFlags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField3 >> 4) & 0b00001111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField3 = (byte)((bitField3 & 0b00001111) | ((value & 0b00001111) << 4));
        }

        public byte ExtendedReturnNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(bitField4 & 0b00001111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField4 = (byte)((bitField4 & 0b11110000) | (value & 0b00001111));
        }

        public byte ExtendedNumberOfReturns
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((bitField4 >> 4) & 0b00001111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitField4 = (byte)((bitField4 & 0b00001111) | ((value & 0b00001111) << 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void WriteFromLasPoint(LasZipPointNative* native, LasPoint point, double scaleFactorX, double scaleFactorY, double scaleFactorZ, double offsetX, double offsetY, double offsetZ)
        {
            native->X = (int)((point.X - offsetX) / scaleFactorX + 0.5);
            native->Y = (int)((point.Y - offsetY) / scaleFactorY + 0.5);
            native->Z = (int)((point.Z - offsetZ) / scaleFactorZ + 0.5);
            native->Intensity = point.Intensity;
            native->ReturnNumber = point.ReturnNumber;
            native->NumberOfReturns = point.NumberOfReturns;
            native->ScanDirectionFlag = point.ScanDirectionFlag;
            native->EdgeOfFlightLine = point.EdgeOfFlightLine;
            native->Classification = point.Classification;
            native->ScanAngleRank = point.ScanAngleRank;
            native->UserData = point.UserData;
            native->PointSourceID = point.PointSourceId;
            native->GpsTime = point.GpsTime;
            native->Rgb[0] = point.Red;
            native->Rgb[1] = point.Green;
            native->Rgb[2] = point.Blue;
        }
    }
}
