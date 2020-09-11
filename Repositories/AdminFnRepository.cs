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

        public void ClearEmployeeAnnualLeaves(DateTime dt){
            var query = _DbContext.employeeannualleaves.Where(b=>b.deadLine < dt).ToList();
            _DbContext.employeeannualleaves.RemoveRange(query);
            _DbContext.SaveChanges();
        }

        public void ClearEmployeeLeaveOfficeApply(DateTime dt){
            var query = _DbContext.leaveofficeapplys.Where(b=>b.createTime < dt).ToList();
            _DbContext.leaveofficeapplys.RemoveRange(query);
            _DbContext.SaveChanges();
        }

        public void ClearMessageAndMsgSendReceive(){
            var canDelMsgID = (((from del1 in _DbContext.msgsendreceive 
                        where del1.rDelete == true && del1.sendID == 0
                        select del1.messageID).ToArray()).Except(
                        (from del0 in _DbContext.msgsendreceive 
                        where del0.rDelete == false && del0.sendID == 0
                        select del0.messageID).ToArray())).ToArray();
            var msgSR = _DbContext.msgsendreceive.Where(b=> canDelMsgID.Contains(b.messageID)).ToList();
            _DbContext.msgsendreceive.RemoveRange(msgSR);
            _DbContext.SaveChanges();
            
            var msg = _DbContext.message.Where(b=>canDelMsgID.Contains(b.ID)).ToList();
            _DbContext.message.RemoveRange(msg);
            _DbContext.SaveChanges();           
        }

        public void ClearOperateLogs(DateTime dt){
            var query = _DbContext.operateLogs.Where(b=>b.createTime < dt).ToList();
            _DbContext.operateLogs.RemoveRange(query);
            _DbContext.SaveChanges();
        }

        public void ClearPunchCardLogs(DateTime dt){
            var query = _DbContext.punchcardlogs.Where(b=>b.logDate < dt).ToList();
            _DbContext.punchcardlogs.RemoveRange(query);
            _DbContext.SaveChanges();
        }

        public void ClearPunchLogWarns(){
            var query = (from a in _DbContext.punchlogwarns
                        join b in _DbContext.punchcardlogs on a.punchLogID equals b.ID into noID
                        from c in noID.DefaultIfEmpty()
                        where c == null
                        select a).ToList();
            _DbContext.punchlogwarns.RemoveRange(query);
            _DbContext.SaveChanges();
        }
        
    }
}