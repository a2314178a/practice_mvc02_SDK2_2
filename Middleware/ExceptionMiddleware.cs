using System;
using System.IO;
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
                await _next(context);
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
    }
    
}


