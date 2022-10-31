/*
 * file: JsonRoomInfo.cs
 * Description: Storage for Room Info
 * 
 * @author Derek Garcia
 */

namespace RITScheduleMaker.API;

/// <summary>
///     Used for API Room requests
/// </summary>
public class JsonRoom
{
    public List<JsonRoomData>? Data;
}

/// <summary>
///     Used for API Room Data
/// </summary>
public class JsonRoomData
{
    public string? Name;
}