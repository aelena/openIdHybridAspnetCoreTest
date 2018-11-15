namespace ProtectedApi.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route ("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BasketController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get () => Ok ();
    }
}