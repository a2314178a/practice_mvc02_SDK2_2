using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class AnnualLeaveRepository : BaseRepository
    {
        private CalAnnualLeave calObj {get;}
        public AnnualLeaveRepository(DBContext dbContext, punchCardFunction fn):base(dbContext)
        {
            calObj = new CalAnnualLeave(this);
        }

        public List<EmployeeDetail> GetAllEmployeeDetail(){
            var query = from a in _DbContext.accounts
                        join b in _DbContext.employeedetails on a.ID equals b.accountID
                        select b;
            return query.ToList();
        }

        public List<AnnualLeaveRule> GetAnnualLeaveRule(){
            var query = _DbContext.annualleaverule.OrderBy(b=>b.seniority);
            return query.ToList();
        }

        public void RecordEmployeeSpDays(EmployeeAnnualLeave data){
            var query = _DbContext.employeeannualleaves.FirstOrDefault(
                            b=>b.employeeID==data.employeeID && b.ruleID==data.ruleID && 
                            b.deadLine == data.deadLine
                        );
            var count = 0;
            if(query == null){
                data.createTime = definePara.dtNow();
                _DbContext.employeeannualleaves.Add(data);
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=data.lastOperaAccID,  employeeID=data.employeeID,
                    active="新增", category="特休", createTime=definePara.dtNow()
                };
                toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref dic, data);
                opLog.content = toNameFn.AddUpEmployeeAnnualDays_convertToText(dic);
                saveOperateLog(opLog);
            }
        }

        public void UpEmployeeSpLeave(int ruleID , int diffSpecialDays, int diffBuffDays){
            var query = _DbContext.employeeannualleaves.Where(b=>b.ruleID == ruleID).ToList();

            var dayToHour = definePara.dayToHour();
            foreach(var tmp in query){
                var oDic = new Dictionary<string,string>{};
                var nDic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    employeeID=tmp.employeeID,
                    active="更新", category="特休", createTime=definePara.dtNow()
                };
                toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref oDic, tmp);

                tmp.updateTime = definePara.dtNow();
                if(diffSpecialDays != 0){
                    tmp.specialDays += diffSpecialDays;
                    tmp.remainHours += diffSpecialDays*dayToHour;
                    tmp.remainHours = tmp.remainHours >=0? tmp.remainHours : 0;
                    _DbContext.SaveChanges();
                }
                if(diffBuffDays != 0){
                    tmp.deadLine = tmp.deadLine.AddMonths(diffBuffDays/30);
                    _DbContext.SaveChanges();
                }
                toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref nDic, tmp);
                opLog.content = toNameFn.AddUpEmployeeAnnualDays_convertToText(nDic, oDic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
        }

        public void DelSomeAnnualLeaveRecord(int ruleID){
            var query = _DbContext.employeeannualleaves.Where(b=>b.ruleID == ruleID).ToList();
            if(query.Count>0){
                foreach(var tmp in query){
                    var dic = new Dictionary<string,string>{};
                    var opLog = new OperateLog(){
                        employeeID=tmp.employeeID,
                        active="刪除", category="特休", createTime=definePara.dtNow()
                    };
                    toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref dic, tmp);

                    _DbContext.employeeannualleaves.Remove(tmp);
                    _DbContext.SaveChanges();

                    opLog.content = toNameFn.AddUpEmployeeAnnualDays_convertToText(dic);
                    saveOperateLog(opLog);    //紀錄操作紀錄
                }  
            }
        }

        public void DelAnnualDaysByEmployeeID(int ID){
            var query = _DbContext.employeeannualleaves.Where(b=>b.employeeID == ID);
            if(query.Count()>0){
                foreach(var tmp in query.ToList()){
                    var dic = new Dictionary<string,string>{};
                    var opLog = new OperateLog(){
                        employeeID=tmp.employeeID,
                        active="刪除", category="特休", createTime=definePara.dtNow()
                    };
                    toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref dic, tmp);

                    _DbContext.employeeannualleaves.Remove(tmp);
                    _DbContext.SaveChanges();

                    opLog.content = toNameFn.AddUpEmployeeAnnualDays_convertToText(dic);
                    saveOperateLog(opLog);    //紀錄操作紀錄
                }
            }
        }

        public void refreshAnnualLeaveHours(AnnualLeaveRule context){
            var oldRecord = _DbContext.employeeannualleaves.Where(
                                b=>b.ruleID == context.ID && b.deadLine >definePara.dtNow()).ToList();
            var oldSeniority = context.seniority*12;
            var oldBuffDays = context.buffDays;
            var spDaysRule = GetAnnualLeaveRule();
            var dayToHour = definePara.dayToHour();
            var useIndex = -1;
            for(var i = 0; i<spDaysRule.Count; i++){    //list<spDaysRule> 有排序 從小到大
                if(oldSeniority >= (spDaysRule[i].seniority)*12){ //年資有到規則設定的年資
                    useIndex = i;
                }
            }
            if(useIndex <= -1)
                return;
            var useRule = spDaysRule[useIndex];
            foreach(var tmp in oldRecord){
                var costHours = tmp.specialDays*dayToHour - tmp.remainHours;
                tmp.ruleID = useRule.ID;
                tmp.specialDays = useRule.specialDays;
                tmp.remainHours = useRule.specialDays*dayToHour;
                tmp.remainHours -= costHours;
                tmp.remainHours = tmp.remainHours >=0? tmp.remainHours : 0;
                tmp.deadLine = tmp.deadLine.AddMonths(-oldBuffDays/30);
                tmp.deadLine = tmp.deadLine.AddMonths(useRule.buffDays/30);
                tmp.updateTime = definePara.dtNow();
                _DbContext.SaveChanges();
            }

        }

        public void refreshLowAnnualLeaveData(AnnualLeaveRule newRule){
            var dayToHour = definePara.dayToHour();
            var normalDeadLineMonth = 12;
            var newSeniorityMon = newRule.seniority*12;
            var dtNow = definePara.dtNow();
            var lowRule = FindLowOneThanThisRule(newRule.ID);
            var lowData = _DbContext.employeeannualleaves
                                    .Where(b=>b.ruleID == lowRule.ID && b.deadLine > dtNow).ToList();
            var emIDs = lowData.Select(b=>b.employeeID).Distinct().ToArray();
            foreach(var emID in emIDs)
            {
                var emDetail = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == emID);
                var totalMonth = calThisEmployeeSeniority(emDetail);
                if(totalMonth < newSeniorityMon){
                    continue;
                }
                var upData = lowData.Where(b=>b.employeeID == emID)
                                    .OrderByDescending(b=>b.deadLine).ToList();   //動使用lowRule2筆(最多2筆)
                if((totalMonth - newSeniorityMon) < normalDeadLineMonth){    //動使用lowRule1筆
                    upData = new List<EmployeeAnnualLeave>{upData[0]};  //取期限最晚的那筆
                }
                foreach(var tmp in upData){
                    var costHours = tmp.specialDays*dayToHour - tmp.remainHours;
                    tmp.ruleID = newRule.ID;
                    tmp.specialDays = newRule.specialDays;
                    tmp.remainHours = newRule.specialDays*dayToHour;
                    tmp.remainHours -= costHours;
                    tmp.remainHours = tmp.remainHours >=0? tmp.remainHours : 0;
                    tmp.deadLine = tmp.deadLine.AddMonths(-(lowRule.buffDays)/30);
                    tmp.deadLine = tmp.deadLine.AddMonths(newRule.buffDays/30);
                    tmp.updateTime = dtNow;
                    _DbContext.SaveChanges();
                }
            }
            calObj.start();
        }
                
        public AnnualLeaveRule FindLowOneThanThisRule(int newRuleID){
            var allRule = GetAnnualLeaveRule();
            var lowRule = new AnnualLeaveRule();
            for(int i=0; i<allRule.Count; i++){
                if(allRule[i].ID == newRuleID){
                    lowRule = i >0 ? allRule[i-1] : new AnnualLeaveRule();
                    break;
                }
            }
            return lowRule;
        }

        public int calThisEmployeeSeniority(EmployeeDetail detail){
            var dtNow = definePara.dtNow();
            if(detail == null || detail.startWorkDate.Year ==1 || detail.startWorkDate > dtNow)
                return 0;

            var dtStart = detail.startWorkDate; //報到日  
            dtNow = dtNow.Day>=dtStart.Day? dtNow : dtNow.AddMonths(-1);   //ex: dtNow=2020-07-09  dtStart=2018-07-10
            var year = (dtNow.Month >= dtStart.Month)? (dtNow.Year - dtStart.Year):(dtNow.Year - dtStart.Year -1);
            var month = (dtNow.Month >= dtStart.Month)? (dtNow.Month - dtStart.Month):(dtNow.Month - dtStart.Month +12);
            var totalMonth = 12*year + month;   //該員工總年資 unit:month
            return totalMonth;
        }

    }
}