using System.Threading;
using System.Threading.Tasks;

namespace Base.IO.Copy
{
    public interface ICopier
    {
         ICopySource Source { get; }
         ICopyTarget Target { get; }
         ICopyBuffer Buffer { get; }
         void Run();
         Task RunAsync(CancellationToken ct);
    }
}