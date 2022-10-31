/*
 * file: Section.cs
 * Description: Intermediate organization class, contains general course details
 *  
 * @author Derek Garcia
 */

using System.Text.Json.Nodes;
using RITScheduleMaker.API;
using Buffer = RITScheduleMaker.Utils.Buffer;

namespace RITScheduleMaker.ScheduleMaker;

/// <summary>
///     Used to get Section info from API
/// </summary>
public class JsonSection
{
    public List<string>? Instructors;
    public List<JsonMeeting>? Meetings;
    public string? Name;
    public string? Section;
}

/// <summary>
///     Section Class
/// </summary>
public class Section
{
    public readonly List<JsonInstructorData> Instructors;
    public readonly List<Meeting> Meetings;


    /// <summary>
    ///     Create a new special 'buffer' section
    /// </summary>
    /// <param name="parentCourse">Parent buffer course</param>
    /// <param name="buffer">Buffer used to make new section</param>
    public Section(Course parentCourse, Buffer buffer)
    {
        ParentCourse = parentCourse;
        Uid = buffer.Name;
        SectionName = "Buffer";
        Meetings = new List<Meeting>();
        Instructors = new List<JsonInstructorData>();
        // add buffer sections
        foreach (var day in buffer.Days!) Meetings.Add(new Meeting(this, day, buffer.StartTime, buffer.EndTime));
    }

    /// <summary>
    ///     Creates a new section of a course
    ///     Converts a JsonSection from WebClient to Section
    /// </summary>
    /// <param name="parentCourse">Parent course of this section</param>
    /// <param name="jsonSection">jsonSection to convert</param>
    /// <param name="startTerm">start date of the term/semester</param>
    /// <param name="endTerm">end date of the term/semester</param>
    public Section(Course parentCourse, JsonSection jsonSection, DateOnly startTerm, DateOnly endTerm)
    {
        ParentCourse = parentCourse;
        SectionName = "";

        // Get parent info
        Uid = jsonSection.Section!;

        // Parse meetings to find Week Schedule for the section
        Meetings = new List<Meeting>();

        // Remove all non-course Meetings and ensure within the term dates
        jsonSection.Meetings?.RemoveAll(m =>
            m.MeetingType != "Course" ||
            m.Date < startTerm ||
            m.Date > endTerm
        );

        // Sort meetings by dates
        if (jsonSection.Meetings != null)
        {
            jsonSection.Meetings = jsonSection.Meetings.OrderBy(m => m.Date).ToList();

            // Keep adding meetings until begin duplicating
            foreach (var meeting in jsonSection.Meetings.Select(
                         m => new Meeting(this, m.Day!, (TimeOnly) m.Start!, (TimeOnly) m.End!, m.Room_id!)))
            {
                if (Meetings.Contains(meeting)) break; // Break if double
                Meetings.Add(meeting); // Else add meeting
            }
        }

        // Sort meetings by Weekday
        Meetings = Meetings.OrderBy(m => m.MeetingDay).ToList();

        Instructors = new List<JsonInstructorData>();
    }

    public string SectionName { get; }
    public Course ParentCourse { get; }
    private string? Uid { get; }

    public JsonObject ToJsonObj()
    {
        // public readonly List<Meeting> Meetings;
        // private string SectionName { get; }
        // public string FullName { get; }
        // public List<JsonInstructorData> Instructors;
        var obj = new JsonObject
        {
            {"uid", Uid},
            {"sectionName", SectionName},
            {"instructors", new JsonArray()},
            {"meetings", new JsonArray()}
        };
        Instructors.ForEach(i => obj["instructors"]?.AsArray().Add(i.ToJsonObj()));
        Meetings.ForEach(m => obj["meetings"]?.AsArray().Add(m.ToJsonObj()));


        return obj;
    }


    /// <summary>
    ///     Check if this section conflicts with any other section
    /// </summary>
    /// <param name="other">other section</param>
    /// <returns>true if conflict, false otherwise</returns>
    public bool ConflictsWith(Section other)
    {
        // check if any meetings conflict
        return Meetings.Any(thisMeeting => other.Meetings.Any(thisMeeting.ConflictsWith));
    }

    /// <summary>
    ///     Writes section name toString
    /// </summary>
    /// <returns>SectionName</returns>
    public override string ToString()
    {
        return Uid ?? SectionName;
    }

    public override int GetHashCode()
    {
        return ParentCourse.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var other = (Section) obj!;
        // todo may cause issues. Prob: a -> b and b -> a both valid, but redundant
        return GetHashCode() == other.GetHashCode();
    }
}