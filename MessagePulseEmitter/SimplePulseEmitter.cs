public sealed class SimplePulseEmitter<T>
    : MessagePulseEmitter<T, SimplePulseShell<T>>
{
    public bool IgnoreBreak { get; }

    public SimplePulseEmitter(bool ignoreBreak)
    {
        IgnoreBreak = ignoreBreak;
    }

    protected override SimplePulseShell<T> PickShell(T msg)
        => new SimplePulseShell<T>(msg);

    protected override void Chain(SimplePulseShell<T> shell, SimplePulseShell<T> next)
        => shell?.Chain(next);

    public void Load(T msg)
        => Load(msg, IgnoreBreak);
}
