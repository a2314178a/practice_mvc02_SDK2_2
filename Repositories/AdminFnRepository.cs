using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class AdminFnRepository : BaseRepository
    {
        public AdminFnRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public object GetOpLogCategory(){
            var query = _DbContext.operateLogs.Select(b=>b.category).Distinct();
            return query.ToList();
        }

        public List<ViewOpLog> GetOperateLog(OpLogFilter filter){
            IQueryable<OperateLog> opLogs = null;
            if(filter.opID >=0 && filter.emID >=0){
                opLogs = _DbContext.operateLogs.Where(b=>b.operateID == filter.opID && b.employeeID == filter.emID);
            }else if(filter.opID >=0){
                opLogs = _DbContext.operateLogs.Where(b=>b.operateID == filter.opID);
            }else if(filter.emID >=0){
                opLogs = _DbContext.operateLogs.Where(b=>b.employeeID == filter.emID);
            }else{
                opLogs = _DbContext.operateLogs;
            }

            var query = from a in opLogs
                        join b in _DbContext.accounts on a.operateID equals b.ID into opTmp
                        from c in opTmp.DefaultIfEmpty()
                        join d in _DbContext.accounts on a.employeeID equals d.ID into emTmp
                        from e in emTmp.DefaultIfEmpty()
                        where a.createTime >= filter.sDate && a.createTime < filter.eDate &&
                              a.active.Contains(filter.active) && a.category.Contains(filter.category)
                        orderby a.createTime descending
                        select new ViewOpLog{
                            opName = c.userName==null? "系統" : c.userName,
                            emName = e.userName==null? "系統" : e.userName,
                            active = a.active,
                            category = a.category,
                            content = a.content,
                            opTime = a.createTime.ToString("yyyy-MM-dd HH:mm:ss")
                        };
            return query.ToList();
        }

        
        
    }
}