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
                ID = data.ID, 
                accountID = data.accountID, 
                principalID = data.principalID,
                leaveID = data.leaveID, 
                optionType = data.optionType, 
                note = data.note,
                startTime = data.startTime, 
                endTime = data.endTime, 
                applyStatus = data.applyStatus,
                unitVal = data.unitVal, 
                unit = data.unit, 
                lastOperaAccID = data.lastOperaAccID,
                createTime = data.createTime, 
                updateTime = data.updateTime
            };
            var isActiveApply = data.accountID ==0? true : false;   //是否個人申請請假 還是主管幫申請
            if(!isActiveApply){    //被申請
                applyData.lastOperaAccID = applyData.principalID = (int)loginID;
                applyData.applyStatus = 1;  //申請狀態直接通過
            }else{
                applyData.accountID = applyData.lastOperaAccID = (int)loginID;
                applyData.principalID = (int)principalID;
                if(!Repository.ChkHasPrincipal(applyData.accountID)){  //沒有代理人不可請假
                    return "noPrincipal";
                }
            }

            if(!Repository.ChkApplyLeaveData(applyData, isActiveApply)){
                return "data_illegal";
            }
            
            applyData.endTime = getLeaveEndTime(applyData);
            var isOverEndWorkTime = (applyData.note == "note_overEndWorkTime"? true : false);
            applyData.note = data.note; //須改回原本的note
            if(!isActiveApply){
                applyData.note += $"({loginName}主管申請)"; //被主管申請的請假進行備註
            }
            if(Repository.ChkLeaveHadSameTimeData(applyData)){
                return "hadSameTimeLeave";
            }

            var leaveName = Repository.getApplyLeaveName(applyData.leaveID);
            if(leaveName == definePara.annualName() || leaveName == definePara.otRestName()){
                if(!Repository.chkEmployeeAnnualLeave(applyData, 2, leaveName)){  //特休/補休 不足不可請假
                    return "notEnough";
                }
            }

            int result = 0;
            
            if(applyData.ID ==0){
                applyData.createTime = definePara.dtNow();
                result = Repository.CreateApplyLeave(applyData, isActiveApply, loginName, leaveName);
            }else{  //編輯更新 未使用
                //applyData.updateTime = definePara.dtNow();
                //result = Repository.UpdateApplyLeave(applyData);
            }
            
            if(isOverEndWorkTime && applyData.unit ==3 && result ==1){
                return "overEndWorkTime";
            }
            return result;
        }

        public int delApplyLeave(int applyingID){
            return Repository.DelApplyLeave(applyingID, (int)loginID);
        }

        public DateTime getLeaveEndTime(LeaveOfficeApply data){ //計算請假結束時間
            
            var workTime = RepositoryPunch.GetThisWorkTime(data.accountID);
            WorkDateTime wdt = punchCardFn.workTimeProcess(workTime);
            var offsetDay = (data.startTime.Date - wdt.sWorkDt.Date).TotalDays;

            var workLengthMinute = (wdt.eWorkDt - wdt.sWorkDt).TotalMinutes;
            workLengthMinute = workLengthMinute <0? workLengthMinute + 24*60 : workLengthMinute;
            var restLengthMinute = (wdt.eRestDt - wdt.sRestDt).TotalMinutes;
            restLengthMinute = restLengthMinute <0? restLengthMinute + 24*60 : restLengthMinute;

            var useHalfVal = data.unit==3? Repository.IsUseHourHalfVal(data.leaveID) : false;
            var myDepartClass = (Repository.GetMyDepartClass(data.accountID)).Split(",");
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
                data.unitVal = 1;   //須改為1 一個半天
            }else{  //天
                data.startTime = wdt.sWorkDt.AddDays(offsetDay);
                totalMin = (workLengthMinute - restLengthMinute)*data.unitVal;
            }

            var eTime = data.startTime;
            var iTmp = 1;
            var restLen_unit3 = 0; 
            var add_restLen_flag = false;
            for(; iTmp<=totalMin; iTmp++)
            {
                if(eTime.Hour == wdt.sRestDt.Hour && eTime.Minute == wdt.sRestDt.Minute){
                    if(data.unit != 3){
                        eTime = eTime.AddMinutes(restLengthMinute); //跳過休息時間
                    }else{  //小時
                        add_restLen_flag = true;
                    }
                }
                if(add_restLen_flag){
                    restLen_unit3++;
                    if(restLen_unit3 > restLengthMinute){
                        restLen_unit3 = (int)restLengthMinute;
                        add_restLen_flag = false;
                    }
                }

                if(eTime.Hour == wdt.eWorkDt.Hour && eTime.Minute ==  wdt.eWorkDt.Minute){
                    //若接下來時間超過下班時間 跳到隔天的上班時間繼續加
                    eTime = eTime.AddDays(1).AddMinutes(-workLengthMinute); 
                    var flag = wdt.type==1? false : true;    //1:排休制(都要上班) 0:固定制
                    while(flag){    //判斷是否休假日 有就略過
                        var spDate = RepositoryPunch.GetThisSpecialDate(eTime); //是否有特殊日期
                        if(spDate == null){     //DayOfWeek.ToString("d") 轉換成星期幾
                            if((eTime.DayOfWeek.ToString("d")== "0" || eTime.DayOfWeek.ToString("d")== "6")){
                                eTime = eTime.AddDays(1);
                            }else{
                                flag = false;
                            }
                        }else{  //1:休假 2:上班     
                            if((spDate.status == 2 && Array.IndexOf(myDepartClass, spDate.departClass) >-1) ||
                                (spDate.status == 1 && Array.IndexOf(myDepartClass, spDate.departClass) == -1)
                                ){ 
                                flag = false;
                            }else{
                                eTime = eTime.AddDays(1);
                            }
                        }
                    } 
                }
                eTime = eTime.AddMinutes(1);
                if(eTime.Hour==wdt.eWorkDt.Hour && eTime.Minute==wdt.eWorkDt.Minute && data.unit==3){
                    break;
                }
            }//for(; iTmp<=totalMin; iTmp++)

            if(data.unit==3){   //小時不用忽略休息時間 
                if(iTmp < totalMin){   //若超過下班時間 以下班時間為底 
                    var hour = iTmp/60;
                    //unitVal為實際請假時數(開始時間到下班時間)
                    data.unitVal = (float)(iTmp%60 >30? (++hour) : iTmp%60 >0? hour+0.5 : hour);    
                    data.note = "note_overEndWorkTime"; //用此判斷是否有超過下班時間
                }
                data.unitVal = ((data.unitVal*60) - restLen_unit3)/60;   //不過若有經過休息時間，unitVal需扣掉(主要是特休用)
                if(useHalfVal){     //可以有0.5小時
                    var dot = data.unitVal - ((int)data.unitVal);
                    if(dot >0){
                        dot = (float)(dot>0.5 ? 1 : 0.5);
                        data.unitVal = (int)data.unitVal + dot;
                    }
                }else{
                    data.unitVal = (float)(Math.Ceiling(data.unitVal)); //無條件進位
                }  
            }

            return eTime;
        }

        #endregion //leaveOffice
        
    }
}
