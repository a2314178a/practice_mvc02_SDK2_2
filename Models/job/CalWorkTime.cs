using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;
using practice_mvc02.Models;

namespace practice_mvc02.Models
{
    public class CalWorkTime
    {
        private PunchCardRepository Repository {get;}
        private punchCardFunction punchCardFn {get;}
        private punchStatusCode psCode;

        public CalWorkTime(PunchCardRepository repository, IHttpContextAccessor httpContextAccessor){
            this.Repository = repository;
            this.punchCardFn = new punchCardFunction(repository, httpContextAccessor);
            psCode = new punchStatusCode();
        } 

        public void start(){
            calEmployeeWorkTime();
        }    

        private void calEmployeeWorkTime(){
            var workEmployee = Repository.GetNeedPunchAcc("全體", 2);
            var dtNow = definePara.dtNow();
            var startDT = dtNow.AddDays(1 - dtNow.Day).Date;
            var endDT = startDT.AddMonths(1).AddDays(-1).Date;
            //var startDT = dtNow.AddMonths(-1).AddDays(1 - dtNow.Day).Date;
            //var endDT = startDT.AddMonths(1).AddDays(-1).Date;
            foreach(var employee in workEmployee){
                var thisMonthAllLog = Repository.GetPunchLogByDateByID(employee.ID, startDT, endDT);
                WorkTimeRule thisWorkTime = Repository.GetThisWorkTime(employee.ID);
                countWorkTime(thisMonthAllLog, thisWorkTime);
            }
        }

        private void countWorkTime(List<PunchCardLog> thisMonthAllLog, WorkTimeRule thisWorkTime){
            var totalTimeMinute = 0.0;
            var restTimeMinute = 0.0;
            var endStartWorkTimeMinute = 0.0;
            var noRestWorkTimeMinute = 0.0;
            var sWorkTime = thisWorkTime.startTime;  //只取時間
            var eWorkTime = thisWorkTime.endTime;
            var sRestTime = thisWorkTime.sRestTime;
            var eRestTime = thisWorkTime.eRestTime;
            var restlength = TimeSpan.Zero;

            if(eRestTime < sRestTime){
                restlength = eRestTime.Add(new TimeSpan(24,0,0)) - sRestTime;
            }else{
                restlength = eRestTime - sRestTime;
            }
            restTimeMinute = restlength.TotalMinutes;

            var endStartLength = TimeSpan.Zero;
            if(eWorkTime < sWorkTime){
                endStartLength = eWorkTime.Add(new TimeSpan(24,0,0)) - sWorkTime;
            }else{
                endStartLength = eWorkTime - sWorkTime;
            }
            endStartWorkTimeMinute = endStartLength.TotalMinutes;
            noRestWorkTimeMinute = (endStartWorkTimeMinute - restTimeMinute);

            foreach(var log in thisMonthAllLog){
                if(log.onlineTime.Year == 1 || log.offlineTime.Year == 1 || log.onlineTime >= log.offlineTime){
                    continue;
                }
                WorkDateTime workTime = punchCardFn.workTimeProcess(thisWorkTime, log);

                //計算工作時間
                if(log.onlineTime <= workTime.sRestDt && log.offlineTime >= workTime.eRestDt){
                    totalTimeMinute += (int)((log.offlineTime - log.onlineTime).TotalMinutes) - restTimeMinute;
                }else if(log.onlineTime < workTime.sRestDt && log.offlineTime <= workTime.eRestDt){
                    if(log.offlineTime < workTime.sRestDt){
                        totalTimeMinute += (int)(log.offlineTime - log.onlineTime).TotalMinutes;
                    }else{
                        totalTimeMinute += (int)(workTime.sRestDt - log.onlineTime).TotalMinutes;
                    }
                }else if(log.onlineTime >= workTime.sRestDt && log.offlineTime > workTime.eRestDt){
                    if(log.onlineTime > workTime.eRestDt){
                        totalTimeMinute += (int)(log.offlineTime - log.onlineTime).TotalMinutes;
                    }else{
                        totalTimeMinute += (int)(log.offlineTime - workTime.eRestDt).TotalMinutes;
                    }
                }
            }
            if(thisMonthAllLog.Count>0){
                saveTotalTimeRecord(thisMonthAllLog[0].accountID, totalTimeMinute);
            }
        }

        public void saveTotalTimeRecord(int accID, double totalTimeMinute){
            var timeRecord = new workTimeTotal();
            timeRecord.accountID = accID;
            timeRecord.dateMonth = definePara.dtNow().AddDays(1 - definePara.dtNow().Day).Date;
            //timeRecord.dateMonth = definePara.dtNow().AddMonths(-1).AddDays(1 - definePara.dtNow().Day).Date;
            timeRecord.totalTime = totalTimeMinute;
            timeRecord.createTime = definePara.dtNow();
            Repository.SaveTotalTimeRecord(timeRecord);
        }
    }
}