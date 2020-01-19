using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Brain2CPU.ExifTool;

namespace PhotoOrganizer
{
    public static class ExtendExifToolWrapper
    {
        public static DateTime? GetTimeByTag(this ExifToolWrapper @this, string tag, string path)
        {
            if (!File.Exists(path))
                return null;

            var cmdRes = @this.SendCommand($"-{tag}\n-s3\n{path}");
            if (!cmdRes)
                return null;

            if (DateTime.TryParseExact(cmdRes.Result,
                "yyyy.MM.dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out DateTime dt))
                return dt;

            return null;
        }

        public static DateTime? GetActualDateTime(this ExifToolWrapper @this, string path) 
        {
            DateTime? dateTime = @this.GetCreationTime(path);
            if (dateTime.HasValue) { return dateTime; }
            dateTime = @this.GetTimeByTag("CreateDate", path) ?? File.GetLastWriteTime(path);
            return dateTime;
        }
    }
    
    class PhotoController
    {
        private static readonly MD5 md5 = MD5.Create();
        private static readonly ExifToolWrapper exif = new ExifToolWrapper();
        private readonly string _path;

        public PhotoController(string path)
        {
            _path = path;
        }
        public MediaFile MakeMediaData()
        {
            CheckExifStatusAndRestartIfNeed();

            DateTime? dateTime = exif.GetActualDateTime(_path);
            
            byte[] computeHash;
            using (var fs = File.Open(_path, FileMode.Open))
            {
                computeHash = md5.ComputeHash(fs);
            }
            var fileName = Path.GetFileName(_path);
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

        private static void CheckExifStatusAndRestartIfNeed() 
        {
            if (exif.Status != ExifToolWrapper.ExeStatus.Ready) 
            {
                Global.Logger.Info($"ExifTool not ready, status: {exif.Status}");
                Global.Logger.Info($"Try restart ExifTool...");
                exif.Start();
                Global.Logger.Info($"ExifTool status, after : {exif.Status}");
            }
        }
    }
}
