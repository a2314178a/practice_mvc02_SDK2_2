using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;

namespace practice_mvc02.Models
{
    public class CalWorkTime
    {
        private PunchCardRepository Repository {get;}
        private punchCardFunction punchCardFn {get;}

        public CalWorkTime(PunchCardRepository repository, IHttpContextAccessor httpContextAccessor){
            this.Repository = repository;
            this.punchCardFn = new punchCardFunction(repository, httpContextAccessor);
        } 

        public void start(){
            calEmployeeWorkTime();
        }

        

        private void calEmployeeWorkTime(){
            var workEmployee = Repository.GetNeedPunchAcc("全體", 2);
            var dtNow = DateTime.Now;
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
            double totalTime = 0.0;
            double restTime = 0.0;
            WorkDateTime workTime = punchCardFn.workTimeProcess(thisWorkTime);
            var workAllTime = workTime.workAllTime;
            var sWorkTime = workTime.sWorkDt.TimeOfDay;  //只取時間
            var eWorkTime = workTime.eWorkDt.TimeOfDay;
            var sRestTime = workTime.sRestDt.TimeOfDay;
            var eRestTime = workTime.eRestDt.TimeOfDay;
            var restlength = TimeSpan.Zero;
            if(eRestTime < sRestTime){
                restlength = eRestTime.Add(new TimeSpan(24,0,0)) - sRestTime;
            }else{
                restlength = eRestTime - sRestTime;
            }
            restTime = restlength.Hours + restlength.Minutes/60.0;
            foreach(var log in thisMonthAllLog){
                if(log.onlineTime.Year == 1 || log.offlineTime.Year == 1 || log.onlineTime >= log.offlineTime){
                    continue;
                }
                var onlineTime = log.onlineTime.TimeOfDay;
                var offlineTime = log.offlineTime.TimeOfDay;
                var subStartTime = (log.punchStatus & 0x02)>0? onlineTime : sWorkTime;
                var subEndTime = (log.punchStatus & 0x04)>0? offlineTime : eWorkTime;
                if(log.punchStatus == 0x01){
                    totalTime += 8.0;
                }else{
                    var length = TimeSpan.Zero;
                    if(subEndTime < subStartTime){  //23:00~08:00   
                        length = subEndTime.Add(new TimeSpan(24,0,0)) - subStartTime;   //8+24 - 23 = 9
                    }else{
                        length = subEndTime - subStartTime;
                    }
                    totalTime += length.Hours - restTime;
                    totalTime = length.Minutes >=30 ? totalTime+0.5 : totalTime;
                } 
            }
            if(thisMonthAllLog.Count>0){
                saveTotalTimeRecord(thisMonthAllLog[0].accountID, totalTime);
            }
        }

        public void saveTotalTimeRecord(int accID, double totalTime){
            var timeRecord = new workTimeTotal();
            timeRecord.accountID = accID;
            timeRecord.dateMonth = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date;
            //timeRecord.dateMonth = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).Date;
            timeRecord.totalTime = totalTime;
            timeRecord.createTime = DateTime.Now;
            Repository.SaveTotalTimeRecord(timeRecord);
        }




    }
}