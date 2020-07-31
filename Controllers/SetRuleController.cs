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
        public punchCardFunction punchCardFn {get;}

        public SetRuleController(SetRuleRepository repository, punchCardFunction fn,
                                    IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
            this.punchCardFn = fn;
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

        public dynamic addUpTimeRule(WorkTimeRule data){
            bool hasSame = Repository.chkSameWorkTime(data);
            bool isLegal = chkDataLegal(data);
            if(!isLegal){
                return "illegal";
            }
            var result = 0;
            data.lastOperaAccID = (int)loginID;
            if(data.ID==0){
                data.createTime = definePara.dtNow();
                result = Repository.AddTimeRule(data);
            }else{
                data.updateTime = definePara.dtNow();
                result = Repository.UpdateTimeRule(data);
            }
            if(hasSame && result == 1){
                return "same";
            }
            return result;
        }

        public int delTimeRule(int timeRuleID){
            return Repository.DelTimeRule(timeRuleID, (int)loginID);
        }

        public bool chkDataLegal(WorkTimeRule data){
            var wt = punchCardFn.workTimeProcess(data);
            var result = true;
            result = (wt.sWorkDt < wt.eWorkDt)? result : false;
            result = (wt.eWorkDt - wt.sWorkDt).TotalMinutes <=720? result : false;  //720mins = 12hours
            result = (wt.sRestDt > wt.sWorkDt && wt.sRestDt < wt.eWorkDt)? result : false;
            result = (wt.eRestDt > wt.sWorkDt && wt.eRestDt < wt.eWorkDt)? result : false;
            result = (wt.sRestDt <= wt.eRestDt)? result : false;
            return result;
        }

        #endregion //timeRule
        
        //-----------------------------------------------------------------------------------------------------

        #region GroupRule
        public object getAllGroup(){
            return Repository.GetAllGroup();
        }

        public int addGroup(GroupRule newGroup){
            newGroup.lastOperaAccID = (int)loginID;
            newGroup.createTime = definePara.dtNow();
            return Repository.AddGroup(newGroup);
        }

        public int delGroup(int groupID){
            return Repository.DelGroup(groupID, (int)loginID);
        }

        public int updateGroup(GroupRule updateGroup){
            updateGroup.lastOperaAccID = (int)loginID;
            updateGroup.updateTime = definePara.dtNow();
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
                spDate.createTime = definePara.dtNow();
                return Repository.AddSpecialTime(spDate);
            }else{
                spDate.updateTime = definePara.dtNow();
                return Repository.UpdateSpecialTime(spDate);
            } 
        }

        public int delSpecialDate(int spDateID){
            return Repository.DelSpecialDate(spDateID, (int)loginID);
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
               data.createTime = definePara.dtNow();
               return Repository.AddLeave(data);
            }else{
                data.updateTime = definePara.dtNow();
                return Repository.UpdateLeave(data);
            }
        }

        public int delLeave(int leaveID){
            return Repository.DelLeave(leaveID, (int)loginID);
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
               data.createTime = definePara.dtNow();
               return Repository.AddSpLeaveRule(data);
            }else{
                data.updateTime = definePara.dtNow();
                return Repository.UpdateSpLeaveRule(data);
            }
        }

        public int delSpLeaveRule(int ruleID){
            return Repository.DelSpLeaveRule(ruleID, (int)loginID);
        }

        #endregion //spLeaveRule

        //------------------------------------------------------------------------------------------------------
        
    }
}
