/*
 * file: Printer.cs
 * Description: Prints classes to stdout and files
 *
 * @author Derek Garcia
 */

using System.Text.Json.Nodes;
using RITScheduleMaker.Backtracking;
using RITScheduleMaker.ScheduleMaker;

namespace RITScheduleMaker.Utils;

/// <summary>
///     Handles all schedule output
/// </summary>
public class Printer
{
    private const string Horiz = "---------------------------------------------------------------";

    /// <summary>
    ///     Outputs schedules to standard output and to a file, if one is given
    /// </summary>
    /// <param name="schedules">list of schedules to print</param>
    /// <param name="format">output format</param>
    /// <param name="filepath">path to write file</param>
    public void Output(List<Schedule> schedules, string? format = null, string? filepath = null)
    {
        var eFormat = StringToFormat(format); // convert string to enum

        Debug.Info($"Writing Mode: {Enum.GetName(eFormat)}");

        // Output based on format
        var output = "";
        switch (eFormat)
        {
            case Format.Basic:
            case Format.Full:
                output = ToFormattedString(eFormat, schedules); // formatted string handled by toString methods
                break;
            case Format.Json:
                output = ToJsonObj(schedules).ToString();
                break;
            default:
                Debug.Abort(true, "Unrecognized output format");
                break;
        }

        // Print to stdout if no file path
        if (filepath == null)
        {
            Console.WriteLine(output);
            Debug.Info("Printing Complete!");
            return;
        }

        // Abort if don't want to overwrite existing file
        if (File.Exists(filepath) && !Debug.Continue($"File '{filepath}' already exists, overwrite?"))
            Debug.Abort(false);

        // Else attempt to write to file
        try
        {
            using var sw = new StreamWriter(filepath);
            sw.Write(output);
            Debug.Info($"Schedules Written to '{filepath}'");
        }
        catch (Exception e)
        {
            Debug.Abort(true, "Failed to Write to file", e);
        }
    }

    /// <summary>
    ///     Converts string format from input to internal enum
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    private Format StringToFormat(string? format)
    {
        if (format == null) return Format.Basic; // default to basic

        // convert
        switch (format.ToLower())
        {
            case "basic":
                return Format.Basic;

            case "full":
                return Format.Full;

            case "json":
                return Format.Json;
            default:
                Debug.Warn($"Format '{format}' was unrecognized; Defaulting to Basic");
                return Format.Basic;
        }
    }

    /// <summary>
    ///     Creates a master string to print
    /// </summary>
    /// <param name="format">text format</param>
    /// <param name="schedules">schedules to print</param>
    /// <returns></returns>
    private string ToFormattedString(Format format, List<Schedule> schedules)
    {
        // init vars
        var id = 1;
        var final = "";

        // toString each variable
        foreach (var schedule in schedules)
        {
            var output = $"[ Schedule {id++} ]\n"; // header

            var meetings = schedule.Sort(); // sort to print correctly

            // build each day of the week
            foreach (var day in Enum.GetNames(typeof(Meeting.Day)))
            {
                // get all meetings on the current day
                var today = meetings.FindAll(m => m.MeetingDay.ToString() == day);

                if (today.Count == 0) continue; // skip if no meetings

                // Format day based on given format
                output += $"{day}\n";
                output = format == Format.Basic
                    ? today.Aggregate(output,
                        (current,
                            meet) => current + $"\t{meet.GetBasic()}\n")
                    : today.Aggregate(output,
                        (current,
                            meet) => current + $"{meet.GetFull(1)}\n\n");
            }

            // append to master string
            output += $"{Horiz}\n\n";
            final += output;
        }

        return final;
    }

    /// <summary>
    ///     Convert list of schedules to a single Json object. Used for writing Json objects to file
    /// </summary>
    /// <param name="schedules">list of schedules</param>
    /// <returns>json object of all schedules</returns>
    private JsonObject ToJsonObj(List<Schedule> schedules)
    {
        // init json obj
        var json = new JsonObject
        {
            {"generated", DateTime.Now.ToLocalTime()},
            {"schedules", new JsonArray()}
        };

        // Append each schedule
        var id = 1;
        foreach (var schedule in schedules)
        {
            // init schedule obj
            var scheduleObj = new JsonObject
            {
                {"id", id++},
                {"sections", new JsonArray()}
            };

            // Convert each section to a json object and append to the current schedule obj
            foreach (var section in schedule.Path.Select(section => section.ToJsonObj()))
                scheduleObj["sections"]?.AsArray().Add(section);

            // Append final schedule obj to master json
            json["schedules"]?.AsArray().Add(scheduleObj);
        }

        return json;
    }

    private enum Format
    {
        Basic,
        Full,
        Json
    }
}