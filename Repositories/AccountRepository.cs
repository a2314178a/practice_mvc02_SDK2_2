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
                       join c in _DbContext.departments on a.departmentID equals c.ID into tmp
                       from cc in tmp.DefaultIfEmpty()
                       where a.account == account && a.password == password
                       select new{
                           a.ID, a.userName, a.departmentID, a.accLV, a.groupID,
                           b.ruleParameter, principalID=(cc==null? 0 : cc.principalID)
                       }).FirstOrDefault();

            if(baseQuery == null){
                var adQuery = (from a in _DbContext.accounts
                            where a.account == account && a.password == password && a.accLV == definePara.getDIMALV() 
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

        public void initLeaveNames(){
            var query = _DbContext.leavenames;
            if(query.Count()==0){
                var leaveName = new string[]{
                    "公差", "特休", "事假", "病假", "公假", "調休", "喪假", "婚假", "產假", "陪產假", "其他"};
                for(var i=0; i< leaveName.Length; i++){
                    var data = new LeaveName(){
                        ID=i+1, leaveName=leaveName[i], timeUnit=1, enable=false,
                        createTime=definePara.dtNow(), updateTime=definePara.dtNow()
                    };
                    _DbContext.leavenames.Add(data);
                    _DbContext.SaveChanges();
                }
            }
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