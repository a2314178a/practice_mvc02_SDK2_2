using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using practice_mvc02.Repositories;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Models
{
    public class punchCardFunction
    {
        public PunchCardRepository Repository { get; }
        private ISession _session;
        private punchStatusCode psCode;
        private int? loginID;
        private int? loginDepartmentID;
        const int lessStHour = -2;
        const int addEtHour = 13;

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
            int resultCount = 0; //0:操作異常 1:成功 2:上班已打卡 3:現在時段不能打下班卡 4:不能補打上班時間
            
            if(logData == null) //今日皆未打卡      
            {
                logData = new PunchCardLog(){
                    accountID = employeeID, departmentID = (int)loginDepartmentID, logDate = wt.sWorkDt.Date,
                    lastOperaAccID = (int)loginID, createTime = DateTime.Now
                };

                if(action == 0){    //下班
                    if(DateTime.Now >= wt.sPunchDT && DateTime.Now <= wt.sWorkDt){
                        return 3;
                    }
                    logData.offlineTime = DateTime.Now;
                }else if(action == 1){  //上班
                    if(DateTime.Now >= wt.eWorkDt){
                        return 4;
                    }
                    logData.onlineTime = DateTime.Now;
                }else{
                    return 0;
                }    
            }
            else   //今日有打過卡 or 電腦生成(跨日才有可能遇到)
            {
                logData.lastOperaAccID = (int)loginID;
                logData.updateTime = DateTime.Now;
                if(action == 0) //打下班卡
                {   
                    if(DateTime.Now >= wt.sPunchDT && DateTime.Now <= wt.sWorkDt && logData.onlineTime.Year == 1){
                        return 3;   //現在時段不能打下班卡
                    }else{
                        logData.offlineTime = DateTime.Now; 
                    }
                }
                else if(action == 1)  //打上班卡
                {  
                    if(logData.onlineTime.Year != 1){    //已打上班卡
                        return 2;   //上班已打卡
                    }else{  
                        if(DateTime.Now >= wt.eWorkDt || logData.offlineTime.Year !=1){
                            return 4;   //不能補打上班時間
                        }else{
                            logData.onlineTime = DateTime.Now;                           
                        }
                    }
                }else{
                    return 0;
                } 
            }//Repository.AddPunchCardLog(logData);
            logData.punchStatus = getStatusCode(wt, logData);
            resultCount = logData.ID ==0? Repository.AddPunchCardLog(logData):Repository.UpdatePunchCard(logData);
            if(resultCount == 1 && logData.punchStatus > 1 && logData.punchStatus != psCode.takeLeave){    //一定要新增log成功 不然會沒logID
                Repository.AddPunchLogWarnAndMessage(logData);
            }
            return resultCount;
        }

        public int forcePunchLogProcess(PunchCardLog processLog, WorkTimeRule thisWorkTime, string action, string from=""){
            WorkDateTime wt = workTimeProcess(thisWorkTime, processLog);
            processLog.logDate = wt.sWorkDt.Date;
            processLog.lastOperaAccID = (int)loginID;
            if(action == "update"){
                processLog.updateTime = DateTime.Now;
            }else{
                processLog.createTime = DateTime.Now;
            }
            
            if(processLog.onlineTime.Year !=1){
                if(processLog.onlineTime < wt.sPunchDT){
                    processLog.onlineTime = processLog.onlineTime.AddDays(1);
                }
            }
            if(processLog.offlineTime.Year !=1){
                if(processLog.offlineTime <= wt.sWorkDt){
                    processLog.offlineTime = processLog.offlineTime.AddDays(1); 
                }
            }

            processLog.punchStatus = getStatusCode(wt, processLog);
            int result = action == "update"? Repository.UpdatePunchCard(processLog) : Repository.AddPunchCardLog(processLog);
            if(result == 1 && processLog.punchStatus > 1 && processLog.punchStatus != psCode.takeLeave){
                Repository.AddPunchLogWarnAndMessage(processLog);
            }
            if(action == "update" && from == "applySign" && result==1){
                Repository.UpdatePunchLogWarn(processLog.ID);
            }
            return result;
        }
      
        public WorkDateTime workTimeProcess(WorkTimeRule thisWorkTime, PunchCardLog customLog = null){
            var wt = new WorkDateTime();
            wt.sWorkDt = DateTime.Now.Date;  //online work dateTime
            wt.eWorkDt = DateTime.Now.Date;   //offline work dateTime
            wt.sPunchDT = DateTime.Now.Date;  //可打卡時間
            wt.ePunchDT = DateTime.Now.Date;  //
            var sRest_start = new TimeSpan(0);
            var eRest_sRest = new TimeSpan(0);
            
            if(customLog != null){
                if(customLog.onlineTime.Year !=1){
                    wt.sWorkDt = wt.eWorkDt = wt.sPunchDT = wt.ePunchDT = customLog.onlineTime.Date;
                }else if(customLog.offlineTime.Year !=1){
                    wt.sWorkDt = wt.eWorkDt = wt.sPunchDT = wt.ePunchDT = customLog.offlineTime.Date;
                }else if(customLog.logDate.Year !=1){
                    wt.sWorkDt = wt.eWorkDt = wt.sPunchDT = wt.ePunchDT = customLog.logDate.Date;
                }
            }                             
            if(thisWorkTime != null)
            { 
                sRest_start = thisWorkTime.sRestTime - thisWorkTime.startTime;
                sRest_start = sRest_start.TotalSeconds <0? sRest_start.Add(new TimeSpan(1, 0, 0, 0)) : sRest_start;
                eRest_sRest = thisWorkTime.eRestTime - thisWorkTime.sRestTime;
                eRest_sRest = eRest_sRest.TotalSeconds <0? eRest_sRest.Add(new TimeSpan(1, 0, 0, 0)) : eRest_sRest;
                wt.workAllTime = false;    
                wt.sWorkDt = wt.sWorkDt + thisWorkTime.startTime;  
                wt.eWorkDt = wt.eWorkDt + thisWorkTime.endTime; 
                wt.eWorkDt = wt.eWorkDt <= wt.sWorkDt ? wt.eWorkDt.AddDays(1) : wt.eWorkDt;  
                wt.sPunchDT = wt.sWorkDt.AddHours(lessStHour);
                wt.ePunchDT = wt.eWorkDt.AddHours(addEtHour);
                if(customLog == null){
                    if(DateTime.Now >= wt.ePunchDT){
                        wt.sPunchDT = wt.sPunchDT.AddDays(1);
                        wt.ePunchDT = wt.ePunchDT.AddDays(1);
                        wt.sWorkDt = wt.sPunchDT.AddHours((lessStHour*-1));
                        wt.eWorkDt = wt.sPunchDT.AddHours((addEtHour*-1));
                    }else if(DateTime.Now < wt.sPunchDT){
                        wt.sPunchDT = wt.sPunchDT.AddDays(-1);   
                        wt.ePunchDT = wt.ePunchDT.AddDays(-1);
                        wt.sWorkDt = wt.sPunchDT.AddHours((lessStHour*-1));
                        wt.eWorkDt = wt.sPunchDT.AddHours((addEtHour*-1));   
                    }
                }
                
            }else{
                wt.workAllTime = true;
                wt.eWorkDt = wt.eWorkDt.AddDays(1);
                wt.ePunchDT = wt.ePunchDT.AddDays(1);
            }
            wt.sRestDt = wt.sWorkDt.AddMinutes(sRest_start.TotalMinutes);
            wt.eRestDt = wt.sRestDt.AddMinutes(eRest_sRest.TotalMinutes);
            return wt;
        }

        public object workTimeProcess02 (WorkTimeRule thisWorkTime, PunchCardLog customLog = null){
            //無使用 等想到更好的寫法 來取代workTimeProcess()
            return null;
        }

        public dynamic getObjectValue(string key, object obj){
            return obj.GetType().GetProperty(key).GetValue(obj);
        }

        public void processPunchlogWarn(PunchCardLog log, WorkTimeRule thisWorkTime){   //排程用
            WorkDateTime wt = workTimeProcess(thisWorkTime, log);
            if(DateTime.Now < wt.ePunchDT){
                return;
            }
            log.punchStatus = getStatusCode(wt, log);
            Repository.UpdatePunchCard(log);
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
            List<LeaveOfficeApply> thisLeave = new List<LeaveOfficeApply>();
            if(leave == null){
                thisLeave = Repository.GetThisTakeLeave(processLog.accountID, wt.sWorkDt, wt.eWorkDt);
            }else{
                thisLeave.Add(leave);
            }                        
            if(thisLeave.Count >0){
                foreach(var tmp in thisLeave){
                    if(wt.sWorkDt >= tmp.startTime && wt.sWorkDt < tmp.endTime){
                        wt.sWorkDt = tmp.endTime>=wt.sRestDt && tmp.endTime<=wt.eRestDt? wt.eRestDt: tmp.endTime;
                    }
                    if(wt.eWorkDt > tmp.startTime && wt.eWorkDt <= tmp.endTime){
                        wt.eWorkDt = tmp.startTime;
                    }
                    fullDayRest = (tmp.startTime <= wt.sWorkDt && tmp.endTime >= wt.eWorkDt)? true : false;
                }
                statusCode |= psCode.takeLeave;
            }

            if(processLog.onlineTime.Year == 1 && processLog.offlineTime.Year == 1){
                statusCode = fullDayRest? statusCode : (statusCode | psCode.noWork);
            }
            else if(processLog.onlineTime.Year > 1 && processLog.offlineTime.Year > 1){
                statusCode = processLog.onlineTime > wt.sWorkDt? (statusCode | psCode.lateIn) : statusCode;
                statusCode = processLog.offlineTime < wt.eWorkDt? (statusCode | psCode.earlyOut) : statusCode;
            }
            else{
                if(processLog.onlineTime.Year > 1){ //只有填上班
                    statusCode = processLog.onlineTime >wt.sWorkDt? (statusCode | psCode.lateIn) : statusCode;
                    statusCode = DateTime.Now >= wt.ePunchDT ? (statusCode | psCode.hadLost) : statusCode; //打不到下班卡了
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