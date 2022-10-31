/*
 * file: ScheduleMaker.cs
 * Description: Handles making potential schedules 
 * 
 * @author Derek Garcia
 */

using RITScheduleMaker.API;
using RITScheduleMaker.Backtracking;
using RITScheduleMaker.Utils;
using Buffer = RITScheduleMaker.Utils.Buffer;

namespace RITScheduleMaker.ScheduleMaker;

/// <summary>
///     Makes Schedules given a series of courses
/// </summary>
public class ScheduleMaker
{
    private readonly Config _config;
    private readonly WebClient _webClient;

    /// <summary>
    ///     Schedule Maker that requests information about the given courses, converts the data into usable objects,
    ///     then creates potential schedules
    /// </summary>
    /// <param name="webClient">webclient for API requests</param>
    /// <param name="config">config file for reference</param>
    public ScheduleMaker(WebClient webClient, Config config)
    {
        _webClient = webClient;
        _config = config;
    }

    /// <summary>
    ///     Generate all possible schedules from the given config file
    /// </summary>
    /// <returns>A List of All Possible Schedules</returns>
    public async Task<List<Course>> GetCourses()
    {
        var courses = new List<Course>();
        if (_config.Courses != null)
        {
            Debug.Log($"Attempting to Retrieve Info for [{string.Join(",", _config.Courses).Replace(",", ", ")}]");

            // Request course info from client for all given courses
            foreach (var courseName in _config.Courses)
            {
                var course = await _webClient.GetCourse(
                    courseName,
                    _config.StartDate,
                    _config.EndDate
                );

                // Warn if Course has no sections between target dates
                if (course != null && course.Sections.Count == 0)
                {
                    Debug.Warn($"{course.Name} has no sections between {_config.StartDate} and {_config.EndDate}");
                    var cont = Debug.Continue($"Continue without {course.Name}");

                    // Abort if not want to continue
                    if (!cont) Debug.Abort(false);

                    // Else omit and continue
                    Console.WriteLine("Omitting from schedule . . .");
                }
                else if (course != null)
                {
                    courses.Add(course);
                    Debug.Log($"{course.Name} Was Successfully Added");
                }
            }
        }

        // Else move to next step
        Debug.Log("All Course Information Has Been Queried");
        return courses;
    }


    /// <summary>
    ///     Second part of generating Schedules. Applies rules and buffers before starting the
    ///     backtracking algorithm to make schedules
    /// </summary>
    /// <param name="courses">list of courses to make schedules from</param>
    /// <returns>List of valid Schedules</returns>
    public List<Schedule> MakeSchedules(List<Course> courses)
    {
        Debug.Log("Preparing Backtracking Algorithm");

        // Apply rules if they exist
        if (_config.Rules != null)
            courses = ApplyRules(courses);

        // Check if continue if rules eliminated courses
        if (courses.Exists(c => c.Sections.Count == 0))
            if (!Debug.Continue("Some Courses Don't Have Any Sections left; Proceed Anyway?"))
                Debug.Abort(false); // abort if not

        courses.RemoveAll(c => c.Sections.Count == 0); // Purge all empty courses
        // Abort if no classes left to make schedules from
        if (courses.Count == 0)
            Debug.Abort(true, "No Remaining Courses to Make Schedule With");

        courses.AddRange(GetBuffers()); // apply buffers

        // Apply layover time to all meetings
        if (_config.Rules?.Layover != null)
            courses.ForEach(c =>
                c.Sections.ForEach(s => s.Meetings.ForEach(m => m.Layover = (int) _config.Rules.Layover)));

        courses = courses.OrderBy(c => c.Sections.Count).ToList(); // initial sorting in increase speed

        // Append all sections
        var sections = new List<Section>();
        var numSchedules = 1;

        foreach (var course in courses)
        {
            sections.AddRange(course.Sections);
            numSchedules *= course.Sections.Count;
        }

        Debug.Log($"Running Backtracking Algorithm. Potential Schedules: {numSchedules}");
        Debug.Info($"Number of Potential Schedules: {numSchedules}");
        // Use solver to find schedules
        var schedules = new Solver().Solve(
            new Schedule(courses.Count, new List<Section>(sections)
            ));
        Debug.Info($"Number of Valid Schedules: {schedules.Count}");
        return schedules;
    }

