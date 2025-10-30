using System.Collections.Generic;

namespace ChekScanner;

public sealed class SystemScanResult
{
    public string SystemInfo { get; set; } = "—";
    public string ScanDuration { get; set; } = "—";
    public string MachineName { get; set; } = "—";
    public string VirtualizationStatus { get; set; } = "—";
    public string HardwareSummary { get; set; } = "—";
    public string VpnStatus { get; set; } = "—";
    public int LocalUserCount { get; set; }
    public string Uptime { get; set; } = "—";
    public string Judgement { get; set; } = "—";

    public List<string> SuspiciousProcesses { get; } = new();
    public List<SuspiciousFile> SuspiciousFiles { get; } = new();
    public List<string> SuspiciousRegistryKeys { get; } = new();
    public List<ProcessInfo> Processes { get; } = new();
    public List<NvidiaDrsEntry> NvidiaDrsEntries { get; } = new();
}

public sealed record SuspiciousFile(string Path, string Size);

public sealed record ProcessInfo(string Name, int Id, string Path, double MemoryMegabytes);

public sealed record NvidiaDrsEntry(string Profile, string Value);
