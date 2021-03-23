using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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
    public class AdminFnController : BaseController
    {
        public AdminFnRepository aRepository { get; }
        public MasterRepository mRepository { get; }
        public PunchCardRepository pRepository { get; }
        private ChkPunchLogWarn chkWarn { get; }
        private CalWorkTime calTime { get; }
        private CalAnnualLeave calAnnual { get; }
        
        public AdminFnController(AdminFnRepository a_repository, MasterRepository m_repository,
                                PunchCardRepository p_repository, AnnualLeaveRepository al_repository,
                                ApplyOvertimeRepository ap_repository, 
                                IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)                
        {
            this.aRepository = a_repository;
            this.mRepository = m_repository;
            this.pRepository = p_repository;
            this.chkWarn = new ChkPunchLogWarn(p_repository, httpContextAccessor);
            this.calTime = new CalWorkTime(p_repository, ap_repository, httpContextAccessor);
            this.calAnnual = new CalAnnualLeave(al_repository);
        }

        public IActionResult Index(string page="operateLog")
        {           
            return selectPage(page);
        }

        public IActionResult selectPage(string page){
            ViewBag.ruleVal = ruleVal;
            ViewBag.Auth = "Y";
            ViewBag.ID = (int)loginID;
            ViewBag.loginAccLV = loginAccLV;
            ViewBag.Page = page;
            return View("AdminFnPage");
        }

        public List<ViewOpLog> getOperateLog(OpLogFilter filter){
            filter.eDate = filter.eDate.AddDays(1);
            filter.active = String.IsNullOrEmpty(filter.active)? "" : filter.active;
            filter.category = String.IsNullOrEmpty(filter.category)? "" : filter.category;
            return aRepository.GetOperateLog(filter);
        }

        public object getFilterOption(){
            var category = aRepository.GetOpLogCategory();
            var userName = mRepository.GetAllPrincipal();
            return new {category, userName};
        }

        public int manual_refreshPunchLogWarn(int day=7){ //day:正整數 刷新前n天打卡紀錄
            var result = 0;
            try{
                chkWarn.start(day);
                result = 1;
            }catch (System.Exception){}
            return result;
        }

        public int manual_calWorkTime(int month=1){  //month:正整數 計算這個月與前n個月 預設為這個月與前1個月
            var result = 0;
            try{
                calTime.start(month);
                result = 1;
            }catch (System.Exception){}
            return result;
        }

        public int manual_calAnnualLeave(){
            var result = 0;
            try{
                calAnnual.start();
                result = 1;
            }catch (System.Exception){}
            return result;
        }

        public object clearEmployeeAnnualLeaves(int delMonth=36){   //清除舊的年假 以deadline做為判斷
             return processClearFn(delMonth, "annual");
        }
        
        public object clearEmployeeLeaveOfficeApply(int delMonth=36){    //清除舊請假紀錄 以createTime為判斷
            return processClearFn(delMonth, "leave");
        }

        public object clearPunchCardLogs(int delMonth=36){   //清除打卡 以logDate為判斷
            return processClearFn(delMonth, "punch");
        }

        public object clearOperateLogs(int delMonth=36){ //清除操作紀錄 以createTime為判斷
            return processClearFn(delMonth, "operate");
        }

        public object processClearFn(int delMonth, string sel){
            if(delMonth <= 5 || delMonth > 1200){         //至少要6個月以前
                return new{statusCode=0};
            }
            var dt = definePara.dtNow().Date;
            dt = dt.AddMonths(-delMonth);
            var count = 0;
            switch (sel){
                case "leave": count = aRepository.ClearEmployeeLeaveOfficeApply(dt); break;
                case "punch": count = aRepository.ClearPunchCardLogs(dt); break;
                case "operate": count = aRepository.ClearOperateLogs(dt); break;
                case "annual": count = aRepository.ClearEmployeeAnnualLeaves(dt); break;
                default: count = -1; break;
            }
            return new{statusCode=(count==-1? 0 : 1), count};
        }

        
        public object clearMessageAndMsgSendReceive(){ //清除訊息 需寄件人與收件人都刪除才可清除
            var count = aRepository.ClearMessageAndMsgSendReceive();
            return new{statusCode=(count==-1? 0 : 1), count};
        }
        
    }

}
