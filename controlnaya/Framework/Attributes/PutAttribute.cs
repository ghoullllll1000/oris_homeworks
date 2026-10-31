namespace Server.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class PutAttribute : Attribute
{
    public string Route { get; }

    public PutAttribute(string route)
    {
        Route = route;
    }
}
