/*
 * file: WebClient.cs
 * Description: Handles all RIT API interactions
 * 
 * @author Derek Garcia
 */

using Newtonsoft.Json;
using RITScheduleMaker.ScheduleMaker;
using RITScheduleMaker.Utils;

namespace RITScheduleMaker.API;

/// <summary>
///     Main class that uses RIT API to request information
/// </summary>
public class WebClient
{
    private const string ApiAddress = "https://api.rit.edu/v1";
    private const string CourseAddress = $"{ApiAddress}/course";
    private const string RoomAddress = $"{ApiAddress}/rooms";
    private const string FacultyAddress = $"{ApiAddress}/faculty";
    private const int MaxNullCount = 10; // used to prevent stopping at TBA classes
    private readonly HttpClient _client = new();

    /// <summary>
    ///     Inits new webclient with a given RITAuthorization Key
    /// </summary>
    /// <param name="ritAuthorization">key to use API</param>
    public WebClient(string ritAuthorization)
    {
        _client.DefaultRequestHeaders.Add("RITAuthorization", ritAuthorization);
        Debug.Log($"Attempting to Validate RITAuthorization Key '{ritAuthorization}'");
        IsValid = _client.GetAsync(RoomAddress).Result.IsSuccessStatusCode;
    }

    public bool IsValid { get; }

    /// <summary>
    ///     Gets all information about a given course and its sections
    /// </summary>
    /// <param name="course">course to query</param>
    /// <param name="startTerm">start date of the term/semester</param>
    /// <param name="endTerm">end date of the term/semester</param>
    /// <returns>Course object</returns>
    public async Task<Course?> GetCourse(string course, DateOnly startTerm, DateOnly endTerm)
    {
        try
        {
            // init course object
            var courseObj = new Course(course, new List<Section>());
            Debug.Log($"Attempting to Request Info About {course}");

            // Keep attempting to until reach failure point
            var nullCount = 0;
            var section = 1;
            while (nullCount < MaxNullCount)
            {
                // Build request url
                var requestUri = $"{CourseAddress}/{course}-0{section++}";

                // request and convert data
                var jsonSection = JsonConvert.DeserializeObject<JsonSection>(await Request(requestUri));

                // null check
                if (jsonSection == null)
                {
                    Debug.Warn("Bad Convert, skipping");
                    continue;
                }

                // Increment if no meetings, ie doesn't exist or no classes assigned yet
                if (jsonSection.Meetings is {Count: 0})
                {
                    Debug.Log($"{requestUri} returned with no courses, skipping");
                    nullCount++;
                    continue;
                }

                nullCount = 0; // reset null count when successful class found

                // Make a new Section and add it to the course
                var newSection = new Section(courseObj, jsonSection, startTerm, endTerm);
                if (newSection.Meetings.Count == 0)
                {
                    Debug.Log($"{newSection} has no meetings between {startTerm} and {endTerm}");
                }
                else
                {
                    // Convert RoomID to Human Readable room
                    foreach (var meeting in newSection.Meetings)
                    {
                        var room = JsonConvert.DeserializeObject<JsonRoom>(
                            await Request($"{RoomAddress}/{meeting.Location}"));
                        // null check
                        if (room == null)
                        {
                            Debug.Warn("Bad Convert, skipping");
                            continue;
                        }

                        // Assign room
                        if (room.Data != null)
                            meeting.Location = (room.Data != null ? room.Data[0].Name : "N / A")!;
                    }

                    // Get instructor info
                    if (jsonSection.Instructors != null)
                        foreach (var instructorId in jsonSection.Instructors)
                        {
                            // get data and fix 'int as key' issue
                            var response =
                                (await Request($"{FacultyAddress}/{instructorId}")).Replace("\"0\"", "\"content\"");
                            var data = JsonConvert.DeserializeObject<JsonInstructorInfo>(response);
                            // null check
                            if (data?.Data == null)
                            {
                                Debug.Warn("Bad Convert, skipping");
                                continue;
                            }

                            // add info
                            newSection.Instructors.Add(data.Data);
                        }

                    // add and report success
                    courseObj.Sections.Add(newSection);
                    Debug.Log($"{jsonSection.Section} Has Been Added to {course}: {jsonSection.Name}");
                }
            }

            // return all section info as courseObject
            return courseObj;
        }
        // Catch any errors
        catch (Exception e)
        {
            Debug.Error("Request Failed", e);
        }

        // null return
        return null;
    }

    /// <summary>
    ///     Request API for info
    /// </summary>
    /// <param name="requestUri">API request url</param>
    /// <returns>json string of return value</returns>
    private async Task<string> Request(string requestUri)
    {
        Debug.Log($"Request URI: {requestUri}");

        // attempt to make request
        try
        {
            var response = await _client.GetStringAsync(requestUri);

            // null check
            if (response.Equals(null))
            {
                Debug.Warn("Response Was Null");
                return "";
            }

            // return result
            Debug.Log("Response Was Successful");
            return response;
        }
        catch (Exception e)
        {
            Debug.Error("Request Failed", e);
            return "";
        }
    }
}