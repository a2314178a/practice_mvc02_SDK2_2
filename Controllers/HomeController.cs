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
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;

namespace practice_mvc02.Controllers
{
    public class HomeController : BaseController
    {
        //private readonly ILogger<HomeController> _logger;
        public AccountRepository Repository { get; }
        public loginFunction loginFn {get;}
        

        public HomeController(AccountRepository repository, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
            this.loginFn = new loginFunction(repository);
        }

        public IActionResult Index()
        {           
            if(loginFn.isLoginInfo(loginID, loginGroupID)){
                return selectPage();
            }else{
                return View();
            }
        }

        public int login(string account, string password){
            var md5password = loginFn.GetMD5((account+password));
            //Account userData = Repository.Login(account, md5password);
            object userData = Repository.Login(account, md5password);
            if(userData != null)
            {
                var userID = (int)getObjectValue("ID", userData);
                DateTime nowTime= definePara.dtNow();
                DateTime Jan1st1970 = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
                long timeStamp = Convert.ToInt64((nowTime - Jan1st1970).TotalMilliseconds);
                int count = Repository.UpdateTimeStamp(userID, timeStamp.ToString(), nowTime);
                if(count == 1){
                    _session.SetString("loginTimeStamp", timeStamp.ToString());
                    _session.SetString("loginAcc", account);
                    _session.SetInt32("loginID", userID);
                    _session.SetInt32("loginAccLV", (int)getObjectValue("accLV", userData));
                    _session.SetString("loginName", (string)getObjectValue("userName", userData).ToString());
                    _session.SetInt32("loginDepartmentID", (int)getObjectValue("departmentID", userData));
                    _session.SetInt32("loginGroupID", (int)getObjectValue("groupID", userData));
                    _session.SetInt32("principalID", (int)getObjectValue("principalID", userData));
                    var ruleVal = (int)getObjectValue("ruleParameter", userData);
                    _session.SetInt32("ruleVal", ruleVal);
                    if(Repository.chkIsAgent(userID)){
                        ruleVal = (ruleVal | ruleCode.applySign);
                        ruleVal = (ruleVal | ruleCode.editPunchLog);
                    }
                    _session.SetInt32("ruleVal", ruleVal);
                    
                    if(_session.GetInt32("loginAccLV") == definePara.getDIMALV()){
                        initTable();
                    }
                    return 1;   
                }
                else{
                    return 0;
                }
            }
            else{
                return 0;
            }
        }

        public IActionResult selectPage(){
            if(!loginFn.isLoginInfo(loginID, loginGroupID)){
                return logOut();
            }
            if(ruleVal > 0){
                if((ruleVal & ruleCode.baseActive) >0){
                    return RedirectToAction("Index", "PunchCard");
                }
                else if((ruleVal & ruleCode.departEmployeeList) >0 || (ruleVal & ruleCode.allEmployeeList) >0){
                    return RedirectToAction("Index", "EmployeeList");
                }
                else if((ruleVal & ruleCode.departmentList) >0){
                    return RedirectToAction("Index", "DepartmentList");
                }
                else if((ruleVal & ruleCode.setRule) >0){
                    return RedirectToAction("Index", "SetRule");
                }
            }
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            ViewBag.ruleVal = 0;
            return View("Welcome");
        }

        public IActionResult logOut(){ 
            ViewBag.Auth = "N";
            _session.Clear();
            return View("Index");
        }


        public dynamic getObjectValue(string key, object obj){
            return obj.GetType().GetProperty(key).GetValue(obj);
        }

        public void initTable(){
            Repository.initLeaveNames();
        }
        



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
