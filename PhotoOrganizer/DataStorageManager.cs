﻿using System;
using System.Linq;
using LiteDB;

namespace PhotoOrganizer
{
    public class IndexInfo
    {
        public int Id { get; set; }
        public string FormatOutputDirectory { get; set; }
        public DateTime DateTimeCreateIndex { get; set; }
        public DateTime DateTimeLastUpdate { get; set; }
        public int LastCount { get; set; }
    }
    class DataStorageManager
    {
        private readonly LiteDatabase db;

        public DataStorageManager(string dbFileName)
        {
            db = new LiteDatabase(@$"Filename={dbFileName}; Mode=Exclusive");
        }

        public IndexInfo ReadIndexInfo()
        {
            var result = db.GetCollection<IndexInfo>("IndexInfo");
            var settings = result.FindAll();
            if (!settings.Any())
            {
                return null;
            }
            if (settings.Count() > 1)
            {
                Global.Logger.Error($"DB has many IndexInfo count={settings.Count()}");
            }
            return settings.First();
        }

        public void InsertOrUpdate(IndexInfo info)
        {
            var result = ReadIndexInfo();
            if(result == null)
            {
                db.GetCollection<IndexInfo>("IndexInfo").Insert(info);
                Global.Logger.Info($"DB IndexInfo inserted!");
                return;
            }
            db.GetCollection<IndexInfo>("IndexInfo").Update(info);
            Global.Logger.Info($"DB IndexInfo updated!");
        }

        public ILiteCollection<MediaFile> Media
        {
            get
            {
                return db.GetCollection<MediaFile>("MediaFile");
            }
        }

        public string CheckExists(MediaFile mediaFile)
        {
            var result = Media.Find(Query.EQ("MD5", mediaFile.MD5));
            if (!result.Any()) { return null; }
            return result.Last().ArchivedPath;
        }


        public bool Save(MediaFile mediaFile)
        {
            try
            {
                Media.Insert(mediaFile);
            }
            catch (LiteException e)
            {
                Global.Logger.Fatal($"DB ERROR:{e.Message}");
                return false;
            }
            return true;
        }

    }
}
