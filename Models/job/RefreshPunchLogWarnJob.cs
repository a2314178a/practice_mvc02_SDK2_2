using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Pomelo.AspNetCore.TimedJob;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;
using practice_mvc02.Models;
using System.IO;

namespace practice_mvc02.Models
{
    public class RefreshPunchLogWarnJob : Job
    {
        private ChkPunchLogWarn chkWarn {get;}

        public RefreshPunchLogWarnJob(PunchCardRepository repository, IHttpContextAccessor httpContextAccessor){
            this.chkWarn = new ChkPunchLogWarn(repository, httpContextAccessor);
        }

        // Begin 起始時間；Interval執行時間間隔，單位是毫秒，建議使用以下格式，ex:3小時(1000 * 3600 * 3)；
        //SkipWhileExecuting是否等待上一個執行完成，true為等待；
        //[Invoke(Begin = "2016-11-29 22:10", Interval = 1000 * 3600*3, SkipWhileExecuting =true)]
        [Invoke(Begin = "2020-02-21 00:00", Interval = 1000*3600*24, SkipWhileExecuting =true, IsEnabled = true)]
        public void Run()
        {
            Console.WriteLine("Start RefreshPunchLogWarnJob------"+definePara.dtNow());
            try{
                chkWarn.start();
            }catch(Exception ex){
                string docPath = Environment.CurrentDirectory;
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "errorLog.txt"), true)){
                   outputFile.WriteLineAsync(definePara.dtNow() + "  " + ex.ToString());
                }
            }
            Console.WriteLine("Finish RefreshPunchLogWarnJob------"+definePara.dtNow());
        }
    }
}