using UnityEngine.Networking;

public class NetMsg
{
    public const short StartCalibration = 100;
    public const short StartLeap = 101;
}

public class CalibrationMessage : MessageBase
{
    public byte[] imageData;
}

public class CalibrationResponseMessage : MessageBase
{
    public string response;
}

public class LeapMessage : MessageBase
{
    public string msg;
}