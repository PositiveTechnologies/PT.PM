namespace PT.PM.Common
{
    public interface ISerializer : ILoggable
    {
        bool LineColumnTextSpans { get; set; }
    }
}