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

            context.Logs.Add(log);

            context.SaveChanges();
        }
    }
}
