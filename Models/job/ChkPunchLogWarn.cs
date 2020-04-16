using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;
using practice_mvc02.Models;

namespace practice_mvc02.Models
{
    public class ChkPunchLogWarn
    {
        private PunchCardRepository Repository {get;}
        private punchCardFunction punchCardFn {get;}

        public ChkPunchLogWarn(PunchCardRepository repository, IHttpContextAccessor httpContextAccessor){
            this.Repository = repository;
            this.punchCardFn = new punchCardFunction(repository, httpContextAccessor);
        } 

        public void start(int chkStatusDay = 7){
            for(int i=1; i<=chkStatusDay; i++){
                addPunchLogWhenNoPunch(i);
            }
            punchCardProcess(chkStatusDay);
        }

        private void addPunchLogWhenNoPunch(int rangeDay){
            var targetDate = (definePara.dtNow().Date).AddDays(-rangeDay);
            var spDate = Repository.GetThisSpecialDate(targetDate);
            List<Account> needPunchAcc = new List<Account>(){};

            if(spDate == null){
                if((targetDate.DayOfWeek.ToString("d")== "0" || targetDate.DayOfWeek.ToString("d")== "6")){
                    return;
                }else{
                    needPunchAcc = Repository.GetNeedPunchAcc("全體", 2);
                }
            }else{
                if(spDate.status == 1 && spDate.departClass=="全體"){ //1:休假 2:上班
                    return;
                }else{  //全體 上班 , 個別 休假
                    needPunchAcc = Repository.GetNeedPunchAcc(spDate.departClass, spDate.status);
                }
            }
            
            foreach(var employee in needPunchAcc){
                var logData = Repository.GetWorkDatePunchLog(employee.ID, targetDate);
                if(logData == null){
                    Repository.AddNullPunchLog(employee.ID, employee.departmentID, targetDate);
                }
            }
        }

        private void punchCardProcess(int chkStatusDay){
            List<PunchCardLog> warnLog = new List<PunchCardLog>();
            warnLog = Repository.GetAllPunchLogWithWarn(chkStatusDay);
            
            foreach (PunchCardLog log in warnLog){
                WorkTimeRule thisWorkTime = Repository.GetThisWorkTime(log.accountID);
                punchCardFn.processPunchlogWarn(log, thisWorkTime);
                //punchCardFn.processTakeLeaveWithLog(thisTakeLeave);
            }   
        }
        







    }
}