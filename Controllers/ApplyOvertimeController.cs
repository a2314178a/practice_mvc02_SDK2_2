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
    public class ApplyOvertimeController : BaseController
    {
        public ApplyOvertimeRepository Repository { get; }

        public ApplyOvertimeController(ApplyOvertimeRepository repository, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)                    
        {
            this.Repository = repository;
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
            return View("ApplyOvertimePage");  
        }

        //---------------------------------------------------------------------------------------
        
        public object getMyApplyOvertime(int page, DateTime sDate, DateTime eDate){
            return Repository.GetMyApplyOvertime((int)loginID, page, sDate, eDate);
        }

        public dynamic addUpApplyOvertime(OvertimeApply data){
            int result = 0;
            if(data.ID == 0){
                data.accountID = data.lastOperaAccID = (int)loginID;
                if(!Repository.chkApplyOvertimeData(data)){
                    return "data_illegal";
                }
                result = Repository.CreateApplyOvertime(data, loginName);
            }
            return result;
        }

        public int delApplyOvertime(int applyingID){
            return Repository.DelApplyOvertime(applyingID);
        }
        
    }
}
