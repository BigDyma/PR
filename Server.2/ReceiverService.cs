using System.Collections.Concurrent;

namespace Server._2
{
    public class SemaphoreLocker
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task LockAsync(Func<Task> worker)
        {
            await _semaphore.WaitAsync();
            try
            {
                await worker();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> LockAsync<T>(Func<Task<T>> worker)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await worker();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public class HttpDataMessage
    {
        public string Data1 { get; set; }
        public string Data2 { get; set; }
    }

    public class HttpDataResponse
    {
        public List<HttpDataMessage> httpDataMessages { get; set; }
    }

    public class ReceiverService : IReceiverService
    {
        private static ConcurrentStack<HttpDataMessage> httpDataMessages = new ConcurrentStack<HttpDataMessage>();
        private static readonly SemaphoreLocker _locker = new SemaphoreLocker();

        public ReceiverService() { }

        public Task<HttpDataResponse> Receive(HttpDataMessage message)
        {
            httpDataMessages.Push(message);
            httpDataMessages.TryPop(out var messag2);
            HttpDataResponse httpData = new HttpDataResponse
            {
                httpDataMessages = new List<HttpDataMessage>
                 {
                      message,
                      messag2
                 }
            };

            return Task.FromResult(httpData);
        }
    }
}
