/*
 * file: Schedule.cs
 * Description: Schedule class used for backtracking and storing schedule info
 *  
 * @author Derek Garcia
 */

using RITScheduleMaker.ScheduleMaker;
using RITScheduleMaker.Utils;

namespace RITScheduleMaker.Backtracking;

/// <summary>
///     Highest level of abstraction, contains section information
/// </summary>
public class Schedule
{
    private readonly int _numCourses;
    private readonly List<Section> _remainingSections;


    /// <summary>
    ///     Create initial empty schedule for backtracking
    /// </summary>
    /// <param name="numCourses">Number of total courses, used for is valid</param>
    /// <param name="nextSections">List of all remaining sections</param>
    public Schedule(int numCourses, List<Section> nextSections)
    {
        Path = new List<Section>();
        _numCourses = numCourses;
        _remainingSections = nextSections;
        Debug.Log("New Initial Path Created: ", 0, this);
    }

    /// <summary>
    ///     Private copy schedule
    ///     Copies current schedule and moves a section from remaining to
    ///     the current path. Also adds the course the section belongs to
    ///     the addedCourses
    /// </summary>
    /// <param name="other">previous config</param>
    /// <param name="next">next section</param>
    private Schedule(Schedule other, Section next)
    {
        _numCourses = other._numCourses;

        // Remove Section
        _remainingSections = new List<Section>(other._remainingSections);
        _remainingSections.Remove(next);

        // Add Section to path
        Path = new List<Section>(other.Path) {next};

        Debug.Log("New Path Created: ", 0, this);
    }

    public List<Section> Path { get; }


    /// <summary>
    ///     Get all successors for current config
    /// </summary>
    /// <returns>list of successors</returns>
    public IEnumerable<Schedule> GetSuccessors()
    {
        var successors = new LinkedList<Schedule>();

        // All sections not belonging to the current section's course are successors
        foreach (var section in _remainingSections.Where(section => !Path.Contains(section)))
            successors.AddLast(new Schedule(this, section));

        return successors;
    }


    /// <summary>
    ///     Checks whether a newly added Section conflicts
    ///     with any previous section
    /// </summary>
    /// <returns>true if valid, false otherwise</returns>
    public bool IsValid()
    {
        // Initial stage always true
        if (Path.Count <= 1) return true;

        // Check each section that isn't the lastAdded to see if there is a conflict
        var prevLastAdded = Path.ElementAt(Path.Count - 2);
        var lastAdded = Path[^1];

        // Test 1: Check if the newest added course has more sections than the previous
        var test1 = prevLastAdded.ParentCourse.Sections.Count <= lastAdded.ParentCourse.Sections.Count;
        if (!test1)
        {
            Debug.Log(
                $"Invalid: {prevLastAdded.ParentCourse.Name} ({prevLastAdded.ParentCourse.Sections.Count} Sections)" +
                $" > {lastAdded.ParentCourse.Name} ({lastAdded.ParentCourse.Sections.Count} Sections) : ",
                0,
                this);
            return false;
        }

        // Test 2: Check if the new section conflicts with any existing section
        var test2 = !Path.Where(section => !Equals(section, lastAdded))
            .Any(section => lastAdded.ConflictsWith(section));

        Debug.Log(test2 ? "Valid: " : "Invalid: Conflict Detected : ", 0, this);
        return test2;
    }


    /// <summary>
    ///     Check if goal is when no next courses remain
    /// </summary>
    /// <returns>true if valid, false otherwise</returns>
    public bool IsGoal()
    {
        return Path.Count == _numCourses;
    }

    /// <summary>
    ///     Sorts a list of meetings by Meeting Day then time
    /// </summary>
    /// <returns>Sorted list of Meetings</returns>
    public List<Meeting> Sort()
    {
        var allMeetings = new List<Meeting>();
        foreach (var section in Path) allMeetings.AddRange(section.Meetings);
        return allMeetings.OrderBy(m => m.MeetingDay).ThenBy(m => m.Start).ToList();
    }

    /// <summary>
    ///     Outlines the current path of this schedule
    /// </summary>
    /// <returns>string path</returns>
    public override string ToString()
    {
        var msg = Path.Aggregate("", (current, section) => current + $"{section} -> ");
        msg = msg[..^4];
        // course a -> course b -> ...
        return msg;
    }


    /// <summary>
    ///     Custom Hashcode for Comparing Schedules
    ///     Hashes current path
    /// </summary>
    /// <returns>Schedule hashcode</returns>
    public override int GetHashCode()
    {
        return Sort().Aggregate("", (current, meeting) => current + $"{meeting}\n").GetHashCode();
    }


    /// <summary>
    ///     Custom comparing function for Comparing Schedules
    /// </summary>
    /// <param name="obj">object to compare</param>
    /// <returns>true if hashcode match, false otherwise</returns>
    public override bool Equals(object? obj)
    {
        var other = (Schedule) obj!;

        return GetHashCode() == other.GetHashCode();
    }
}