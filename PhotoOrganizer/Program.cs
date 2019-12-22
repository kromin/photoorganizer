using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ExifLib;
using LiteDB;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using NDesk.Options;

namespace PhotoOrganizer
{
    public class MediaFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalPath { get; set; }
        public string ArchivedPath { get; set; }
        public string DateTimeOriginal { get; set; }
        public byte[] MD5 { get; set; }
    }
    
    class PhotoController
    {
        private static MD5 md5 = MD5.Create();

        private readonly string _path;
        private readonly FileStream _fileStream;
        private readonly JpegInfo _jpegInfo;
        private readonly byte[] _computeHash;

        public PhotoController(string path)
        {
            _path = path;
            _fileStream = File.Open(_path, System.IO.FileMode.Open);
            _computeHash = md5.ComputeHash(_fileStream);
        }

        public MediaFile MakeMediaData()
        {
            if (_path.ToLower().EndsWith(".jpg", StringComparison.CurrentCulture))
            {
                var jpegInfo = ReadExifByExifLib(_fileStream);

                var mediaFile = new MediaFile
                {
                    Name = jpegInfo.FileName,
                    OriginalPath = _path,
                    DateTimeOriginal = jpegInfo.DateTimeOriginal,
                    MD5 = _computeHash
                };
                return mediaFile;
            }

            var directories = ImageMetadataReader.ReadMetadata(_path);
        }
        private static 
        private static JpegInfo ReadExifByExifLib(FileStream _fileStream)
        {
            return ExifLib.ExifReader.ReadJpeg(_fileStream);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            using FileStream fs = File.Open("/Users/olegkromin/Pictures/Медиатека Фото.photoslibrary/Masters/2019/09/04/20190904-094011/787E503D-084C-4313-8EE3-4FB6FBE73442.jpg", System.IO.FileMode.Open);
            var jpegInfo = ExifReader.ReadJpeg(fs);
            Console.WriteLine($"{jpegInfo.DateTimeOriginal}");
            Console.WriteLine($"{jpegInfo.Model}");
            Console.WriteLine($"{jpegInfo.Software}");
            using var md5 = MD5.Create();
            var computeHash = md5.ComputeHash(fs);
            Console.WriteLine($"{BitConverter.ToString(computeHash)}");

            using var db = new LiteDatabase(@"Filename=Photo.db; Mode=Exclusive");
            // Get customer collection
            var photos = db.GetCollection<Photo>("photos");

            // Create your new customer instance
            var photo = new Photo
            {
                Name = jpegInfo.FileName,
                OriginalPath = fs.Name,
                DateTimeOriginal = jpegInfo.DateTimeOriginal,
                MD5 = computeHash
            };
            var result = photos.Find(Query.EQ("MD5", computeHash));
            if (result.Any()) {
                Console.WriteLine($"Founded!");
                return;
            }
            // Insert new customer document (Id will be auto-incremented)
            photos.Insert(photo);
            Console.WriteLine($"Added! Total { photos.Count()}");

            stopWatch.Stop();
            Console.WriteLine($"{stopWatch.Elapsed.ToString()}");

        }
    }
}
