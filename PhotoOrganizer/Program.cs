using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Config.Net;
using NDesk.Options;
using NLog;

namespace PhotoOrganizer
{
    public static class Global
    {
        public static NLog.Logger Logger { get; } = LogManager.GetCurrentClassLogger();
    }

    public interface ISettings
    {
        string DataBasePath { get; }

        string FormatOutputDirectory { get; }

        string SupportFormat { get; }
    }

    class Program
    {

        public static IEnumerable<string> GetAllFiles(string path, Func<FileInfo, bool> checkFile = null)
        {
            string mask = Path.GetFileName(path);
            if (string.IsNullOrEmpty(mask))
                mask = "*.*";
            path = Path.GetDirectoryName(path);
            string[] files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (checkFile == null || checkFile(new FileInfo(file)))
                    yield return file;
            }
        }

        static void Main(string[] args)
        {
            Global.Logger.Info($"Start photo organize at {DateTime.Now.ToString("dd.MM.yyyy HH:MM:ss")}");
            ISettings settings = new ConfigurationBuilder<ISettings>().UseAppConfig().Build();
            Global.Logger.Info($"Config: DataBasePath={settings.DataBasePath}, FormatOutputDirectory={settings.FormatOutputDirectory}, SupportFormat={settings.SupportFormat}");
            
            var dbPath = settings.DataBasePath;
            string sourceDirectory = null;
            string destinationDirectory = null;
            var formatDirectory = settings.FormatOutputDirectory;
            var supportedFormat = settings.SupportFormat.Split(";");

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
            var pathRules = new PathRules(destinationDirectory, formatDirectory);
            var mediaMover = new MediaMoverController(pathRules, dataStorage.CheckExists, dataStorage.Save);
            if (!System.IO.Directory.Exists(sourceDirectory))
            {
                Global.Logger.Error($"Source directory: {sourceDirectory} not found");
                return;
            }

            var fileEntries = GetAllFiles(sourceDirectory, (info) => supportedFormat.Contains(Path.GetExtension(info.Name).ToLower()));
            Global.Logger.Info($"Source direstory file count: {fileEntries.Count()}");
            foreach (string fileName in fileEntries)
            {
                var result = mediaMover.Move(new PhotoController(fileName).MakeMediaData());
                if (result == null)
                {
                    Global.Logger.Error($"{fileName} cannot move!");
                    continue;
                }
            }

            stopWatch.Stop();
            Global.Logger.Info($"Time elapsed: {stopWatch.Elapsed.ToString()}");

        }
        
    }
}
