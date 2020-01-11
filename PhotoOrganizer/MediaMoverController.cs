using System;
using System.IO;

namespace PhotoOrganizer
{
    class MediaMoverController
    {
        private readonly PathRules _pathRules;
        private readonly Func<MediaFile, string> _isAllreadyInDB;
        private readonly Func<MediaFile, bool> _saveInDb;
        public MediaMoverController(PathRules pathRules, Func<MediaFile, string> isAllreadyInDB, Func<MediaFile, bool> saveInDb)
        {
            _pathRules = pathRules;
            _isAllreadyInDB = isAllreadyInDB;
            _saveInDb = saveInDb;
        }

        private MediaFile PrepareToMove(MediaFile mediaFile)
        {
            mediaFile.ArchivedPath = _pathRules.MakePath(ref mediaFile);
            var allreadyInDbPath = _isAllreadyInDB(mediaFile);
            if (!string.IsNullOrEmpty(allreadyInDbPath))
            {
                Global.Logger.Info($"File {mediaFile.OriginalPath} allready in DB with archived path {allreadyInDbPath}!");
                if (IsArchivedExists(allreadyInDbPath))
                {
                    Global.Logger.Info($"File {mediaFile.OriginalPath} allready in {mediaFile.ArchivedPath}, nothing to do");
                    return null;
                }
                mediaFile.ArchivedPath = allreadyInDbPath;
                Global.Logger.Warn($"File {mediaFile.OriginalPath} not found in {mediaFile.ArchivedPath}, copy file");
                return mediaFile;
            }
            return mediaFile;
        }

        public string Move(MediaFile mediaFile)
        {
            var preparedMediaFile = PrepareToMove(mediaFile);
            if (preparedMediaFile == null)
            {
                return mediaFile.ArchivedPath;
            }
            try
            {
                File.Copy(mediaFile.OriginalPath, mediaFile.ArchivedPath);
                Global.Logger.Info($"File {mediaFile.OriginalPath} copy to {mediaFile.ArchivedPath} success!");
            }
            catch (Exception e)
            {
                Global.Logger.Fatal($"File {mediaFile.OriginalPath} ERROR: {e.Message}");
                return null;
            }

            var result = _saveInDb(mediaFile);
            if (result)
            {
                Global.Logger.Info($"File {mediaFile.OriginalPath} save in DB!");
            }
            else
            {
                Global.Logger.Error($"Error! File {mediaFile.OriginalPath} not saved in DB!");
            }

            return result ? mediaFile.ArchivedPath : null;
        }

        private bool IsArchivedExists(string path)
        {
            return File.Exists(path);
        }
    }
}
