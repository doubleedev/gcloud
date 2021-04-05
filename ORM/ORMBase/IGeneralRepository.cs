using ORMBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARGORM.Repository.ORMBase
{
    public interface IGeneralRepository
    {
        T GetSingle<T>(string field, object value) where T : IOrmGenerated;
        IEnumerable<T> GetAll<T>() where T : IOrmGenerated;
        IEnumerable<T> GetWhere<T>(string field, object value) where T : IOrmGenerated;
        IEnumerable<T> GetWhere<T>(Dictionary<string, object> parameters) where T : IOrmGenerated;
        void Update<T>(T model) where T : IOrmGenerated;
        object Insert<T>(T model) where T : IOrmGenerated;
    }
}
