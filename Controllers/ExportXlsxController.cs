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
    public class ExportXlsxController : BaseController
    {
        public ExportXlsxRepository Repository { get; }
        private exportPunchLogFunction xlsxFn { get; }
        private IHostingEnvironment _hostingEnvironment;
        
        public ExportXlsxController(ExportXlsxRepository repository, PunchCardRepository repository02, IHttpContextAccessor httpContextAccessor, 
                                    IHostingEnvironment hostingEnvironment):base(httpContextAccessor)
        {
            xlsxFn = new exportPunchLogFunction(repository, repository02, httpContextAccessor);
            this._hostingEnvironment = hostingEnvironment;
            this.Repository = repository;
        }

        public IActionResult Export()
        {
            var fileName = "punchCardReport.xlsx";
            return File(_session.GetString("punchLogXlsxFile"), 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        fileName);
        }

        public object getPrintTableData(exportPunchLogXlsxPara exportPara)
        {
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string subFolder = definePara.getPunchExcelSubFolder();
            string fileFolderPath = Path.Combine(sWebRootFolder, subFolder);
            if (!Directory.Exists(fileFolderPath)){     //需用完整目錄來檢查
                Directory.CreateDirectory(fileFolderPath);
            }
            string sFileName = $"{Guid.NewGuid()}.xlsx";

            FileInfo file = new FileInfo(Path.Combine(fileFolderPath, sFileName));
            exportPara.crossDepart = ((ruleVal & ruleCode.allEmployeeList) > 0)? true: false;
            exportPara.loginID = (int)loginID;
            var tableData = xlsxFn.createXlsxPunchLog(file, exportPara);
            if(tableData != null){
                _session.SetString("punchLogXlsxFile", Path.Combine(subFolder, sFileName)); //若下載是從根目錄開始算
            }
            return tableData;
        }

        public int chkFileStatus(){
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string filePathAbs = Path.Combine(sWebRootFolder, _session.GetString("punchLogXlsxFile"));
            return System.IO.File.Exists(filePathAbs)? 1 : 0;
        }

        public object getDepartment(){
            var crossDepart = ((ruleVal & ruleCode.allEmployeeList) > 0)? true: false;
            return Repository.GetDepartment((int)loginID, crossDepart);
        }

        public object getDepartmentEmployee(string depart){
            var crossDepart = ((ruleVal & ruleCode.allEmployeeList) > 0)? true: false;
            return Repository.GetDepartmentEmployee(depart, (int)loginID, crossDepart);
        }
    }
}
