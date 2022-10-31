/*
 * file: Meeting.cs
 * Description: Lowest organization class, contains many of the actual course details
 *
 * @author Derek Garcia
 */

using System.Text.Json.Nodes;

namespace RITScheduleMaker.ScheduleMaker;

/// <summary>
///     Used to get Meeting info from API
/// </summary>
public class JsonMeeting
{
    public DateOnly? Date;
    public string? Day;
    public TimeOnly? End;
    public string? MeetingType;
    public string? Room_id;
    public TimeOnly? Start;
}

/// <summary>
///     Meeting object for Sections
/// </summary>
public class Meeting
{
    // Enums for Days of Week
    public enum Day
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }

    private readonly TimeOnly _end;
    private readonly Section _parent;
    public readonly Day MeetingDay;


    /// <summary>
    ///     Create a new Meeting for a Section
    /// </summary>
    /// <param name="parent">Parent / Course for this Section</param>
    /// <param name="meetingDay">Day of Week Class meets</param>
    /// <param name="location">Room for this meeting</param>
    /// <param name="start">Starting time</param>
    /// <param name="end">Ending time</param>
    public Meeting(Section parent, string meetingDay, TimeOnly start, TimeOnly end, string location = "")
    {
        _parent = parent;

        // convert string to enum
        MeetingDay = meetingDay.ToLower() switch
        {
            "sunday" => Day.Sunday,
            "monday" => Day.Monday,
            "tuesday" => Day.Tuesday,
            "wednesday" => Day.Wednesday,
            "thursday" => Day.Thursday,
            "friday" => Day.Friday,
            "saturday" => Day.Saturday,
            _ => MeetingDay
        };
        Start = start;
        _end = end;

        Location = location;
        Layover = 0; // default 0 layover, used in backtracking algorithm
    }

    public string Location { get; set; }
    public TimeOnly Start { get; }
    public int Layover { set; get; }


    /// <summary>
    ///     Check if this meeting conflicts with another meeting
    /// </summary>
    /// <param name="other">other meeting to compare</param>
    /// <returns>true if conflicts, false otherwise</returns>
    public bool ConflictsWith(Meeting other)
    {
        // Can't conflict if not on same day
        if (MeetingDay != other.MeetingDay) return false;

        // If are on same day, check if times overlap
        return !(Start > other._end.AddMinutes(other.Layover) || other.Start > _end.AddMinutes(Layover));
    }

    /// <summary>
    ///     Convert Meeting object to json version
    /// </summary>
    /// <returns>JsonMeeting object</returns>
    public JsonObject ToJsonObj()
    {
        return new JsonObject
        {
            {"day", Enum.GetName(MeetingDay)},
            {"room", Location},
            {"start", Start.ToString()},
            {"end", _end.ToString()}
        };
    }

    /// <summary>
    ///     Returns Meeting details in 'basic' toString form
    /// </summary>
    /// <param name="tabShift">number of tabs to append</param>
    /// <returns>'basic' toString</returns>
    public string GetBasic(int tabShift = 0)
    {
        var tab = GetTabShift(tabShift);
        var start = Start.ToString();
        var end = _end.ToString();
        if (Start.ToString().Length < 8)
            start = " " + start;
        if (end.Length < 8)
            end = " " + end;
        // ex. 10:00 AM - 12:15 PM | CSCI-243
        return $"{tab}{start} - {end} | {_parent}";
    }

    /// <summary>
    ///     Returns Meeting details in 'full' toString form
    /// </summary>
    /// <param name="tabShift">number of tabs to append</param>
    /// <returns>'full' toString</returns>
    public string GetFull(int tabShift = 0)
    {
        var tab = GetTabShift(tabShift);
        var full = $"{tab}{_parent.ParentCourse} | {_parent.SectionName}\n{tab}Room: {Location}\n";
        // append all instructor data
        full = _parent.Instructors.Aggregate(full,
            (current, inst) =>
                current + $"{tab}{inst.Displayname} | {inst.Mail} | {inst.Physicaldeliveryofficename}\n");
        full += $"{tab}Start: {Start}\n{tab}End: {_end}";
        /* ex.
         * CSCI-243 | Mechanics of Programming
         * Room: GOL-1400
         * Foo Bar | foobar@email.com | GOL-3123
         * Start: 10:00 AM
         * End: 12:15 PM
         */
        return full;
    }

    /// <summary>
    ///     Pretty Prints Meeting
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        return $"{_parent} | {Location} | {MeetingDay}: {Start} - {_end}";
    }


    /// <summary>
    ///     Custom Hashcode for parsing
    /// </summary>
    /// <returns>meeting hashcode</returns>
    public override int GetHashCode()
    {
        return $"{MeetingDay}{Location}{Start}{_end}".GetHashCode(); // for comparision purposes
    }


    /// <summary>
    ///     Custom comparing function for parsing
    /// </summary>
    /// <param name="obj">object to compare</param>
    /// <returns>true if hashcode match, false otherwise</returns>
    public override bool Equals(object? obj)
    {
        var other = (Meeting) obj!;
        return GetHashCode() == other.GetHashCode();
    }

    /// <summary>
    ///     Gets a string of tabs
    /// </summary>
    /// <param name="tabShift">Number of tabs to append</param>
    /// <returns>string of tabs</returns>
    private string GetTabShift(int tabShift)
    {
        var tab = "";
        for (var i = 0; i < tabShift; i++)
            tab += "\t";
        return tab;
    }
}