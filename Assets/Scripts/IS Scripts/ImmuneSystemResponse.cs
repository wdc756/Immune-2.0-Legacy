public class ImmuneSystemResponse
{
    public float LevelPercent {  get; set; }
    public ResponseType Type { get; private set; }

    public enum ResponseType
    {
        MACNEUTRO,
        COMPLIMENT,
        KILLERT,
        ANTIBODIES
    }

    public void UpdateResponse(float delta)
    {
        this.LevelPercent += delta;
    }


    public ImmuneSystemResponse(ResponseType type, float level)
    {
        Type = type;
        LevelPercent = level;
    }

    public override string ToString()
    {
        return $"{Type}: {LevelPercent}";
    }
}
