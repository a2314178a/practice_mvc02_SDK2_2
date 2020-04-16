using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Pomelo.AspNetCore.TimedJob;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;

namespace practice_mvc02.Models
{
    public class countWorkTimeJob : Job
    {
        private CalWorkTime calTime {get;}


        public countWorkTimeJob(PunchCardRepository repository, IHttpContextAccessor httpContextAccessor){
            this.calTime = new CalWorkTime(repository, httpContextAccessor);
        }


        // Begin 起始時間；Interval執行時間間隔，單位是毫秒，建議使用以下格式，ex:3小時(1000 * 3600 * 3)；
        //SkipWhileExecuting是否等待上一個執行完成，true為等待；
        //[Invoke(Begin = "2016-11-29 22:10", Interval = 1000 * 3600*3, SkipWhileExecuting =true)]
        [Invoke(Begin = "2020-02-21 00:01", Interval = 1000*3600*24, SkipWhileExecuting =true, IsEnabled = true)]
        public void Run()
        {
            Console.WriteLine("Start countWorkTimeJob------"+DateTime.Now);
            calTime.start();
            Console.WriteLine("Finish countWorkTimeJob------"+DateTime.Now);
        }

        

     



    }
}