using System;
using System.Threading;

public sealed class SimplePulseShell<T>
    : IPulseShell<T>
{
    private ManualResetEvent _MRE = new ManualResetEvent(false);
    public T Payload { get; }

    public SimplePulseShell(T payload)
    {
        Payload = payload;
    }

    private void Wait()
    {
        try
        {
            _MRE?.WaitOne();
        }
        catch (ObjectDisposedException)
        { }
    }

    private IPulseShell<T> _Next;
    public IPulseShell<T> Next
    {
        get
        {
            Wait();
            return _Next;
        }
    }

    internal void Chain(SimplePulseShell<T> next)
    {
        var mre = Interlocked.Exchange(ref _MRE, null);
        if (mre == null)
            throw new InvalidOperationException();
        _Next = next;
        mre.Set();
        mre.Dispose();
    }
}
