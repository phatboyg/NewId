namespace NewId
{
    public interface WorkerIdProvider
    {
        byte[] GetWorkerId(int index);
    }
}