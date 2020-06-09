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
    public class PunchCardController : BaseController
    {
        public PunchCardRepository Repository { get; }
        public punchCardFunction punchCardFn {get;}

        public PunchCardController(PunchCardRepository repository, punchCardFunction fn,
                                    IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
            this.punchCardFn = fn;
        }

        public IActionResult Index(string page, int target=0)
        {
            if(target == 0){
                target = (int)loginID;
            }
            return selectPage(page, target);
        }

        public IActionResult selectPage(string page, int targetID){
            if(targetID != loginID){
                return getEmployeeLog(targetID, page);
            }
            ViewBag.ruleVal = ruleVal;
            ViewData["loginName"] = loginName;
            ViewBag.targetID = targetID;
            ViewBag.Auth = "Y";
            ViewBag.loginAccLV = loginAccLV;
            ViewBag.Operator = "myself";
            ViewBag.punchLogName = loginName;
            ViewBag.canEditPunchLog = false;
            ViewBag.showPunchBtn = true;
            if(page == "log"){
                return View("PunchCardLogPage_cal");
            }else if(page == "total"){
                return View("TimeTotalPage");
            }
            return View("PunchCardPage");
        }

        public IActionResult getEmployeeLog(int targetID, string page){
            
            object accDetail = Repository.GetAccountDetail(targetID);
            var employeeAccLV = accDetail.GetType().GetProperty("accLV").GetValue(accDetail);
            var employeeName = accDetail.GetType().GetProperty("userName").GetValue(accDetail);
            var employeeDepartID = accDetail.GetType().GetProperty("departmentID").GetValue(accDetail);
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            ViewBag.loginAccLV = loginAccLV;
            ViewBag.targetID = targetID;
            ViewBag.punchLogName = employeeName;
            ViewBag.lookEmployeeDepartID = employeeDepartID;
            ViewBag.ruleVal = ruleVal;
            ViewBag.showPunchBtn = targetID == (int)loginID? true : false;
            ViewBag.canEditPunchLog = (ruleVal & ruleCode.editPunchLog)>0 ? true : false;
            if(page=="log"){
                return View("PunchCardLogPage_cal");
            }
            return View("TimeTotalPage");
            //return RedirectToAction("logOut", "Home"); //轉址到特定Controller的ACTION名字
        }

        
        //---------------------------------------------------------------------------------------
        public object getTodayPunchStatus(){    //look my today PunchStatus
            WorkTimeRule thisWorkTime = Repository.GetThisWorkTime((int)loginID);
            var dataLog = Repository.GetTodayPunchLog((int)loginID, thisWorkTime);
            if(dataLog == null){
                dataLog = new PunchCardLog();
            }
            dynamic onlineTime = dataLog.onlineTime;
            dynamic offlineTime = dataLog.offlineTime;
            if(onlineTime.Year == 1){ onlineTime = false; }      
            if(offlineTime.Year == 1){ offlineTime = false; }
            return new {
                onlineTime = onlineTime, offlineTime = offlineTime,
            };
        }

        public int addPunchCardLog(int action, int employeeID = 0){
            if(employeeID == 0){
                employeeID = (int)loginID;
            }
            WorkTimeRule thisWorkTime = Repository.GetThisWorkTime(employeeID);
            PunchCardLog logData = Repository.GetTodayPunchLog(employeeID, thisWorkTime);
            return punchCardFn.punchCardProcess(logData, thisWorkTime, action, employeeID);
        }

        public object getPunchLogByIDByMonth(int employeeID, DateTime queryStart, DateTime queryEnd){
            if(employeeID == 0)
                employeeID = (int)loginID;
            return Repository.GetPunchLogByIDByMonth(employeeID, queryStart, queryEnd);
        }

        public object getPunchLogByIDByDate(int employeeID, DateTime qDateStr){
            if(employeeID == 0)
                employeeID = (int)loginID;
            
            WorkDateTime workTime = punchCardFn.workTimeProcess(Repository.GetThisWorkTime(employeeID), new PunchCardLog(){logDate=qDateStr});
            var sWorkDt = workTime.sWorkDt;
            var eWorkDt = workTime.eWorkDt;
            var punchLog = Repository.GetPunchLogByIDByDate(employeeID, qDateStr);
            List<LeaveOfficeApply> takeLeave = Repository.GetThisTakeLeave(employeeID, sWorkDt, eWorkDt);
            List<object> leave = new List<object>();
            foreach(var tmp in takeLeave){
                leave.Add(new{tmp.startTime, tmp.endTime});
            }

            return new{punchLog=punchLog, takeLeave=leave};
        }



        public int forceAddPunchCardLog(PunchCardLog newPunchLog){
            if( newPunchLog.accountID == 0 || newPunchLog.departmentID == 0 || 
               (newPunchLog.onlineTime.Year == 1 && newPunchLog.offlineTime.Year == 1) ){
                  return 2; //此打卡紀錄不合法
            }
            WorkTimeRule thisWorkTime = Repository.GetThisWorkTime(newPunchLog.accountID);
            return punchCardFn.forcePunchLogProcess(newPunchLog, thisWorkTime, "add");
        }

        public int forceUpdatePunchCardLog(PunchCardLog updatePunchLog, string from){
            if( updatePunchLog.ID == 0 || (updatePunchLog.onlineTime.Year == 1 && updatePunchLog.offlineTime.Year == 1) ){
                  return 2; //此打卡紀錄不合法
            }
            updatePunchLog.accountID = Repository.GetThisLogAccID(updatePunchLog.ID);
            WorkTimeRule thisWorkTime = Repository.GetThisWorkTime(updatePunchLog.accountID);
            return punchCardFn.forcePunchLogProcess(updatePunchLog, thisWorkTime, "update", from);
        }

        public int delPunchCardLog(int punchLogID){
            return Repository.DelPunchCardLog(punchLogID, (int)loginID);
        }

        public object getTimeTotalByID(int targetID){
            return Repository.GetTimeTotalByID(targetID);
        }


        

        
    }
}
