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

        public dynamic addUpApplyLeave(LeaveOfficeApply data){
            var applyData = new LeaveOfficeApply(){
                ID=data.ID, accountID=data.accountID, principalID=data.principalID,
                leaveID=data.leaveID, optionType=data.optionType, note=data.note,
                startTime=data.startTime, endTime=data.endTime, applyStatus=data.applyStatus,
                unitVal=data.unitVal, unit=data.unit, lastOperaAccID=data.lastOperaAccID,
                createTime=data.createTime, updateTime=data.updateTime
            };

            var hasPrincipal = Repository.ChkHasPrincipal((int)loginID);
            if(!hasPrincipal){
                return "noPrincipal";
            }

            applyData.endTime = getLeaveEndTime(applyData);
            applyData.accountID = applyData.lastOperaAccID = (int)loginID;

            var leaveName = Repository.getApplyLeaveName(applyData.leaveID);
            if(leaveName == definePara.annualName()){
                if(!Repository.chkEmployeeAnnualLeave(applyData)){
                    return "notEnough";
                }
            }
            int result = 0;
            applyData.principalID = (int)principalID;
            if(applyData.ID ==0){
                applyData.createTime = definePara.dtNow();
                result = Repository.CreateApplyLeave(applyData);
                if(result == 1){
                    Repository.systemSendMessage(loginName, (int)loginID, "leave");
                }
            }else{
                applyData.updateTime = definePara.dtNow();
                result = Repository.UpdateApplyLeave(applyData);
            }
            return result;
        }

        public int delApplyLeave(int applyingID){
            return Repository.DelApplyLeave(applyingID, (int)loginID);
        }

        public DateTime getLeaveEndTime(LeaveOfficeApply data){ //計算請假結束時間
            
            var workTime = RepositoryPunch.GetThisWorkTime((int)loginID);
            WorkDateTime wdt = punchCardFn.workTimeProcess(workTime);
            var offsetDay = (data.startTime.Date - wdt.sWorkDt.Date).TotalDays;

            var workLengthMinute = (wdt.eWorkDt - wdt.sWorkDt).TotalMinutes;
            workLengthMinute = workLengthMinute <0? workLengthMinute + 24*60 : workLengthMinute;
            var restLengthMinute = (wdt.eRestDt - wdt.sRestDt).TotalMinutes;
            restLengthMinute = restLengthMinute <0? restLengthMinute + 24*60 : restLengthMinute;

            var myDepartClass = (Repository.GetMyDepartClass((int)loginID)).Split(",");
            var totalMin = 0.0;
            
            if(data.unit == 3){ //小時
                totalMin = data.unitVal * 60;
            }else if(data.unit==2){ //半天
                data.startTime = wdt.sWorkDt.AddDays(offsetDay);
                totalMin = (workLengthMinute - restLengthMinute)*0.5;   //半天時長
                if(data.unitVal == 2){  //1=上半天  2=下半天
                    for(int i=1; i<= totalMin; i++){
                        data.startTime = data.startTime.AddMinutes(1);
                        if(data.startTime.Hour == wdt.sRestDt.Hour && data.startTime.Minute == wdt.sRestDt.Minute){
                            data.startTime = data.startTime.AddMinutes(restLengthMinute);
                        }
                    }
                }
            }else{  //天
                data.startTime = wdt.sWorkDt.AddDays(offsetDay);
                totalMin = (workLengthMinute - restLengthMinute)*data.unitVal;
            }
            var eTime = data.startTime;
            var iTmp = 1;
            for(; iTmp<=totalMin; iTmp++){
                if(eTime.Hour == wdt.sRestDt.Hour && eTime.Minute == wdt.sRestDt.Minute){
                    //if(data.unit != 3){
                        eTime = eTime.AddMinutes(restLengthMinute); //跳過休息時間
                    //}else{  //小時
                    //    var restHour = restLengthMinute/60;
                    //}
                }
                if(eTime.Hour == wdt.eWorkDt.Hour && eTime.Minute ==  wdt.eWorkDt.Minute){
                    //若接下來時間超過下班時間 跳到隔天的上班時間繼續加
                    eTime = eTime.AddDays(1).AddMinutes(-workLengthMinute); 
                    var flag = true;    //判斷是否休假日 有就略過
                    do{
                        var spDate = RepositoryPunch.GetThisSpecialDate(eTime); //是否有特殊日期
                        if(spDate == null){     //DayOfWeek.ToString("d") 轉換成星期幾
                            if((eTime.DayOfWeek.ToString("d")== "0" || eTime.DayOfWeek.ToString("d")== "6")){
                                eTime = eTime.AddDays(1);
                            }else{
                                flag = false;
                            }
                        }else{  //1:休假 2:上班     
                            if(spDate.status == 1 && Array.IndexOf(myDepartClass, spDate.departClass) >-1){ 
                                eTime = eTime.AddDays(1);
                            }else{
                                flag = false;
                            }
                        }
                    }while(flag); 
                }
                eTime = eTime.AddMinutes(1);
                if(eTime.Hour==wdt.eWorkDt.Hour && eTime.Minute==wdt.eWorkDt.Minute){
                    break;
                }
            }

            if(data.unit==3 && iTmp <= totalMin){
                var hour = iTmp/60;
                hour = iTmp%60 >0? (++hour) : hour; 
                data.unitVal = hour;
            }
            return eTime;
        }

        #endregion //leaveOffice
        
    }
}
