using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Base.Lang;

namespace Base.IO.Impl
{
    public class TempFile : Disposable, ITempFile
    {
        private readonly FileInfo _file;
        private bool _claimed;

        public FileInfo File => _file;
        
        public bool IsClaimed => _claimed;

        public TempFile(string ext = null)
        {
            if (ext == null)
                _file = new FileInfo(Path.GetTempFileName());
            else
            {
                if (ext.Length > 0 && ext[0] != '.') ext = "." + ext;
                _file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext));
            }
        }

        private TempFile(FileInfo file)
        {
            _file = file;
        }

        public void Claim()
        {
            _claimed = true;
        }

        protected override void OnDispose()
        {
            try
            {
                if (_file.Exists && !_claimed) _file.Delete();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"unable to delete temporary file {_file.FullName}: {e.Message}");
            }
        }

        public bool TryRename(string name)
        {
            if (!_file.Exists || _claimed) return false;
            try
            {
                _file.MoveTo(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public TextReader OpenTextReader()
        {
            CheckDisposed();
            return new StreamReader(_file.FullName);
        }

        public TextWriter OpenTextWriter()
        {
            CheckDisposed();
            return new StreamWriter(_file.FullName);
        }

        public Stream OpenWriter()
        {
            CheckDisposed();
            return new FileStream(_file.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public Stream OpenReader()
        {
            CheckDisposed();
            return new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }


        public void CopyFrom(Stream aStream)
        {
            CheckDisposed();
            if (aStream != null)
                using (var vOut = File.Open(FileMode.Create))
                {
                    // don't use file.OpenWrite, it does not truncate!
                    aStream.CopyTo(vOut);
                    vOut.Flush();
                }
        }

        public async Task CopyFromAsync(Stream aStream, CancellationToken ct)
        {
            CheckDisposed();
            if (aStream != null)
                using (var vOut = File.Open(FileMode.Create)) // don't use file.OpenWrite, it does not truncate!
                {
                    await aStream.CopyToAsync(vOut, Extensions.DefaultStreamCopyBufferSize, ct);
                    await vOut.FlushAsync(ct);
                }
        }

        public override string ToString()
        {
            return _file.FullName;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case TempFile vOther when vOther.File.FullName == File.FullName:
                case string s when s == File.FullName:
                case FileInfo info when info.FullName == File.FullName:
                    return true;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return File.FullName.GetHashCode();
        }

        public static implicit operator string(TempFile a)
        {
            return a.ToString();
        }

        public static TempFile From(FileInfo file)
        {
            return new TempFile(file);
        }
    }
}
