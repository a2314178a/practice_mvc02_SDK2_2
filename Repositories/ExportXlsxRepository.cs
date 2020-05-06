using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class ExportXlsxRepository : BaseRepository
    {
        public ExportXlsxRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public List<PunchCardLog> GetLogs(){
            var query = _DbContext.punchcardlogs.ToList();
            return query;
        }

        public List<LeaveName> GetLeaveName(){
            var query = _DbContext.leavenames;
            return query.ToList();
        }
        
        public List<exportXlsxData> GetNormalDetail_Month(exportPunchLogXlsxPara qPara){
            var depart = qPara.departName == "所有部門"? "" : qPara.departName;
            var query = from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                        where b.department.Contains(depart)
                        select new exportXlsxData{
                            accID=a.ID, name=a.userName, workClass=c.name, 
                            department=b.department, position=b.position
                        };
            return query.ToList();
        }

        public exportXlsxData GetNormalDetail_Day(exportPunchLogXlsxPara qPara){
            var query = from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                        where a.ID == qPara.accID
                        select new exportXlsxData{
                            accID=a.ID, name=a.userName, workClass=c.name,
                            startTime=c.startTime, endTime=c.endTime, 
                            department=b.department, position=b.position
                        };
            return query.FirstOrDefault();
        }

        public List<PunchCardLog> GetRequestPunchLog(exportPunchLogXlsxPara qPara, int id){
            var query = _DbContext.punchcardlogs.Where(
                                b=>b.accountID == id &&
                                b.logDate >= qPara.sDate && b.logDate <= qPara.eDate)
                                .OrderBy(b=>b.logDate);
            return query.ToList();
        }

        public WorkTimeRule GetThisWorkTimeRule(int accID){
            var query = (from a in _DbContext.accounts
                        join b in _DbContext.worktimerules on a.timeRuleID equals b.ID
                        where a.ID == accID 
                        select b).FirstOrDefault();
            return query;
        }

        public List<LeaveOfficeApply> GetApplyLeaveLogs(exportPunchLogXlsxPara qPara, int id){
            var query = _DbContext.leaveofficeapplys.Where(a=>
                            a.accountID == id && a.applyStatus == 1 &&
                            ((a.startTime >= qPara.sDate && a.startTime < qPara.eDate) || 
                             (a.endTime > qPara.sDate && a.endTime <= qPara.eDate)
                            )
                        );
            return query.ToList();
        }

        public object GetDepartment(){
            var query = _DbContext.departments.Select(b=>b.department).Distinct();
            return query.ToList();
        }

        public object GetDepartmentEmployee(string depart){
            var query = from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        where b.department == depart
                        select new{
                            a.ID, a.userName
                        };
            return query.ToList();
        }
    }
}