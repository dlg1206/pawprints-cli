/*
 * file: PawPrints.cs
 * Description: Main driver program that handles command line arguments
 * 
 * @author Derek Garcia
 */

using System.Diagnostics;
using RITScheduleMaker.API;
using RITScheduleMaker.Backtracking;
using RITScheduleMaker.Utils;
using Debug = RITScheduleMaker.Utils.Debug;

namespace RITScheduleMaker;

/// <summary>
///     Main driver program that handles command line arguments
/// </summary>
public static class PawPrints
{
    /// <summary>
    ///     Parses command line inputs and executes their respective commands
    ///     If the command is invalid or unrecognized, the correct usage is displayed
    /// </summary>
    /// <param name="args">command line arguments</param>
    public static void Main(string[] args)
    {
        // Check that command line args exist
        if (args.Length == 0)
        {
            PrintUsage();
            Debug.Abort(false);
        }

        // turn on debug if flags are passed
        if (args.Contains("-s") || args.Contains("--silent"))
            Debug.DbStatus = Debug.DebugStatus.Silent;


        // turn on debug if flags are passed
        if (args.Contains("-d") || args.Contains("--debug"))
        {
            if (Debug.DbStatus == Debug.DebugStatus.Silent)
                Debug.Warn("Debug Flag Overrides Silent Flag");
            Debug.DbStatus = Debug.DebugStatus.Activate;
            Debug.Log("Debug mode is ON");
        }

        // Tokenize args and validate
        var token = Tokenizer.Tokenize(args);
        if (!Tokenizer.IsValid(token))
            Debug.Abort(true, "Token Is Invalid");


        var p = new Printer();

        // Init vars
        Config? config = null;
        // Use token and walk if command
        switch (token.Command)
        {
            // Generate a config using CLI tool
            case "genconfig":
                config = ConfigurationBuilder.GenerateConfig();
                break;
            // Convert tokenized args to config
            case "walk":
                config = token.ToConfig();
                break;
            // Unknown command
            default:
                PrintWalkUsage();
                PrintGenConfigUsage();
                Debug.Abort(true, $"Command '{token.Command}' is not recognized");
                break;
        }

        // If the key is empty, can't access API
        if (token.Key == "" || config == null) return;

        // Else attempt to generate schedules
        var schedules = Walk(token.Key, config);
        p.Output(schedules, config.Format, config.Output);
    }

    /// <summary>
    ///     walk command to get course information and generate schedules
    /// </summary>
    /// <param name="key">API key to use</param>
    /// <param name="config">config to use for reference</param>
    /// <returns>A list of Schedules</returns>
    private static List<Schedule> Walk(string key, Config config)
    {
        // Create and Test Webclient
        var webClient = new WebClient(key);
        if (!webClient.IsValid)
            Debug.Abort(true, "'RITAuthorization' Key was Invalid");

        Debug.Log("'RITAuthorization' Key is Valid");

        // Start timing approx. query time
        var timer = new Stopwatch();
        timer.Start();
        var sm = new ScheduleMaker.ScheduleMaker(webClient, config); // init ScheduleMaker
        var task = sm.GetCourses(); // get course via API requests
        Debug.Wait(task, "Querying Database", 500);

        var courses = task.Result;

        // Report status
        Debug.Info($"Queried {courses.Count} Courses; Time Taken: {timer.Elapsed:m\\:ss\\.fff}");
        timer.Restart();

        // Make Schedules
        var schedules = sm.MakeSchedules(courses);

        // Report status
        timer.Stop();
        Debug.Info($"Time Taken: {timer.Elapsed:m\\:ss\\.fff}");

        return schedules;
    }

    /// <summary>
    ///     Print About Message
    /// </summary>
    private static void PrintAbout()
    {
        Console.WriteLine("    ____  ___ _       ______  ____  _____   _____________");
        Console.WriteLine("   / __ \\/   | |     / / __ \\/ __ \\/  _/ | / /_  __/ ___/");
        Console.WriteLine("  / /_/ / /| | | /| / / /_/ / /_/ // //  |/ / / /  \\__ \\ ");
        Console.WriteLine(" / ____/ ___ | |/ |/ / ____/ _, _// // /|  / / /  ___/ / ");
        Console.WriteLine("/_/   /_/  |_|__/|__/_/   /_/ |_/___/_/ |_/ /_/  /____/  v1.0.0");
        Console.WriteLine("A Command Line Tool for Generating Schedules for Rochester Institute of Technology");
        Console.WriteLine();
    }

    /// <summary>
    ///     Print proper usages of the 'walk' command
    /// </summary>
    private static void PrintWalkUsage()
    {
        Console.WriteLine("[ WALK USAGE ]");
        Console.WriteLine("walk: Walks a Path With Given Arguments to Generate Schedules");
        Console.WriteLine("dotnet run walk -k <key> -cf <configFile>");
        Console.WriteLine("\t-k  | --key        : RITAuthorization key");
        Console.WriteLine("\t-cf | --configFile : Config YAML File to Use");
        Console.WriteLine("dotnet run walk -k <key> -sd <startDate> -ed <endDate> -c <courses> <optional arguments>");
        Console.WriteLine("\t-k  | --key        : RITAuthorization key");
        Console.WriteLine("\t-sd | --startDate  : Starting Date to Search for Classes (MM/DD/YEAR)");
        Console.WriteLine("\t-ed | --endDate    : Ending Date to Search for Classes (MM/DD/YEAR)");
        Console.WriteLine("\t-c  | --courses    : Space Seperated List of Courses to Search for (COURSE-NUMBER)");
        Console.WriteLine("[ OPTIONAL ARGUMENTS ]");
        Console.WriteLine("\t-st | --startTime  : Classes Must Start After This Time (24hr)");
        Console.WriteLine("\t-et | --endTime    : Classes Must End After This Time (24hr)");
        Console.WriteLine("\t-f  | --format     : Format for Output; default = basic (basic, full, json, html)");
        Console.WriteLine("\t-o  | --output     : Path to Output File");
        Console.WriteLine("\t-d  | --debug      : Turn on Debug Mode");
        Console.WriteLine("\t-s  | --silent     : Turn on Silent Mode");
        Console.WriteLine();
    }

    /// <summary>
    ///     Print proper use of the 'genconfig' command
    /// </summary>
    private static void PrintGenConfigUsage()
    {
        Console.WriteLine("[ GENCONFIG USAGE ]");
        Console.WriteLine("genconfig: Generate a Configuration File ");
        Console.WriteLine("dotnet run genconfig <optional arguments>");
        Console.WriteLine("[ OPTIONAL ARGUMENTS ]");
        Console.WriteLine("\t-k | --key    : RITAuthorization key; Will Automatically walk With New ConfigFile");
        Console.WriteLine("\t-d | --debug  : Turn on Debug Mode");
        Console.WriteLine("\t-s | --silent : Turn on Silent Mode");
        Console.WriteLine();
    }

    /// <summary>
    ///     Prints the correct usage of the command line commands
    /// </summary>
    private static void PrintUsage()
    {
        PrintAbout();
        PrintWalkUsage();
        PrintGenConfigUsage();
    }
}