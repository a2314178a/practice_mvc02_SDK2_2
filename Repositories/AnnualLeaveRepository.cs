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
                    tmp.remainHours = tmp.remainHours+diffSpecialDays*dayToHour >=0? tmp.remainHours+diffSpecialDays*dayToHour :0;
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
        
        public int FindLowOneThanThisRule(int ruleID){
            var allRule = GetAnnualLeaveRule();
            var targetID = 0;
            for(int i=0; i<allRule.Count; i++){
                if(allRule[i].ID == ruleID){
                    targetID = i >0 ? allRule[i-1].ID : 0;
                    break;
                }
            }
            return targetID;
        }

        public void DelSomeAnnualLeaveRecord(int[] targetID_Arr){
            var query = _DbContext.employeeannualleaves.Where(b=>targetID_Arr.Contains(b.ruleID));
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

        public void StartCalAnnualLeave(){
            calObj.start();
        }

        public float GetSpLeaveTotalHours(int employeeID, DateTime sDT, DateTime eDT){
            var query = from a in _DbContext.leaveofficeapplys
                        join b in _DbContext.leavenames on a.leaveID equals b.ID
                        where a.accountID == employeeID && a.applyStatus ==1 && 
                                a.startTime >= sDT && a.endTime < eDT &&
                                b.leaveName == definePara.annualName()
                        select a;
            var dayToHour = definePara.dayToHour();
            var leaveHour = 0.0F;
            foreach(var leave in query.ToList()){
                switch(leave.unit){
                    case 1: leaveHour+= (leave.unitVal)*dayToHour; break;
                    case 2: leaveHour+= (leave.unitVal)*dayToHour/2; break;
                    case 3: leaveHour+= (leave.unitVal)*1; break;
                }
            }
            return leaveHour;
        }

    }
}