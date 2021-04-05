using API.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : Controller
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
	}
}