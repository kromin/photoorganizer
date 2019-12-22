using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ExifLib;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoOrganizer
{
    class PhotoController
    {
        private static readonly MD5 md5 = MD5.Create();

        private readonly string _path;
        private readonly FileStream _fileStream;
        private readonly byte[] _computeHash;

        public PhotoController(string path)
        {
            _path = path;
            _fileStream = File.Open(_path, FileMode.Open);
            _computeHash = md5.ComputeHash(_fileStream);
        }

        public MediaFile MakeMediaData()
        {
            if (_path.ToLower().EndsWith(".jpg", StringComparison.CurrentCulture))
            {
                var jpegInfo = ReadExifByExifLib(_fileStream);
                DateTime.TryParse(jpegInfo.DateTimeOriginal, out var dateTimeExtracted);
                var mediaFileFromExifLib = new MediaFile
                {
                    Name = jpegInfo.FileName,
                    OriginalPath = _path,
                    DateTimeOriginal = dateTimeExtracted,
                    MD5 = _computeHash
                };
                return mediaFileFromExifLib;
            }

            var directory = ReadExifByMetaDataExtractor(_path);
            var fileName = directory.GetStringValue(ExifDirectoryBase.TagDocumentName);
            directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTime);
            var mediaFile = new MediaFile
            {
                Name = fileName.ToString(),
                OriginalPath = _path,
                DateTimeOriginal = dateTime,
                MD5 = _computeHash
            };
            return mediaFile;
        }

        private static MetadataExtractor.Directory ReadExifByMetaDataExtractor(string path)
        {
            var directories = ImageMetadataReader.ReadMetadata(path);
            var directory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if (directory == null)
                return null;

            return directory;
        }

        private static JpegInfo ReadExifByExifLib(FileStream _fileStream)
        {
            return ExifLib.ExifReader.ReadJpeg(_fileStream);
        }
    }
}
