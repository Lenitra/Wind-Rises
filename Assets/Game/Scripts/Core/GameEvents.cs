using UnityEngine;

namespace WindRises.Core
{
    // === FLIGHT ===
    public struct SpeedChanged
    {
        public float Speed;
        public float MaxSpeed;
    }

    public struct AltitudeChanged
    {
        public float Altitude;
    }

    public struct PositionUpdated
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    // === GAMEPLAY ===
    public struct CollectibleCollected
    {
        public string Id;
        public Vector3 Position;
        public int Points;
    }

    public struct CheckpointReached
    {
        public string Id;
        public int Index;
        public float Time;
    }

    public struct GameplayStarted
    {
        public string Id;
        public string Type;
    }

    public struct GameplayCompleted
    {
        public string Id;
        public bool Success;
        public float Time;
        public int Score;
    }

    public struct RaceFinished
    {
        public string Id;
        public float Time;
        public bool NewRecord;
    }

    // === ENVIRONMENT ===
    public struct WeatherChanged
    {
        public string Type;
        public float Intensity;
    }

    public struct TimeOfDayChanged
    {
        public float Time;
        public bool IsNight;
    }
}
