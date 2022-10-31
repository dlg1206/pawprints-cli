/*
 * file: Config.cs
 * Description: Contains all Config variants and all classes that a config would contain
 *
 * @author Derek Garcia
 */

namespace RITScheduleMaker.Utils;

/// <summary>
///     Yaml version of config class, used when writing to file
/// </summary>
public class YamlConfig
{
    public List<Buffer>? Buffers;
    public List<string>? Courses;
    public string? EndDate;
    public string? Format;
    public string? Name;
    public string? Output;
    public Rules? Rules;
    public string? StartDate;
}

/// <summary>
///     Config Object
///     Contains details how to generate schedules
/// </summary>
public class Config
{
    public List<Buffer>? Buffers;
    public List<string>? Courses;
    public DateOnly EndDate;
    public string? Format;
    public string? Name;
    public string? Output;
    public Rules? Rules;
    public DateOnly StartDate;
}

/// <summary>
///     Rules Object
///     List of specific rules that can be applied as a filter
/// </summary>
public class Rules
{
    public bool? AllowOnline;
    public int? Layover;
    public TimeOnly? NoClassAfter;
    public TimeOnly? NoClassBefore;
    public List<string>? NoClassOn;
}

/// <summary>
///     Buffer Object
///     Special block used to protect a section of time
/// </summary>
public class Buffer
{
    public List<string>? Days;
    public TimeOnly EndTime;
    public string? Name;
    public TimeOnly StartTime;
}