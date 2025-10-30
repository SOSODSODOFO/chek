using System.Collections.Generic;

namespace CS2Scanner.Scanning;

internal sealed class SystemReport
{
    public string SystemInfo { get; init; } = string.Empty;
    public string ScanDuration { get; init; } = string.Empty;
    public string MachineName { get; init; } = string.Empty;
    public string VmStatus { get; init; } = string.Empty;
    public string HardwareSpec { get; init; } = string.Empty;
    public string VpnStatus { get; init; } = string.Empty;
    public int UserCount { get; init; }
    public string BootTime { get; init; } = string.Empty;
    public string Judgement { get; init; } = string.Empty;
    public IReadOnlyList<string> SuspiciousProcesses { get; init; } = new List<string>();
    public IReadOnlyList<FileEntry> SuspiciousFiles { get; init; } = new List<FileEntry>();
    public IReadOnlyList<string> SuspiciousRegistryKeys { get; init; } = new List<string>();
    public IReadOnlyList<ProcessEntry> Processes { get; init; } = new List<ProcessEntry>();
}

internal sealed record FileEntry(string Path, string Size, bool Exists);

internal sealed record ProcessEntry(string Name, int Id, string Location, double MemoryMb);
