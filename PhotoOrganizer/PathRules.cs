using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PhotoOrganizer
{
    public static class ExtendDateFormat
    {
        private static readonly string ErrorPath = "ProblemsFile";
        public static string ToString(this DateTime? time, string format, string cultureInfoName) 
        {
            if (time.HasValue) 
            {
                var cultureInfo = CultureInfo.CreateSpecificCulture(cultureInfoName);
                cultureInfo.DateTimeFormat.MonthNames = 
                  cultureInfo.DateTimeFormat.MonthNames.Select(m => cultureInfo.TextInfo.ToTitleCase(m)).ToArray();
                cultureInfo.DateTimeFormat.MonthGenitiveNames = 
                  cultureInfo.DateTimeFormat.MonthGenitiveNames.Select(m => cultureInfo.TextInfo.ToTitleCase(m)).ToArray();
                return time.Value.ToString(format, cultureInfo);
            }
            return ErrorPath;
        }
    }
    class PathRules
    {
        private readonly string _outputDirectory;
        private readonly string[] _outputFormat;
        private readonly string _outputFormatFileName;
        private readonly string _cultureInfo;
        public PathRules(string outputDirectory, ISettings settings)
        {
            _outputDirectory = outputDirectory;
            _outputFormat = settings.FormatOutputDirectory.Split('|');
            _cultureInfo = settings.CultureInfo;
            _outputFormatFileName = settings.FormatOutputFileName;
        }

        public string MakePath(ref MediaFile mediaFile)
        {
            var result = CheckAndMake(ref mediaFile);
            Global.Logger.Trace($"Formatted path {result}");

            var onlyDirectory = Path.GetDirectoryName(result);
            Directory.CreateDirectory(onlyDirectory);
            Global.Logger.Trace($"Create directory {onlyDirectory}");
            return result;
        }

        private string CheckAndMake(ref MediaFile mediaFile) 
        {
            var extensionOriginalFile = new FileInfo(mediaFile.Name).Extension;
            var makeNewFileName = $"{mediaFile.DateTimeOriginal.ToString(Path.Combine(_outputFormatFileName), _cultureInfo)}_{mediaFile.DuplicateName}{extensionOriginalFile}";
            var originalDateTime = mediaFile.DateTimeOriginal;
            var outputFormattedPath = _outputFormat.Select(item => originalDateTime.ToString(item, _cultureInfo));
            var result = Path.Combine(new string[]
            {
                _outputDirectory,
                Path.Combine(outputFormattedPath.ToArray()),
                makeNewFileName
            });
            if (File.Exists(result))
            {
                mediaFile.DuplicateName += 1;
                return CheckAndMake(ref mediaFile);
            }
            return result;
        }
    }
}
