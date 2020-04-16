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
    public class DepartmentListController : BaseController
    {
        public MasterRepository Repository { get; }

        public DepartmentListController(MasterRepository repository, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
        }

        public IActionResult Index()
        {           
            return selectPage();
            //return RedirectToAction("logOut", "Home"); //轉址到特定Controller的ACTION名字
        }

        public IActionResult selectPage(){
            ViewBag.ruleVal = ruleVal;
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            ViewBag.loginAccLV = loginAccLV;
            return View("DepartmentListPage");
        }
        //---------------------------------------------------------------------------------------
        
        public object getAllDepartPosition(){
            return Repository.GetAllDepartPosition();
        }

        public object getAllPrincipal(){
            return Repository.GetAllPrincipal();
        }
        
        public int createDepartment(Department newData){
            newData.lastOperaAccID = (int)loginID;
            newData.createTime = DateTime.Now;   
            return Repository.CreateDepartment(newData);
        }

        public int delDepartment(int departID){
            return Repository.DelDepartment(departID);
        }

        public int updateDepartment(Department updateDate){
            updateDate.lastOperaAccID = (int)loginID;
            updateDate.updateTime = DateTime.Now; 
            return Repository.UpdateDepartment(updateDate);
        }





        
    }
}
