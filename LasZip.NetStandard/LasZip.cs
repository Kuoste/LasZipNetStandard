using System;
using System.Runtime.InteropServices;

namespace Kuoste.LasZipNetStandard
{
    public class LasZip
    {
        private const string LaszipDll = "laszip64.dll";

        private IntPtr _pLasZipReader;
        private IntPtr _pPoint;

        LasHeader _header;

        // Import the functions from the DLL
        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_create(ref IntPtr pointer);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_open_reader(IntPtr pointer, string filename, ref bool isCompressed);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_get_header_pointer(IntPtr pointer, ref IntPtr headerPointer);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_read_point(IntPtr pointer);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_get_coordinates(IntPtr pointer, ref IntPtr coordinatesPointer);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_get_point_pointer(IntPtr pointer, ref IntPtr pointPointer);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_close_reader(IntPtr pointer);

        [DllImport(LaszipDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int laszip_destroy(ref IntPtr pointer);

        public LasZip()
        {
            if (laszip_create(ref _pLasZipReader) != 0)
            {
                throw new Exception("Failed to create LasZip pointer");
            }

            _header = new LasHeader();
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

        public LasHeader GetHeader()
        {
            IntPtr pHeader = IntPtr.Zero;
            if (laszip_get_header_pointer(_pLasZipReader, ref pHeader) != 0)
            {
                throw new Exception("Failed to get LasZip header pointer");
            }

            _header = LasHeader.ConvertHeader(Marshal.PtrToStructure<LaszipHeaderStruct>(pHeader));
            return _header;
        }

        public void ReadPoint(ref LasPoint point)
        {
            // Get point location in LasZip library
            if (_pPoint == IntPtr.Zero)
            {
                if (laszip_get_point_pointer(_pLasZipReader, ref _pPoint) != 0)
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
            LasPoint.ConvertPoint(Marshal.PtrToStructure<LasZipPointStruct>(_pPoint), ref point);

            // Scale the coordinates and add offsets. Not using the laszip_get_coordinates in order to make things faster.
            point.X = point.X * _header.ScaleFactorX + _header.OffsetX;
            point.Y = point.Y * _header.ScaleFactorY + _header.OffsetY;
            point.Z = point.Z * _header.ScaleFactorZ + _header.OffsetZ;
        }

        public void CloseReader()
        {
            laszip_close_reader(_pLasZipReader);
            laszip_destroy(ref _pLasZipReader);

            _pLasZipReader = IntPtr.Zero;
            _pPoint = IntPtr.Zero;
        }
    }
}
