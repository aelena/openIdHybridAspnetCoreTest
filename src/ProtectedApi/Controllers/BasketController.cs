using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace ProtectedApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get() => Ok();
    }
}