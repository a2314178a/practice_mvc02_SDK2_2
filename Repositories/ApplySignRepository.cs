using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class ApplySignRepository : BaseRepository
    {
        private punchStatusCode psCode;
        private punchCardFunction punchCardFn {get;} 
        private string specialName = definePara.annualName();
        
        public ApplySignRepository(DBContext dbContext, punchCardFunction fn):base(dbContext)
        {
            this.psCode = new punchStatusCode();
            this.punchCardFn = fn;
        }

        #region punchWarn

        public object GetPunchLogWarn(int loginID){
            var query =  from a in _DbContext.punchcardlogs
                         join b in _DbContext.punchlogwarns on a.ID equals b.punchLogID
                         join c in _DbContext.accounts on a.accountID equals c.ID
                         join d in _DbContext.employeeprincipals on a.accountID equals d.employeeID
                         where (d.principalID == loginID || d.principalAgentID == loginID) &&
                                a.accountID != loginID && b.warnStatus <2
                         orderby a.logDate descending
                         select new{
                            c.userName, a.ID, a.logDate, a.onlineTime, a.offlineTime, a.punchStatus, b.warnStatus
                        };
            return query.ToList();
        }

        public int IgnorePunchLogWarn(int punchLogID){
            int count = 0;
            var context = _DbContext.punchlogwarns.FirstOrDefault(b=>b.punchLogID == punchLogID);
            if(context != null){
                context.warnStatus = 2;
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        #endregion //punchWarn

        //-----------------------------------------------------------------------------------------------------

        #region  LeaveOffice

        public bool ChkHasPrincipal(int emID){
                var query = (from a in _DbContext.employeeprincipals
                            join b in _DbContext.accounts on a.principalID equals b.ID
                            where a.employeeID == emID
                            select b).FirstOrDefault();
                            
            return query==null? false : true;
        }

        public bool ChkApplyLeaveData(LeaveOfficeApply data, bool isActiveApply){
            var query = _DbContext.leavenames.FirstOrDefault(b=>b.ID == data.leaveID);
            if(query == null || (data.unit > query.timeUnit)){
                return false;
            }
            var result = true;
            result = data.unitVal <=0 ? false : result; 
            result = (data.unit==1 && (data.unitVal-(int)data.unitVal)!=0)? false : result; //單位天 值不可小數
            result = (data.unit==2 && data.unitVal >2)? false : result; //單位半天 1:上半天 2:下半天
            if(data.unit == 3){ //單位小時
                var halfVal = query.halfVal;    //可否0.5小時
                result = (!halfVal && (data.unitVal-(int)data.unitVal)!=0)? false : result; //不可小數
                result = (halfVal && (data.unitVal%0.5)!=0)? false : result; //值不為0.5倍數
            }
            result = (!isActiveApply && data.unit == 1 && data.unitVal != 1)? false : result;   //被請假 只能請當天(1天)
            return result;
        }

        public bool ChkLeaveHadSameTimeData(LeaveOfficeApply data){
            var query = _DbContext.leaveofficeapplys.FirstOrDefault(
                                    b=>b.accountID == data.accountID && b.startTime == data.startTime &&
                                    b.endTime == data.endTime && b.applyStatus < 2);
            return query == null? false : true;
        }

        public bool IsUseHourHalfVal(int leaveID){
            var query = _DbContext.leavenames.FirstOrDefault(b=>b.ID == leaveID);
            return query == null? false : query.halfVal;
        }

        public object GetMyApplyLeave(int loginID, int page, DateTime sDate, DateTime eDate){
            var feDate = eDate.Year == 1? eDate.AddYears(9998) : eDate.AddDays(1);
            var selStatus = page == 0? 1 : 3;   //0: 待審 1:通過 2:不通過

            var query = from a in _DbContext.leavenames
                        join b in _DbContext.leaveofficeapplys on a.ID equals b.leaveID
                        where b.accountID == loginID && b.applyStatus < selStatus &&
                              b.createTime >= sDate && b.createTime < feDate
                        orderby b.createTime descending
                        select new{
                            a.leaveName, a.timeUnit,
                            b.ID, b.leaveID, b.note, b.startTime, b.endTime, b.applyStatus, b.createTime, b.unitVal, b.unit
                        };
            return query.ToList();
        }

        public object GetLeaveOption(){
            var query = _DbContext.leavenames.Where(b=>b.enable==true)
                                            .Select(b=>new{b.ID, b.leaveName, b.timeUnit, b.halfVal});
            return query.ToList();
        }

        public object GetEmployeeApplyLeave(int loginID, int page, DateTime sDate, DateTime eDate){
            var feDate = eDate.Year == 1? eDate.AddYears(9998) : eDate.AddDays(1);
            var selStatus = page == 0? 1 : 3;   //0: 待審 1:通過 2:不通過

            var query = from a in _DbContext.leaveofficeapplys
                        join b in _DbContext.accounts on a.accountID equals b.ID
                        join c in _DbContext.employeeprincipals on a.accountID equals c.employeeID
                        join d in _DbContext.leavenames on a.leaveID equals d.ID
                        where (c.principalID == loginID || c.principalAgentID == loginID) &&
                                a.accountID != loginID && a.applyStatus < selStatus && 
                                a.createTime >= sDate && a.createTime < feDate
                        orderby a.createTime descending
                        select new{
                            a.ID, a.leaveID, a.note, a.startTime, a.endTime, a.applyStatus, a.createTime, 
                            b.userName, d.leaveName, d.timeUnit
                        };
            return query.ToList();
        }

        public string GetMyDepartClass(int loginID){
            var query = (from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                        from bb in noDepart.DefaultIfEmpty()
                        join c in _DbContext.worktimerules on a.timeRuleID equals c.ID into tmp
                        from d in tmp.DefaultIfEmpty()
                        where a.ID == loginID
                        select new{
                            department=(bb==null? "未指派":bb.department), 
                            name=(d==null? null:d.name)
                        }).FirstOrDefault();
            return query != null? ($"全體,{query.department},{query.name}") : "全體";
        }

        public int CreateApplyLeave(LeaveOfficeApply newApply){
            int count = 0;
            try{
                _DbContext.leaveofficeapplys.Add(newApply);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(count == 1){
                var leaveName = getApplyLeaveName(newApply.leaveID);
                if(leaveName == specialName)
                    refreshEmployeeAnnualLeave(newApply, 2);    //減時數
            }
            return count;
        }

        public int DelApplyLeave(int applyLeaveID, int loginID){
            int count = 0;
            var context = _DbContext.leaveofficeapplys.FirstOrDefault(b=>b.ID == applyLeaveID);
            if(context != null){
                var leaveName = getApplyLeaveName(context.leaveID);
                _DbContext.Remove(context);
                count = _DbContext.SaveChanges();
                if(count == 1 && leaveName == specialName){
                    refreshEmployeeAnnualLeave(context, 1); //加時數
                }
            }
            return count;
        }

        public int UpdateApplyLeave(LeaveOfficeApply updateApply){  //暫無使用到
            int count = 0;
            var context = _DbContext.leaveofficeapplys.FirstOrDefault(b=>b.ID == updateApply.ID);
            if(context != null && context.applyStatus == 0){
                context.leaveID = updateApply.leaveID;
                context.note = updateApply.note;
                context.startTime = updateApply.startTime;
                context.endTime = updateApply.endTime;
                context.unitVal = updateApply.unitVal;
                context.unit = updateApply.unit;
                context.lastOperaAccID = updateApply.lastOperaAccID;
                context.updateTime = updateApply.updateTime;
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                //之後有使用到此更新功能 須在這新增更新後的特休刷新處理
            }
            return count;
        }

        public LeaveOfficeApply IsAgreeApplyLeave(int applyID, int newStatus, int loginID){
            int count = 0;
            var leaveStatus = 0;    //0:沒動作 1:加回特休時數 2:減特休時數
            var context = _DbContext.leaveofficeapplys.FirstOrDefault(b=>b.ID == applyID);
            var leaveName = getApplyLeaveName(context.leaveID);

            if(context != null){
                if(leaveName == specialName){   //申請假別為特休
                    var oldStatus = context.applyStatus;    //0:待審 1:通過 2:未通過
                    if((oldStatus == 2 && newStatus == 0) || (oldStatus == 2 && newStatus == 1)){
                        leaveStatus = 2;    //減特休時數
                    }else if((oldStatus == 1 && newStatus == 2) || (oldStatus == 0 && newStatus == 2)){
                        leaveStatus = 1;    //加回特休時數
                    }
                    if(!chkEmployeeAnnualLeave(context, leaveStatus)){
                        return null;
                    }
                }
   
                context.applyStatus = newStatus;
                context.lastOperaAccID = loginID;
                context.updateTime = definePara.dtNow();
                count = _DbContext.SaveChanges();  
            }
            if(count == 1 && leaveName == specialName && leaveStatus >0){
                refreshEmployeeAnnualLeave(context, leaveStatus);
            }
            return count == 1? context : null;
        }

        public string getApplyLeaveName(int leaveID){
            var query = _DbContext.leavenames.FirstOrDefault(b=>b.ID == leaveID);
            return query == null? "" : query.leaveName;
        }

        public bool chkEmployeeAnnualLeave(LeaveOfficeApply context, int leaveStatus=2){
            if(leaveStatus != 2){
                return true;
            }else{  //若減時數 需確認還有多餘的時可以以扣
                var DayToHour = definePara.dayToHour();
                var applyHours = 0.0f;
                switch(context.unit){
                    case 1: applyHours = (context.unitVal)*DayToHour; break;
                    case 2: applyHours = (context.unitVal)*DayToHour/2; break;
                    case 3: applyHours = (context.unitVal)*1; break;
                }
                var query = _DbContext.employeeannualleaves
                            .Where(b=>b.employeeID == context.accountID && context.startTime < b.deadLine);
                var haveHours = 0.0f;
                foreach(var spDays in query){
                    haveHours += spDays.remainHours;
                }
                return haveHours >= applyHours? true : false;
            }
        }

        public void refreshEmployeeAnnualLeave(LeaveOfficeApply context, int leaveStatus){
            var query = _DbContext.employeeannualleaves
                            .Where(b=>b.employeeID == context.accountID && context.startTime < b.deadLine)
                            .OrderBy(b=>b.deadLine).ToList();

            var DayToHour = definePara.dayToHour();
            var applyHours = 0.0f;  
            switch(context.unit){
                case 1: applyHours = (context.unitVal)*DayToHour; break;
                case 2: applyHours = (context.unitVal)*DayToHour/2; break;
                case 3: applyHours = (context.unitVal); break;
            } 
            if(leaveStatus == 2){   //減時數
                for(int i =0; i<query.Count; i++){
                    var remainHours = query[i].remainHours;
                    if(remainHours >= applyHours){
                        remainHours -= applyHours;
                        applyHours = 0;
                    }else{
                        applyHours -= remainHours;
                        remainHours = 0;
                    }
                    query[i].remainHours = remainHours;
                    query[i].updateTime = definePara.dtNow();
                }
            }else if(leaveStatus == 1){ //加回時數
                for(int i = query.Count-1; i>=0; i--){
                    var remainHours = query[i].remainHours;
                    if((remainHours + applyHours) > (query[i].specialDays)*DayToHour){
                        applyHours -= ((query[i].specialDays)*DayToHour - remainHours);
                        remainHours = (query[i].specialDays)*DayToHour;
                    }else{
                        remainHours += applyHours;
                        applyHours = 0;
                    }
                    query[i].remainHours = remainHours;
                    query[i].updateTime = definePara.dtNow();  
                }
            }   
            _DbContext.SaveChanges();                       
        }

        public void punchLogWithTakeLeave(LeaveOfficeApply restLog){
            var restST = restLog.startTime;
            var restET = restLog.endTime;
            var wtLength = 720;   //工作時間長度 unit:minute
            //因工作時間可能跨日 不能直接用restST.Date EX: 2300-0800 休下半天0400-0800
            var workDate = restST.Date;  //工作天日期(logDate) 
            var wtRule = (from a in _DbContext.accounts
                             join b in _DbContext.worktimerules on a.timeRuleID equals b.ID
                             where a.ID == restLog.accountID
                             select b).FirstOrDefault();

           if(wtRule != null){
                if(wtRule.endTime < wtRule.startTime){
                    wtLength = (int)(wtRule.endTime.Add(new TimeSpan(24,0,0)) - wtRule.startTime).TotalMinutes;
                }else{
                    wtLength = (int)(wtRule.endTime - wtRule.startTime).TotalMinutes;
                }
                var restSTime = new TimeSpan(restST.Hour, restST.Minute, restST.Second);
                workDate = (wtRule.startTime > restSTime)? workDate.AddDays(-1) : workDate;
            }
            
            do{
                var log = _DbContext.punchcardlogs.FirstOrDefault(b=>
                                        b.accountID == restLog.accountID && b.logDate == workDate);
                if(log != null){
                    log.lastOperaAccID = 0;
                    log.updateTime = definePara.dtNow();
                    if(restLog.applyStatus == 1){  
                        WorkDateTime wt = punchCardFn.workTimeProcess(wtRule, log);
                        log.punchStatus = punchCardFn.getStatusCode(wt, log, restLog);
                    }else{
                        log.punchStatus &= ~psCode.takeLeave;
                        if(log.logDate > definePara.dtNow()){
                            _DbContext.Remove(log);
                        }else{
                            if(log.onlineTime.Year ==1 && log.offlineTime.Year ==1){
                                log.punchStatus |= psCode.noWork;
                            }
                        }
                    }
                    _DbContext.SaveChanges();    
                }else{
                    if(restLog.applyStatus == 1){
                        var applyAcc = _DbContext.accounts.FirstOrDefault(b=>b.ID == restLog.accountID);
                        var departID = applyAcc==null? 0 : applyAcc.departmentID;
                        PunchCardLog newLog = new PunchCardLog{
                            accountID = restLog.accountID, departmentID = departID,
                            logDate = workDate, createTime = definePara.dtNow(),
                        };
                        WorkDateTime wt = punchCardFn.workTimeProcess(wtRule, newLog);
                        newLog.punchStatus = punchCardFn.getStatusCode(wt, newLog, restLog);
                        _DbContext.punchcardlogs.Add(newLog);
                        _DbContext.SaveChanges();
                    }
                }
            
                if(restST.AddMinutes(wtLength) < restET){
                    restST = restST.AddDays(1);
                    workDate = workDate.AddDays(1);
                }else{
                    restST = restST.AddMinutes(wtLength);
                }
            }while(restST < restET);
        }

        
        #endregion //leaveOffice


    }
}