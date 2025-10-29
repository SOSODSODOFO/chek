using System.Collections.Generic;

namespace CS2Scanner.Models;

internal sealed class ScanReport
{
    public string ScanTime { get; set; } = string.Empty;
    public string SystemInfo { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string VmStatus { get; set; } = string.Empty;
    public string HardwareSpec { get; set; } = string.Empty;
    public string VpnStatus { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public string BootTime { get; set; } = string.Empty;
    public string Judgement { get; set; } = string.Empty;

    public IReadOnlyList<string> SuspiciousProcesses { get; set; } = new List<string>();
    public IReadOnlyList<FileFinding> SuspiciousFiles { get; set; } = new List<FileFinding>();
    public IReadOnlyList<string> SuspiciousRegistry { get; set; } = new List<string>();
    public IReadOnlyList<ProcessDetail> Processes { get; set; } = new List<ProcessDetail>();
    public IReadOnlyList<DrsEntry> NvidiaDrs { get; set; } = new List<DrsEntry>();
}
