namespace MassTransit
{
    public interface INewIdFormatter
    {
        string Format(byte[] bytes);
    }
}