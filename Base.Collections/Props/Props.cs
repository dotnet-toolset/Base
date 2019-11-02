using System.Collections.Concurrent;

namespace Base.Collections.Props
{
    public sealed class Props : ConcurrentDictionary<int, object>
    {
        internal static int Counter;
    }
}
