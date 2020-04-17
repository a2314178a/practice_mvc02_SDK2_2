using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class BaseRepository
    {
        protected DBContext _DbContext {get;set;}
        protected string specialName;

        public BaseRepository(DBContext dbContext)
        {
            this._DbContext = dbContext;
            this.specialName = definePara.annualName();
        }

        public string QueryTimeStamp(int? id){
            string result = "";
            var query = _DbContext.accounts.Where(u=>u.ID == id).Select(u => u.loginTime);
            if(query.Count()>0){
                result = query.ToList()[0];                                                                             
            }
            return result; 
        }       

        public object GetAccountDetail(int employeeID){
            var detail = (from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        join c in _DbContext.employeedetails on a.ID equals c.accountID 
                        where a.ID == employeeID
                        select new{
                            a.account, a.userName, a.timeRuleID, a.groupID, a.accLV,
                            departmentID=b.ID, b.department, b.position, 
                            c.sex, c.birthday, c.humanID, c.myAgentID, c.agentEnable, c.startWorkDate,
                        }).FirstOrDefault();
            return detail;
        }

        public void systemSendMessage(string name, int loginID, string type) {
            var title = "系統通知";
            var text = "";
            switch(type){
                case "punch": text = "打卡異常，如有需要請前往處理，謝謝"; break;
                case "leave": text = "辦理了請假手續，若方便請前往處理，謝謝"; break;
            }
            var content = name + text;
            var msg = new Message{title=title, content=content, createTime=definePara.dtNow()};
            _DbContext.message.Add(msg);
            _DbContext.SaveChanges();
            var msgID = msg.ID;
            if(msgID > 0){
                var arr = _DbContext.employeeprincipals.Where(b=>b.employeeID == loginID).ToList();
                var hadSend = new List<int>();
                foreach(var tmp in arr){
                    var record = new MsgSendReceive{messageID=msgID, createTime=definePara.dtNow()};
                    if(hadSend.IndexOf(tmp.principalID) == -1){
                        hadSend.Add(tmp.principalID);
                        record.receiveID = tmp.principalID;
                        _DbContext.msgsendreceive.Add(record);
                        _DbContext.SaveChanges();
                    }
                    if(hadSend.IndexOf(tmp.principalAgentID) == -1){
                        hadSend.Add(tmp.principalAgentID);
                        record.ID = 0;
                        record.receiveID = tmp.principalAgentID;
                        _DbContext.msgsendreceive.Add(record);
                        _DbContext.SaveChanges();
                    }
                }
            }
        }

        public object GetAllTimeRule(){
            object result = null;
            var query = (from a in _DbContext.worktimerules
                        orderby a.startTime
                        select new {
                            a.ID, a.name, a.startTime, a.endTime, a.sRestTime, a.eRestTime
                        });
            result = query.ToList();
            return result;
        }

        public object GetAllGroup(){
            object result = null;
            var query = from a in _DbContext.grouprules
                        orderby a.ruleParameter
                        select new {
                            a.ID, a.groupName, a.ruleParameter
                        };
            result = query.ToList();
            return result;
        }

        public int UpdateMyDetail(int loginID, MyDetail data){
            int count = 0;
            if(data.password != null){
                var context = _DbContext.accounts.FirstOrDefault(b=>b.ID == loginID);
                if(context != null){
                    context.password = data.password;
                    context.lastOperaAccID = loginID;
                    context.updateTime = definePara.dtNow();
                    count = _DbContext.SaveChanges() > 0? 1: count;
                }
            }
            var context2 = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == loginID);
            if(context2 != null){
                context2.myAgentID = data.myAgentID;
                context2.agentEnable = data.agentEnable;
                context2.lastOperaAccID = loginID;
                context2.updateTime = definePara.dtNow();
                count = _DbContext.SaveChanges() > 0? 1: count;
                setPrincipalAgent(loginID, context2.myAgentID, context2.agentEnable);
            }
            return count;
        }

        public void setPrincipalAgent(int principalID, int agentID, bool enable){
            var context = _DbContext.employeeprincipals.Where(b=>b.principalID == principalID);
            foreach(var tmp in context.ToList()){
                tmp.principalAgentID = (agentID == 0 || !enable) ? 0 : agentID;
                _DbContext.SaveChanges();
            }
        }

        //----------------------------------------------------------------------------------------
        
        
    }
}