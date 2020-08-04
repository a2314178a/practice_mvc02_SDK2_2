using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using practice_mvc02.Models;


namespace practice_mvc02.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if(chkIsFromDesktop(context)){
                    await _next(context);
                }else{
                    context.Response.StatusCode = 403;  //伺服器已經理解請求，但是拒絕執行它
                }   
            }
            catch (Exception ex)
            {
                string docPath = Environment.CurrentDirectory;
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "errorLog.txt"), true)){
                   await outputFile.WriteLineAsync(definePara.dtNow() + "  " + ex.ToString() + "\r\n");
                }
                context.Response.StatusCode = 500;
                //context.Response.Redirect("/Home/Error");
                //await context.Response
                    //.WriteAsync($"{GetType().Name} catch exception.");
                //
            }
        }

        private bool chkIsFromDesktop(HttpContext context){
            var userAgent = context.Request.Headers["User-Agent"].ToString().ToLower();
            //Console.WriteLine(userAgent);
            var desktop = new Regex(@"windows 98|windows me|windows nt");
            var mobile = new Regex(@"android|ipad|iphone|mobile|windows phone");
            if(desktop.IsMatch(userAgent)==true && mobile.IsMatch(userAgent)==false) {
                return true;
            }
            return false;
        }
    }
    
}


