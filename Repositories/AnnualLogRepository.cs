using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class AnnualLogRepository : BaseRepository
    {
        public AnnualLogRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public object GetAnnualLeaveTimeUnit(){
            var query = _DbContext.leavenames.FirstOrDefault(b=>b.leaveName == definePara.annualName());                
            if(query != null){
                return new {query.timeUnit, query.halfVal};
            }else{
                return new{timeUnit=3, halfVal=false};
            }
        }

        public object GetAnnualLog(int id, DateTime sDate, DateTime eDate){
            var query = (from a in _DbContext.leaveofficeapplys
                        join b in _DbContext.leavenames on a.leaveID equals b.ID
                        where b.leaveName == definePara.annualName() && a.accountID == id
                            && (a.applyStatus == 0 || a.applyStatus == 1) &&
                            ((a.startTime >= sDate && a.endTime <= eDate) || 
                             (a.endTime >= sDate && a.endTime <= eDate)) 
                        orderby a.startTime descending
                        select new{
                            a.startTime, a.endTime, unit=(int)a.unit, a.unitVal, a.note
                        });
            return query.ToList();
        }

        public object GetOffsetLog(int id, DateTime sDate, DateTime eDate){
            var query = from a in _DbContext.employeeannualleaves
                        join b in _DbContext.annualdaysoffset on a.ID equals b.emAnnualID
                        join c in _DbContext.accounts on b.lastOperaAccID equals c.ID
                        where a.employeeID == id && b.createTime >= sDate && b.createTime < eDate
                        orderby b.createTime descending
                        select new{
                            b.reason, b.value, b.createTime , c.userName
                        };
            return query.ToList();
        }

        public bool chkDeadLineLength(EmployeeAnnualLeave data){
            var isLegal = false;
            var context = _DbContext.employeeannualleaves.FirstOrDefault(b=>b.ID==data.ID);
            if(context != null){
                var length = (data.deadLine - context.deadLine).Duration();
                isLegal = length.Days <= 120? true : false;
            }
            return isLegal;
        }

        public int UpEmployeeAnnualDays(EmployeeAnnualLeave data, AnnualDaysOffset offsetData){

            using(var trans = _DbContext.Database.BeginTransaction()){
                var count = 0;
                try
                {
                    if(offsetData.reason == "" || offsetData.reason == null){return 0;}
                    _DbContext.annualdaysoffset.Add(offsetData);
                    count = _DbContext.SaveChanges();
                    if(count == 0){return 0;}

                    var oDic = new Dictionary<string,string>{};
                    var nDic = new Dictionary<string,string>{};
                    var opLog = new OperateLog(){
                        operateID=data.lastOperaAccID, 
                        active="更新", category="特休", createTime=definePara.dtNow()
                    };
                    var context = _DbContext.employeeannualleaves.FirstOrDefault(b=>b.ID==data.ID);
                    if(context != null){
                        toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref oDic, context);
                        opLog.employeeID = context.employeeID;

                        context.remainHours = data.remainHours;
                        context.deadLine = data.deadLine;
                        context.lastOperaAccID = data.lastOperaAccID;
                        context.updateTime = data.updateTime;
                        count = _DbContext.SaveChanges();
                    }
                    if(count == 1){
                        toNameFn.AddUpEmployeeAnnualDays_convertToDic(ref nDic, context);
                        opLog.content = toNameFn.AddUpEmployeeAnnualDays_convertToText(nDic, oDic);
                        opLog.content += $"，原因:{offsetData.reason}"; //主管調整員工餘額
                        saveOperateLog(opLog);    //紀錄操作紀錄
                    }
                    trans.Commit();
                }
                catch (Exception ex){
                   count = catchErrorProcess(ex, count);
                }
                return count;
            }//using
        }
        
    }
}