using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class ApplyOvertimeRepository : BaseRepository
    {
        private punchStatusCode psCode;
        private punchCardFunction punchCardFn {get;} 
        private string specialName = definePara.annualName();
        
        
        public ApplyOvertimeRepository(DBContext dbContext, punchCardFunction fn):base(dbContext)
        {
            this.psCode = new punchStatusCode();
            this.punchCardFn = fn;
        }

        public object GetMyApplyOvertime(int loginID, int page, DateTime sDate, DateTime eDate){
            var feDate = eDate.Year == 1? eDate.AddYears(9998) : eDate.AddDays(1);
            //var selStatus = page == 0? 1 : 3;   //0: 待審 1:通過 2:不通過   page=0:leave / page=1=log
            var query = _DbContext.overtimeApply.Where(
                                b=>b.accountID == loginID && b.createTime >= sDate && b.createTime < feDate)
                            .Select(b=>new{
                                b.ID, b.note, b.applyStatus, b.workDate, timeLength=b.timeLength/6, b.createTime})
                            .OrderByDescending(b=>b.createTime).ToList();
            if(page == 1){
                query = query.Where(b=>b.applyStatus == 1 || b.applyStatus == 2).ToList();
            }else{
                query = query.Where(b=>b.applyStatus == 0).ToList();
            }
            return query;
        }

        public List<OvertimeApply> GetOvertimeApplyByDateByID(int accID, DateTime sDate, DateTime eDate){
            var query = _DbContext.overtimeApply.Where(
                                    b=>b.accountID == accID && b.applyStatus == 1 &&
                                    b.workDate >= sDate && b.workDate <= eDate)
                                .OrderBy(b=>b.workDate);
            return query.ToList();
        }

        public bool chkApplyOvertimeData(OvertimeApply data){
            var result = true;
            var punchLog = _DbContext.punchcardlogs.FirstOrDefault(b=>b.accountID == data.accountID && b.logDate == data.workDate);
            if(punchLog == null){
                result = false;
            }else{
                result = punchLog.onlineTime.Year > 1 && punchLog.offlineTime.Year > 1? true : false;
            }
            return result;
        }

        public int CreateApplyOvertime(OvertimeApply newApply, String loginName){
            using(var trans = _DbContext.Database.BeginTransaction()){
                var count = 0;
                try
                {
                    newApply.createTime = newApply.updateTime = definePara.dtNow();
                    newApply.timeLength = newApply.timeLength * 6;  //timeLength/10 * 60
                    _DbContext.overtimeApply.Add(newApply);
                    count = _DbContext.SaveChanges();
                    if(count == 1){
                        systemSendMessage(loginName, newApply.accountID, "overtime");
                    }
                    trans.Commit();
                }
                catch (Exception ex){
                    count = catchErrorProcess(ex, count);
                }
                return count;
            }   
        }

        public int DelApplyOvertime(int applyID){
            var result = 0;
            var context = _DbContext.overtimeApply.FirstOrDefault(b=>b.ID == applyID);
            if(context != null){
                _DbContext.overtimeApply.Remove(context);
                result = _DbContext.SaveChanges();
            }
            return result;
        }

        
        //-----------------------------------------------------------------------------------------------------

        
    }
}