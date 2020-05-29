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

        public int AddOffsetData(AnnualDaysOffset data){
            var count = 0;
            try{
                if(data.reason == "" || data.reason == null)return 0;
                _DbContext.annualdaysoffset.Add(data) ;
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        public int UpEmployeeAnnualDays(EmployeeAnnualLeave data){
            var context = _DbContext.employeeannualleaves.FirstOrDefault(b=>b.ID==data.ID);
            var count = 0;
            if(context != null){
                context.remainHours = data.remainHours;
                context.deadLine = data.deadLine;
                context.updateTime = data.updateTime;
                _DbContext.SaveChanges();
            }
            return count;
        }
        
    }
}