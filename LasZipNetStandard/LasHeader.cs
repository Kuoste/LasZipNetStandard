using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kuoste.LasZipNetStandard
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LaszipHeaderStruct
    {
        public ushort FileSourceId;
        public ushort GlobalEncoding;
        public uint ProjectId1;
        public ushort ProjectId2;
        public ushort ProjectId3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] ProjectId4;
        public byte VersionMajor;
        public byte VersionMinor;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] SystemIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] GeneratingSoftware;
        public ushort CreationDayOfYear;
        public ushort CreationYear;
        public ushort HeaderSize;
        public uint OffsetToPointData;
        public uint NumberOfVariableLengthRecords;
        public byte PointDataFormat;
        public ushort PointDataRecordLength;
        public uint NumberOfPointRecords;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] NumberOfPointsByReturn;
        public double ScaleFactorX;
        public double ScaleFactorY;
        public double ScaleFactorZ;
        public double OffsetX;
        public double OffsetY;
        public double OffsetZ;
        public double MaxX;
        public double MinX;
        public double MaxY;
        public double MinY;
        public double MaxZ;
        public double MinZ;

        // LAS 1.3 and higher only
        public ulong StartOfWaveformDataPacketRecord;

        // LAS 1.4 and higher only
        public ulong StartOfFirstExtendedVariableLengthRecord;
        public uint NumberOfExtendedVariableLengthRecords;
        public ulong ExtendedNumberOfPointRecords;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public ulong[] ExtendedNumberOfPointsByReturn;

        // optional
        public uint UserDataInHeaderSize;
        public IntPtr UserDataInHeader;

        // optional VLRs
        public IntPtr Vlrs;

        // optional
        public uint UserDataAfterHeaderSize;
        public IntPtr UserDataAfterHeader;
    }
}
