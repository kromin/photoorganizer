using System;

namespace PhotoOrganizer
{
    public class MediaFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalPath { get; set; }
        public string ArchivedPath { get; set; }
        public DateTime? DateTimeOriginal { get; set; }
        public int DuplicateName { get; set; }
        public byte[] MD5 { get; set; }
    }
}
