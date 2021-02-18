using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class BaseRepository
    {
        protected DBContext _DbContext {get;set;}
        protected operateLogFunction toNameFn;
        protected String noDepartStr = definePara.noDepart();

        public BaseRepository(DBContext dbContext)
        {
            this._DbContext = dbContext;
            this.toNameFn = new operateLogFunction(dbContext);
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
                        join b in _DbContext.departments on a.departmentID equals b.ID into deTmp
                        from bb in deTmp.DefaultIfEmpty()
                        join c in _DbContext.employeedetails on a.ID equals c.accountID into tailTmp
                        from cc in tailTmp.DefaultIfEmpty()
                        where a.ID == employeeID
                        select new{
                            a.account, a.userName, a.timeRuleID, a.groupID, a.accLV,
                            departmentID=(bb==null? 0:bb.ID), 
                            department=(bb==null? noDepartStr : bb.department), 
                            position=(bb==null? null:bb.position), 
                            sex=(cc==null? 0:cc.sex),
                            birthday=(cc==null? definePara.dtNow().ToString("yyyy-MM-dd"):cc.birthday.ToString("yyyy-MM-dd")),
                            humanID=(cc==null? null:cc.humanID),
                            myAgentID=(cc==null? 0:cc.myAgentID),
                            agentEnable=(cc==null? false:cc.agentEnable),
                            startWorkDate=(cc==null? definePara.dtNow().ToString("yyyy-MM-dd"):cc.startWorkDate.ToString("yyyy-MM-dd")),
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
                    if(hadSend.IndexOf(tmp.principalID) == -1){
                        var record = new MsgSendReceive{messageID=msgID, createTime=definePara.dtNow()};
                        hadSend.Add(tmp.principalID);
                        record.receiveID = tmp.principalID;
                        _DbContext.msgsendreceive.Add(record);
                        _DbContext.SaveChanges();
                    }
                    if(tmp.principalAgentID >0 && hadSend.IndexOf(tmp.principalAgentID) == -1){
                        hadSend.Add(tmp.principalAgentID);
                        var record2 = new MsgSendReceive{messageID=msgID, createTime=definePara.dtNow()};
                        record2.receiveID = tmp.principalAgentID;
                        _DbContext.msgsendreceive.Add(record2);
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
                            a.ID, a.name, a.type, a.startTime, a.endTime, 
                            a.sRestTime, a.eRestTime, a.elasticityMin
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

            using (var trans = _DbContext.Database.BeginTransaction()){
                var oDic = new Dictionary<string,string>{};
                var nDic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=loginID, employeeID=loginID,
                    active="更新", category="員工資料", createTime=definePara.dtNow()
                };
                int psCount = 0, agCount = 0;

                try
                {
                    if(data.password != null){
                        var context = _DbContext.accounts.FirstOrDefault(b=>b.ID == loginID);
                        if(context != null){
                            context.password = data.password;
                            context.lastOperaAccID = loginID;
                            context.updateTime = definePara.dtNow();
                            psCount = _DbContext.SaveChanges();
                        }
                    }
                    var context2 = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == loginID);
                    if(context2 != null){
                        toNameFn.AddUpEmployeeDetail_convertToDic(ref oDic, context2);

                        context2.myAgentID = data.myAgentID;
                        context2.agentEnable = data.agentEnable;
                        context2.lastOperaAccID = loginID;
                        context2.updateTime = definePara.dtNow();
                        agCount = _DbContext.SaveChanges();
                        setPrincipalAgent(loginID, context2.myAgentID, context2.agentEnable);
                    }
                    if(psCount == 1 || agCount == 1){    
                        if(agCount == 1){
                            toNameFn.AddUpEmployeeDetail_convertToDic(ref nDic, context2);
                        }
                        opLog.content += psCount==0?"" : $"修改了密碼，";
                        opLog.content += (agCount == 1? toNameFn.AddUpEmployeeDetail_convertTotext(nDic, oDic) : "");
                        saveOperateLog(opLog);   
                    }
                    trans.Commit();
                }
                catch (Exception ex){
                    recordError(ex);
                    return 0;
                }
                return (psCount == 1 || agCount == 1)? 1 : 0;
            }
        }

        public void setPrincipalAgent(int principalID, int agentID, bool enable){
            var context = _DbContext.employeeprincipals.Where(b=>b.principalID == principalID);
            foreach(var tmp in context.ToList()){
                tmp.principalAgentID = (agentID == 0 || !enable) ? 0 : agentID;
                _DbContext.SaveChanges();
            }
        }

        //----------------------------------------------------------------------------------------

        public void saveOperateLog(OperateLog log){
            /*string docPath = Environment.CurrentDirectory;
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "operateLogs.txt"), true)){
                outputFile.WriteLineAsync(log.createTime.ToString("yyyy-MM-dd HH:mm:ss") + " " + log.content);
            }*/
            _DbContext.operateLogs.Add(log);
            _DbContext.SaveChanges();
        }

        public async void recordError(Exception ex){
            string docPath = Environment.CurrentDirectory;
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "errorLog.txt"), true)){
                await outputFile.WriteLineAsync(definePara.dtNow() + "  " + ex.ToString() + "\r\n");
            }
        }

        public int catchErrorProcess(Exception ex, int oldRes){
            recordError(ex);
            try{
                return oldRes == 1? 0 : ((MySqlException)ex.InnerException).Number;
            }
            catch (System.Exception){
                return 0;
            } 
        }
        
    }
}