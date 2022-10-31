/*
 * file: ConfigurationBuilder.cs
 * Description: Main class for 'genconfig' command and all supporting methods
 *
 * @author Derek Garcia
 */

using YamlDotNet.Serialization;

namespace RITScheduleMaker.Utils;

/// <summary>
///     Main class for 'genconfig' command and all supporting methods
/// </summary>
public static class ConfigurationBuilder
{
    /// <summary>
    ///     Genconfig CLI tool
    /// </summary>
    /// <returns>newly generated config</returns>
    public static Config GenerateConfig()
    {
        Debug.Log("Using Genconfig CLI");
        Console.WriteLine("[ Configuration CLI Builder ]");

        // Get initial values
        var config = new Config
        {
            Name = GetInput("Configuration Name"),
            StartDate = GetDate("Semester Start Date"),
            EndDate = GetDate("Semester End Date"),

            // Buffers and rules not set with CLI tool, set to null as placeholder
            Buffers = null,
            Rules = null
        };

        // Get all courses
        Console.WriteLine("Enter Course IDs [NAME-NUM or NAME-NUM-SEC]; Enter to Complete");
        config.Courses = new List<string>();
        string input;
        // Repeat until at least one class is added
        for (;;)
        {
            input = GetInput($"Course {config.Courses.Count}");
            if (input == "" && config.Courses.Count != 0) break; // break if no input and have one class

            // Prevent breaking if no courses
            if (input == "" && config.Courses.Count == 0)
            {
                Debug.Info("At least 1 course is required");
                continue;
            }

            config.Courses.Add(input); // else add course  
        }

        // Get output format
        input = GetInput("Schedule Output Format (basic, full, json)");
        config.Format = input == "" ? null : input;

        // Get output path
        input = GetInput("Schedule Output Path");
        config.Output = input == "" ? null : input;

        Debug.Info("Configuration has Been Generated!"); // report success

        // Continue attempting to write to file
        for (;;)
        {
            // Get filepath to save to
            var filepath = GetInput("Save Config File to");

            if (filepath == "")
            {
                filepath = config.Name == "" ? "config.yml" : $"{config.Name}-config.yml"; // default
            }
            else
            {
                // Append .yml extension if not included
                var name = filepath.Split('.');
                var n = filepath.Split('.').Length - 1;
                if (name[n] != "yml" && name[n] != "yaml")
                    filepath += ".yml";
            }

            // Confirm overwrite existing file
            if (File.Exists(filepath) && !Debug.Continue($"File '{filepath}' already exists, overwrite?"))
                continue; // get new filepath if not overwriting

            // Attempt to write to the given path
            try
            {
                Debug.Log($"Attempting to write to {filepath}");
                using var sw = new StreamWriter(filepath);
                var s = new Serializer();

                // Convert config to yamlconfig to write
                sw.Write(s.Serialize(
                    new YamlConfig
                    {
                        Name = config.Name,
                        StartDate = config.StartDate.ToShortDateString(),
                        EndDate = config.EndDate.ToShortDateString(),
                        Rules = config.Rules,
                        Courses = config.Courses,
                        Buffers = config.Buffers,
                        Format = config.Format,
                        Output = config.Output
                    }));
                Debug.Info($"Config File Written to '{filepath}'");
            }
            catch (Exception e)
            {
                Debug.Error("Failed to Write to file", e);
            }

            break;
        }

        return config;
    }

    /// <summary>
    ///     Get user input
    /// </summary>
    /// <param name="msg">prompt message to display</param>
    /// <returns>User input</returns>
    private static string GetInput(string msg)
    {
        Console.Write($"{msg}: ");
        var result = Console.ReadLine();

        if (result != null) return result; // return user input

        // Abort if null input
        Debug.Abort(false, "Abort Keypress");
        return "";
    }

    /// <summary>
    ///     Ensures that input date is valid
    /// </summary>
    /// <param name="msg">prompt message</param>
    /// <returns>DateOnly object of input date</returns>
    private static DateOnly GetDate(string msg)
    {
        var date = "";
        // Repeat until get valid date
        for (;;)
            try
            {
                date = GetInput(msg);
                DateOnly.Parse(date);
                break;
            }
            catch (Exception)
            {
                Debug.Info($"Unable to parse date '{date}'; Use MM/DD/YYYY Format ");
            }

        return DateOnly.Parse(date);
    }
}