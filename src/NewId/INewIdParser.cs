namespace MassTransit
{
    public interface INewIdParser
    {
        NewId Parse(string text);
    }
}