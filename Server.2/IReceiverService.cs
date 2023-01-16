namespace Server._2
{
    public interface IReceiverService
    {
        Task<HttpDataResponse> Receive(HttpDataMessage message);
    }
}