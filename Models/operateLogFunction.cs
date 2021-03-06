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
    public class operateLogFunction
    {
        protected DBContext _DbContext {get;set;}
        public operateLogFunction(DBContext dbContext){
            this._DbContext = dbContext;
        }

        public string GetNameByID(int opID){
            var query = (from a in _DbContext.accounts
                        where a.ID == opID
                        select a.userName).FirstOrDefault();
            return query;
        }

        public void AddUpEmployee_covertToDic(ref Dictionary<string, string> Dic, Account baseData, EmployeeDetail detailData){
            var noDepart = definePara.noDepart();
            var query = (from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID into deTmp
                        from bb in deTmp.DefaultIfEmpty()
                        join c in _DbContext.grouprules on a.groupID equals c.ID
                        join d in _DbContext.worktimerules on a.timeRuleID equals d.ID into tmp
                        from e in tmp.DefaultIfEmpty()
                        where a.ID == baseData.ID
                        select new {
                            department=(bb==null? noDepart : bb.department),
                            position=(bb==null? " " : bb.position),
                            c.groupName, 
                            name=(e==null? "不受限" : e.name) 
                        }).FirstOrDefault();
            if(detailData == null){
                detailData = new EmployeeDetail();
            }

            Dic.Add("account", baseData.account);
            Dic.Add("password", baseData.password);
            Dic.Add("userName", baseData.userName);
            Dic.Add("accLV", baseData.accLV.ToString());
            Dic.Add("departName", query==null? "" : query.department);
            Dic.Add("position", query==null? "" : query.position);
            Dic.Add("groupName", query==null? "" : query.groupName);
            Dic.Add("timeRuleName", query==null? "" : query.name);
            Dic.Add("sex", detailData.sex==0? "女": detailData.sex==1? "男":"其他");
            Dic.Add("birthday", detailData.birthday.ToString("yyyy-MM-dd"));
            Dic.Add("humanID", detailData.humanID);
            Dic.Add("sWorkDate", detailData.startWorkDate.ToString("yyyy-MM-dd"));
            Dic.Add("myAgentID", detailData.myAgentID.ToString());
            Dic.Add("myAgent", detailData.myAgentID==0? "無" : GetNameByID(detailData.myAgentID));
            Dic.Add("agentEnable", detailData.agentEnable? "啟用" : "停用");
        }

        public string AddUpEmployee_covertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "帳號:"+ (nDic["account"]==oDic["account"]? $"{nDic["account"]}，" : $"{oDic["account"]}=>{nDic["account"]}，");
            txt += "密碼:"+ (nDic["password"]==oDic["password"]? "秘密，" : $"變更了密碼，");
            txt += "姓名:"+ (nDic["userName"]==oDic["userName"]? $"{nDic["userName"]}，" : $"{oDic["userName"]}=>{nDic["userName"]}，");
            txt += "性別:"+ (nDic["sex"]==oDic["sex"]? $"{nDic["sex"]}，" : $"{oDic["sex"]}=>{nDic["sex"]}，");
            txt += "身分證號:"+ (nDic["humanID"]==oDic["humanID"]? $"{nDic["humanID"]}，" : $"{oDic["humanID"]}=>{nDic["humanID"]}，");  
            txt += "生日:"+ (nDic["birthday"]==oDic["birthday"]? $"{nDic["birthday"]}，" : $"{oDic["birthday"]}=>{nDic["birthday"]}，");
            txt += "報到日:"+ (nDic["sWorkDate"]==oDic["sWorkDate"]? $"{nDic["sWorkDate"]}，" : $"{oDic["sWorkDate"]}=>{nDic["sWorkDate"]}，");
            if(nDic["departName"] != oDic["departName"] || nDic["position"] != oDic["position"]){
                txt += $"部門職位:{oDic["departName"]}{oDic["position"]}=>{nDic["departName"]}{nDic["position"]}，";
            }else{
                txt += $"部門職位:{nDic["departName"]}{nDic["position"]}，";
            }
            if(nDic.ContainsKey("principal") && oDic.ContainsKey("principal")){
                txt += "負責人:"+ (nDic["principal"]==oDic["principal"]? $"{nDic["principal"]}，" : $"{oDic["principal"]}=>{nDic["principal"]}，");
            }
            
            txt += "職務代理人:"+ (nDic["myAgentID"]==oDic["myAgentID"]? $"{nDic["myAgent"]}，" : $"{oDic["myAgent"]}=>{nDic["myAgent"]}，");
            txt += "代理人授權:"+ (nDic["agentEnable"]==oDic["agentEnable"]? $"{nDic["agentEnable"]}，" : $"{oDic["agentEnable"]}=>{nDic["agentEnable"]}，");
            txt += "動作權限:"+ (nDic["groupName"]==oDic["groupName"]? $"{nDic["groupName"]}，" : $"{oDic["groupName"]}=>{nDic["groupName"]}，");
            txt += "帳號等級:"+ (nDic["accLV"]==oDic["accLV"]? $"{nDic["accLV"]}，" : $"{oDic["accLV"]}=>{nDic["accLV"]}，");
            txt += "工作班別:"+ (nDic["timeRuleName"]==oDic["timeRuleName"]? $"{nDic["timeRuleName"]}，" : $"{oDic["timeRuleName"]}=>{nDic["timeRuleName"]}，");

            return txt.Substring(0,txt.Length - 1);
        }

        public void GetEmployeePrincipalName(List<EmployeePrincipal> query, ref Dictionary<string, string> Dic){
            var principal = "";
            foreach(var data in query){
                var name = (from a in _DbContext.accounts
                            join b in _DbContext.employeeprincipals on a.ID equals b.principalID
                            where a.ID == data.principalID
                            select a.userName).FirstOrDefault();
                principal += name + "、";
            }
            Dic.Add("principal", principal=="" ? "無": principal.Substring(0,principal.Length - 1));
        }
        
        public void AddUpDepartPosition_covertToDic(ref Dictionary<string, string> Dic, Department newData){
            if(newData == null){
                newData = new Department();
            }
            Dic.Add("depart", newData.department);
            Dic.Add("position", newData.position);
            Dic.Add("principalID", newData.principalID.ToString()); //記ID 因為姓名有可能一樣
            Dic.Add("principalName", newData.principalID<=0? "無" : GetNameByID(newData.principalID));
        }

        public string AddUpDepartPosition_covertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            if(nDic["depart"]!=oDic["depart"] || nDic["position"]!=oDic["position"]){
                txt += $"部門職位:{oDic["depart"]}{oDic["position"]}=>{nDic["depart"]}{nDic["position"]}，";
            }else{
                txt += $"部門職位:{nDic["depart"]}{nDic["position"]}，";
            }
            txt += "主管:"+ (nDic["principalID"]==oDic["principalID"]? $"{nDic["principalName"]}，" : $"{oDic["principalName"]}=>{nDic["principalName"]}，");
            return txt.Substring(0,txt.Length - 1);
        }
 
        public void IsAgreeApplyLeave_convertToDic(ref Dictionary<string, string> Dic, 
                                                    LeaveOfficeApply leave, String leaveName){
            if(leave == null){
                leave = new LeaveOfficeApply();
            }
            Dic.Add("leaveName", leaveName);
            Dic.Add("applyStatus", leave.applyStatus==0? "待審核" : leave.applyStatus==1? "通過" : "未通過");
            Dic.Add("applyDate", leave.createTime.ToString("yyyy-MM-dd HH:mm"));
            Dic.Add("startTime", leave.startTime.ToString("yyyy-MM-dd HH:mm"));
            Dic.Add("endTime", leave.endTime.ToString("yyyy-MM-dd HH:mm"));
        }

        public string IsAgreeApplyLeave_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt="";
            txt += $"申請日期:{nDic["applyDate"]}，";
            txt += $"請假名稱:{nDic["leaveName"]}，";
            txt += $"請假時間:{nDic["startTime"]}~{nDic["endTime"]}，";
            txt += $"結果:{oDic["applyStatus"]}=>{nDic["applyStatus"]}，";
            return txt.Substring(0,txt.Length - 1);
        }

        public void IsAgreeOvertime_convertToDic(ref Dictionary<string, string> Dic, OvertimeApply apply){                             
            if(apply == null){
                apply = new OvertimeApply();
            }
            Dic.Add("applyDate", apply.createTime.ToString("yyyy-MM-dd HH:mm"));
            Dic.Add("workDate", apply.workDate.ToString("yyyy-MM-dd"));
            Dic.Add("timeLength", apply.timeLength.ToString());
            Dic.Add("applyStatus", apply.applyStatus==0? "待審核" : apply.applyStatus==1? "通過" : "未通過");   
        }

        public string IsAgreeOvertime_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt="";
            txt += $"申請日期:{nDic["applyDate"]}，";
            txt += $"加班工作日:{nDic["workDate"]}，";
            txt += $"加班時間長度:{nDic["timeLength"]}分鐘，";
            txt += $"結果:{oDic["applyStatus"]}=>{nDic["applyStatus"]}，";
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpApplyLeave_convertToDic(ref Dictionary<string, string> Dic, LeaveOfficeApply data){
            var query = _DbContext.leavenames.FirstOrDefault(b=>b.ID==data.leaveID);
            if(query == null){
                query = new LeaveName();
            }
            data = (data==null? new LeaveOfficeApply() : data);
            Dic.Add("note", data.note==null? "無" : data.note);
            Dic.Add("sDate", data.startTime.ToString("yyyy-MM-dd HH:mm"));
            Dic.Add("eDate", data.endTime.ToString("yyyy-MM-dd HH:mm"));
            Dic.Add("applyDate", data.createTime.ToString("yyyy-MM-dd HH:mm"));
            Dic.Add("leaveName", query.leaveName);
        }

        public string AddUpApplyLeave_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += $"申請時間：{oDic["applyDate"]}，";
            txt += "請假名稱:"+ (nDic["leaveName"]==oDic["leaveName"]? $"{nDic["leaveName"]}，" : $"{oDic["leaveName"]}=>{nDic["leaveName"]}，");
            if(nDic["sDate"]!=oDic["sDate"] || nDic["eDate"]!=oDic["eDate"]){
                txt += $"請假時間:{oDic["sDate"]}~{oDic["eDate"]}=>{nDic["sDate"]}~{nDic["eDate"]}，";
            }else{
                txt += $"請假時間:{nDic["sDate"]}~{nDic["eDate"]}，";
            }
            txt += "備註:"+ (nDic["note"]==oDic["note"]? $"{nDic["note"]}，" : $"{oDic["note"]}=>{nDic["note"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpPunchCardLog_convertToDic(ref Dictionary<string, string> Dic, PunchCardLog data){
            var code = new punchStatusCode();
            var status = " ";
            data = (data==null? new PunchCardLog() : data);
            status = (data.punchStatus & code.lateIn)>0 ? status+="遲到/" : status;
            status = (data.punchStatus & code.earlyOut)>0 ? status+="早退/" : status;
            status = (data.punchStatus & code.overtime)>0 ? status+="加班/" : status;
            status = (data.punchStatus & code.hadLost)>0 ? status+="缺卡/" : status;
            status = (data.punchStatus & code.takeLeave)>0 ? status+="請假/" : status;
            status = (data.punchStatus & code.noWork)>0 ? status+="曠職" : status;
            status = (data.punchStatus & code.normal)>0 && status == " " ? status+="正常" : status;
            status = status[status.Length-1] == '/' ? status.Substring(0, status.Length -1) :status; 
            Dic.Add("operateID", data.lastOperaAccID.ToString());
            Dic.Add("accID", data.accountID.ToString());
            Dic.Add("logDate", data.logDate.ToString("yyyy-MM-dd"));
            Dic.Add("onTime", data.onlineTime.Year==1?"無" : data.onlineTime.ToString("HH:mm:ss"));
            Dic.Add("offTime", data.offlineTime.Year==1?"無" : data.offlineTime.ToString("HH:mm:ss"));
            Dic.Add("status", status);
        }

        public string AddUpPunchCardLog_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += $"日期:{nDic["logDate"]}，";
            txt += "上班打卡時間:"+ (nDic["onTime"]==oDic["onTime"]? $"{nDic["onTime"]}，" : $"{oDic["onTime"]}=>{nDic["onTime"]}，");
            txt += "下班打卡時間:"+ (nDic["offTime"]==oDic["offTime"]? $"{nDic["offTime"]}，" : $"{oDic["offTime"]}=>{nDic["offTime"]}，");
            txt += "打卡狀態:"+ (nDic["status"]==oDic["status"]? $"{nDic["status"]}，" : $"{oDic["status"]}=>{nDic["status"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        #region setRule

        public void AddUpTimeRule_convertToDic(ref Dictionary<string, string> Dic, WorkTimeRule data){
            data = (data==null? new WorkTimeRule() : data);
            Dic.Add("ruleName", data.name);
            Dic.Add("type", data.type==1? "排休制":"固定制");
            Dic.Add("sWorkTime", data.startTime.ToString(@"hh\:mm"));
            Dic.Add("eWorkTime", data.endTime.ToString(@"hh\:mm"));
            Dic.Add("sRestTime", data.sRestTime.ToString(@"hh\:mm"));
            Dic.Add("eRestTime", data.eRestTime.ToString(@"hh\:mm"));
            Dic.Add("elasticityMin", data.elasticityMin.ToString());
        }

        public string AddUpTimeRule_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "班別名稱:"+ (nDic["ruleName"]==oDic["ruleName"]? $"{nDic["ruleName"]}，" : $"{oDic["ruleName"]}=>{nDic["ruleName"]}，");
            txt += "類型:"+ (nDic["type"]==oDic["type"]? $"{nDic["type"]}，" : $"{oDic["type"]}=>{nDic["type"]}，");
            if(nDic["sWorkTime"]!=oDic["sWorkTime"] || nDic["eWorkTime"]!=oDic["eWorkTime"]){
                txt += $"工作時間:{oDic["sWorkTime"]}~{oDic["eWorkTime"]}=>{nDic["sWorkTime"]}~{nDic["eWorkTime"]}，";
            }else{
                txt += $"工作時間:{nDic["sWorkTime"]}~{nDic["eWorkTime"]}，";
            }
            if(nDic["sRestTime"]!=oDic["sRestTime"] || nDic["eRestTime"]!=oDic["eRestTime"]){
                txt += $"休息時間:{oDic["sRestTime"]}~{oDic["eRestTime"]}=>{nDic["sRestTime"]}~{nDic["eRestTime"]}，";
            }else{
                txt += $"休息時間:{nDic["sRestTime"]}~{nDic["eRestTime"]}，";
            }
            txt += "彈性時間:"+ (nDic["elasticityMin"]==oDic["elasticityMin"]? $"{nDic["elasticityMin"]}，" : $"{oDic["elasticityMin"]}=>{nDic["elasticityMin"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpSpecialTime_convertToDic(ref Dictionary<string, string> Dic, SpecialDate data){
            data = (data==null? new SpecialDate() : data);
            Dic.Add("date", data.date.ToString("yyyy-MM-dd"));
            Dic.Add("departClass", data.departClass);
            Dic.Add("status", data.status==1 ? "休假" : "上班");
            Dic.Add("note", data.note==null ? "無" : data.note);
        }

        public string AddUpSpecialTime_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "日期:"+ (nDic["date"]==oDic["date"]? $"{nDic["date"]}，" : $"{oDic["date"]}=>{nDic["date"]}，");
            txt += "班別部門:"+ (nDic["departClass"]==oDic["departClass"]? $"{nDic["departClass"]}，" : $"{oDic["departClass"]}=>{nDic["departClass"]}，");
            txt += "狀態:"+ (nDic["status"]==oDic["status"]? $"{nDic["status"]}，" : $"{oDic["status"]}=>{nDic["status"]}，");
            txt += "備註:"+ (nDic["note"]==oDic["note"]? $"{nDic["note"]}，" : $"{oDic["note"]}=>{nDic["note"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpLeave_convertToDic(ref Dictionary<string, string> Dic, LeaveName data){
            data = (data==null? new LeaveName() : data);
            Dic.Add("leaveName", data.leaveName);
            Dic.Add("timeUnit", (data.timeUnit==1? "全天":data.timeUnit==2? "半天":"小時"));
            Dic.Add("halfVal", (data.halfVal? "半小時" : "一小時"));
        }

        public string AddUpLeave_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "請假名稱:"+ (nDic["leaveName"]==oDic["leaveName"]? $"{nDic["leaveName"]}，" : $"{oDic["leaveName"]}=>{nDic["leaveName"]}，");
            txt += "請假時間單位:"+ (nDic["timeUnit"]==oDic["timeUnit"]? $"{nDic["timeUnit"]}，" : $"{oDic["timeUnit"]}=>{nDic["timeUnit"]}，");
            txt += "小時單位:"+ (nDic["halfVal"]==oDic["halfVal"]? $"{nDic["halfVal"]}，" : $"{oDic["halfVal"]}=>{nDic["halfVal"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpSpLeave_convertToDic(ref Dictionary<string, string> Dic, AnnualLeaveRule data){
            data = (data==null? new AnnualLeaveRule() : data);
            Dic.Add("seniority", data.seniority==0.5 ? "6個月" : (data.seniority.ToString()+"年"));
            Dic.Add("spDays", data.specialDays.ToString());
            Dic.Add("buffDays", data.buffDays.ToString());
        }

        public string AddUpSpLeave_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "年資:"+ (nDic["seniority"]==oDic["seniority"]? $"{nDic["seniority"]}，" : $"{oDic["seniority"]}=>{nDic["seniority"]}，");
            txt += "特休天數:"+ (nDic["spDays"]==oDic["spDays"]? $"{nDic["spDays"]}，" : $"{oDic["spDays"]}=>{nDic["spDays"]}，");
            txt += "緩衝天數:"+ (nDic["buffDays"]==oDic["buffDays"]? $"{nDic["buffDays"]}，" : $"{oDic["buffDays"]}=>{nDic["buffDays"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpGroup_covertToDic(ref Dictionary<string, string> Dic, GroupRule data){
            var code =  new groupRuleCode();     
            data = (data==null? new GroupRule() : data);
            Dic.Add("groupName", data.groupName);
            Dic.Add("rulePara", data.ruleParameter.ToString());
            Dic.Add("baseFn", (data.ruleParameter & code.baseActive)>0 ? "啟用" : "停用");  //打卡/紀錄/請假/外出
            Dic.Add("editPunLog", (data.ruleParameter & code.editPunchLog)>0 ? "啟用" : "停用");    //編輯出勤紀錄
            Dic.Add("departEm", (data.ruleParameter & code.departEmployeeList)>0 ? "啟用" : "停用");    //查看部門員工
            Dic.Add("allEm", (data.ruleParameter & code.allEmployeeList)>0 ? "啟用" : "停用");   //查看所有員工      
            Dic.Add("editEm", (data.ruleParameter & code.employeeEdit)>0 ? "啟用" : "停用");    //編輯員工
            Dic.Add("setDepart", (data.ruleParameter & code.departmentList)>0 ? "啟用" : "停用");   //部門職位相關
            Dic.Add("setRule", (data.ruleParameter & code.setRule)>0 ? "啟用" : "停用");    //設定規則
            Dic.Add("applySign", (data.ruleParameter & code.applySign)>0 ? "啟用" : "停用");    //相關審核
            Dic.Add("adminFn", (data.ruleParameter & code.adminFn)>0 ? "啟用" : "停用");    //開發者用功能
        }

        public string AddUpGroup_covertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "群組名:"+ (nDic["groupName"]==oDic["groupName"]? $"{nDic["groupName"]}，" : $"{oDic["groupName"]}=>{nDic["groupName"]}，");
            txt += "基本功能:"+ (nDic["baseFn"]==oDic["baseFn"]? $"{nDic["baseFn"]}，" : $"{oDic["baseFn"]}=>{nDic["baseFn"]}，");
            txt += "編輯出勤紀錄:"+ (nDic["editPunLog"]==oDic["editPunLog"]? $"{nDic["editPunLog"]}，" : $"{oDic["editPunLog"]}=>{nDic["editPunLog"]}，");
            txt += "查看部門員工:"+ (nDic["departEm"]==oDic["departEm"]? $"{nDic["departEm"]}，" : $"{oDic["departEm"]}=>{nDic["departEm"]}，");
            txt += "查看所有員工:"+ (nDic["allEm"]==oDic["allEm"]? $"{nDic["allEm"]}，" : $"{oDic["allEm"]}=>{nDic["allEm"]}，");
            txt += "編輯員工:"+ (nDic["editEm"]==oDic["editEm"]? $"{nDic["editEm"]}，" : $"{oDic["editEm"]}=>{nDic["editEm"]}，");
            txt += "設定部門職位:"+ (nDic["setDepart"]==oDic["setDepart"]? $"{nDic["setDepart"]}，" : $"{oDic["setDepart"]}=>{nDic["setDepart"]}，");
            txt += "相關審核:"+ (nDic["applySign"]==oDic["applySign"]? $"{nDic["applySign"]}，" : $"{oDic["applySign"]}=>{nDic["applySign"]}，");
            txt += "設定規則:"+ (nDic["setRule"]==oDic["setRule"]? $"{nDic["setRule"]}，" : $"{oDic["setRule"]}=>{nDic["setRule"]}，");
            txt += "管理功能:"+ (nDic["adminFn"]==oDic["adminFn"]? $"{nDic["adminFn"]}，" : $"{oDic["adminFn"]}=>{nDic["adminFn"]}，");

            return txt.Substring(0,txt.Length - 1);
        }

        #endregion setRule

        public void AddUpEmployeeAnnualDays_convertToDic(ref Dictionary<string, string> Dic, EmployeeAnnualLeave data){
            var query = (from a in _DbContext.employeeannualleaves
                        join b in _DbContext.annualleaverule on a.ruleID equals b.ID
                        where a.ID == data.ID
                        select new{
                            b.seniority
                        }).FirstOrDefault();
            data = (data==null? new EmployeeAnnualLeave() : data);

            Dic.Add("seniority", query==null? "不存在" : query.seniority==0.5? "6個月" : $"{query.seniority}年");
            Dic.Add("spDays", data.specialDays.ToString());

            var dayToHour = definePara.dayToHour();
            var day = (int)data.remainHours/dayToHour;
            var hour = (int)data.remainHours%dayToHour;
            Dic.Add("remain", $"{day}天" + (hour==0? "": $"{hour}小時")); 
            Dic.Add("deadLine", data.deadLine.ToString("yyyy-MM-dd"));          
        }

        public string AddUpEmployeeAnnualDays_convertToText(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "年資:"+ (nDic["seniority"]==oDic["seniority"]? $"{nDic["seniority"]}，" : $"{oDic["seniority"]}=>{nDic["seniority"]}，");
            txt += "特休天數:"+ (nDic["spDays"]==oDic["spDays"]? $"{nDic["spDays"]}，" : $"{oDic["spDays"]}=>{nDic["spDays"]}，");
            txt += "餘額:"+ (nDic["remain"]==oDic["remain"]? $"{nDic["remain"]}，" : $"{oDic["remain"]}=>{nDic["remain"]}，");
            txt += "期限:"+ (nDic["deadLine"]==oDic["deadLine"]? $"{nDic["deadLine"]}，" : $"{oDic["deadLine"]}=>{nDic["deadLine"]}，");
            return txt.Substring(0,txt.Length - 1);
        }

        public void AddUpEmployeeDetail_convertToDic(ref Dictionary<string, string> Dic, EmployeeDetail data){
            data = (data==null? new EmployeeDetail() : data);
            var oldAgName = GetNameByID(data.myAgentID);
            Dic.Add("agName", oldAgName==null? "無" : oldAgName);
            Dic.Add("agEnable", data.agentEnable? "啟用" : "停用");      
        }

        public string AddUpEmployeeDetail_convertTotext(Dictionary<string, string> nDic, Dictionary<string, string> oDic=null){
            if(oDic == null){
                oDic = nDic;
            }
            var txt ="";
            txt += "職務代理人:"+ (nDic["agName"]==oDic["agName"]? $"{nDic["agName"]}，" : $"{oDic["agName"]}=>{nDic["agName"]}，");
            txt += "代理人授權:"+ (nDic["agEnable"]==oDic["agEnable"]? $"{nDic["agEnable"]}，" : $"{oDic["agEnable"]}=>{nDic["agEnable"]}，");
            return txt.Substring(0,txt.Length - 1);
        }
        
        public void SendMessage_convertToDic(ref Dictionary<string, string> Dic, MsgSendReceive data){
            var query = (from a in _DbContext.msgsendreceive
                        join b in _DbContext.message on a.messageID equals b.ID
                        where a.ID == data.ID
                        select new{
                            b.title, b.content
                        }).FirstOrDefault();
            Dic.Add("title", query==null? "" : query.title);
            Dic.Add("content", query==null? "" : query.content);
        }

        public string SendMessage_convertToText(Dictionary<string, string> nDic){
            var txt ="";
            txt += "標題:"+  $"{nDic["title"]}，" ;
            txt += "內容:"+  $"{nDic["content"]}，";
            return txt.Substring(0,txt.Length - 1);
        }




    }//class
}