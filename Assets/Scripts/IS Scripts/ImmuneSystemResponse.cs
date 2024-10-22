public class ImmuneSystemResponse
{
    /* This class is not a unity related class, it just stores very basic information on the response
     * such as its type and its response level as a percent (0-100)
     * And a helper method or two
     * Would it be beneficial to rewrite as ScriptableObject? idk and I'm honestly too lazy to find out
     * works just fine
     * (Trent)
     */
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
