using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using practice_mvc02.Repositories;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Models
{
    public class convertToNameFunction
    {
        protected DBContext _DbContext {get;set;}
        public convertToNameFunction(DBContext dbContext){
            this._DbContext = dbContext;
        }

        public Dictionary<string, string> UpdateEmployee_IDtoName(Account baseData, EmployeeDetail detailData=null){
            if(detailData == null){
                detailData = new EmployeeDetail();
            }
            var dic = new Dictionary<string, string>(){};

            var query = (from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        join c in _DbContext.grouprules on a.groupID equals c.ID
                        join d in _DbContext.worktimerules on a.timeRuleID equals d.ID into tmp
                        from e in tmp.DefaultIfEmpty()
                        where a.ID == baseData.ID
                        select new {
                            a.userName, b.department, b.position, c.groupName, e.name
                        }).FirstOrDefault();     
            
            var query02 = _DbContext.accounts
                            .Where(b=>b.ID == detailData.lastOperaAccID || b.ID == detailData.myAgentID)
                            .Select(b=>new {b.ID, b.userName}).ToList();

            dic.Add("userName", query.userName);                
            dic.Add("departName", query.department);
            dic.Add("position", query.position);
            dic.Add("groupName", query.groupName);
            dic.Add("timeRuleName", query.name==null? "不受限": query.name);
            foreach(var list in query02){
                if(list.ID == detailData.lastOperaAccID){
                    dic.Add("operateName", list.userName);
                }else if(list.ID == detailData.myAgentID){
                    dic.Add("myAgentName", list.userName);
                }
            }
            if(!dic.ContainsKey("myAgentName")){
                dic.Add("myAgentName", "無");
            }
            return dic;         
        }
        











    }//class
}