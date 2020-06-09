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
        
        public AdminFnController(AdminFnRepository repository, MasterRepository m_repository,
                                IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)                
        {
            this.aRepository = repository;
            this.mRepository = m_repository;
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
    }

}
