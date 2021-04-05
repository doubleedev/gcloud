using System.Collections.Generic;
using System.Linq;
using API.Entities;
using DataAccess.Repositories;

namespace API.BusinessLogic
{
	public class UserRepository : IUserRepository
	{
		private readonly IGeneralDataRepository _GeneralDataRepository;

		public UserRepository(IGeneralDataRepository generalDataRespository)
		{
			_GeneralDataRepository = generalDataRespository;
		}

		public IEnumerable<User> GetAll()
		{
			var users = _GeneralDataRepository.GetAll<DataAccess.Users>();

			return users.Select(c => new User
			{
				Id = c.Id,
				Name = c.Name
			});
		}
	}
}