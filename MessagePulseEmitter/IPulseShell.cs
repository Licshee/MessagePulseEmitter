public interface IPulseShell<T>
{
    T Payload { get; }
    IPulseShell<T> Next { get; }
}