    /// <summary>
    ///     Applies a given rule to the given course
    /// </summary>
    /// <param name="course">course to apply rule to</param>
    /// <param name="rule">'rule' to apply, ie Section to remove from course if matches</param>
    /// <param name="ruleMsg">rule details</param>
    private void ApplyRule(Course course, Section rule, string ruleMsg)
    {
        // Remove sections that match rule
        course.Sections.RemoveAll(s => s.ConflictsWith(rule));
        // Warn that a course will be removed
        if (course.Sections.Count == 0)
            Debug.Warn($"All {course.Name} Sections Violate Rule: {ruleMsg}");
    }


    /// <summary>
    ///     Checks the config file and applies all rules
    /// </summary>
    /// <param name="courses">course list to apply rules to</param>
    /// <returns>updated course list</returns>
    private List<Course> ApplyRules(List<Course> courses)
    {
        // init rule variables
        Section? noClassOn = null;
        Section? noClassBefore = null;
        Section? noClassAfter = null;

        if (_config.Rules == null) return courses; // null check

        // Make new noClassOn Rule
        if (_config.Rules.NoClassOn != null)
            noClassOn = new Section(new Course("NoClassOn"), new Buffer
            {
                Name = "",
                Days = _config.Rules.NoClassOn,
                StartTime = TimeOnly.MinValue,
                EndTime = TimeOnly.MaxValue
            });

        // Make new noClassBefore Rule
        if (_config.Rules.NoClassBefore != null)
            noClassBefore = new Section(new Course("NoClassBefore"), new Buffer
            {
                Name = "",
                Days = new List<string> {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"},
                StartTime = TimeOnly.MinValue,
                EndTime = (TimeOnly) _config.Rules.NoClassBefore
            });

        // Make new noClassAfter Rule
        if (_config.Rules.NoClassAfter != null)
            noClassAfter = new Section(new Course("NoClassAfter"), new Buffer
            {
                Name = "",
                Days = new List<string> {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"},
                StartTime = (TimeOnly) _config.Rules.NoClassAfter,
                EndTime = TimeOnly.MaxValue
            });

        // temp storage arrays
        var remove = new List<Section>();

        // Apply new rules if exist
        foreach (var course in courses)
        {
            // No Classes On Rule
            if (noClassOn != null && _config.Rules.NoClassOn != null)
                ApplyRule(course, noClassOn, $"No Class on {string.Join(", ", _config.Rules.NoClassOn)}");
            // No Classes Before Rule
            if (noClassBefore != null && _config.Rules.NoClassBefore != null)
                ApplyRule(course, noClassBefore, $"No Class before {_config.Rules.NoClassBefore}");
            // No Classes After Rule
            if (noClassAfter != null && _config.Rules.NoClassAfter != null)
                ApplyRule(course, noClassAfter, $"No Class before {_config.Rules.NoClassAfter}");
            // No Online Classes Rule
            if (_config.Rules.AllowOnline == null) continue;
            remove.AddRange(course.Sections.Where(section => section.Meetings.Exists(m => m.Location == "Online")));
            course.Sections.RemoveAll(s => remove.Contains(s));
            if (course.Sections.Count == 0)
                Debug.Warn($"All {course.Name} Sections Violate Rule: No Online Classes");
        }

        return courses; // return updated courses
    }


    /// <summary>
    ///     Converts buffers from config file to special 'buffer' courses
    /// </summary>
    /// <returns>List of all new Buffer Courses</returns>
    private IEnumerable<Course> GetBuffers()
    {
        // init vars
        var buffers = new List<Course>();
        if (_config.Buffers == null) return buffers; // ensure !null

        // Create a new buffer course and add it to the list
        foreach (var buffer in _config.Buffers)
        {
            var bufName = buffer.Name ?? "buffer";
            var bufCourse = new Course(bufName, new List<Section>());
            bufCourse.Sections.Add(new Section(bufCourse, buffer));
            buffers.Add(bufCourse);
        }

        return buffers;
    }
}