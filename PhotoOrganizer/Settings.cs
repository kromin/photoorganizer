using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoOrganizer
{
    public interface ISettings
    {
        string DataBasePath { get; }
        string FormatOutputDirectory { get; }
        string FormatOutputFileName { get; }
        string CultureInfo { get; }
        string SupportFormat { get; }
    }
}
