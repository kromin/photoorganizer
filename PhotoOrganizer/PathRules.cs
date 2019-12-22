using System;
namespace PhotoOrganizer
{
    class PathRules
    {
        private readonly string _outputFormat;

        public PathRules(string outputFormat)
        {
            _outputFormat = outputFormat;
        }

        public string MakePath(MediaFile mediaFile)
        {
            return mediaFile.DateTimeOriginal.ToString(_outputFormat);
        }
    }
}
