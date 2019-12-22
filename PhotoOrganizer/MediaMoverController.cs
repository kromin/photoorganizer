using System;
using System.IO;

namespace PhotoOrganizer
{
    class MediaMoverController
    {
        private readonly PathRules _pathRules;
        private readonly Func<MediaFile, bool> _isAllreadyInDB;
        private readonly Func<MediaFile, bool> _saveInDb;
        public MediaMoverController(PathRules pathRules, Func<MediaFile, bool> isAllreadyInDB, Func<MediaFile, bool> saveInDb)
        {
            _pathRules = pathRules;
            _isAllreadyInDB = isAllreadyInDB;
            _saveInDb = saveInDb;
        }

        private MediaFile PrepareToMove(MediaFile mediaFile)
        {
            mediaFile.ArchivedPath = _pathRules.MakePath(mediaFile);
            if (_isAllreadyInDB(mediaFile))
            {
                if (IsArchivedExists(mediaFile.ArchivedPath))
                {
                    return null;
                }
                return mediaFile;
            }
            _isAllreadyInDB(mediaFile);
            return mediaFile;
        }

        public string Move(MediaFile mediaFile)
        {
            File.Copy(mediaFile.OriginalPath, mediaFile.ArchivedPath);
            _saveInDb(mediaFile);
            return mediaFile.ArchivedPath;
        }

        private bool IsArchivedExists(string path)
        {
            return File.Exists(path);
        }
    }
}
