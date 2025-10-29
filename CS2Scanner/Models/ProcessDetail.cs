namespace CS2Scanner.Models;

internal sealed class ProcessDetail
{
    public string Name { get; init; } = string.Empty;
    public int Id { get; init; }
    public string Path { get; init; } = string.Empty;
    public double MemoryMb { get; init; }
}
