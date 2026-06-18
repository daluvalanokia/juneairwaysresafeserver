namespace AirwaysMergeSafeServer.Models;

public enum ZoneStatus
{
    Active,
    Inactive,
    Fault,
    Maintenance
}

public enum ServerStatus
{
    Online,
    Offline,
    Degraded,
    Fault
}

public enum SensorStatus
{
    Online,
    Offline,
    Fault,
    Calibrating,
    Maintenance
}

public enum EventType
{
    Detection,
    Merge,
    Conflict,
    Speeding,
    Fault
}

public enum UserType
{
    Admin,
    Operator,
    Viewer
}

public enum SourceType
{
    Physical,
    Satellite,
    Telecom,
    Tracker
}
