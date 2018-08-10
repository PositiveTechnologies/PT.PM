using PT.PM.Common.Exceptions;
using ReactiveUI;
using System;

namespace PT.PM.PatternEditor.ViewModels
{
    public class ErrorViewModel : ReactiveObject
    {
        public Exception Exception { get; }

        public ErrorViewModel(Exception ex)
        {
            Exception = ex ?? throw new ArgumentNullException(nameof(ex));
        }

        public override string ToString()
        {
            return Exception is PMException pmException ? pmException.Message : Exception.ToString();
        }
    }
}
