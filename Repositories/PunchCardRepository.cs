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
        public PunchCardRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public WorkDateTime workTimeProcess(WorkTimeRule thisWorkTime, PunchCardLog customLog = null){
            var wt = new WorkDateTime();
            wt.sWorkDt = definePara.dtNow().Date;  //online work dateTime
            wt.eWorkDt = definePara.dtNow().Date;   //offline work dateTime
            wt.sPunchDT = definePara.dtNow().Date;  //可打卡時間
            wt.ePunchDT = definePara.dtNow().Date;  //
            var sRest_start = new TimeSpan(0);
            var eRest_sRest = new TimeSpan(0);
            
            if(customLog != null){
                if(customLog.logDate.Year !=1){
                    wt.sWorkDt = wt.eWorkDt = wt.sPunchDT = wt.ePunchDT = customLog.logDate.Date;
                }else if(customLog.onlineTime.Year !=1){
                    wt.sWorkDt = wt.eWorkDt = wt.sPunchDT = wt.ePunchDT = customLog.onlineTime.Date;
                }else if(customLog.offlineTime.Year !=1){
                    wt.sWorkDt = wt.eWorkDt = wt.sPunchDT = wt.ePunchDT = customLog.offlineTime.Date;
                }
            }                             
            if(thisWorkTime != null)
            {
                var eWork_sWork = thisWorkTime.endTime - thisWorkTime.startTime;
                eWork_sWork = eWork_sWork.TotalSeconds <0? eWork_sWork.Add(new TimeSpan(1, 0, 0, 0)) : eWork_sWork;
                wt.addEtHour = (int)(24*60 + wt.lessStHour - eWork_sWork.TotalMinutes);   //單位改為分鐘
                sRest_start = thisWorkTime.sRestTime - thisWorkTime.startTime;
                sRest_start = sRest_start.TotalSeconds <0? sRest_start.Add(new TimeSpan(1, 0, 0, 0)) : sRest_start;
                eRest_sRest = thisWorkTime.eRestTime - thisWorkTime.sRestTime;
                eRest_sRest = eRest_sRest.TotalSeconds <0? eRest_sRest.Add(new TimeSpan(1, 0, 0, 0)) : eRest_sRest;
                wt.type = thisWorkTime.type;
                wt.workAllTime = false;    
                wt.sWorkDt = wt.sWorkDt + thisWorkTime.startTime;  
                wt.eWorkDt = wt.eWorkDt + thisWorkTime.endTime; 
                wt.eWorkDt = wt.eWorkDt <= wt.sWorkDt ? wt.eWorkDt.AddDays(1) : wt.eWorkDt;  
                wt.sPunchDT = wt.sWorkDt.AddMinutes(wt.lessStHour);
                wt.ePunchDT = wt.eWorkDt.AddMinutes(wt.addEtHour);
                wt.elasticityMin = thisWorkTime.elasticityMin;
                if(customLog == null){
                    if(definePara.dtNow() >= wt.ePunchDT){
                        wt.sPunchDT = wt.sPunchDT.AddDays(1);
                        wt.ePunchDT = wt.ePunchDT.AddDays(1);
                        wt.sWorkDt = wt.sPunchDT.AddMinutes((wt.lessStHour*-1));
                        wt.eWorkDt = wt.ePunchDT.AddMinutes((wt.addEtHour*-1));
                    }else if(definePara.dtNow() < wt.sPunchDT){
                        wt.sPunchDT = wt.sPunchDT.AddDays(-1);   
                        wt.ePunchDT = wt.ePunchDT.AddDays(-1);
                        wt.sWorkDt = wt.sPunchDT.AddMinutes((wt.lessStHour*-1));
                        wt.eWorkDt = wt.ePunchDT.AddMinutes((wt.addEtHour*-1));   
                    }
                }
                
            }else{
                wt.workAllTime = true;
                wt.eWorkDt = wt.eWorkDt.AddDays(1);
                wt.ePunchDT = wt.ePunchDT.AddDays(1);
            }
            wt.sRestDt = wt.sWorkDt.AddMinutes(sRest_start.TotalMinutes);
            wt.eRestDt = wt.sRestDt.AddMinutes(eRest_sRest.TotalMinutes);
            return wt;
        } 

        public PunchCardLog GetTodayPunchLog(int employeeID, WorkTimeRule thisWorkTime){
            var wt = workTimeProcess(thisWorkTime);
            var query = (from a in _DbContext.punchcardlogs
                        where a.accountID == employeeID && 
                        (
                            (a.onlineTime < wt.ePunchDT && a.onlineTime >= wt.sPunchDT) ||
                            (a.offlineTime < wt.ePunchDT && a.offlineTime > wt.sPunchDT) ||
                            (a.logDate.Date == wt.sWorkDt.Date && 
                            a.onlineTime.Year == 1 && a.offlineTime.Year == 1)
                        )
                        select a).FirstOrDefault();
            return query;
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

        public int AddPunchCardLog(PunchCardLog newData, bool normal){  //true正常打卡新增/false編輯新增
            int count = 0;
            var query = _DbContext.punchcardlogs.Where(b=>b.accountID == newData.accountID && b.logDate == newData.logDate);
            if(query.Count() > 0){
                return 1062;    //該日期已有紀錄
            }
            _DbContext.punchcardlogs.Add(newData);
            count = _DbContext.SaveChanges();

            if(count ==1 && !normal){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=newData.lastOperaAccID, employeeID=newData.accountID, 
                    active="新增", category="打卡紀錄", createTime=definePara.dtNow()
                };
                toNameFn.AddUpPunchCardLog_convertToDic(ref dic, newData);
                opLog.content = toNameFn.AddUpPunchCardLog_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int UpdatePunchCard(PunchCardLog updateData, bool normal){   //true正常打卡新增/false編輯新增
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=updateData.lastOperaAccID, employeeID=updateData.accountID, 
                active="更新", category="打卡紀錄", createTime=definePara.dtNow()
            };
            int count = 0;
            var query = _DbContext.punchcardlogs.Where(b=>b.accountID == updateData.accountID && 
                                                        b.logDate == updateData.logDate && b.ID != updateData.ID);
            if(query.Count() > 0){
                return 1062;    //該日期已有紀錄
            }
            PunchCardLog context = _DbContext.punchcardlogs.FirstOrDefault(b=>b.ID == updateData.ID);
            if(context != null){
                toNameFn.AddUpPunchCardLog_convertToDic(ref oDic, context);

                context.logDate = updateData.logDate;
                context.onlineTime = updateData.onlineTime;
                context.offlineTime = updateData.offlineTime;
                context.punchStatus = updateData.punchStatus;
                context.lastOperaAccID = updateData.lastOperaAccID;
                context.updateTime = updateData.updateTime;
                count = _DbContext.SaveChanges();
            }
            if(count == 1 && !normal){
                toNameFn.AddUpPunchCardLog_convertToDic(ref nDic, context);
                opLog.content = toNameFn.AddUpPunchCardLog_convertToText(nDic, oDic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int DelPunchCardLog(int punchLogID, int loginID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=loginID, active="刪除", 
                category="打卡紀錄", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.punchcardlogs.FirstOrDefault(b=>b.ID == punchLogID);
            if(context != null){
                toNameFn.AddUpPunchCardLog_convertToDic(ref dic, context);
                opLog.employeeID = context.accountID;

                _DbContext.Remove(context);
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                opLog.content = toNameFn.AddUpPunchCardLog_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
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
                var query = (from a in _DbContext.accounts 
                            join b in _DbContext.punchcardlogs on a.ID equals b.accountID
                            join c in _DbContext.departments on b.departmentID equals c.ID into tmp
                            from d in tmp.DefaultIfEmpty()
                            where b.ID == log.ID
                            select new{a.userName, b.accountID, 
                            principalID=(d==null? 0:d.principalID)
                            }).FirstOrDefault();

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

        public void delPunchLogWarnAndMessage(PunchCardLog log){
            var context = _DbContext.punchlogwarns.FirstOrDefault(b=>b.punchLogID == log.ID);
            if(context != null){
                _DbContext.punchlogwarns.Remove(context);
                _DbContext.SaveChanges();
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

        public List<Account> GetNeedPunchAcc(string departClass, int type, bool weekFilter){ //type 1:休假 2:上班
            List<Account> query = new List<Account>(){};
            string filter = departClass == "全體"? "": departClass;

            var joinTB = (from a in _DbContext.accounts
                         join b in _DbContext.departments on a.departmentID equals b.ID into tmp
                         from bb in tmp.DefaultIfEmpty()
                         join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                         select new {em=a, 
                            dp=(bb==null? new Department{department="未指派"}:bb), wtc=c
                         }).ToList();
            if(weekFilter){ //排除固定制六日不用上班的人員
                joinTB = joinTB.Where(b=>b.wtc.type == 1).ToList(); //wtc.type 0:固定制 1:排休制
            }
                        
            if(type==2){
                query = joinTB.Where(b=>b.dp.department.Contains(filter) ||
                             b.wtc.name.Contains(filter) || b.wtc.type == 1)
                            .Select(b=>b.em).ToList();
            }else if(type==1){
                query = joinTB.Where(b=>(!b.dp.department.Contains(filter) && !b.wtc.name.Contains(filter)) ||
                                        b.wtc.type == 1)
                            .Select(b=>b.em).ToList();
            }
            return query;
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
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
        }

        public List<PunchCardLog> GetAllPunchLogWithWarn(int day){
            var dtRange = definePara.dtNow().Date.AddDays(-1*day);
            var query = _DbContext.punchcardlogs.Where(b=> b.logDate >= dtRange &&
                                (b.punchStatus != 0x01 || b.onlineTime.Year ==1 || b.offlineTime.Year ==1));
                                                                                
            return query.ToList();
        }

        public List<LeaveOfficeApply> GetThisTakeLeave(int accID, DateTime sWorkTime, DateTime eWorkTime){
            var query = _DbContext.leaveofficeapplys.Where(
                    b=>b.accountID == accID && b.applyStatus == 1 && 
                    ((b.startTime <= sWorkTime && sWorkTime < b.endTime) || 
                     (b.startTime < eWorkTime && eWorkTime <= b.endTime) ||
                     (sWorkTime < b.startTime && b.endTime < eWorkTime))
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