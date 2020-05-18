using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;
using practice_mvc02.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace practice_mvc02.Models
{
    public class RemoveXlsxFile
    {
        private IHostingEnvironment _hostingEnvironment;
        public RemoveXlsxFile(IHostingEnvironment hostingEnvironment){
            this._hostingEnvironment = hostingEnvironment;
        } 

        public void start(int delDay = 7){ 
            delFile(delDay);    //刪除幾天前的檔案
        }

        private void delFile(int delDay){                               
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string subFolder = definePara.getPunchExcelSubFolder();
            string fileFolderPath = Path.Combine(sWebRootFolder, subFolder);
            if (!Directory.Exists(fileFolderPath)){     
                return;
            }                                                           
            DirectoryInfo dir = new DirectoryInfo(fileFolderPath);
            FileInfo[] files = dir.GetFiles();
            if(files != null){      
                DateTime dtNow = definePara.dtNow();                
                foreach(FileInfo file in files){
                    var fileTime = file.CreationTime;
                    if(fileTime < dtNow.AddDays(-1*delDay)){
                        file.Delete();
                    }
                }
            }
        }
    }
}