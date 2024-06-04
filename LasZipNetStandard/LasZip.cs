using System;
using System.Runtime.InteropServices;

namespace Kuoste.LasZipNetStandard
{
    public class LasZip
    {
        private const string _lasZipDll = "laszip64.dll";

        private IntPtr _pLasZipReader;
        private IntPtr _pLasZipWriter;

        private IntPtr _pPointReader;
        private IntPtr _pPointWriter;

        LaszipHeaderStruct _headerReader;
        LaszipHeaderStruct _headerWriter;

        private IntPtr _pHeaderWriter;

        // Import the functions from the DLL
        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_create(ref IntPtr pointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_get_version(ref byte VersionMajor, ref byte VersionMinor, ref UInt16 VersionRevision, ref UInt32 VersionBuild);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_open_reader(IntPtr pointer, string filename, ref bool isCompressed);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_open_writer(IntPtr pointer, string filename, bool isCompressed);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_get_header_pointer(IntPtr pointer, ref IntPtr headerPointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_set_header(IntPtr pointer, IntPtr headerPointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_read_point(IntPtr pointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_set_point(IntPtr pointer, IntPtr pointPointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_write_point(IntPtr pointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_get_point_pointer(IntPtr pointer, ref IntPtr pointPointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_close_reader(IntPtr pointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_close_writer(IntPtr pointer);

        [DllImport(_lasZipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_destroy(IntPtr pointer);

        public LasZip(out string Version)
        {
            byte VersionMajor = 0;
            byte VersionMinor = 0;
            UInt16 VersionRevision = 0;
            UInt32 VersionBuild = 0;

            laszip_get_version(ref VersionMajor, ref VersionMinor, ref VersionRevision, ref VersionBuild);

            Version = string.Format("v" + VersionMajor + '.' + VersionMinor + '.' + VersionRevision + '.' + VersionBuild);

            _headerReader = new LaszipHeaderStruct();
            _headerWriter = new LaszipHeaderStruct();


            if (laszip_create(ref _pLasZipReader) != 0)
            {
                throw new Exception("Failed to create LasZip reader pointer");
            }


            if (laszip_create(ref _pLasZipWriter) != 0)
            {
                throw new Exception("Failed to create LasZip writer pointer");
            }
        }

        public bool OpenReader(string filename)
        {
            bool isCompressed = false;
            if (laszip_open_reader(_pLasZipReader, filename, ref isCompressed) != 0)
            {
                return false;
            }

            return true;
        }

        public bool OpenWriter(string filename, bool isCompressed)
        {
            if (laszip_open_writer(_pLasZipWriter, filename, isCompressed) != 0)
            {
                return false;
            }

            return true;
        }

        public LaszipHeaderStruct GetReaderHeader()
        {
            IntPtr pHeader = IntPtr.Zero;
            if (laszip_get_header_pointer(_pLasZipReader, ref pHeader) != 0)
            {
                throw new Exception("Failed to get LasZip header pointer");
            }

            _headerReader = Marshal.PtrToStructure<LaszipHeaderStruct>(pHeader);
            return _headerReader;
        }

        public void SetWriterHeader(LaszipHeaderStruct header)
        {
            if (laszip_get_header_pointer(_pLasZipWriter, ref _pHeaderWriter) != 0)
            {
                throw new Exception("Failed to get LasZip header pointer");
            }

            Marshal.StructureToPtr(header, _pHeaderWriter, false);

            //_headerWriter = Marshal.PtrToStructure<LaszipHeaderStruct>(_pHeaderWriter);

            _headerWriter = header;



            //if (laszip_get_header_pointer(_pLasZipReader, ref _pHeaderWriter) != 0)
            //{
            //    throw new Exception("Failed to get LasZip header pointer");
            //}

            //if (laszip_set_header(_pLasZipWriter, _pHeaderWriter) != 0)
            //{
            //    throw new Exception("Failed to set LasZip header pointer");
            //}
        }

        public void ReadPoint(ref LasPoint point)
        {
            // Get point location in LasZip library
            if (_pPointReader == IntPtr.Zero)
            {
                if (laszip_get_point_pointer(_pLasZipReader, ref _pPointReader) != 0)
                {
                    throw new Exception("Failed to get LasZip point pointer");
                }
            }

            // Read new point 
            if (laszip_read_point(_pLasZipReader) != 0)
            {
                throw new Exception("Failed to read LasZip point");
            }

            // Copy point data from C++ struct to C# class
            LasPoint.ConvertPoint(Marshal.PtrToStructure<LasZipPointStruct>(_pPointReader), ref point);

            // Scale the coordinates and add offsets. Not using the laszip_get_coordinates in order to make things faster.
            point.X = point.X * _headerReader.ScaleFactorX + _headerReader.OffsetX;
            point.Y = point.Y * _headerReader.ScaleFactorY + _headerReader.OffsetY;
            point.Z = point.Z * _headerReader.ScaleFactorZ + _headerReader.OffsetZ;
        }

        public void WritePoint(ref LasPoint point)
        {
            //// Get point location in LasZip library
            //if (_pPointWriter == IntPtr.Zero)
            //{
            //    if (laszip_get_point_pointer(_pLasZipWriter, ref _pPointWriter) != 0)
            //    {
            //        throw new Exception("Failed to get LasZip point pointer");
            //    }
            //}

            //// Scale the coordinates and add offsets. Not using the laszip_set_coordinates in order to make things faster.
            //point.X = (point.X - _headerWriter.OffsetX) / _headerWriter.ScaleFactorX;
            //point.Y = (point.Y - _headerWriter.OffsetY) / _headerWriter.ScaleFactorY;
            //point.Z = (point.Z - _headerWriter.OffsetZ) / _headerWriter.ScaleFactorZ;

            //Marshal.StructureToPtr(LasZipPointStruct.ConvertPoint(point), _pPointWriter, false);

            //if (laszip_set_point(_pLasZipWriter, _pPointWriter) != 0)
            //{
            //    throw new Exception("Failed to set LasZip point");
            //}

            //// Write point 
            //if (laszip_write_point(_pLasZipWriter) != 0)
            //{
            //    throw new Exception("Failed to write LasZip point");
            //}


            laszip_set_point(_pLasZipWriter, _pPointReader);

            // Write point 
            if (laszip_write_point(_pLasZipWriter) != 0)
            {
                throw new Exception("Failed to write LasZip point");
            }
        }

        public void CloseReader()
        {
            laszip_close_reader(_pLasZipReader);
            laszip_destroy(_pLasZipReader);

            _pLasZipReader = IntPtr.Zero;
            _pPointReader = IntPtr.Zero;
        }

        public void CloseWriter()
        {
            laszip_close_writer(_pLasZipWriter);
            laszip_destroy(_pLasZipWriter);

            _pLasZipWriter = IntPtr.Zero;
            _pPointWriter = IntPtr.Zero;
        }

        public void Dispose()
        {
            CloseReader();
            CloseWriter();
        }
    }
}
