using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using practice_mvc02.Repositories;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;

namespace practice_mvc02.Models
{
    public class punchCardFunction
    {
        public PunchCardRepository Repository { get; }
        private ISession _session;
        private punchStatusCode psCode;
        private int? loginID;
        private int? loginDepartmentID;
        //const int lessStHour = -2;
        //private int addEtHour = 13;

        public punchCardFunction(PunchCardRepository repository, IHttpContextAccessor httpContextAccessor){
            this.Repository = repository;
            if(httpContextAccessor.HttpContext != null){
                this._session = httpContextAccessor.HttpContext.Session;
                this.loginID = _session.GetInt32("loginID");
                this.loginDepartmentID = _session.GetInt32("loginDepartmentID");
            }
            this.psCode = new punchStatusCode();
        }

        //正常打卡
        public int punchCardProcess(PunchCardLog logData, WorkTimeRule thisWorkTime, int action, int employeeID)
        {   
            WorkDateTime wt = workTimeProcess(thisWorkTime);
            //需new一個出來 用原本的logData會等於context，一旦修改logData之後query後的context也會是修改後的(雖然資料庫沒變)
            PunchCardLog newLogData;    
            int resultCount = 0; //0:操作異常 1:成功 
            
            if(logData == null) //今日皆未打卡      
            {
                newLogData = new PunchCardLog(){
                    accountID = employeeID, departmentID = (int)loginDepartmentID, logDate = wt.sWorkDt.Date,
                    lastOperaAccID = (int)loginID, onlineTime = definePara.dtNow(), createTime = definePara.dtNow()
                };    
            }
            else   //今日有打過卡 or 電腦生成(跨日才有可能遇到)
            {
                newLogData = new PunchCardLog(); 
                newLogData.ID = logData.ID;
                newLogData.accountID = logData.accountID;
                newLogData.departmentID = logData.departmentID;
                newLogData.logDate = logData.logDate;
                newLogData.onlineTime = logData.onlineTime;
                newLogData.offlineTime = logData.offlineTime;
                newLogData.punchStatus = logData.punchStatus;
                newLogData.createTime = logData.createTime;

                newLogData.lastOperaAccID = (int)loginID;
                newLogData.updateTime = definePara.dtNow();

                if(newLogData.onlineTime.Year ==1 && newLogData.offlineTime.Year ==1){
                    newLogData.onlineTime = definePara.dtNow();
                }else{
                    newLogData.offlineTime = definePara.dtNow();
                }
            }
            newLogData.punchStatus = getStatusCode(wt, newLogData);
            resultCount = newLogData.ID ==0? Repository.AddPunchCardLog(newLogData, true):Repository.UpdatePunchCard(newLogData, true);
            if(resultCount == 1){    //一定要新增log成功 不然會沒logID
                if(newLogData.punchStatus > psCode.normal && newLogData.punchStatus != psCode.takeLeave){
                    Repository.AddPunchLogWarnAndMessage(newLogData);
                }
                else if(newLogData.punchStatus == psCode.normal){
                    Repository.delPunchLogWarnAndMessage(newLogData);
                }
            }
            return resultCount;
        }

        public int forcePunchLogProcess(PunchCardLog processLog, WorkTimeRule thisWorkTime, string action, string from=""){
            WorkDateTime wt = workTimeProcess(thisWorkTime, processLog);
            var et_big_st = thisWorkTime.endTime >= thisWorkTime.startTime? true: false;
            processLog.logDate = wt.sWorkDt.Date;
            processLog.lastOperaAccID = (int)loginID;
            if(action == "update"){
                processLog.updateTime = definePara.dtNow();
            }else{
                processLog.createTime = definePara.dtNow();
            }

            if(processLog.onlineTime.Year !=1){     
                if(processLog.onlineTime < wt.sPunchDT){    //2300-0800 if write 0000-0800
                    processLog.onlineTime = processLog.onlineTime.AddDays(1);
                }
                if(processLog.onlineTime >= wt.ePunchDT){    //0000-0800 if write 2355-0800
                    processLog.onlineTime = processLog.onlineTime.AddDays(-1);
                }
            }
            if(processLog.offlineTime.Year !=1){
                if(processLog.offlineTime <= wt.sWorkDt){   //2300-0800 if write 2300-0800
                    processLog.offlineTime = processLog.offlineTime.AddDays(1); 
                }
            }
            if(et_big_st && processLog.onlineTime.Year !=1 && processLog.offlineTime.Year !=1){
                if(processLog.onlineTime >= processLog.offlineTime){
                    processLog.onlineTime = processLog.onlineTime.AddDays(-1);
                }
            }else if(!et_big_st && processLog.onlineTime.Year !=1 && processLog.offlineTime.Year !=1){
                if(processLog.offlineTime <= processLog.onlineTime){
                    processLog.offlineTime = processLog.offlineTime.AddDays(1);
                }
            }
            
            processLog.punchStatus = getStatusCode(wt, processLog);
            int result = action == "update"? Repository.UpdatePunchCard(processLog, false) : Repository.AddPunchCardLog(processLog, false);
            if(result == 1){
                if(processLog.punchStatus > psCode.normal && processLog.punchStatus != psCode.takeLeave){
                    Repository.AddPunchLogWarnAndMessage(processLog);
                }
                if(action == "update" && from == "applySign"){
                    Repository.UpdatePunchLogWarn(processLog.ID);
                }
            }
            return result;
        }
      
