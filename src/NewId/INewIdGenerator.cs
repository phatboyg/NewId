namespace MassTransit
{
    public interface INewIdGenerator
    {
        NewId Next();
    }
}