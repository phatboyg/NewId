namespace NewId
{
    public interface IWorkerIdProvider
    {
        byte[] GetWorkerId(int index);
    }
}