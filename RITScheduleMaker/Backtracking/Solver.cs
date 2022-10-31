/*
 * file: Solver.cs
 * Description: Backtracking Solver for creating schedules
 *  
 * @author Derek Garcia
 */

using RITScheduleMaker.Utils;

namespace RITScheduleMaker.Backtracking;

/// <summary>
///     Main solver
/// </summary>
public class Solver
{
    private readonly List<Schedule> _paths;

    /// <summary>
    ///     Inits new solver
    /// </summary>
    public Solver()
    {
        _paths = new List<Schedule>();
        Debug.Log("Init new solver");
    }

    /// <summary>
    ///     Continuously adds courses to current Schedule until new unique schedule is added
    /// </summary>
    /// <param name="schedule">Schedule to add classes to</param>
    /// <returns>List of unique schedules</returns>
    public List<Schedule> Solve(Schedule schedule)
    {
        // Add to path if goal and is unique
        if (schedule.IsGoal() && !_paths.Contains(schedule))
        {
            Debug.Log("Solution Found!: ", 0, schedule);
            _paths.Add(schedule);
        }

        // Solve each valid successor schedule
        foreach (var child in schedule.GetSuccessors().Where(child => child.IsValid())) Solve(child);

        return _paths;
    }
}