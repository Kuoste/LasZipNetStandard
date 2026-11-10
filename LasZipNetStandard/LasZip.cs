using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kuoste.LasZipNetStandard
{
    public class LasZip : IDisposable
    {
        private bool _disposed = false;
        private bool _readerOpen;
        private bool _writerOpen;
        private const string _lasZipDll = "laszip64";

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
        private static extern int laszip_get_error(IntPtr pointer, ref IntPtr error);

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
        public static extern int laszip_update_inventory(IntPtr pointer);

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

            Version = string.Format(VersionMajor.ToString() + "." + VersionMinor + " r" + VersionRevision + " (" + VersionBuild + ")");

            _headerReader = new LaszipHeaderStruct();
            _headerWriter = new LaszipHeaderStruct();


            if (laszip_create(ref _pLasZipReader) != 0)
            {
                throw new LasZipException("Failed to create LasZip reader pointer");
            }


            if (laszip_create(ref _pLasZipWriter) != 0)
            {
                laszip_destroy(_pLasZipReader);
                _pLasZipReader = IntPtr.Zero;
                throw new LasZipException("Failed to create LasZip writer pointer");
            }
        }

        /// <summary>
        /// Creates an exception with the message appended by the last error reported by the native LasZip object.
        /// </summary>
        private static LasZipException CreateException(IntPtr pLasZip, string message)
        {
            IntPtr pError = IntPtr.Zero;
            if (pLasZip != IntPtr.Zero && laszip_get_error(pLasZip, ref pError) == 0 && pError != IntPtr.Zero)
            {
                string? nativeError = Marshal.PtrToStringAnsi(pError);
                if (!string.IsNullOrEmpty(nativeError))
                {
                    return new LasZipException(message + ": " + nativeError);
                }
            }

            return new LasZipException(message);
        }

        public bool OpenReader(string filename)
        {
            ThrowIfDisposed();

            bool isCompressed = false;
            if (laszip_open_reader(_pLasZipReader, filename, ref isCompressed) != 0)
            {
                return false;
            }

            _readerOpen = true;

            // ReadPoint needs the scale factors and offsets even if GetReaderHeader is never called
            GetReaderHeader();

            return true;
        }

        public bool OpenWriter(string filename, bool isCompressed)
        {
            ThrowIfDisposed();

            if (laszip_open_writer(_pLasZipWriter, filename, isCompressed) != 0)
            {
                return false;
            }

            _writerOpen = true;
            return true;
        }

        public LaszipHeaderStruct GetReaderHeader()
        {
            ThrowIfDisposed();
            ThrowIfReaderNotOpen();

            IntPtr pHeader = IntPtr.Zero;
            if (laszip_get_header_pointer(_pLasZipReader, ref pHeader) != 0)
            {
                throw CreateException(_pLasZipReader, "Failed to get LasZip header pointer");
            }

            _headerReader = Marshal.PtrToStructure<LaszipHeaderStruct>(pHeader);
            return _headerReader;
        }

        /// <summary>
        /// Sets the writer header. Native pointer fields (VLRs, user data) from
        /// the source header are written as-is so OpenWriter can use them.
        /// They are cleared internally before close/destroy to prevent double-free.
        /// See GitHub issue #15 for proper VLR write support.
        /// </summary>
        public void SetWriterHeader(LaszipHeaderStruct header)
        {
            ThrowIfDisposed();

            if (laszip_get_header_pointer(_pLasZipWriter, ref _pHeaderWriter) != 0)
            {
                throw CreateException(_pLasZipWriter, "Failed to get LasZip header pointer");
            }

            Marshal.StructureToPtr(header, _pHeaderWriter, false);

            _headerWriter = header;
        }

        /// <summary>
        /// Reads next point from the file.
        /// </summary>
        /// <param name="point"> Point data is read to this reference. </param>
        /// <remarks> The given instance is overwritten, so copy the point if it needs to be stored. </remarks>
        /// <exception cref="LasZipException"> Reading failed. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ReadPoint(ref LasPoint point)
        {
            ThrowIfDisposed();
            ThrowIfReaderNotOpen();

            // Get the memory location for the point in LasZip library
            if (_pPointReader == IntPtr.Zero)
            {
                if (laszip_get_point_pointer(_pLasZipReader, ref _pPointReader) != 0)
                {
                    throw CreateException(_pLasZipReader, "Failed to get LasZip point pointer");
                }
            }

            // Read new point
            if (laszip_read_point(_pLasZipReader) != 0)
            {
                throw CreateException(_pLasZipReader, "Failed to read LasZip point");
            }

            LasPoint.ConvertPoint((LasZipPointNative*)_pPointReader, ref point,
                _headerReader.ScaleFactorX, _headerReader.ScaleFactorY, _headerReader.ScaleFactorZ,
                _headerReader.OffsetX, _headerReader.OffsetY, _headerReader.OffsetZ);
        }

        /// <summary>
        /// Writes the point given as reference.
        /// </summary>
        /// <param name="point"> LasPoint to write. The coordinates are scaled and offset
        /// using the header values before writing. </param>
        /// <param name="updateInventory"> True to update the native header inventory after writing the point. </param>
        /// <exception cref="ArgumentOutOfRangeException"> A coordinate does not fit the scale factor and offset of the writer header. Nothing is written. </exception>
        /// <exception cref="LasZipException"> Writing failed. </exception>
        public unsafe void WritePoint(ref LasPoint point, bool updateInventory = false)
        {
            ThrowIfDisposed();
            ThrowIfWriterNotOpen();

            // Get the memory location for the point in LasZip library
            if (_pPointWriter == IntPtr.Zero)
            {
                if (laszip_get_point_pointer(_pLasZipWriter, ref _pPointWriter) != 0)
                {
                    throw CreateException(_pLasZipWriter, "Failed to get LasZip point pointer");
                }
            }

            LasZipPointNative.WriteFromLasPoint((LasZipPointNative*)_pPointWriter, point,
                _headerWriter.ScaleFactorX, _headerWriter.ScaleFactorY, _headerWriter.ScaleFactorZ,
                _headerWriter.OffsetX, _headerWriter.OffsetY, _headerWriter.OffsetZ);

            // Write point 
            if (laszip_write_point(_pLasZipWriter) != 0)
            {
                throw CreateException(_pLasZipWriter, "Failed to write LasZip point");
            }

            if (updateInventory && laszip_update_inventory(_pLasZipWriter) != 0)
            {
                throw CreateException(_pLasZipWriter, "Failed to update LasZip inventory");
            }
        }

        public void CloseReader()
        {
            ThrowIfDisposed();

            if (_readerOpen && laszip_close_reader(_pLasZipReader) != 0)
            {
                throw CreateException(_pLasZipReader, "Failed to close reader");
            }

            _readerOpen = false;
            _pPointReader = IntPtr.Zero;
        }

        public void DestroyReader()
        {
            ThrowIfDisposed();

            if (laszip_destroy(_pLasZipReader) != 0)
            {
                throw CreateException(_pLasZipReader, "Failed to destroy reader");
            }

            _readerOpen = false;
            _pLasZipReader = IntPtr.Zero;
            _pPointReader = IntPtr.Zero;
        }

        public void CloseWriter()
        {
            ThrowIfDisposed();

            ClearWriterNativePointers();

            if (_writerOpen && laszip_close_writer(_pLasZipWriter) != 0)
            {
                throw CreateException(_pLasZipWriter, "Failed to close writer");
            }

            _writerOpen = false;
            _pPointWriter = IntPtr.Zero;
            _pHeaderWriter = IntPtr.Zero;
        }

        public void DestroyWriter()
        {
            ThrowIfDisposed();

            ClearWriterNativePointers();

            if (laszip_destroy(_pLasZipWriter) != 0)
            {
                throw CreateException(_pLasZipWriter, "Failed to destroy writer");
            }

            _writerOpen = false;
            _pLasZipWriter = IntPtr.Zero;
            _pPointWriter = IntPtr.Zero;
            _pHeaderWriter = IntPtr.Zero;
        }

        /// <summary>
        /// Zeros VLR and user-data pointers in the writer's native header
        /// so the native library does not free memory owned by another object.
        /// </summary>
        private void ClearWriterNativePointers()
        {
            if (_pHeaderWriter == IntPtr.Zero)
            {
                return;
            }

            LaszipHeaderStruct h = Marshal.PtrToStructure<LaszipHeaderStruct>(_pHeaderWriter);
            h.UserDataInHeaderSize = 0;
            h.UserDataInHeader = IntPtr.Zero;
            h.NumberOfVariableLengthRecords = 0;
            h.Vlrs = IntPtr.Zero;
            h.UserDataAfterHeaderSize = 0;
            h.UserDataAfterHeader = IntPtr.Zero;
            Marshal.StructureToPtr(h, _pHeaderWriter, false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LasZip));
            }
        }

        private void ThrowIfReaderNotOpen()
        {
            if (!_readerOpen)
            {
                throw new InvalidOperationException();
            }
        }

        private void ThrowIfWriterNotOpen()
        {
            if (!_writerOpen)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Releases all resources used by the LasZip instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Clean up unmanaged resources
            if (_pLasZipReader != IntPtr.Zero)
            {
                if (_readerOpen)
                {
                    laszip_close_reader(_pLasZipReader);
                    _readerOpen = false;
                }

                laszip_destroy(_pLasZipReader);
                _pLasZipReader = IntPtr.Zero;
                _pPointReader = IntPtr.Zero;
            }

            if (_pLasZipWriter != IntPtr.Zero)
            {
                ClearWriterNativePointers();

                if (_writerOpen)
                {
                    laszip_close_writer(_pLasZipWriter);
                    _writerOpen = false;
                }

                laszip_destroy(_pLasZipWriter);
                _pLasZipWriter = IntPtr.Zero;
                _pPointWriter = IntPtr.Zero;
                _pHeaderWriter = IntPtr.Zero;
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer to ensure unmanaged resources are released.
        /// </summary>
        ~LasZip()
        {
            Dispose(false);
        }
    }
}
