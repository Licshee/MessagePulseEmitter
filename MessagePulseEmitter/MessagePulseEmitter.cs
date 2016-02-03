using System.Collections;
using System.Collections.Generic;
using System.Threading;

public abstract class MessagePulseEmitter<TMsg, TShell>
    : IEnumerable<TMsg>, IEnumerable
    where TShell : class, IPulseShell<TMsg>
{
    private volatile TShell _TopShell;
    protected MessagePulseEmitter()
    {
        _TopShell = PickShell(default(TMsg));
    }

    protected abstract TShell PickShell(TMsg msg);
    protected abstract void Chain(TShell shell, TShell next);

    protected void Load(TMsg msg, bool ignoreBreak)
    {
        var next = PickShell(msg);
        TShell old;

        if (ignoreBreak)
            old = Interlocked.Exchange(ref _TopShell, next);
        else
        {
            old = _TopShell;
            while (old != null)
            {
                var tmp = Interlocked.CompareExchange(ref _TopShell, next, old);
                if (tmp == old) break;
                old = tmp;
            }
        }
        Chain(old, next);
    }
    public void Break()
        => Chain(Interlocked.Exchange(ref _TopShell, null), null);

    IEnumerator<TMsg> IEnumerable<TMsg>.GetEnumerator()
    {
        IPulseShell<TMsg> shell = _TopShell;
        while ((shell = shell.Next) != null)
            yield return shell.Payload;
    }
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TMsg>)this).GetEnumerator();
    public IEnumerator<TMsg> GetEnumerator()
        => ((IEnumerable<TMsg>)this).GetEnumerator();
}
