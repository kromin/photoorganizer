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
            Global.Logger.Info($"Config: " +
                $" DataBasePath={settings.DataBasePath}," +
                $" FormatOutputDirectory={settings.FormatOutputDirectory}," +
                $" FormatOutputFileName={settings.FormatOutputFileName}," +
                $" CultureInfo={settings.CultureInfo}," + 
                $" SupportFormat={settings.SupportFormat}");

            string sourceDirectory = null;
            string destinationDirectory = null;
            var formatDirectory = settings.FormatOutputDirectory;
            var supportedFormat = settings.SupportFormat.Split(";");

            var showHelp = false;

            var options = new OptionSet() {
                { "s|source=","{Source} directory with media files", v => sourceDirectory = v },
                { "d|dest=", "{Destination} directory where storage media files", v => destinationDirectory = v },
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

            if(!Directory.Exists(destinationDirectory))
            {
                Global.Logger.Warn($"Output directory '{destinationDirectory}' does not exsist, make him!");
                Directory.CreateDirectory(destinationDirectory);
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var dbPath =  Path.Combine(destinationDirectory, settings.DataBasePath);

            Global.Logger.Info($"Parametres " +
                $"dbPath={dbPath}, " +
                $"formatDirectory={formatDirectory}, " +
                $"sourceDirectory={sourceDirectory}, " +
                $"destinationDirectory={destinationDirectory}, " +
                $"formatDirectory={formatDirectory}");

            var dataStorage = new DataStorageManager(dbPath);

            var indexInfo = dataStorage.ReadIndexInfo();
            if (indexInfo == null)
            {
                Global.Logger.Warn($"Settings in {dbPath} not found, may be first request");
                indexInfo = new IndexInfo
                {
                    DateTimeCreateIndex = DateTime.Now,
                    FormatOutputDirectory = formatDirectory
                };
            }

            if(!indexInfo.FormatOutputDirectory.Equals(formatDirectory))
            {
                Global.Logger.Error($"Format output directory from Index('{indexInfo.FormatOutputDirectory}') not equal format from settings('{formatDirectory}')");
                return;
            }

            var pathRules = new PathRules(destinationDirectory, settings);
            var mediaMover = new MediaMoverController(pathRules, dataStorage.CheckExists, dataStorage.Save);
            if (!Directory.Exists(sourceDirectory))
            {
                Global.Logger.Error($"Source directory: {sourceDirectory} not found");
                return;
            }

            var fileEntries = GetAllFiles(sourceDirectory, (info) => supportedFormat.Contains(Path.GetExtension(info.Name).ToLower()));
            Global.Logger.Info($"Source direstory file count: {fileEntries.Count()}");
            Global.Logger.Info($"Current index file count: {indexInfo.LastCount}");
            foreach (string fileName in fileEntries)
            {
                var result = mediaMover.Move(new PhotoController(fileName).MakeMediaData());
                if (result == null)
                {
                    Global.Logger.Error($"{fileName} cannot move!");
                    continue;
                }
            }
            var indexCount = dataStorage.Media.Count();
            Global.Logger.Info($"Was added new file: { indexCount - indexInfo.LastCount}");
            var indexCountWithProblem = dataStorage.Media.Find(x => !x.DateTimeOriginal.HasValue);
            Global.Logger.Warn($"Problem file: { indexCountWithProblem }");
            indexInfo.LastCount = indexCount;
            indexInfo.DateTimeLastUpdate = DateTime.Now;
            dataStorage.InsertOrUpdate(indexInfo);
            stopWatch.Stop();
            Global.Logger.Info($"Time elapsed: {stopWatch.Elapsed.ToString()}");

        }
        
    }
}
