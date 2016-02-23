using System;
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

    protected bool Load(TMsg msg, bool notBreaking)
    {
        var next = PickShell(msg);
        TShell old;

        if (notBreaking)
            old = Interlocked.Exchange(ref _TopShell, next);
        else
        {
            notBreaking = (old = _TopShell) != null;
            while (old != null)
            {
                var tmp = Interlocked.CompareExchange(ref _TopShell, next, old);
                if (tmp == old)
                    break;
                notBreaking = (old = tmp) != null;
            }
        }

        Chain(old, next);

        return notBreaking;
    }
    public void Break()
        => Chain(Interlocked.Exchange(ref _TopShell, null), null);

    class Enumerator : IEnumerator<TMsg>
    {
        IPulseShell<TMsg> _Top;
        int state = 0;

        public Enumerator(TShell top)
        {
            _Top = top;
        }

        public TMsg Current
        {
            get
            {
                if (state == 1)
                    return _Top.Payload;
                throw new InvalidOperationException();
            }
        }
        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            switch (state)
            {
                case 0:
                    state = 1;
                    goto case 1;
                case 1:
                    _Top = _Top.Next;
                    return _Top != null;
            }

            throw new InvalidOperationException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    IEnumerator<TMsg> IEnumerable<TMsg>.GetEnumerator()
        => new Enumerator(_TopShell);
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TMsg>)this).GetEnumerator();
    public IEnumerator<TMsg> GetEnumerator()
        => ((IEnumerable<TMsg>)this).GetEnumerator();
}
