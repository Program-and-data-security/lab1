using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    internal class MyStream : FileStream
    {
        public MyStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public MyStream(IntPtr handle, FileAccess access) : base(handle, access)
        {
        }

        public MyStream(SafeFileHandle handle, FileAccess access) : base(handle, access)
        {
        }

        public MyStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }

        public MyStream(IntPtr handle, FileAccess access, bool ownsHandle) : base(handle, access, ownsHandle)
        {
        }

        public MyStream(SafeFileHandle handle, FileAccess access, int bufferSize) : base(handle, access, bufferSize)
        {
        }

        public MyStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }

        public MyStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize) : base(handle, access, ownsHandle, bufferSize)
        {
        }

        public MyStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) : base(handle, access, bufferSize, isAsync)
        {
        }

        public MyStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) : base(path, mode, access, share, bufferSize)
        {
        }

        public MyStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync) : base(handle, access, ownsHandle, bufferSize, isAsync)
        {
        }

        public MyStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : base(path, mode, access, share, bufferSize, options)
        {
        }

        public MyStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
        {
        }

        public MyStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options) : base(path, mode, rights, share, bufferSize, options)
        {
        }

        public MyStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity) : base(path, mode, rights, share, bufferSize, options, fileSecurity)
        {
        }

        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            base.Flush();
        }
    }
}
