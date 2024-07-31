namespace IntelliTect.Analyzer
{
    public enum AnalyzerBlock
    {
        None,
        [Description("00XX.Naming")]
        Naming,
        [Description("01XX.Formatting")]
        Formatting,
        [Description("02XX.Reliability")]
        Reliability,
        [Description("03XX.Performance")]
        Performance
    }
}
