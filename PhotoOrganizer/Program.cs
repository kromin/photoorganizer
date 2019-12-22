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
using NLog;
using Config.Net;

namespace PhotoOrganizer
{
    public static class Global
    {
        public static NLog.Logger Logger { get; } = LogManager.GetCurrentClassLogger();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Global.Logger.Info($"Start photo organize at {DateTime.Now.ToString("dd.MM.yyyy HH:MM:ss")}");
            
            var dbPath = "media.db";
            string sourceDirectory = null;
            string destinationDirectory = null;
            var formatDirectory = "yyyy/MM/";
            var showHelp = false;

            var options = new OptionSet() {
                { "db=", $"Path to {{DB}} file default: {dbPath}", v => dbPath = v },
                { "s|source=","{Source} directory with media files", v => sourceDirectory = v },
                { "d|dest=", "{Destination} directory where storage media files", v => destinationDirectory = v },
                { "f|format=", $"{{Format}} output directory default {formatDirectory}", v => formatDirectory = v},
                { "h|help",  "Show this message and exit", v => showHelp = v != null },
            };
            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("Wrong args: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try '--help' for more information.");
                
                return;
            }
            if (string.IsNullOrEmpty(sourceDirectory) || string.IsNullOrEmpty(destinationDirectory))
            {
                Global.Logger.Error("Source and Dest requred!");
                showHelp = true;
            }
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                return;
            }


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Global.Logger.Info($"Parametres " +
                $"dbPath={dbPath}, " +
                $"formatDirectory={formatDirectory}, " +
                $"sourceDirectory={sourceDirectory}, " +
                $"destinationDirectory={destinationDirectory}, " +
                $"formatDirectory={formatDirectory}");

            var dataStorage = new DataStorageManager(dbPath);
            var pathRules = new PathRules(formatDirectory);
            var mediaMover = new MediaMoverController(pathRules, dataStorage.CheckExists, dataStorage.Save);
            if (!System.IO.Directory.Exists(sourceDirectory))
            {
                Global.Logger.Error($"Source directory: {sourceDirectory} not found");
                return;
            }
            var fileEntries = System.IO.Directory.EnumerateFiles(sourceDirectory);
            Global.Logger.Info($"Source direstory file count: {fileEntries.Count()}");
            foreach (string fileName in fileEntries)
            {
                var result = mediaMover.Move(new PhotoController(fileName).MakeMediaData());
                Global.Logger.Info($"{fileName} copy to {result}");
            }

            stopWatch.Stop();
            Global.Logger.Info($"Time elapsed: {stopWatch.Elapsed.ToString()}");

        }
        
    }
}
