using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Server1
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

    public class SenderService : ISenderService
    {
        private static HttpClient _httpClient = new HttpClient();
        private static List<HttpDataMessage> httpDataMessages = new List<HttpDataMessage>();
        private static readonly SemaphoreLocker _locker = new SemaphoreLocker();

        public SenderService()
        {
            httpDataMessages = new List<HttpDataMessage>
            {
                new HttpDataMessage
                {
                    Data1 = "1",
                    Data2 = "2",
                },
                new HttpDataMessage
                {
                    Data1 = "3",
                    Data2 = "4",
                },
                new HttpDataMessage
                {
                    Data1 = "5",
                    Data2 = "5",
                },
                new HttpDataMessage
                {
                    Data1 = "6",
                    Data2 = "6",
                },
                  new HttpDataMessage
                {
                    Data1 = "1",
                    Data2 = "2",
                },
            };
        }

        public async Task SendDataInParallel()
        {
            await AsyncParallelForEach(httpDataMessages, async (msg) =>
            {
                string serialized = JsonConvert.SerializeObject(msg);
                var content = new StringContent(serialized, Encoding.UTF8, "application/json");

                var res = await _httpClient.PostAsync("", content);
                var responseJson = await res.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<HttpDataResponse>(responseJson);
                await _locker.LockAsync(async () =>
                {
                    httpDataMessages.Add(msg);
                });
            }, 5);
        }

        public static Task AsyncParallelForEach<T>(IEnumerable<T> source, Func<T, Task> body, 
            int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };
            if (scheduler != null)
                options.TaskScheduler = scheduler;

            var block = new ActionBlock<T>(body, options);

            foreach (var item in source)
                block.Post(item);

            block.Complete();
            return block.Completion;
        }
    }
}
