using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;

namespace practice_mvc02.Repositories
{
    public class PunchCardRepository : BaseRepository
    {
        const int lessStHour = -2;
        const int addEtHour = 13;

        public PunchCardRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public PunchCardLog GetTodayPunchLog(int employeeID, WorkTimeRule thisWorkTime){
            //
            DateTime sDateTime = definePara.dtNow().Date;
            DateTime eDateTime = definePara.dtNow().Date;
            if(thisWorkTime != null){
                sDateTime = definePara.dtNow().Date + thisWorkTime.startTime;
                eDateTime = definePara.dtNow().Date + thisWorkTime.endTime;
                eDateTime = eDateTime <= sDateTime ? eDateTime.AddDays(1): eDateTime;
                sDateTime = sDateTime.AddHours(lessStHour);
                eDateTime = eDateTime.AddHours(addEtHour);
                if(definePara.dtNow() >= eDateTime){
                    sDateTime = sDateTime.AddDays(1);
                    eDateTime = eDateTime.AddDays(1);
                }else if(definePara.dtNow() < sDateTime){
                    sDateTime = sDateTime.AddDays(-1);
                    eDateTime = eDateTime.AddDays(-1);
                }
            }else{
                eDateTime = eDateTime.AddDays(1);
            }
            PunchCardLog result = null;
            var query = from a in _DbContext.punchcardlogs
                        where a.accountID == employeeID && 
                        (a.onlineTime < eDateTime && a.onlineTime >= sDateTime ||
                         a.offlineTime <= eDateTime && a.offlineTime > sDateTime ||
                         (a.logDate.Date == (sDateTime.AddHours((lessStHour*-1))).Date && 
                          a.onlineTime.Year == 1 && a.offlineTime.Year == 1)
                        )
                        select a;

            if(query.Count() > 0){
                result = query.ToList()[0];
            }
            return result;
        }

        public object GetPunchLogByIDByMonth(int employeeID, DateTime startMonth, DateTime endMonth){
            var query = from a in _DbContext.punchcardlogs
                        where a.accountID == employeeID && a.logDate >= startMonth && a.logDate <= endMonth 
                        orderby a.logDate descending
                        select new{
                            a.ID, a.logDate, a.onlineTime, a.offlineTime, a.punchStatus
                        };
            return query.ToList();
        }

        public object GetPunchLogByIDByDate(int employeeID, DateTime queryDate){
            var query = from a in _DbContext.punchcardlogs    
                        where a.accountID == employeeID && a.logDate == queryDate  
                        select new{
                            a.ID, a.logDate, a.onlineTime, a.offlineTime, a.punchStatus,
                        };
            return query.ToList();
        }

        public int AddPunchCardLog(PunchCardLog newData){
            int count = 0;
            var query = _DbContext.punchcardlogs.Where(b=>b.accountID == newData.accountID && b.logDate == newData.logDate);
            if(query.Count() > 0){
                return 1062;    //該日期已有紀錄
            }
            _DbContext.punchcardlogs.Add(newData);
            count = _DbContext.SaveChanges();
            return count;
        }

