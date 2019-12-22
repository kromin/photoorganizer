using System;
using System.Linq;
using LiteDB;

namespace PhotoOrganizer
{
    class DataStorageManager
    {
        private readonly LiteDatabase db;

        public DataStorageManager(string dbFileName)
        {
            db = new LiteDatabase(@$"Filename={dbFileName}; Mode=Exclusive");
        }

        public LiteCollection<MediaFile> Media
        {
            get
            {
                return db.GetCollection<MediaFile>("MediaFile");
            }
        }

        public bool CheckExists(MediaFile mediaFile)
        {
            return Media.Find(Query.EQ("MD5", mediaFile.MD5)).Any();
        }


        public bool Save(MediaFile mediaFile)
        {
            try
            {
                Media.Insert(mediaFile);
            }
            catch (LiteException)
            {
                return false;
            }
            return true;
        }

    }
}
