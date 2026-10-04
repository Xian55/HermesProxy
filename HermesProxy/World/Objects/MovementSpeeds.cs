namespace HermesProxy.World.Objects;

/// <summary>
/// The speeds a living object's create carries after its movement block, in wire order.
/// </summary>
public struct MovementSpeeds
{
    public float Walk;
    public float Run;
    public float RunBack;
    public float Swim;
    public float SwimBack;
    public float Flight;
    public float FlightBack;
    public float TurnRate;
    public float PitchRate;

    public readonly void Write(WorldPacket data)
    {
        data.WriteFloat(Walk);
        data.WriteFloat(Run);
        data.WriteFloat(RunBack);
        data.WriteFloat(Swim);
        data.WriteFloat(SwimBack);
        data.WriteFloat(Flight);
        data.WriteFloat(FlightBack);
        data.WriteFloat(TurnRate);
        data.WriteFloat(PitchRate);
    }
}
