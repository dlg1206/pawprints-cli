/*
 * file: JsonInstructorInfo.cs
 * Description: Storage for Instructor Info
 * 
 * @author Derek Garcia
 */

using System.Text.Json.Nodes;

namespace RITScheduleMaker.API;

/// <summary>
///     Used for API Instructor Data
/// </summary>
public class JsonInstructorInfo
{
    public JsonInstructorData? Data;
}

/// <summary>
///     Used for API Instructor Data
/// </summary>
public class JsonInstructorData
{
    public Detail? Displayname;
    public Detail? Division;
    public Detail? Mail;
    public Detail? Physicaldeliveryofficename;

    /// <summary>
    ///     Convert to Json Object
    /// </summary>
    /// <returns>Instructor info as JsonObject</returns>
    public JsonObject ToJsonObj()
    {
        return new JsonObject
        {
            {"name", Displayname?.ToString()},
            {"college", Division?.ToString()},
            {"email", Mail?.ToString()},
            {"office", Physicaldeliveryofficename?.ToString()}
        };
    }
}

/// <summary>
///     Used to detail field for instructor data
/// </summary>
public class Detail
{
    public string? Content;

    /// <summary>
    ///     Return string value, if one exists
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        return Content ?? "N / A";
    }
}