namespace CS2Scanner.Models;

public sealed class SystemScanResult
{
    public string SystemInfo { get; init; } = string.Empty;
    public string ScanDuration { get; init; } = string.Empty;
    public string MachineName { get; init; } = string.Empty;
    public string VmStatus { get; init; } = string.Empty;
    public string HardwareSpec { get; init; } = string.Empty;
    public string VpnStatus { get; init; } = string.Empty;
    public int UserCount { get; init; }
    public string BootTime { get; init; } = string.Empty;
    public IReadOnlyList<string> CheatProcesses { get; init; } = Array.Empty<string>();
    public IReadOnlyList<FileFinding> SuspiciousFiles { get; init; } = Array.Empty<FileFinding>();
    public IReadOnlyList<string> SuspiciousRegistryKeys { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProcessDetail> Processes { get; init; } = Array.Empty<ProcessDetail>();
    public IReadOnlyList<DrsEntry> DrsEntries { get; init; } = Array.Empty<DrsEntry>();

    public bool IsClean => !CheatProcesses.Any() && !SuspiciousFiles.Any() && !SuspiciousRegistryKeys.Any();
}

public sealed record FileFinding(string Path, string SizeDescription);

public sealed record ProcessDetail(string Name, int Pid, string ExecutablePath, double MemoryMb);

public sealed record DrsEntry(string Profile, string Value);
