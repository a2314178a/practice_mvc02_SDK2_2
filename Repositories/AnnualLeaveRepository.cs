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
            if(query == null){
                data.createTime = DateTime.Now;
                _DbContext.employeeannualleaves.Add(data);
                _DbContext.SaveChanges();
            }
        }

        public void UpEmployeeSpLeave(int ruleID , int diffSpecialDays, int diffBuffDays){
            var query = _DbContext.employeeannualleaves.Where(b=>b.ruleID == ruleID).ToList();
            foreach(var tmp in query){
                tmp.updateTime = DateTime.Now;
                if(diffSpecialDays != 0){
                    tmp.specialDays += diffSpecialDays;
                    tmp.remainHours += diffSpecialDays*8;
                    tmp.remainHours = tmp.remainHours+diffSpecialDays*8 >=0? tmp.remainHours+diffSpecialDays*8:0;
                    _DbContext.SaveChanges();
                }
                if(diffBuffDays != 0){
                    tmp.deadLine = tmp.deadLine.AddMonths(diffBuffDays/30);
                    _DbContext.SaveChanges();
                }
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
                _DbContext.employeeannualleaves.RemoveRange(query);
                _DbContext.SaveChanges();
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
                                b.leaveName == specialName
                        select a;
            var leaveHour = 0.0F;
            foreach(var leave in query.ToList()){
                switch(leave.unit){
                    case 1: leaveHour+= (leave.unitVal)*8; break;
                    case 2: leaveHour+= (leave.unitVal)*4; break;
                    case 3: leaveHour+= (leave.unitVal)*1; break;
                }
            }
            return leaveHour;
        }

    }
}