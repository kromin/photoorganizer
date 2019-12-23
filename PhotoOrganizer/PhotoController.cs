using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoOrganizer
{
    class PhotoController
    {
        private static readonly MD5 md5 = MD5.Create();

        private readonly string _path;

        public PhotoController(string path)
        {
            _path = path;
        }

        public MediaFile MakeMediaData()
        {
            var directory = ReadExifByMetaDataExtractor(_path);
            var fileName = Path.GetFileName(_path);
            directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTime);
            byte[] computeHash;
            using (var fs = File.Open(_path, FileMode.Open))
            {
                computeHash = md5.ComputeHash(fs);
            }

            var mediaFile = new MediaFile
            {
                Name = fileName,
                OriginalPath = _path,
                DateTimeOriginal = dateTime,
                MD5 = computeHash
            };

            Global.Logger.Trace($"File: {_path} original datetime: {dateTime.ToString()}");
            return mediaFile;
        }

        private static MetadataExtractor.Directory ReadExifByMetaDataExtractor(string path)
        {
            Global.Logger.Trace($"Extract by MetadataExtractor: {path}");
            var directories = ImageMetadataReader.ReadMetadata(path);
            var directory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if (directory == null)
                return null;

            return directory;
        }
    }
}
