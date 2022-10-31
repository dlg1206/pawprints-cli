/*
 * file: Debug.cs
 * Description: Handles all Output messaging and input for program details
 *
 * @author Derek Garcia
 */

using RITScheduleMaker.Backtracking;

namespace RITScheduleMaker.Utils;

/// <summary>
///     Handles all debug messages
/// </summary>
public static class Debug
{
    // Available Debug Statuses
    public enum DebugStatus
    {
        InActivate,
        Activate,
        Silent
    }

    private const string InfoFlag = "INFO";
    private const string DebugFlag = "DEBUG";
    private const string WarningFlag = "WARNING";
    private const string ErrorFlag = "ERROR";
    private static bool _inWaitMode; // Used for wait

    public static DebugStatus DbStatus { get; set; } = DebugStatus.InActivate;

    /// <summary>
    ///     Print an info message
    /// </summary>
    /// <param name="msg">info message</param>
    public static void Info(string msg)
    {
        if (DbStatus == DebugStatus.Silent) return; // Don't print if silent
        Print(InfoFlag, msg);
    }

    /// <summary>
    ///     Log a debug message
    /// </summary>
    /// <param name="msg">Debug message</param>
    /// <param name="outCode">optional special printing code</param>
    /// <param name="obj">optional object argument</param>
    public static void Log(string msg, int? outCode = null, object? obj = null)
    {
        if (DbStatus != DebugStatus.Activate) return; // skip if debug not active

        // Account for special case
        switch (outCode)
        {
            // Forms path during backtracking
            // obj - Schedule
            case 0:
                if (obj == null || obj.GetType() != typeof(Schedule)) break;
                var schedule = (Schedule) obj;
                // build path
                msg += schedule.Path.Aggregate("", (current, section) => current + $"{section} -> ");
                msg = msg[..^4];
                break;
        }

        Print(DebugFlag, msg);
    }

    /// <summary>
    ///     Print a warning message
    /// </summary>
    /// <param name="msg">warning message</param>
    public static void Warn(string msg)
    {
        if (DbStatus == DebugStatus.Silent) return; // don't print on silent
        Print(WarningFlag, msg);
    }

    /// <summary>
    ///     Prints an error message
    /// </summary>
    /// <param name="msg">error message</param>
    /// <param name="e">optional exception information</param>
    public static void Error(string msg, Exception? e = null)
    {
        Print(ErrorFlag, msg);
        // print exception if given
        if (e == null) return;
        Print(ErrorFlag, $"Exception Message: {e.Message}");
    }

    /// <summary>
    ///     Aborts the program if called and exits with a 0 return value
    /// </summary>
    /// <param name="isErrorAbort">Boolean if Abort was controlled or caused by some error</param>
    /// <param name="reason">Optional reason for Abort</param>
    /// <param name="e">Optional Exception data</param>
    public static void Abort(bool isErrorAbort, string? reason = null, Exception? e = null)
    {
        if (reason != null && isErrorAbort) Error(reason, e); // Print error reason if error abort
        if (reason != null && !isErrorAbort) Print(InfoFlag, reason); // Print info if not error abort

        Print(isErrorAbort ? WarningFlag : InfoFlag, "Terminating . . .");
        Environment.Exit(0);
    }

    /// <summary>
    ///     Prompt user if want to continue or not
    /// </summary>
    /// <param name="prompt">prompt message</param>
    /// <returns>true if continue, false otherwise</returns>
    public static bool Continue(string prompt)
    {
        _inWaitMode = false;
        Console.Write($"{prompt} (y/n): ");
        var ans = Console.ReadLine();
        _inWaitMode = true;
        return ans != null && ans.ToLower()[0] == 'y';
    }

    /// <summary>
    ///     Waits on a task to display an animation until the task finished
    /// </summary>
    /// <param name="task">task to wait on</param>
    /// <param name="msg">Wait message</param>
    /// <param name="milliseconds">time to sleep</param>
    public static void Wait(Task task, string msg, int milliseconds)
    {
        if (DbStatus != DebugStatus.InActivate) return; // Only use when debug or silent mode are not in use
        Console.Write($"{DateTime.Now.ToString("u").Replace('Z', ' ')}[{InfoFlag}] {msg}");
        _inWaitMode = true;

        // Print animation while wait
        while (!task.IsCompleted)
        {
            if (!_inWaitMode) continue;
            Console.Write(" .");
            Thread.Sleep(milliseconds);
        }

        _inWaitMode = false;
        Console.Write(" Complete!\n");
    }


    /// <summary>
    ///     Actual printer method
    /// </summary>
    /// <param name="flag">Message type</param>
    /// <param name="msg">Output message</param>
    private static void Print(string flag, string msg)
    {
        if (_inWaitMode) Console.WriteLine(); // add newline if in wait mode
        var outMsg = $"{DateTime.Now.ToString("u").Replace('Z', ' ')}[{flag}] {msg}";

        if (flag is DebugFlag or InfoFlag)
            Console.WriteLine(outMsg);
        else
            Console.Error.WriteLine(outMsg);
    }
}