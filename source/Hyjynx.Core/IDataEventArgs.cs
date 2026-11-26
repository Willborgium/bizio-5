#pragma warning disable CA1711

namespace Hyjynx.Core;

public interface IDataEventArgs<out T>
{
    T Data { get; }
}