        public WorkDateTime workTimeProcess(WorkTimeRule thisWorkTime, PunchCardLog customLog = null){
            return Repository.workTimeProcess(thisWorkTime, customLog);
        }

        public dynamic getObjectValue(string key, object obj){
            return obj.GetType().GetProperty(key).GetValue(obj);
        }

        public void processPunchlogWarn(PunchCardLog log, WorkTimeRule thisWorkTime){   //排程用
            WorkDateTime wt = workTimeProcess(thisWorkTime, log);
            if(definePara.dtNow() < wt.ePunchDT){
                return;
            }
            log.punchStatus = getStatusCode(wt, log);
            log.lastOperaAccID = 0;
            Repository.UpdatePunchCard(log, true);
            if(log.punchStatus > 1 && log.punchStatus != psCode.takeLeave){
                Repository.AddPunchLogWarnAndMessage(log);
            }
        }

        public int getStatusCode(WorkDateTime wt, PunchCardLog processLog, LeaveOfficeApply leave=null){
            var fullDayRest = false;
            var statusCode = 0;  //statusCode:  0x01:正常 0x02:遲到 0x04:早退 0x08:加班 0x10:缺卡 0x20:曠職 0x40:請假
            if(wt.workAllTime){
                return psCode.normal;
            }
            var elasticityMin = wt.elasticityMin;
            List<LeaveOfficeApply> thisLeave = new List<LeaveOfficeApply>();
            if(leave == null){
                thisLeave = Repository.GetThisTakeLeave(processLog.accountID, wt.sWorkDt, wt.eWorkDt);
            }else{
                thisLeave.Add(leave);
            }                        
            if(thisLeave.Count >0){
                foreach(var tmp in thisLeave){
                    if(wt.sWorkDt >= tmp.startTime && wt.sWorkDt < tmp.endTime){
                        //wt.sWorkDt = tmp.endTime>=wt.sRestDt && tmp.endTime<=wt.eRestDt? wt.eRestDt: tmp.endTime;
                    }
                    if(wt.eWorkDt > tmp.startTime && wt.eWorkDt <= tmp.endTime){
                        //wt.eWorkDt = tmp.startTime;
                    }
                    fullDayRest = (tmp.startTime <= wt.sWorkDt && tmp.endTime >= wt.eWorkDt)? true : false;
                }
                statusCode |= psCode.takeLeave;
            }

            if(processLog.onlineTime.Year == 1 && processLog.offlineTime.Year == 1  //&&
                //processLog.logDate.AddDays(1) < definePara.dtNow()
                //definePara.dtNow() >= wt.ePunchDT
                ){
                statusCode = fullDayRest? statusCode : (statusCode | psCode.noWork);
            }
            else if(processLog.onlineTime.Year > 1 && processLog.offlineTime.Year > 1){
                var newStartWt = wt.sWorkDt.AddMinutes(elasticityMin +1);   //+1因遲到以時分為主 09:00:59 也不算遲到
                statusCode = processLog.onlineTime >= newStartWt? (statusCode | psCode.lateIn) : statusCode;
                var timeLen = (int)((processLog.onlineTime - wt.sWorkDt).TotalMinutes);
                timeLen = (timeLen < 0 || timeLen >elasticityMin)? 0: timeLen; 
                statusCode = processLog.offlineTime< wt.eWorkDt.AddMinutes(timeLen)? (statusCode | psCode.earlyOut):statusCode;
            }
            else{
                if(processLog.onlineTime.Year > 1){ //只有填上班
                    var newStartWt = wt.sWorkDt.AddMinutes(elasticityMin +1);
                    statusCode = processLog.onlineTime >= newStartWt? (statusCode | psCode.lateIn) : statusCode;
                    statusCode = definePara.dtNow() >= wt.ePunchDT ? (statusCode | psCode.hadLost) : statusCode; //打不到下班卡了
                }
                else{   //只有填下班
                    statusCode |= psCode.hadLost;
                    statusCode = processLog.offlineTime < wt.eWorkDt ? (statusCode | psCode.earlyOut) : statusCode;
                }
            }
            return statusCode==0? (psCode.normal) : statusCode;
        }



        
    }
}