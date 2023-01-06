namespace TeleBaseBotFW.TeleBase
{
    internal class TaskInfo
    {
        internal readonly SemaphoreSlim Semaphore = new(0);
        internal readonly Queue<UpdateInfo> Updates = new();
        internal Task? Task;
    }

    internal interface IGetNext
    { Task<UpdateInfo> NextUpdateAsync(CancellationToken cancel); }
}