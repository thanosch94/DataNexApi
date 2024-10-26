using DataNex.Data;
using DataNex.Model.Enums;
using DataNex.Model.Models;

namespace DataNexApi.Services
{
    public class LogService
    {
        public static void CreateLog(string Name, LogTypeEnum logType, LogOriginEnum logOrigin, Guid? userId, ApplicationDbContext context)
        {
            var log = new Log();
            log.LogName = Name;
            log.LogType = logType;
            log.LogOrigin = logOrigin;
            log.UserAdded = userId;
            //TODO user may have more than one companies
            var companyId = context.Users.FirstOrDefault(u => u.Id == userId)?.CompanyId;
            log.CompanyId = companyId;
            context.Logs.Add(log);

            context.SaveChanges();
        }
    }
}
