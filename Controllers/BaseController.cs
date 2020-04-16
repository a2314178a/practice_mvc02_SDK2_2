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
    public class BaseController : Controller
    {
        protected ISession _session;
        protected string loginAcc;
        protected int? loginID;
        protected int? loginAccLV;
        protected int? loginGroupID;
        protected string loginName;
        protected string loginTimeStamp;
        protected int? loginDepartmentID;
        protected int? ruleVal;
        protected int? principalID;
        public groupRuleCode ruleCode {get;}

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            this._session = httpContextAccessor.HttpContext.Session;
            this.loginAcc = _session.GetString("loginAcc");
            this.loginID = _session.GetInt32("loginID");
            this.loginAccLV = _session.GetInt32("loginAccLV");
            this.loginName = _session.GetString("loginName");
            this.loginTimeStamp = _session.GetString("loginTimeStamp");
            this.loginDepartmentID = _session.GetInt32("loginDepartmentID");
            this.loginGroupID = _session.GetInt32("loginGroupID");
            this.ruleVal = _session.GetInt32("ruleVal");
            this.principalID = _session.GetInt32("principalID");
            this.ruleCode = new groupRuleCode();
        }

        

    }
}
