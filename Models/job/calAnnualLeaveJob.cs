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
    public class calAnnualLeaveJob : Job
    {
        private CalAnnualLeave work {get;}


        public calAnnualLeaveJob(AnnualLeaveRepository repository){
            this.work = new CalAnnualLeave(repository);
        }


        // Begin 起始時間；Interval執行時間間隔，單位是毫秒，建議使用以下格式，ex:3小時(1000 * 3600 * 3)；
        //SkipWhileExecuting是否等待上一個執行完成，true為等待；
        //[Invoke(Begin = "2016-11-29 22:10", Interval = 1000 * 3600*3, SkipWhileExecuting =true)]
        [Invoke(Begin = "2020-07-01 16:10", Interval = 1000*3600*24, SkipWhileExecuting =true, IsEnabled = true)]
        public void Run()
        {
            Console.WriteLine("Start calAnnualLeaveJob------"+definePara.dtNow());
            try{
                work.start();
            }catch(Exception ex){
                string docPath = Environment.CurrentDirectory;
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "errorLog.txt"), true)){
                    var str = $"{definePara.dtNow()} - calAnnualLeaveJob  ";
                    outputFile.WriteLine(str + ex.ToString() + "\r\n");
                }
            }
            Console.WriteLine("Finish calAnnualLeaveJob------"+definePara.dtNow());
        }

        

     



    }
}