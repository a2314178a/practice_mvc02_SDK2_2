using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;

namespace practice_mvc02.Repositories
{
    public class SetRuleRepository : BaseRepository
    {
        private AnnualLeaveRepository AnnualLeave {get;}
        public SetRuleRepository(AnnualLeaveRepository repository, DBContext dbContext):base(dbContext)
        {
            this.AnnualLeave = repository;
        }

        #region timeRule CRUD
        //in baseRepository
        /*public object GetAllTimeRule(){
            object result = null;
            var query = (from a in _DbContext.worktimerules
                        orderby a.startTime
                        select new {
                            a.ID, a.name, a.startTime, a.endTime, a.restTime
                        });
            result = query.ToList();
            return result;
        }*/
        public bool chkSameWorkTime(WorkTimeRule newRule){
            var query = _DbContext.worktimerules.FirstOrDefault(
                b=>b.startTime==newRule.startTime && b.endTime==newRule.endTime);
            return (query == null || query.ID == newRule.ID)? false : true;
        }

        public int AddTimeRule(WorkTimeRule newRule){
            int count = 0;
            try{
                _DbContext.worktimerules.Add(newRule);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(count ==1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=newRule.lastOperaAccID, active="新增", 
                    category="工作時間設定", createTime=definePara.dtNow()
                };
                toNameFn.AddUpTimeRule_convertToDic(ref dic, newRule);
                opLog.content = toNameFn.AddUpTimeRule_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int DelTimeRule(int id, int loginID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=loginID, active="刪除", 
                category="工作時間設定", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.worktimerules.FirstOrDefault(b=>b.ID == id);
            if(context != null){
                toNameFn.AddUpTimeRule_convertToDic(ref dic, context);

                _DbContext.worktimerules.Remove(context);
                count = _DbContext.SaveChanges();
            }
            if(count ==1){
                var query = _DbContext.accounts.Where(b=>b.timeRuleID == id).ToList();
                foreach(var tmp in query){
                    tmp.timeRuleID = 0;
                    tmp.lastOperaAccID  = 0;
                    tmp.updateTime = definePara.dtNow();
                    _DbContext.SaveChanges();
                }
                opLog.content = toNameFn.AddUpTimeRule_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int UpdateTimeRule(WorkTimeRule updateData){
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=updateData.lastOperaAccID, active="更新", 
                category="工作時間設定", createTime=definePara.dtNow()
            };
            int count = 0;
            try{
                var context = _DbContext.worktimerules.FirstOrDefault(b=>b.ID == updateData.ID);
                if(context != null){
                    toNameFn.AddUpTimeRule_convertToDic(ref oDic, context);

                    context.name = updateData.name;
                    context.type = updateData.type;
                    context.startTime = updateData.startTime;
                    context.endTime = updateData.endTime;
                    context.sRestTime = updateData.sRestTime;
                    context.eRestTime = updateData.eRestTime;
                    context.elasticityMin = updateData.elasticityMin;
                    context.lastOperaAccID = updateData.lastOperaAccID;
                    context.updateTime = updateData.updateTime;
                    count = _DbContext.SaveChanges();

                    if(count == 1){
                        toNameFn.AddUpTimeRule_convertToDic(ref nDic, context);
                        opLog.content = toNameFn.AddUpTimeRule_convertToText(nDic, oDic);
                        saveOperateLog(opLog);    //紀錄操作紀錄
                    }
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion  //timeRule

        //-----------------------------------------------------------------------------------------------------------

        #region Group
        //in baseRepository
        /*public object GetAllGroup(){
            object result = null;
            var query = from a in _DbContext.grouprules
                        select new {
                            a.ID, a.groupName, a.ruleParameter
                        };
            result = query.ToList();
            return result;
        }*/

        public int AddGroup(GroupRule newGroup){
            int count = 0;
            try{
                _DbContext.grouprules.Add(newGroup);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(count == 1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=newGroup.lastOperaAccID, active="新增", 
                    category="動作權限設定", createTime=definePara.dtNow()
                };
                toNameFn.AddUpGroup_covertToDic(ref dic, newGroup);
                opLog.content = toNameFn.AddUpGroup_covertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int DelGroup(int id, int loginID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=loginID, active="刪除", 
                category="動作權限設定", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.grouprules.FirstOrDefault(b=>b.ID == id);
            if(context != null){
                toNameFn.AddUpGroup_covertToDic(ref dic, context);

                _DbContext.grouprules.Remove(context);
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                opLog.content = toNameFn.AddUpGroup_covertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int UpdateGroup(GroupRule updateGroup){
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=updateGroup.lastOperaAccID, active="更新", 
                category="動作權限設定", createTime=definePara.dtNow()
            };
            int count = 0;
            try{
                var context = _DbContext.grouprules.FirstOrDefault(b=>b.ID == updateGroup.ID);
                if(context != null){    
                    toNameFn.AddUpGroup_covertToDic(ref oDic, context);

                    context.groupName = updateGroup.groupName;
                    context.ruleParameter = updateGroup.ruleParameter;
                    context.lastOperaAccID = updateGroup.lastOperaAccID;
                    context.updateTime = updateGroup.updateTime;
                    count = _DbContext.SaveChanges();
                    if(count == 1){
                        toNameFn.AddUpGroup_covertToDic(ref nDic, context);
                        opLog.content = toNameFn.AddUpGroup_covertToText(nDic, oDic);
                        saveOperateLog(opLog);    //紀錄操作紀錄
                    }
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion  //group
        
        //-----------------------------------------------------------------------------------------------------------

        #region sp date

        public object GetAllSpecialDate(){
            var query = _DbContext.specialdate.Select(b=> new{
                b.ID, b.departClass, b.date, b.status, b.note
            });
            return query.ToList();
        }

        public object GetClassDepart(){
            var query = ((from a in _DbContext.departments
                            select a.department)).Union(
                        (from b in _DbContext.worktimerules
                            select b.name));
            return query.ToList();
        }

        public int AddSpecialTime(SpecialDate newDate){
            int count = 0;
            try{
                _DbContext.specialdate.Add(newDate);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(count ==1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=newDate.lastOperaAccID, active="新增", 
                    category="特殊日期設定", createTime=definePara.dtNow()
                };
                toNameFn.AddUpSpecialTime_convertToDic(ref dic, newDate);
                opLog.content = toNameFn.AddUpSpecialTime_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int DelSpecialDate(int spDateID, int loginID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=loginID, active="刪除", 
                category="特殊日期設定", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.specialdate.FirstOrDefault(b=>b.ID == spDateID);
            if(context != null){
                toNameFn.AddUpSpecialTime_convertToDic(ref dic, context);

                _DbContext.specialdate.Remove(context);
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                opLog.content = toNameFn.AddUpSpecialTime_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int UpdateSpecialTime(SpecialDate updateData){
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=updateData.lastOperaAccID, active="更新", 
                category="特殊日期設定", createTime=definePara.dtNow()
            };
            int count = 0;
            try{
                var context = _DbContext.specialdate.FirstOrDefault(b=>b.ID == updateData.ID);
                if(context != null){
                    toNameFn.AddUpSpecialTime_convertToDic(ref oDic, context);

                    context.date = updateData.date;
                    context.departClass = updateData.departClass;
                    context.status = updateData.status;
                    context.note = updateData.note;
                    context.lastOperaAccID = updateData.lastOperaAccID;
                    context.updateTime = updateData.updateTime;
                    count = _DbContext.SaveChanges();
                    
                    if(count == 1){
                        toNameFn.AddUpSpecialTime_convertToDic(ref nDic, context);
                        opLog.content = toNameFn.AddUpSpecialTime_convertToText(nDic, oDic);
                        saveOperateLog(opLog);    //紀錄操作紀錄
                    }
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion  //sp date

        //-----------------------------------------------------------------------------------------------------

        #region leaveRule

        public object GetAllLeaveRule(){
            var query = _DbContext.leavenames.Select(b=>new{
                b.ID, b.leaveName, b.timeUnit, b.halfVal, b.enable
            });
            return query.ToList();
        }

        public int AddLeave(LeaveName data){
            int count = 0;
            var query = _DbContext.leavenames.FirstOrDefault(b=>b.leaveName==data.leaveName && b.enable == false);
            if(query != null){
                try{
                    query.enable = true;
                    query.timeUnit = data.timeUnit;
                    query.halfVal = (data.timeUnit==3? data.halfVal : false);
                    count = _DbContext.SaveChanges();
                }catch(Exception e){
                    count = ((MySqlException)e.InnerException).Number;
                }
            }else{
                count = 1062;
            }
            if(count == 1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=data.lastOperaAccID, active="新增", 
                    category="請假時間設定", createTime=definePara.dtNow()
                };
                toNameFn.AddUpLeave_convertToDic(ref dic, data);
                opLog.content = toNameFn.AddUpLeave_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int DelLeave(int leaveID, int loginID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=loginID, active="刪除", 
                category="請假時間設定", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.leavenames.FirstOrDefault(b=>b.ID == leaveID);
            if(context != null){
                toNameFn.AddUpLeave_convertToDic(ref dic, context);
                context.enable = false;
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                opLog.content = toNameFn.AddUpLeave_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }
            return count;
        }

        public int UpdateLeave(LeaveName data){
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=data.lastOperaAccID, active="更新", 
                category="請假時間設定", createTime=definePara.dtNow()
            };
            int count = 0;
            try{
                var context = _DbContext.leavenames.FirstOrDefault(b=>b.ID == data.ID);
                if(context != null){
                    var query = _DbContext.leavenames.FirstOrDefault(b=>b.leaveName == data.leaveName);
                    if(query == null || query.ID == context.ID){
                        toNameFn.AddUpLeave_convertToDic(ref oDic, context);

                        context.leaveName = data.leaveName;
                        context.timeUnit = data.timeUnit;
                        context.halfVal = (data.timeUnit==3? data.halfVal : false);
                        context.lastOperaAccID = data.lastOperaAccID;
                        context.updateTime = data.updateTime;
                        count = _DbContext.SaveChanges();
                        if(count == 1){
                            toNameFn.AddUpLeave_convertToDic(ref nDic, context);
                            opLog.content = toNameFn.AddUpLeave_convertToText(nDic, oDic);
                            saveOperateLog(opLog);    //紀錄操作紀錄
                        }
                    }else{
                        count = 1062;
                    }
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion //leaveRule

        //--------------------------------------------------------------------------------------------------

        #region spLeaveRule

        public object GetAllSpLeaveRule(){
            var query = _DbContext.annualleaverule.Select(
                b=>new{b.ID, b.seniority, b.specialDays, b.buffDays}
            ).OrderBy(b=>b.seniority);
            return query.ToList();
        }

        public int AddSpLeaveRule(AnnualLeaveRule data){
            int count = 0;
            try{
                _DbContext.annualleaverule.Add(data);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(count == 1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=data.lastOperaAccID, active="新增", 
                    category="特休天數設定", createTime=definePara.dtNow()
                };
                toNameFn.AddUpSpLeave_convertToDic(ref dic, data);
                opLog.content = toNameFn.AddUpSpLeave_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄

                if(data.ID >0){ //data.ID = new add dataID
                    AnnualLeave.refreshLowAnnualLeaveData(data);
                }
            }
            return count;
        }

        public int UpdateSpLeaveRule(AnnualLeaveRule data){
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=data.lastOperaAccID, active="更新", 
                category="特休天數設定", createTime=definePara.dtNow()
            };
            int count = 0;
            int diffSpecialDays=0, diffBuffDays=0;
            try{
                var context = _DbContext.annualleaverule.FirstOrDefault(b=>b.ID == data.ID);
                if(context != null){
                    toNameFn.AddUpSpLeave_convertToDic(ref oDic, context);

                    diffSpecialDays = data.specialDays - context.specialDays;
                    diffBuffDays = data.buffDays - context.buffDays;

                    //context.seniority = data.seniority;
                    context.specialDays = data.specialDays;
                    context.buffDays = data.buffDays;
                    context.lastOperaAccID = data.lastOperaAccID;
                    context.updateTime = data.updateTime;
                    count = _DbContext.SaveChanges();

                    if(count == 1){
                        toNameFn.AddUpSpLeave_convertToDic(ref nDic, context);
                        opLog.content = toNameFn.AddUpSpLeave_convertToText(nDic, oDic);
                        saveOperateLog(opLog);    //紀錄操作紀錄
                        AnnualLeave.UpEmployeeSpLeave(data.ID, diffSpecialDays, diffBuffDays);
                    }
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }
//新增:找使用比她小1規則的年資移除重計算, 刪除:移除後重計算, 
//修改:修改對應資料(年資改為不可修改)
        public int DelSpLeaveRule(int ruleID, int loginID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=loginID, active="刪除", 
                category="特休天數設定", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.annualleaverule.FirstOrDefault(b=>b.ID == ruleID);
            if(context != null){
                toNameFn.AddUpSpLeave_convertToDic(ref dic, context);

                _DbContext.annualleaverule.Remove(context);
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                opLog.content = toNameFn.AddUpSpLeave_convertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄

                AnnualLeave.refreshAnnualLeaveHours(context);
                AnnualLeave.DelSomeAnnualLeaveRecord(ruleID);    
            }
            return count;
        }

        #endregion  //spLeaveRule

        //--------------------------------------------------------------------------------------------------

    }
}