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
    public class MessageController : BaseController
    {
        public MessageRepository Repository { get; }

        public MessageController(MessageRepository repository, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            this.Repository = repository;
        }

        public IActionResult Index(string page="new")
        {           
            return selectPage(page);
            //return RedirectToAction("logOut", "Home"); //轉址到特定Controller的ACTION名字
        }

        public IActionResult selectPage(string page){
            ViewBag.ruleVal = ruleVal;
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            switch(page){
                case "new": ViewBag.Page = "new";break;
                case "all": ViewBag.Page = "all";break;
                //case "write": ViewBag.Page = "write";break;
                //case "backup": ViewBag.Page = "backup";break;
            }
            return View("MessagePage");  
        }

        //---------------------------------------------------------------------------------------
        
       
        #region  Message

        public object getReceiveMessage(int readStatus){
            return Repository.GetReceiveMessage((int)loginID, readStatus);
        }

        public object getSendMessage(){
            return Repository.GetSendMessage((int)loginID);
        }

        public int setHadReadMsg(int[] msgID){
            return Repository.SetHadReadMsg(msgID, (int)loginID);
        }

        public int delMessage(int[] msgID, string sel){
            return Repository.DelMessage(msgID, sel ,(int)loginID);
        }
        #endregion //Message

        //-----------------------------------------------------------------------------------------

        #region  write Message

        public object getReceiveOption(){           
            return Repository.GetReceiveOption();
        }

        public int sendMsg(Message msg, string depart, string receiveID){
            depart = String.IsNullOrEmpty(depart)? "" : depart;
            receiveID = String.IsNullOrEmpty(receiveID)? "" : receiveID;
            if((depart == "" || depart != "-1") && receiveID == ""){
                return 0;
            }
            msg.lastOperaAccID = (int)loginID;
            msg.createTime = definePara.dtNow();
            return Repository.createMessage(msg, depart, receiveID);
        }

        #endregion //write Message
        
    }
}
