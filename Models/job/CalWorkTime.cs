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
        private ApplyOvertimeRepository aRepository {get;}
        private punchStatusCode psCode;

        public CalWorkTime(PunchCardRepository repository, ApplyOvertimeRepository applyRepository,
                            IHttpContextAccessor httpContextAccessor){
            this.Repository = repository;
            this.aRepository = applyRepository;
            this.punchCardFn = new punchCardFunction(repository, httpContextAccessor);
            psCode = new punchStatusCode();
        } 

        public void start(int month=1){
            calEmployeeWorkTime(month);
        }    

        private void calEmployeeWorkTime(int month){
            var nMonth = (month > 12) || (month < 0)? 1 : month;
            var workEmployee = Repository.GetNeedPunchAcc("全體", 2, false);
            var dtNow = definePara.dtNow();
            var startDT = dtNow.AddDays(1 - dtNow.Day).Date;
            var endDT = startDT.AddMonths(1).AddDays(-1).Date;
            var doCount = 1 + nMonth ;
            for(var i = 0; i < doCount; i++){       //計算這個月與上n個月
                foreach(var employee in workEmployee){
                    var thisMonthAllLog = Repository.GetPunchLogByDateByID(employee.ID, startDT, endDT);
                    WorkTimeRule thisWorkTime = Repository.GetThisWorkTime(employee.ID);
                    var otApplies = aRepository.GetOvertimeApplyByDateByID(employee.ID, startDT, endDT);
                    countWorkTime(thisMonthAllLog, otApplies, thisWorkTime, startDT);
                }
                startDT = startDT.AddMonths(-1).Date;
                endDT = startDT.AddMonths(1).AddDays(-1).Date;
            }   
        }

        private void countWorkTime(List<PunchCardLog> thisMonthAllLog, List<OvertimeApply> otApplies, WorkTimeRule thisWorkTime, DateTime startDT){
            var totalTimeMinute = 0.0;
            var totalOvertimeMinute = 0;
            var restTimeMinute = 0.0;
            var endStartWorkTimeMinute = 0.0;
            var noRestWorkTimeMinute = 0.0;
            var sWorkTime = thisWorkTime.startTime;  //只取時間
            var eWorkTime = thisWorkTime.endTime;
            var sRestTime = thisWorkTime.sRestTime;
            var eRestTime = thisWorkTime.eRestTime;
            var restlength = TimeSpan.Zero;
            var thisAccLeaves = getThisAccLeaves(thisMonthAllLog, thisWorkTime, startDT);

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

                foreach(var leave in thisAccLeaves){    //工作時間需會扣掉請假時間 
                    if(leave.startTime >= workTime.sWorkDt && leave.endTime <= workTime.eWorkDt &&
                         leave.unit ==3){   //只處理單位為小時的請假 
                        if(leave.startTime > workTime.sWorkDt && leave.endTime < workTime.eWorkDt){
                            totalTimeMinute -= leave.unitVal*60;
                        }
                    }
                }
            }

            foreach (var apply in otApplies){               //計算加班
                totalOvertimeMinute += apply.timeLength;
            }

            if(thisMonthAllLog.Count>0){
                saveTotalTimeRecord(thisMonthAllLog[0].accountID, startDT, totalTimeMinute, totalOvertimeMinute);
            }
        }

        public List<LeaveOfficeApply> getThisAccLeaves(List<PunchCardLog> thisMonthAllLog, WorkTimeRule thisWorkTime, DateTime startDT){
            if(thisMonthAllLog.Count == 0){
                return new List<LeaveOfficeApply>();
            }
            var sDT = startDT;
            var eDT = startDT.AddMonths(1).AddDays(-1).Date;
            return Repository.GetThisTakeLeave(thisMonthAllLog[0].accountID, sDT, eDT);
        }

        public void saveTotalTimeRecord(int accID, DateTime startDT, double totalTimeMinute, int totalOvertimeMinute){
            var timeRecord = new workTimeTotal();
            timeRecord.accountID = accID;
            timeRecord.dateMonth = startDT;
            //timeRecord.dateMonth = definePara.dtNow().AddMonths(-1).AddDays(1 - definePara.dtNow().Day).Date;
            timeRecord.totalTime = totalTimeMinute;
            timeRecord.totalOvertime = totalOvertimeMinute;
            timeRecord.createTime = definePara.dtNow();
            Repository.SaveTotalTimeRecord(timeRecord);
        }
    }
}