        public int UpdatePunchCard(PunchCardLog updateData){
            int count = 0;
            var query = _DbContext.punchcardlogs.Where(b=>b.accountID == updateData.accountID && 
                                                        b.logDate == updateData.logDate && b.ID != updateData.ID);
            if(query.Count() > 0){
                return 1062;    //該日期已有紀錄
            }
            PunchCardLog context = _DbContext.punchcardlogs.FirstOrDefault(b=>b.ID == updateData.ID);
            if(context != null){
                context.logDate = updateData.logDate;
                context.onlineTime = updateData.onlineTime;
                context.offlineTime = updateData.offlineTime;
                context.punchStatus = updateData.punchStatus;
                context.lastOperaAccID = updateData.lastOperaAccID;
                context.updateTime = updateData.updateTime;
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        public int DelPunchCardLog(int punchLogID){
            int count = 0;
            var context = _DbContext.punchcardlogs.FirstOrDefault(b=>b.ID == punchLogID);
            if(context != null){
                _DbContext.Remove(context);
                count = _DbContext.SaveChanges();
            }
            return count;
        }


        public int GetThisLogAccID(int logID){
            int result = 0;
            var context = _DbContext.punchcardlogs.FirstOrDefault(b=>b.ID == logID);
            if(context != null){
                result = context.accountID;
            }
            return result;
        }

        public WorkTimeRule GetThisWorkTime(int employeeID){
            var query = (from a in _DbContext.accounts
                        join b in _DbContext.worktimerules on a.timeRuleID equals b.ID
                        where a.ID == employeeID
                        select b
                        ).FirstOrDefault();
            return query;
        }

        public void AddPunchLogWarnAndMessage(PunchCardLog log){
            var context = _DbContext.punchlogwarns.FirstOrDefault(b=>b.punchLogID == log.ID);
            if(context == null){
                var query = (from a in _DbContext.departments 
                        join b in _DbContext.punchcardlogs on a.ID equals b.departmentID
                        join c in _DbContext.accounts on b.accountID equals c.ID
                        where b.ID == log.ID
                        select new{a.principalID, b.accountID, c.userName}).FirstOrDefault();

                var warnLog = new PunchLogWarn();
                warnLog.accountID = log.accountID;
                warnLog.principalID = query.principalID;
                warnLog.punchLogID = log.ID;
                warnLog.warnStatus = 0;
                warnLog.createTime = definePara.dtNow();

                _DbContext.punchlogwarns.Add(warnLog);
                if(_DbContext.SaveChanges() > 0){
                    systemSendMessage(query.userName, query.accountID, "punch");
                }
            }
        }

        public void UpdatePunchLogWarn(int punchLogID){
            var context = _DbContext.punchlogwarns.FirstOrDefault(b=>b.punchLogID == punchLogID);
            if(context != null){
                context.warnStatus = 1;
                context.updateTime = definePara.dtNow();
                _DbContext.SaveChanges();
            }
        }


        //-----------------------------------------------------------------------------------------------


        public SpecialDate GetThisSpecialDate(DateTime targetDay){
            var query = _DbContext.specialdate.FirstOrDefault(b=>b.date == targetDay.Date);
            return query;
        }

        public List<Account> GetNeedPunchAcc(string departClass, int type){
            List<Account> query = new List<Account>(){};
            string filter = departClass == "全體"? "": departClass;

            if(type == 2){  //上班
                query = (from a in _DbContext.accounts
                         join b in _DbContext.departments on a.departmentID equals b.ID
                         join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                         where (b.department.Contains(filter) || c.name.Contains(filter))
                         select a
                         ).ToList();
            }else if(type == 1){    
                query = (from a in _DbContext.accounts
                         join b in _DbContext.departments on a.departmentID equals b.ID
                         join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                         where (b.department != filter && c.name != filter)
                         select a
                         ).ToList();
            }
            return query.ToList();
        }

        public PunchCardLog GetWorkDatePunchLog(int accID, DateTime targetDate){
            var query = _DbContext.punchcardlogs.FirstOrDefault(b=>b.accountID == accID && b.logDate == targetDate);
            return query;
        }

        public void AddNullPunchLog(int accID, int departID, DateTime targetDate){
            var thisAccDetail = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == accID);
            if(thisAccDetail != null){
                if(targetDate < thisAccDetail.startWorkDate){   //避免處理報到日之前的日期
                    return;
                }
            }

            var count = 0;
            var nullPunchLog = new PunchCardLog();
            nullPunchLog.accountID = accID;
            nullPunchLog.departmentID = departID;
            nullPunchLog.logDate = targetDate;
            nullPunchLog.punchStatus = 0;
            nullPunchLog.createTime = definePara.dtNow();
            try{
                _DbContext.punchcardlogs.Add(nullPunchLog);
                _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
        }

        public List<PunchCardLog> GetAllPunchLogWithWarn(int day){
            var dtRange = definePara.dtNow().Date.AddDays(-1*day);
            var query = _DbContext.punchcardlogs.Where(b=> b.punchStatus != 0x01 && b.logDate >= dtRange);                                           
            return query.ToList();
        }

        public List<LeaveOfficeApply> GetThisTakeLeave(int accID, DateTime sWorkTime, DateTime eWorkTime){
            var query = _DbContext.leaveofficeapplys.Where(
                    b=>b.accountID == accID && b.applyStatus == 1 && 
                    ((sWorkTime >= b.startTime && sWorkTime < b.endTime) || 
                     (eWorkTime > b.startTime && eWorkTime <= b.endTime))
                );
            return query.ToList();
        }

        public List<PunchCardLog> GetPunchLogByDateByID(int targetID, DateTime sDT, DateTime eDT){
            var query = _DbContext.punchcardlogs.Where(b=>b.accountID == targetID && 
                                                        b.logDate >=sDT && b.logDate <= eDT);
            return query.ToList();
        }

        public void SaveTotalTimeRecord(workTimeTotal data){
            var context = _DbContext.worktimetotals.FirstOrDefault(
                            b=>b.accountID == data.accountID && b.dateMonth == data.dateMonth);
            if(context == null){
                _DbContext.worktimetotals.Add(data);
                _DbContext.SaveChanges();
            }else{
                context.totalTime = data.totalTime;
                context.updateTime = definePara.dtNow();
                _DbContext.SaveChanges();
            }
        }

        public object GetTimeTotalByID(int targetID){
            var query = _DbContext.worktimetotals.Where(b=>b.accountID == targetID).OrderByDescending(b=>b.dateMonth);
            return query.ToList();
        }

    }
}