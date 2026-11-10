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

        /// <summary>
        /// Rounds half away from zero like LASlib's I32_QUANTIZE. Casting alone truncates
        /// towards zero, which would round negative values the wrong way.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> The scaled value does not fit into a 32-bit integer or is NaN. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Quantize(double scaledValue, string axis, double coordinate)
        {
            // Negated check so that NaN is rejected too
            if (!(scaledValue > int.MinValue - 0.5 && scaledValue < int.MaxValue + 0.5))
            {
                ThrowCoordinateOutOfRange(axis, coordinate);
            }

            return scaledValue >= 0 ? (int)(scaledValue + 0.5) : (int)(scaledValue - 0.5);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowCoordinateOutOfRange(string axis, double coordinate)
        {
            throw new ArgumentOutOfRangeException("point", coordinate,
                $"{axis} coordinate cannot be stored with the scale factor and offset of the writer header.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void WriteFromLasPoint(LasZipPointNative* native, LasPoint point, double scaleFactorX, double scaleFactorY, double scaleFactorZ, double offsetX, double offsetY, double offsetZ)
        {
            native->X = Quantize((point.X - offsetX) / scaleFactorX, "X", point.X);
            native->Y = Quantize((point.Y - offsetY) / scaleFactorY, "Y", point.Y);
            native->Z = Quantize((point.Z - offsetZ) / scaleFactorZ, "Z", point.Z);
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
