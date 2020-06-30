using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;

namespace practice_mvc02.Repositories
{
    public class MessageRepository : BaseRepository
    {
        public MessageRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public object GetReceiveMessage(int loginID, int readStatus){
            var query = from a in _DbContext.msgsendreceive
                        join b in _DbContext.message on a.messageID equals b.ID
                        join c in _DbContext.accounts on a.sendID equals c.ID into tmp
                        from d in tmp.DefaultIfEmpty()
                        where a.receiveID == loginID && a.read <= readStatus && a.rDelete == false
                        orderby a.createTime descending
                        select new{
                            a.ID, a.messageID, a.read, a.createTime, 
                            b.title, b.content, 
                            userName=(d==null? null:d.userName),
                        };
            return query.ToList();
        }

        public object GetSendMessage(int loginID){
            var query = from a in _DbContext.msgsendreceive
                        join b in _DbContext.message on a.messageID equals b.ID
                        join c in _DbContext.accounts on a.receiveID equals c.ID into tmp
                        from d in tmp.DefaultIfEmpty()
                        where a.sendID == loginID && a.sDelete == false
                        orderby a.createTime descending
                        select new{
                            a.ID, a.messageID, a.createTime, 
                            b.title, b.content,
                            userName=(d==null? null:d.userName),
                        };
            return query.ToList();
        }

        public int SetHadReadMsg(int[] msgID, int loginID){
            int count = 0;
            var context = _DbContext.msgsendreceive.Where(b=>msgID.Contains(b.ID)).ToList();
            foreach(var msg in context){
                msg.read = 1;
                msg.lastOperaAccID = loginID;
                msg.updateTime = definePara.dtNow();
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        public int DelMessage(int[] msgID, string sel, int loginID){
            int count = 0;
            var context = _DbContext.msgsendreceive.Where(b=>msgID.Contains(b.ID)).ToList();
            foreach(var msg in context){
                if(sel=="rDel"){
                    msg.rDelete = true;
                    msg.lastOperaAccID = loginID;
                    msg.updateTime = definePara.dtNow();
                    count = _DbContext.SaveChanges();
                }else if(sel=="sDel"){
                    var sameSend = _DbContext.msgsendreceive
                                    .Where(b=>b.messageID==msg.messageID && b.sendID==loginID).ToList();
                    foreach(var tmp in sameSend){
                        tmp.sDelete = true;
                        tmp.lastOperaAccID = loginID;
                        tmp.updateTime = definePara.dtNow();
                    }
                    count = _DbContext.SaveChanges();
                }
                var allDel = _DbContext.msgsendreceive.Where(b=>b.rDelete && b.sDelete).ToList();
                _DbContext.msgsendreceive.RemoveRange(allDel);
                _DbContext.SaveChanges();
                
            }
            return count;
        }
        
        public object GetReceiveOption(){
            var query = from a in _DbContext.departments
                        join b in _DbContext.accounts on a.ID equals b.departmentID into tmp
                        from c in tmp.DefaultIfEmpty()
                        orderby a.department
                        select new {
                            a.department, 
                            userName=(c==null? null:c.userName), accID=(c==null? 0:c.ID)
                        };
            return query.ToList();
        }

        public int createMessage(Message msg){
            _DbContext.message.Add(msg);
            _DbContext.SaveChanges();
            return msg.ID;
        }

        public int SendMessage(MsgSendReceive record){
            var count = 0;
            _DbContext.msgsendreceive.Add(record);
            count = _DbContext.SaveChanges();
            return count;
        }

        public List<Account> GetAllAccID(){
            var query = _DbContext.accounts;
            return query.ToList();
        }

        public List<Account> GetDepartAllAccID(string depart){
            var query = from a in _DbContext.departments
                        join b in _DbContext.accounts on a.ID equals b.departmentID
                        where a.department == depart
                        select b;
            return query.ToList();
        }

        
    }
}