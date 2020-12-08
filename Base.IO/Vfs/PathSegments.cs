using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base.Lang;

namespace Base.IO.Vfs
{
    public class PathSegments : IReadOnlyList<string>
    {
        private readonly IReadOnlyList<string> _segments;
        private string _cachedPath;
        private PathSegments _cachedParent;

        public readonly char Separator;
        public int Count => _segments.Count;
        public string this[int index] => _segments[index];

        public string First => _segments[0];
        public string Last => _segments[_segments.Count - 1];
        public bool IsRoot => _segments.Count == 2 && _segments[0].Length == 0 && _segments[1].Length == 0;
        public bool IsEmpty => _segments.Count == 1 && _segments[0].Length == 0;
        public bool IsRelative => _segments[0].Length != 0;

        public PathSegments Parent
        {
            get
            {
                if (_cachedParent == null)
                    lock (this)
                        if (_cachedParent == null)
                            _cachedParent = MakeParent();
                return _cachedParent;
            }
        }

        public PathSegments(IReadOnlyList<string> segments, char separator)
        {
            _segments = segments.CheckNotNull(nameof(segments));
            if (_segments.Count == 0) throw new ArgumentException(nameof(segments), "must have at least one segment");
            Bugs.Assert(segments.All(s => s != null));
            Separator = separator;
        }

        public PathSegments Add(string segment)
        {
            var l = _segments.Count;
            var s = new List<string>(l + 1);
            s.AddRange(_segments);
            s.Add(segment);
            return new PathSegments(s, Separator);
        }

        public PathSegments Combine(PathSegments other)
        {
            if (IsEmpty) return other;
            if (other.IsEmpty) return this;
            if (!other.IsRelative) return other;
            var list = new List<string>(_segments.Count + other._segments.Count);
            list.AddRange(_segments);
            list.AddRange(other._segments);
            return Create(list);
        }

        public PathSegments Combine(string other)
        {
            return Combine(Parse(other));
        }

        public PathSegments RelativeTo(PathSegments other, bool caseSensitive)
        {
            var i = 0;
            while (i < _segments.Count && i < other._segments.Count && Equal(_segments[i], other._segments[i], caseSensitive))
                i++;
            if (i == 0) return null;
            return From(i);
        }

        public PathSegments From(int ofs)
        {
            if (ofs < 0 || ofs > _segments.Count) throw new ArgumentException(nameof(ofs));
            if (ofs == 0) return this;
            if (ofs == _segments.Count) return Empty;
            return new PathSegments(_segments.Skip(ofs).ToList(), Separator);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _segments.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private PathSegments MakeParent()
        {
            if (IsRoot) return null;
            var l = _segments.Count;
            if (l == 1) return null;
            return new PathSegments(_segments.Take(l - 1).ToList(), Separator);
        }

        private string MakePath()
        {
            if (IsRoot) return "/";
            var result = new StringBuilder();
            var i = 0;
            foreach (var v in _segments)
            {
                if (i > 0)
                    result.Append(Separator);
                result.Append(v);
                i++;
            }
            return result.ToString();
        }

        public override string ToString()
        {
            if (_cachedPath == null)
                lock (this)
                    if (_cachedPath == null)
                        _cachedPath = MakePath();
            return _cachedPath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            switch (obj)
            {
                case PathSegments other:
                    return other.ToString() == ToString();
                case string s:
                    return s == ToString();
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        private static bool Equal(string a, string b, bool cs)
        {
            return string.Equals(a, b,
                cs ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
        }

        public static readonly PathSegments Root = new PathSegments(new[] { string.Empty, string.Empty }, '/');
        public static readonly PathSegments Empty = new PathSegments(new[] { string.Empty }, '/');

        private static IReadOnlyList<string> Reduce(IReadOnlyList<string> parts)
        {
            List<string> reduced = null;
            var i = 0;
            void InitReduced()
            {
                if (reduced != null) return;
                reduced = new List<string>(parts.Count);
                if (i > 0) for (var j = 0; j < i; j++) reduced.Add(parts[j]);
            }
            foreach (var part in parts)
            {
                switch (part)
                {
                    case ".":
                        InitReduced();
                        if (reduced.Count == 1 && reduced[0].Length == 0)
                            reduced.Add(string.Empty);
                        break;
                    case "..":
                        InitReduced();
                        if (reduced.Count == 0) throw new ArgumentException("invalid path");
                        if (reduced.Count > 1 || reduced[0].Length != 0)
                            reduced.RemoveAt(reduced.Count - 1);
                        if (reduced.Count == 1 && reduced[0].Length == 0)
                            reduced.Add(string.Empty);
                        break;
                    default:
                        reduced?.Add(part);
                        break;
                }
                i++;
            }
            if (reduced != null)
            {
                parts = reduced.ToList();
                reduced = null;
            }
            var lastBlank = false;
            if (parts.Count > 2)
                for (i = 1; i < parts.Count; i++)
                {
                    if (parts[i].Length == 0)
                        if (reduced != null)
                        {
                            if (reduced[reduced.Count - 1].Length == 0)
                            {
                                lastBlank = true;
                                continue;
                            }
                        }
                        else if (parts[i - 1].Length == 0)
                        {
                            lastBlank = true;
                            InitReduced();
                            continue;
                        }
                    reduced?.Add(parts[i]);
                    lastBlank = false;
                }

            if (lastBlank && reduced != null && reduced.Count == 1 && reduced[0].Length == 0) return Root._segments;
            if (reduced != null)
                parts = reduced.ToArray();
            return parts;
        }

        public static PathSegments Parse(string path)
        {
            if (path == null) return null;
            if (path.Length == 0) return Empty;
            if (path == "/" || path == "\\") return Root;
            return Create(path.Split('/', '\\'));
        }

        public static PathSegments Create(IReadOnlyList<string> parts)
        {
            parts = Reduce(parts);
            switch (parts.Count)
            {
                case 0: return Empty;
                case 1:
                    if (parts[0].Length == 0) return Empty;
                    break;
                case 2:
                    if (parts[0].Length == 0 && parts[1].Length == 0) return Root;
                    break;
            }
            return new PathSegments(parts, '/');
        }
    }
}
