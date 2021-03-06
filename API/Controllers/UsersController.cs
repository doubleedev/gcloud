using API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _UserRepository;

        public UsersController(IUserRepository userRepository)
        {
            _UserRepository = userRepository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _UserRepository.GetAll();
            return Ok(users);
        }

        [HttpGet("getsomething")]
        public ActionResult<string> GetSomethiong()
        {
            return "Just returning to test something";
        }
    }
}