using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using practice_mvc02.Models;
using practice_mvc02.Repositories;

namespace practice_mvc02.filters
{
    public class ActionFilter : IActionFilter
    {
        AccountRepository  Repository;
        private int? loginID;
        private int? loginAccLV;
        private int? loginGroupID;
        private int? ruleVal;
        private string loginTimeStamp;
        private ISession _session;
        private groupRuleCode ruleCode {get;}
        public const int UTC_offset = 8;

        public ActionFilter(AccountRepository repository){
            this.Repository = repository;
            this.ruleCode = new groupRuleCode();
        }

        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            this._session = context.HttpContext.Session;
            this.loginID = _session.GetInt32("loginID");
            this.loginAccLV = _session.GetInt32("loginAccLV");
            this.loginGroupID = _session.GetInt32("loginGroupID");
            this.ruleVal = _session.GetInt32("ruleVal");
            this.loginTimeStamp = _session.GetString("loginTimeStamp");
            var controllerName = context.RouteData.Values["controller"].ToString();
            var actionName = context.RouteData.Values["action"].ToString();

            if(chkLoginStatusIsInvalid(controllerName, actionName)){
                gotoLogOut(context);
            }  
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            /*var err = new errorMsg(){};
            var result = new cusResult<dynamic>{
                Code = 200, Error = err, Result = context.Result, status="success"
            };
            context.Result = new OkObjectResult(result);*/
        }


        private bool chkLoginStatusIsInvalid(string controllerName, string actionName){
            var goLogOut = false;
            if(ruleVal==null || !isLoginInfo(loginID, loginGroupID) || !chkCurrentUser(loginID, loginTimeStamp)){
                return true; 
            }
            switch(controllerName){
                case "AdminFn": goLogOut = ((ruleVal & ruleCode.adminFn) == 0)? true : goLogOut; break;
                case "ApplicationSign": goLogOut = ((ruleVal & ruleCode.applySign) == 0)? true : goLogOut; break;
                case "ApplyLeave": goLogOut = ((ruleVal & ruleCode.baseActive) == 0)? true : goLogOut; break;
                case "ApplyOvertime": goLogOut = ((ruleVal & ruleCode.baseActive) == 0)? true : goLogOut; break;
                case "DepartmentList": goLogOut = ((ruleVal & ruleCode.departmentList) == 0)? true : goLogOut; break;
                case "EmployeeDetail": break;
                case "EmployeeList": goLogOut = ((ruleVal & ruleCode.allEmployeeList) == 0 && 
                                                 (ruleVal & ruleCode.departEmployeeList) ==0)? true : goLogOut; break;
                case "Home": break;
                case "Message": break;
                case "PunchCard": goLogOut = ((ruleVal & ruleCode.baseActive) == 0)? true : goLogOut; break;
                case "SetRule": goLogOut = ((ruleVal & ruleCode.setRule) == 0)? true : goLogOut; break;
                default: goLogOut = true; break;
            }
            if(controllerName == "EmployeeList"){
                switch(actionName){
                    case "createEmployee": 
                    case "updateEmployee": 
                    case "delEmployee": goLogOut = ((ruleVal & ruleCode.employeeEdit) == 0)? true : goLogOut; break;
                    default: break;
                }
            }
            if(controllerName == "PunchCard"){
                switch(actionName){
                    case "forceAddPunchCardLog": 
                    case "forceUpdatePunchCardLog": 
                    case "delPunchCardLog": goLogOut = ((ruleVal & ruleCode.editPunchLog) == 0)? true : goLogOut; break;
                    default: break;
                }
            }
            return goLogOut;
        }

        private bool chkCurrentUser(int? loginID, string loginTimeStamp){
            string getTimeStamp = Repository.QueryTimeStamp(loginID);
            if(loginTimeStamp == getTimeStamp){
                return true;
            }else{
                return false;
            }
        }

        private bool isLoginInfo(int? loginID, int? loginGroupID)
        {
            if(loginID == null || loginGroupID == null){
                return false;
            }else{
                return true;
            }
        }

        private void gotoLogOut(ActionExecutingContext context){
             _session.Clear();
 
            if(context.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest"){
                var err = new errorMsg(){};
                var result = new cusResult<dynamic>{
                    Code = 401, Error = err, Result = "-2", StatusText="fail"
                };
                context.Result = new BadRequestObjectResult(result);
            }else{
                context.Result = new RedirectResult("/Home/logOut");
            }
        }
        

        //------------------------------------------------------------------------------------------------------------------
        //filterContext.HttpContext.Response.Redirect("/Login/Index");  //这种跳转方式，会继续向下执行Controller的方法并返回ActionResult
        //context.Result = Redirect("/Login/Index");//这种跳转方式直接返回一个ActionResult，不会继续向下执行，而是直接跳转。速度快。
        //context.Result = new OkObjectResult(-2);
        //context.Result = new OkObjectResult(result);
        //context.Result = new RedirectResult("/Home/logOut");
        //context.Result = new BadRequestObjectResult(result);
        //var actionName = context.RouteData.Values["action"].ToString();
    }

    
}


