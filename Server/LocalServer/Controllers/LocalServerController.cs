using Microsoft.AspNetCore.Mvc;

namespace LocalServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LocalServerController : ControllerBase
    {
        private readonly ILogger<LocalServerController> _logger;

        public LocalServerController(ILogger<LocalServerController> logger)
        {
            _logger = logger;
        }
    }
}
