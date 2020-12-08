namespace Base.IO.Vfs
{
    public class VfsSearchParams
    {
        /// <summary>
        /// Name or wildcard
        /// </summary>
        public readonly string Pattern;

        public bool CaseSensitive;
        public bool Recursive;

        public VfsSearchParams(string pattern)
        {
            Pattern = pattern;
        }
    }
}
