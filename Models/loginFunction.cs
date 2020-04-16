using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using practice_mvc02.Repositories;

namespace practice_mvc02.Models
{
    public class loginFunction
    {
        public BaseRepository Repository { get; }
        public loginFunction(BaseRepository repository){
            this.Repository = repository;
        }

        public bool isLoginInfo(int? loginID, int? loginGroupID)
        {
            if(loginID == null || loginGroupID == null){
                return false;
            }else{
                return true;
            }
        }

        public string GetMD5(string original) 
        { 
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider(); 
            byte[] b = md5.ComputeHash(Encoding.UTF8.GetBytes(original)); 
            return BitConverter.ToString(b).Replace("-", string.Empty); 
        }

        public bool chkCurrentUser(int? loginID, string loginTimeStamp){
            string getTimeStamp = Repository.QueryTimeStamp(loginID);
            if(loginTimeStamp == getTimeStamp){
                return true;
            }else{
                return false;
            }
        }
        
    }
}