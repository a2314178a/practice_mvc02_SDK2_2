using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class AccountRepository : BaseRepository
    {
        public AccountRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public object Login(string account, string password)
        {
            //var query = _DbContext.accounts.FirstOrDefault(b=>b.account == account && b.password == password);  
            var baseQuery = (from a in _DbContext.accounts
                       join b in _DbContext.grouprules on a.groupID equals b.ID
                       join c in _DbContext.departments on a.departmentID equals c.ID
                       where a.account == account && a.password == password
                       select new{
                           a.ID, a.userName, a.departmentID, a.accLV, a.groupID,
                           b.ruleParameter, c.principalID
                       }).FirstOrDefault();

            if(baseQuery == null){
                var adQuery = (from a in _DbContext.accounts
                            where a.account == account && a.password == password && a.accLV == 999 
                            select new{
                                a.ID, a.userName, a.departmentID, a.accLV, a.groupID,
                                ruleParameter = 65535, principalID=0
                            }).FirstOrDefault();
                return adQuery;
            }else{
                return baseQuery;
            }
        }

        public int UpdateTimeStamp(int id, string timeStamp, DateTime updateTime)
        {
            int count = 0;
            var userContext = _DbContext.accounts.FirstOrDefault(u=>u.ID == id);
            userContext.loginTime = timeStamp;
            userContext.updateTime = updateTime;
            count = _DbContext.SaveChanges();
            return count;
        }

        public bool chkIsAgent(int id){
            var context = _DbContext.employeeprincipals.Where(b=>b.principalAgentID == id);
            return context.Count()>0? true : false;
        }

        /*public int getThisGroupRuleVal(int groupID){
            var query = _DbContext.grouprules.FirstOrDefault(b=>b.ID == groupID);
            return query == null ? 0 : query.ruleParameter;
        }
        public int getThisPrincipalID(int departID){
            var query = _DbContext.departments.FirstOrDefault(b=>b.ID == departID);
            return query == null ? 0 : query.principalID;
        }*/
        
    }
}