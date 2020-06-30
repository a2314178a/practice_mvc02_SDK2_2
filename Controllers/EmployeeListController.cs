using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class EmployeeListController : BaseController
    {
        public MasterRepository Repository { get; }
        public loginFunction loginFn {get;}

        public EmployeeListController(MasterRepository repository, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
            this.loginFn = new loginFunction(repository);
        }

        public IActionResult Index(string page="list")
        {       
            return selectPage(page);    
        }
        
        public IActionResult selectPage(string page){
            ViewBag.ruleVal = ruleVal;
            ViewBag.canEmployeeEdit = (ruleVal & ruleCode.employeeEdit) > 0 ? true : false;
            ViewBag.seeAllEm = (ruleVal & ruleCode.allEmployeeList) > 0 ? 1 : 0;
            ViewBag.seeDepartEm = (ruleVal & ruleCode.departEmployeeList) > 0 ? 1 : 0;
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            ViewBag.ID = (int)loginID;
            ViewBag.loginAccLV = loginAccLV;
            ViewBag.Page = page;
            return View("EmployeeListPage");
        }
 
        //--------------------------------------------------------------------------------
        #region employee
        public object getThisLvAllAcc(string fName, string fDepart, string fPosition){
            fName = String.IsNullOrEmpty(fName)? "" : fName;
            fDepart = String.IsNullOrEmpty(fDepart)? "" : fDepart;
            fPosition = String.IsNullOrEmpty(fPosition)? "" : fPosition;
            var crossDepart = ((ruleVal & ruleCode.allEmployeeList) > 0)? true: false;
            return Repository.GetThisLvAllAcc((int)loginID, crossDepart, (int)loginAccLV, fName, fDepart, fPosition);
        }

        public object getFilterOption(){
            var crossDepart = ((ruleVal & ruleCode.allEmployeeList) > 0)? true: false;
            object departOption = Repository.GetDepartOption((int)loginID, crossDepart);
            object positionOption = Repository.GetPositionOption((int)loginID, crossDepart);
            return new{department = departOption, position = positionOption};
        }

        public object getAccountDetail(int employeeID){
            object detail = Repository.GetAccountDetail(employeeID);
            object thisAllManager = Repository.GetThisAllManager(employeeID);
            return new{detail, manager=thisAllManager};
        }

        public int delEmployee(int employeeID){
            return Repository.DelEmployee(employeeID, (int)loginID);
        }

        public int addUpdateEmployee(Account accData, EmployeeDetail employeeDetail, int[] thisManager, string action){
            int result = 0;
            accData.lastOperaAccID = employeeDetail.lastOperaAccID = (int)loginID;
            if(action =="add"){
                accData.password = loginFn.GetMD5(accData.account + accData.password);
                accData.createTime = employeeDetail.createTime = definePara.dtNow();
                result = Repository.CreateEmployee(accData, employeeDetail, thisManager);  //-2:mulUserlongin -1:already account, 0:add fail, 1:add success
            }else if(action == "update"){
                if(accData.password != null){
                    accData.password = loginFn.GetMD5((accData.account + accData.password));
                }
                accData.updateTime = employeeDetail.updateTime = definePara.dtNow();
                result = Repository.UpdateEmployee(accData, employeeDetail, thisManager);
            }
            return result;
        }
        #endregion

        //----------------------------------------------------------------------------------------

        #region subWindow
        public int chkLoginStatus(){
            return 1;
        }

        public IActionResult showAddForm(int ID){
            if(((ruleVal & ruleCode.employeeEdit) == 0)){
                return RedirectToAction("logOut", "Home"); //轉址到特定Controller的ACTION名字
            }
            ViewBag.ruleVal = ruleVal;
            ViewBag.loginAccLV = loginAccLV;
            if(ID == 0){
                ViewBag.Action = "add";
                ViewBag.mainText = "新增人員";
            }else{     
                ViewBag.Action = "update";
                ViewBag.mainText = "編輯員工";
                ViewBag.ID = ID;   
            }
            return View("subPage/addUpdateForm");
        }

        public object getSelOption(){
            object departOption = null;
            if((ruleVal & ruleCode.allEmployeeList) > 0){
                departOption = Repository.GetAllDepartPosition();
            }else{
                departOption = Repository.GetThisDepartPosition((int)loginID);
            }
            object timeOption = Repository.GetAllTimeRule();
            object groupOption = Repository.GetAllGroup();
            object employeeOption = Repository.GetAllPrincipal();

            return new{departOption, timeOption, groupOption, employeeOption};             
        }
        
        #endregion


    }



}
