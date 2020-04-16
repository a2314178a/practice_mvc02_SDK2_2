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
    public class SetRuleController : BaseController
    {
        //private readonly ILogger<HomeController> _logger;
        public SetRuleRepository Repository { get; }

        public SetRuleController(SetRuleRepository repository, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
        }

        public IActionResult Index(string page)
        {           
            return selectPage(page);
        }

       public IActionResult selectPage(string page){
            ViewBag.ruleVal = ruleVal;
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            ViewBag.loginAccLV = loginAccLV;
            ViewBag.Page = page;
            return View("SetRulePage");
        }

       //--------------------------------------------------------------------------------------

        #region timeRule
        public object getAllTimeRule(){
           return Repository.GetAllTimeRule();
       }

        public int addUpTimeRule(WorkTimeRule data){
            data.lastOperaAccID = (int)loginID;
            if(data.ID==0){
                data.createTime = DateTime.Now;
                return Repository.AddTimeRule(data);
            }else{
                data.updateTime = DateTime.Now;
                return Repository.UpdateTimeRule(data);
            }
        }

        public int delTimeRule(int timeRuleID){
            return Repository.DelTimeRule(timeRuleID);
        }

        #endregion //timeRule
        
        //-----------------------------------------------------------------------------------------------------

        #region GroupRule
        public object getAllGroup(){
            return Repository.GetAllGroup();
        }

        public int addGroup(GroupRule newGroup){
            newGroup.lastOperaAccID = (int)loginID;
            newGroup.createTime = DateTime.Now;
            return Repository.AddGroup(newGroup);
        }

        public int delGroup(int groupID){
            return Repository.DelGroup(groupID);
        }

        public int updateGroup(GroupRule updateGroup){
            updateGroup.lastOperaAccID = (int)loginID;
            updateGroup.updateTime = DateTime.Now;
            return Repository.UpdateGroup(updateGroup);
        }

        #endregion  //GroupRule

        //-----------------------------------------------------------------------------------------------------

        #region specialDate
        
        public object getAllSpecialDate(){
            return Repository.GetAllSpecialDate();
        }

        public object getClassDepart(){
            return Repository.GetClassDepart();
        }

        public int addUpSpecialTime(SpecialDate spDate){
            spDate.lastOperaAccID = (int)loginID;
            if(spDate.ID == 0){     
                spDate.createTime = DateTime.Now;
                return Repository.AddSpecialTime(spDate);
            }else{
                spDate.updateTime = DateTime.Now;
                return Repository.UpdateSpecialTime(spDate);
            } 
        }

        public int delSpecialDate(int spDateID){
            return Repository.DelSpecialDate(spDateID);
        }

        #endregion //specialDate

        //----------------------------------------------------------------------------------------------------------------------

        #region leaveTimeRule
        
        public object getAllLeaveRule(){
            return Repository.GetAllLeaveRule();
        }

        public int addUpLeave(LeaveName data){
            data.lastOperaAccID = (int)loginID;
            if(data.ID == 0){
               data.createTime = DateTime.Now;
               return Repository.AddLeave(data);
            }else{
                data.updateTime = DateTime.Now;
                return Repository.UpdateLeave(data);
            }
        }

        public int delLeave(int leaveID){
            return Repository.DelLeave(leaveID);
        }

        #endregion //leaveTimeRule

        //------------------------------------------------------------------------------------------------------

        #region spLeaveRule

        public object getAllSpLeaveRule(){
            return Repository.GetAllSpLeaveRule();
        }

        public int addUpSpLeaveRule(AnnualLeaveRule data){
            data.lastOperaAccID = (int)loginID;
            if(data.ID == 0){
               data.createTime = DateTime.Now;
               return Repository.AddSpLeaveRule(data);
            }else{
                data.updateTime = DateTime.Now;
                return Repository.UpdateSpLeaveRule(data);
            }
        }

        public int delSpLeaveRule(int ruleID){
            return Repository.DelSpLeaveRule(ruleID);
        }

        #endregion //spLeaveRule

        //------------------------------------------------------------------------------------------------------
        
    }
}
