/*
 * file: Tokenizer.cs
 * Description: Handles tokenizing command line arguments into config objects
 *
 * @author Derek Garcia
 */

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RITScheduleMaker.Utils;

/// <summary>
///     Token Object of command line args
/// </summary>
public class Token
{
    public string Command { set; get; } = "";
    public string Key { set; get; } = "";
    public string ConfigPath { set; get; } = "";
    public string StartDate { set; get; } = "";
    public string EndDate { set; get; } = "";
    public string StartTime { set; get; } = "";
    public string EndTime { set; get; } = "";
    public List<string>? Courses { set; get; }
    public string? Format { set; get; }
    public string? Output { set; get; }


    /// <summary>
    ///     Converts current token to config file
    /// </summary>
    /// <returns>new config object</returns>
    public Config ToConfig()
    {
        var config = new Config();

        // Attempt use given config path
        if (ConfigPath != "")
        {
            try
            {
                using var sr = new StreamReader(ConfigPath);
                var yamlString = sr.ReadToEnd();
                var d = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                config = d.Deserialize<Config>(yamlString);
                if (config.Courses == null) throw new Exception("Config must have courses");
                // Update values
                for (var i = 0; i < config.Courses.Count; i++)
                    config.Courses[i] = config.Courses[i].ToUpper().Replace(" ", "-");
            }
            // Catch any parsing errors
            catch (Exception e)
            {
                Debug.Abort(true, "Parsing Failed", e);
                return config; // config ignored
            }

            Debug.Log("configFile loaded successfully");
        }
        // Else convert token to config
        else
        {
            config = new Config
            {
                Name = "cli",
                Courses = Courses!.ToList(),
                Format = Format,
                Output = Output
            };
            // attempt convert start / end dates
            try
            {
                config.StartDate = DateOnly.Parse(StartDate);
                config.EndDate = DateOnly.Parse(EndDate);
            }
            catch (Exception e)
            {
                Debug.Abort(true, "Unable to Parse Dates", e);
            }

            // check dates ok
            if (config.StartDate >= config.EndDate)
                Debug.Warn(
                    $"Start Date ({config.StartDate}) occurs after End Date ({config.EndDate}); This may cause issues!");


            // add optional start / end times
            if (StartTime == "" && EndTime == "") return config;

            config.Rules = new Rules(); // init rules
            // Add start time rule
            try
            {
                if (StartTime != "")
                    config.Rules.NoClassBefore = TimeOnly.Parse(StartTime);
            }
            catch (Exception e)
            {
                Debug.Error("Unable to Parse Start Time; Excluding Rule", e);
            }

            // Add end time rule
            try
            {
                if (EndTime != "")
                    config.Rules.NoClassAfter = TimeOnly.Parse(EndTime);
            }
            catch (Exception e)
            {
                Debug.Error("Unable to Parse End Time; Excluding Rule", e);
            }
        }

        return config;
    }
}

/// <summary>
///     Takes in command line arguments and creates token object
/// </summary>
public static class Tokenizer
{
    // All available flags
    private const string GenConfig = "genconfig";
    private const string Walk = "walk";
    private const string KeyFlagShort = "-k";
    private const string KeyFlagFull = "--key";
    private const string ConfigFileFlagShort = "-cf";
    private const string ConfigFileFlagFull = "--configFile";
    private const string StartDateFlagShort = "-sd";
    private const string StartDateFlagFull = "--startDate";
    private const string EndDateFlagShort = "-ed";
    private const string EndDateFlagFull = "--endDate";
    private const string StartTimeFlagShort = "-st";
    private const string StartTimeFlagFull = "--startTime";
    private const string EndTimeFlagShort = "-et";
    private const string EndTimeFlagFull = "--endTime";
    private const string CoursesFlagShort = "-c";
    private const string CoursesFlagFull = "--courses";
    private const string DebugFlagShort = "-d";
    private const string DebugFlagFull = "--debug";
    private const string FormatFlagShort = "-f";
    private const string FormatFlagFull = "--format";
    private const string OutputFlagShort = "-o";
    private const string OutputFlagFull = "--output";
    private const string SilentFlagShort = "-s";
    private const string SilentFlagFull = "--silent";


    /// <summary>
    ///     Tokenizes command line arguments
    /// </summary>
    /// <param name="args">command line arguments</param>
    /// <returns>token object</returns>
    public static Token Tokenize(string[] args)
    {
        var token = new Token();
        var i = 0;
        // Parse all commandline args
        while (i < args.Length)
        {
            Debug.Log($"Tokenizing '{args[i]}'");
            switch (args[i])
            {
                case GenConfig:
                case Walk:
                    token.Command = args[i];
                    break;

                case KeyFlagShort:
                case KeyFlagFull:
                    token.Key = args[++i];
                    break;

                case ConfigFileFlagShort:
                case ConfigFileFlagFull:
                    token.ConfigPath = args[++i];
                    break;

                case StartDateFlagShort:
                case StartDateFlagFull:
                    token.StartDate = args[++i];
                    break;
                case EndDateFlagShort:
                case EndDateFlagFull:
                    token.EndDate = args[++i];
                    break;

                case StartTimeFlagShort:
                case StartTimeFlagFull:
                    token.StartTime = args[++i];
                    break;

                case EndTimeFlagShort:
                case EndTimeFlagFull:
                    token.EndTime = args[++i];
                    break;

                case CoursesFlagShort:
                case CoursesFlagFull:
                    token.Courses = new List<string>();
                    i++;
                    while (i < args.Length && args[i][0] != '-')
                        // add in correct format
                        token.Courses.Add(args[i++].ToUpper().Replace(" ", "-"));

                    i--;
                    break;

                case DebugFlagShort:
                case DebugFlagFull:
                    break;

                case SilentFlagShort:
                case SilentFlagFull:
                    break;

                case FormatFlagShort:
                case FormatFlagFull:
                    token.Format = args[++i];
                    break;

                case OutputFlagShort:
                case OutputFlagFull:
                    token.Output = args[++i];
                    break;

                default:
                    Debug.Abort(true, $"Flag '{args[i]}' is unknown");
                    return token; // return is ignored
            }

            i++;
        }

        Debug.Log("Tokenization Successful");
        return token;
    }


    /// <summary>
    ///     Validate tokenization
    /// </summary>
    /// <param name="token">token to validate</param>
    /// <returns>true if valid, false otherwise</returns>
    public static bool IsValid(Token token)
    {
        // test all required args are given
        switch (token.Command)
        {
            case null:
                Debug.Warn("No Command Given");
                return false;
            case Walk when token.ConfigPath != "":
                return true;
            case Walk when token.Key == "":
                Debug.Warn("No Key Argument Given");
                return false;
            case Walk when token.StartDate == "":
                Debug.Warn("No StartDate Argument Given");
                return false;
            case Walk when token.EndDate == "":
                Debug.Warn("No EndDate Argument Given");
                return false;
            case Walk when token.Courses == null || token.Courses.Count == 0:
                Debug.Warn("No Courses Argument Given");
                return false;
            default:
                Debug.Log("Token is valid");
                return true;
        }
    }
}