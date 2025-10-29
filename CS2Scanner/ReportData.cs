using System.Collections.Generic;

namespace CS2Scanner
{
    public class ReportData
    {
        public string ScanTime { get; set; } = "—";
        public string SystemInfo { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string VmStatus { get; set; } = string.Empty;
        public string HardwareSpec { get; set; } = string.Empty;
        public string VpnStatus { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public string BootTime { get; set; } = string.Empty;
        public string Judgement { get; set; } = "Clean ✅";
        public IReadOnlyList<string> SuspiciousProcesses { get; set; } = new List<string>();
        public IReadOnlyList<SuspiciousFile> SuspiciousFiles { get; set; } = new List<SuspiciousFile>();
        public IReadOnlyList<string> SuspiciousRegistry { get; set; } = new List<string>();
        public IReadOnlyList<ProcessDetails> Processes { get; set; } = new List<ProcessDetails>();
    }

    public class SuspiciousFile
    {
        public string Name { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
    }

    public class ProcessDetails
    {
        public string Name { get; set; } = string.Empty;
        public int Pid { get; set; }
        public string Path { get; set; } = string.Empty;
        public double MemoryMb { get; set; }
    }
}
