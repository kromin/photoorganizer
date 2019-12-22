using System;
using System.IO;

namespace PhotoOrganizer
{
    class PathRules
    {
        private readonly string _outputDirectory;
        private readonly string _outputFormat;

        public PathRules(string outputDirectory, string outputFormat)
        {
            _outputDirectory = outputDirectory;
            _outputFormat = outputFormat;
        }

        public string MakePath(MediaFile mediaFile)
        {
            var result = Path.Combine(new string[] { _outputDirectory, mediaFile.DateTimeOriginal.ToString(_outputFormat), mediaFile.Name });
            Global.Logger.Trace($"Formatted path {result}");

            var onlyDirectory = Path.GetDirectoryName(result);
            Directory.CreateDirectory(onlyDirectory);
            Global.Logger.Trace($"Create directory {onlyDirectory}");
            return result;
        }
    }
}
