using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kuoste.LasZipNetStandard
{
    public class LasHeader
    {
        public ushort FileSourceId { get; set; }
        public ushort GlobalEncoding { get; set; }
        public uint ProjectId1 { get; set; }
        public ushort ProjectId2 { get; set; }
        public ushort ProjectId3 { get; set; }
        public byte[] ProjectId4 { get; set; }
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public string SystemIdentifier { get; set; }
        public string GeneratingSoftware { get; set; }
        public ushort CreationDayOfYear { get; set; }
        public ushort CreationYear { get; set; }
        public ushort HeaderSize { get; set; }
        public uint OffsetToPointData { get; set; }
        public uint NumberOfVariableLengthRecords { get; set; }
        public byte PointDataFormat { get; set; }
        public ushort PointDataRecordLength { get; set; }
        public uint NumberOfPointRecords { get; set; }
        public uint[] NumberOfPointsByReturn { get; set; }
        public double ScaleFactorX { get; set; }
        public double ScaleFactorY { get; set; }
        public double ScaleFactorZ { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double OffsetZ { get; set; }
        public double MaxX { get; set; }
        public double MinX { get; set; }
        public double MaxY { get; set; }
        public double MinY { get; set; }
        public double MaxZ { get; set; }
        public double MinZ { get; set; }

        // LAS 1.3 and higher only
        public ulong StartOfWaveformDataPacketRecord { get; set; }

        // LAS 1.4 and higher only
        public ulong StartOfFirstExtendedVariableLengthRecord { get; set; }
        public uint NumberOfExtendedVariableLengthRecords { get; set; }
        public ulong ExtendedNumberOfPointRecords { get; set; }
        public ulong[] ExtendedNumberOfPointsByReturn { get; set; }

        internal static LasHeader ConvertHeader(LaszipHeaderStruct headerStruct)
        {
            var header = new LasHeader
            {
                FileSourceId = headerStruct.file_source_id,
                GlobalEncoding = headerStruct.global_encoding,
                ProjectId1 = headerStruct.project_id1,
                ProjectId2 = headerStruct.project_id2,
                ProjectId3 = headerStruct.project_id3,
                ProjectId4 = headerStruct.project_id4,
                VersionMajor = headerStruct.version_major,
                VersionMinor = headerStruct.version_minor,
                SystemIdentifier = headerStruct.system_identifier,
                GeneratingSoftware = headerStruct.generating_software,
                CreationDayOfYear = headerStruct.creation_day_of_year,
                CreationYear = headerStruct.creation_year,
                HeaderSize = headerStruct.header_size,
                OffsetToPointData = headerStruct.offset_to_point_data,
                NumberOfVariableLengthRecords = headerStruct.number_of_variable_length_records,
                PointDataFormat = headerStruct.point_data_format,
                PointDataRecordLength = headerStruct.point_data_record_length,
                NumberOfPointRecords = headerStruct.number_of_point_records,
                NumberOfPointsByReturn = headerStruct.number_of_points_by_return,
                ScaleFactorX = headerStruct.scale_factor_x,
                ScaleFactorY = headerStruct.scale_factor_y,
                ScaleFactorZ = headerStruct.scale_factor_z,
                OffsetX = headerStruct.offset_x,
                OffsetY = headerStruct.offset_y,
                OffsetZ = headerStruct.offset_z,
                MaxX = headerStruct.max_x,
                MinX = headerStruct.min_x,
                MaxY = headerStruct.max_y,
                MinY = headerStruct.min_y,
                MaxZ = headerStruct.max_z,
                MinZ = headerStruct.min_z,
            };

            if (headerStruct.version_major > 1 || (headerStruct.version_major == 1 && headerStruct.version_minor >= 3))
            {
                header.StartOfWaveformDataPacketRecord = headerStruct.start_of_waveform_data_packet_record;
            }

            if (headerStruct.version_major > 1 || (headerStruct.version_major == 1 && headerStruct.version_minor >= 4))
            {
                header.StartOfFirstExtendedVariableLengthRecord = headerStruct.start_of_first_extended_variable_length_record;
                header.NumberOfExtendedVariableLengthRecords = headerStruct.number_of_extended_variable_length_records;
                header.ExtendedNumberOfPointRecords = headerStruct.extended_number_of_point_records;
                header.ExtendedNumberOfPointsByReturn = headerStruct.extended_number_of_points_by_return;
            }
            else
            {
                header.ExtendedNumberOfPointsByReturn = new ulong[0];
            }
            
            return header;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LaszipHeaderStruct
    {
        public ushort file_source_id;
        public ushort global_encoding;
        public uint project_id1;
        public ushort project_id2;
        public ushort project_id3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] project_id4;
        public byte version_major;
        public byte version_minor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string system_identifier;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string generating_software;
        public ushort creation_day_of_year;
        public ushort creation_year;
        public ushort header_size;
        public uint offset_to_point_data;
        public uint number_of_variable_length_records;
        public byte point_data_format;
        public ushort point_data_record_length;
        public uint number_of_point_records;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] number_of_points_by_return;
        public double scale_factor_x;
        public double scale_factor_y;
        public double scale_factor_z;
        public double offset_x;
        public double offset_y;
        public double offset_z;
        public double max_x;
        public double min_x;
        public double max_y;
        public double min_y;
        public double max_z;
        public double min_z;

        // LAS 1.3 and higher only
        public ulong start_of_waveform_data_packet_record;

        // LAS 1.4 and higher only
        public ulong start_of_first_extended_variable_length_record;
        public uint number_of_extended_variable_length_records;
        public ulong extended_number_of_point_records;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public ulong[] extended_number_of_points_by_return;
    }
}
