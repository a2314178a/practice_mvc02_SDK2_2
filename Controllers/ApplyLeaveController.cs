using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using practice_mvc02.filters;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;

namespace practice_mvc02.Controllers
{
    [TypeFilter(typeof(ActionFilter))]
    public class ApplyLeaveController : BaseController
    {
        public ApplySignRepository Repository { get; }
        public PunchCardRepository RepositoryPunch {get;}
        public punchCardFunction punchCardFn {get;}

        public ApplyLeaveController(ApplySignRepository repository, PunchCardRepository repository02,
                                    IHttpContextAccessor httpContextAccessor, punchCardFunction fn):base(httpContextAccessor)
        {
            this.Repository = repository;
            this.RepositoryPunch = repository02;
            this.punchCardFn = fn;
        }

        public IActionResult Index(string page)
        {           
            return selectPage(page);
            //return RedirectToAction("logOut", "Home"); //轉址到特定Controller的ACTION名字
        }

        public IActionResult selectPage(string page){
            ViewBag.ruleVal = ruleVal;
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            ViewBag.Page = page == "log" ? "log" : "apply";
            return View("ApplyLeavePage");  
        }

        //---------------------------------------------------------------------------------------
        
       
        #region  LeaveOffice

        public object getMyApplyLeave(int page, DateTime sDate, DateTime eDate){
            return Repository.GetMyApplyLeave((int)loginID, page, sDate, eDate);
        }

        public object getLeaveOption(){
            return Repository.GetLeaveOption();
        }

        public int addUpApplyLeave(LeaveOfficeApply data){
            data.endTime = getLeaveEndTime(data);
            data.accountID = data.lastOperaAccID = (int)loginID;

            var leaveName = Repository.getApplyLeaveName(data.leaveID);
            if(leaveName == Repository.spName){
                if(!Repository.chkEmployeeAnnualLeave(data)){
                    return -3;
                }
            }

            int result = 0;
            data.principalID = (int)principalID;
            if(data.ID ==0){
                data.createTime = DateTime.Now;
                result = Repository.CreateApplyLeave(data);
                if(result == 1){
                    Repository.systemSendMessage(loginName, (int)loginID, "leave");
                }
            }else{
                data.updateTime = DateTime.Now;
                result = Repository.UpdateApplyLeave(data);
            }
            return result;
        }

        public int delApplyLeave(int applyingID){
            return Repository.DelApplyLeave(applyingID);
        }

        public DateTime getLeaveEndTime(LeaveOfficeApply data){

            var eTime = data.startTime;
            var workTime = RepositoryPunch.GetThisWorkTime((int)loginID);
            WorkDateTime wdt = punchCardFn.workTimeProcess(workTime);
            var workLengthMinute = (wdt.eWorkDt - wdt.sWorkDt).TotalMinutes;
            workLengthMinute = workLengthMinute <0? workLengthMinute + 24*60 : workLengthMinute;
            var restLengthMinute = (wdt.eRestDt - wdt.sRestDt).TotalMinutes;
            restLengthMinute = restLengthMinute <0? restLengthMinute + 24*60 : restLengthMinute;

            var myDepartClass = (Repository.GetMyDepartClass((int)loginID)).Split(",");
            var totalMin = 0.0;

            switch(data.unit){
                case 1: totalMin = data.unitVal * (workLengthMinute - restLengthMinute); break;
                case 2: totalMin = 0.5 * data.unitVal * (workLengthMinute - restLengthMinute); break;
                case 3: totalMin = data.unitVal * 60; break;
            }
 
            for(int i=1; i<=totalMin; i++){
                if(eTime.Hour == wdt.sRestDt.Hour && eTime.Minute == wdt.sRestDt.Minute){
                    eTime = eTime.AddMinutes(restLengthMinute);
                }
                if(eTime.Hour == wdt.eWorkDt.Hour && eTime.Hour ==  wdt.eWorkDt.Hour){
                    eTime = eTime.AddDays(1).AddMinutes(-workLengthMinute);
                    var flag = true;
                    do{
                        var spDate = RepositoryPunch.GetThisSpecialDate(eTime);
                        if(spDate == null){
                            if((eTime.DayOfWeek.ToString("d")== "0" || eTime.DayOfWeek.ToString("d")== "6")){
                                eTime = eTime.AddDays(1);
                            }else{
                                flag = false;
                            }
                        }else{
                            if(spDate.status == 1 && Array.IndexOf(myDepartClass, spDate.departClass) >-1){ //1:休假 2:上班
                                eTime = eTime.AddDays(1);
                            }else{
                                flag = false;
                            }
                        }
                    }while(flag); 
                }
                eTime = eTime.AddMinutes(1);
            }
            return eTime;
        }

        #endregion //leaveOffice
        
    }
}
