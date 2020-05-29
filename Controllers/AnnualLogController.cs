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
    //[TypeFilter(typeof(ActionFilter))]
    public class AnnualLogController : BaseController
    {
        public AnnualLogRepository aRepository { get; }
        public ExportXlsxRepository eRepository { get; }
        public MasterRepository mRepository { get; }
        
        public AnnualLogController(AnnualLogRepository repository, ExportXlsxRepository repository02, 
                                   MasterRepository repository03, IHttpContextAccessor httpContextAccessor
                                    ):base(httpContextAccessor)
        {
            this.aRepository = repository;
            this.eRepository = repository02;
            this.mRepository = repository03;
        }

        public object getDepartment(){
            return eRepository.GetDepartment();
        }

        public object getDepartmentEmployee(string depart){
            return eRepository.GetDepartmentEmployee(depart);
        }

        public object getAnnualLog(int selID, DateTime sDate, DateTime eDate){
            var log = aRepository.GetAnnualLog(selID, sDate, eDate.AddDays(1));
            var offset = aRepository.GetOffsetLog(selID, sDate, eDate.AddDays(1));
            var day = mRepository.GetMyAnnualLeave(selID);
            return new{log, day, offset};
        }

        public int addUpAnnualStatus(EmployeeAnnualLeave annualData, AnnualDaysOffset offsetData){
            offsetData.createTime = annualData.updateTime = definePara.dtNow();
            offsetData.lastOperaAccID = (int)loginID;
            var res = 0;
            if(aRepository.AddOffsetData(offsetData) == 1){
                res = aRepository.UpEmployeeAnnualDays(annualData);
            }
            return res;
        }
    }
}
