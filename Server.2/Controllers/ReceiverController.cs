using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Server._2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReceiverController : ControllerBase
    {
        private readonly IReceiverService receiverService;
        private readonly ILogger<ReceiverController> _logger;

        public ReceiverController(ILogger<ReceiverController> logger,
            IReceiverService receiverService)
        {
            _logger = logger;
            this.receiverService = receiverService;
        }

        [HttpPost("receive")]
        public async Task<IActionResult> PostReceive(HttpDataMessage httpDataMessage)
        {
            var obj = await receiverService.Receive(httpDataMessage);
            return Ok(obj);
        }
    }
}