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
                                IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)                
        {
            this.aRepository = a_repository;
            this.mRepository = m_repository;
            this.pRepository = p_repository;
            this.chkWarn = new ChkPunchLogWarn(p_repository, httpContextAccessor);
            this.calTime = new CalWorkTime(p_repository, httpContextAccessor);
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

        public void manual_refreshPunchLogWarn(){
            if(((int)ruleVal & new groupRuleCode().adminFn) >0){
                chkWarn.start();
            }
        }

        public void manual_calWorkTime(){
            if(((int)ruleVal & new groupRuleCode().adminFn) >0){
                calTime.start();
            }
        }

        public void manual_calAnnualLeave(){
            if(((int)ruleVal & new groupRuleCode().adminFn) >0){
                calAnnual.start();
            }
        }

        public object getFilterOption(){
            var category = aRepository.GetOpLogCategory();
            var userName = mRepository.GetAllPrincipal();
            return new {category, userName};
        }
    }

}
