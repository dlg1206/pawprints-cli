/*
 * file: Course.cs
 * Description: Highest Container class to help with organization
 *
 * @author Derek Garcia
 */

namespace RITScheduleMaker.ScheduleMaker;

public class Course
{
    /// <summary>
    ///     Create a new Course
    /// </summary>
    /// <param name="name">Name of course</param>
    /// <param name="sections">List of all sections for this course</param>
    public Course(string name, List<Section>? sections = null)
    {
        Name = name;
        Sections = sections ?? new List<Section>();
    }

    public string Name { get; }
    public List<Section> Sections { get; }

    /// <summary>
    ///     Gets name
    /// </summary>
    /// <returns>Course name</returns>
    public override string ToString()
    {
        return Name;
    }
}