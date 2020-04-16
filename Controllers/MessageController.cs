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

        public IActionResult Index(string page)
        {           
            return selectPage(page);
            //return RedirectToAction("logOut", "Home"); //轉址到特定Controller的ACTION名字
        }

        public IActionResult selectPage(string page){
            ViewBag.ruleVal = ruleVal;
            ViewData["loginName"] = loginName;
            ViewBag.Auth = "Y";
            switch(page){
                case "all": ViewBag.Page = "all";break;
                case "write": ViewBag.Page = "write";break;
                case "backup": ViewBag.Page = "backup";break;
                default : ViewBag.Page = "new"; break;
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

        public int ignoreMessage(int relatedID){
            return Repository.IgnoreMessage(relatedID, (int)loginID);
        }

        public int delMessage(int relatedID, string sel){
            return Repository.DelMessage(relatedID, sel ,(int)loginID);
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
            msg.createTime = DateTime.Now;
            var msgID = Repository.createMessage(msg);
            if(msgID >0){
                return sendMessage(msgID, depart, receiveID);
            }else{
                return 0;
            }
        }

        public int sendMessage(int msgID, string depart, string receiveID){
            var sendSel = 0;
            if((depart == "-1") || (depart == "" && receiveID == "-1")){
                sendSel = 1;    //send All
            }else if(receiveID == "-1"){
                sendSel = 2;    //send depart all
            }else{
                sendSel = 3;    //send one
            }
            List<Account> receiveAllID = new List<Account>(){};
            switch(sendSel){
                case 1: receiveAllID = Repository.GetAllAccID();  break;
                case 2: receiveAllID = Repository.GetDepartAllAccID(depart);  break;
                case 3: var result = 0; 
                        Int32.TryParse(receiveID, out result);
                        receiveAllID.Add(new Account{ID = result}); 
                        break;
            }
            var count = 0;
            foreach(var account in receiveAllID){
                var record = new MsgSendReceive();
                record.messageID = msgID;
                record.sendID = record.lastOperaAccID = (int)loginID;
                record.receiveID = account.ID;
                record.createTime = DateTime.Now;
                count += Repository.SendMessage(record);
            }
            return count;
        }

        #endregion //write Message
        
    }
}
