using System.Collections.Generic;
using API.Entities;

namespace API.BusinessLogic
{
	public interface IUserRepository
	{
		IEnumerable<User> GetAll();
	}
}