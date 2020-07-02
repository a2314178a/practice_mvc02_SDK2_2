using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;
using Newtonsoft.Json;

namespace practice_mvc02.Repositories
{
    public class MasterRepository : BaseRepository
    {
        private CalAnnualLeave calAnnualDays {get;}
        
        public MasterRepository(DBContext dbContext, AnnualLeaveRepository repository):base(dbContext)
        {
            this.calAnnualDays = new CalAnnualLeave(repository);
        }

        #region employee CRUD

        public object GetThisLvAllAcc(int loginID, bool crossDepart, int loginAccLV, 
                                        string fName, string fDepart, string fPosition){
            object result = null;
            string departName = fDepart;
            if(crossDepart)
            {
                if(fDepart == "未指派" && fPosition == ""){
                    var query = from a in _DbContext.accounts
                                where (a.departmentID == 0 && a.userName.Contains(fName)) &&
                                    (a.accLV < definePara.getDIMALV() || loginAccLV == definePara.getDIMALV())
                                select new{
                                    a.ID, a.account, a.userName, department="", position=""
                                };
                    result = query.ToList();
                                
                }else if(fDepart == "未指派" && fPosition != ""){
                    result = new List<object>(){};
                }else{
                    var query = from a in _DbContext.accounts
                                join b in _DbContext.departments on a.departmentID equals b.ID into tmp
                                from c in tmp.DefaultIfEmpty()
                                where (a.userName.Contains(fName) && c.department.Contains(fDepart) && 
                                    c.position.Contains(fPosition)) &&
                                    (loginAccLV == definePara.getDIMALV() || a.accLV < definePara.getDIMALV())
                                orderby c.department
                                select new {
                                    a.ID, a.account, a.userName,
                                    department=(c==null? null:c.department), 
                                    position=(c==null? null:c.position),  
                                };
                    result = query.ToList();
                }
            }
            else
            {
                if(fDepart == "未指派" && fPosition == ""){
                    var query = from a in _DbContext.accounts
                                join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                                where a.departmentID == 0 && a.userName.Contains(fName) &&
                                      (b.principalID == loginID || b.principalAgentID == loginID)
                                      && a.accLV < definePara.getDIMALV()
                                select new{
                                    a.ID, a.account, a.userName, department="", position=""
                                };
                    result = query.ToList();

                }else if(fDepart == "未指派" && fPosition != ""){
                    result = new List<object>(){};
                }else{
                    var query = ((from a in _DbContext.accounts
                                join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                                from bb in noDepart.DefaultIfEmpty()
                                join c in _DbContext.employeeprincipals on a.ID equals c.employeeID
                                where (c.principalID == loginID || c.principalAgentID == loginID) 
                                    && a.userName.Contains(fName) && 
                                    bb.department.Contains(fDepart) && bb.position.Contains(fPosition)                                  
                                select new {
                                    a.ID, a.account, a.userName,
                                    department=(bb==null? "未指派":bb.department),
                                    position=(bb==null? "":bb.position),  
                                }).Union(
                                    from a in _DbContext.accounts
                                    join b in _DbContext.departments on a.departmentID equals b.ID
                                    where a.ID == loginID && a.userName.Contains(fName) && 
                                    b.department.Contains(fDepart) && b.position.Contains(fPosition)
                                    select new {
                                        a.ID, a.account, a.userName,
                                        department=b.department, position=b.position,  
                                    }
                                ));
                    result = query.ToList();
                }
            } 
            return result;
        }

        public int CreateEmployee(Account newEmployee, EmployeeDetail newDetail, int[] thisManager){
            int count = 0;
            var context = _DbContext.accounts.FirstOrDefault(b=>b.account == newEmployee.account);
            if(context != null){
                return -1;  //had account
            }
            _DbContext.accounts.Add(newEmployee);   //新增帳號
            count = _DbContext.SaveChanges();
            if(newEmployee.ID > 0){
                var context2 = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == newEmployee.ID);
                if(context2 == null){
                    newDetail.accountID = newEmployee.ID;
                    _DbContext.employeedetails.Add(newDetail);  //新增細項
                    _DbContext.SaveChanges();
                    setPrincipalAgent(newDetail.accountID, newDetail.myAgentID, newDetail.agentEnable); //設定職務代理人
                    calAnnualDays.calThisEmployeeAnnualDays(newDetail); //計算此人的年假

                    //record operateLOg
                    var dic = new Dictionary<string,string>{};
                    var opLog = new OperateLog(){
                        operateID=newEmployee.lastOperaAccID, employeeID=newEmployee.ID,
                        active="新增", category="員工資料", createTime=definePara.dtNow()
                    };
                    toNameFn.AddUpEmployee_covertToDic(ref dic, newEmployee, newDetail);    
                    var principal = "";
                    if(thisManager.Length>1){
                        for(var i =1; i< thisManager.Length; i++){
                           principal += toNameFn.GetNameByID(thisManager[i]) + "、";
                        }
                    }
                    dic.Add("principal", principal=="" ? "無": principal.Substring(0,principal.Length - 1));
                    opLog.content = toNameFn.AddUpEmployee_covertToText(dic);
                    saveOperateLog(opLog);    //紀錄操作紀錄
                }
                saveEmployeePrincipals(newEmployee, thisManager);   //新增所屬主管
            }
            return count;
        }

        public int DelEmployee(int employeeID, int operateID){
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=operateID, employeeID=employeeID,
                active="刪除", category="員工資料", createTime=definePara.dtNow()
            };
            int count = 0;
            var context = _DbContext.accounts.FirstOrDefault(b=>b.ID == employeeID);
            var detail = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == employeeID);
            if(context != null){
                toNameFn.AddUpEmployee_covertToDic(ref dic, context, detail);

                _DbContext.Remove(context);
                count = _DbContext.SaveChanges();
                if(count == 1){
                    if(detail != null){
                        _DbContext.Remove(detail);
                        _DbContext.SaveChanges();
                    }
                    //record operateLOg
                    opLog.content = toNameFn.AddUpEmployee_covertToText(dic);
                    saveOperateLog(opLog);    //紀錄操作紀錄
                }
            }
            return count;
        }

        public int UpdateEmployee(Account updateData, EmployeeDetail upDetail, int[] thisManager){
            int count = 0;
            var context = _DbContext.accounts.FirstOrDefault(b=>b.ID == updateData.ID);
            var detail = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == updateData.ID);
            //var oldContext = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(context));
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=updateData.lastOperaAccID, employeeID=updateData.ID,
                active="更新", category="員工資料", createTime=definePara.dtNow()
            };

            //employee base data
            if(context != null){
                toNameFn.AddUpEmployee_covertToDic(ref oDic, context, detail);
                var departIsChange = context.departmentID == updateData.departmentID? false :  true;
                if(departIsChange){
                    delPrincipal(ref thisManager, context.departmentID);                    
                }
                if(updateData.password != null){
                    context.password = updateData.password;
                }
                context.userName = updateData.userName;
                context.departmentID = updateData.departmentID;
                context.accLV = updateData.accLV;
                context.timeRuleID = updateData.timeRuleID;
                context.groupID = updateData.groupID;
                context.lastOperaAccID = updateData.lastOperaAccID;
                context.updateTime = updateData.updateTime;
                count = _DbContext.SaveChanges();       //更新帳號

                if(count ==1 && (thisManager[0]==-2 || departIsChange)){    //thisManager[0]=>-1:負責人無更動 -2:有更動
                    var oldQuery = _DbContext.employeeprincipals.Where(b=>b.employeeID==updateData.ID);
                    toNameFn.GetEmployeePrincipalName(oldQuery.ToList(), ref oDic);
                    _DbContext.RemoveRange(oldQuery);      //刪除舊所屬主管
                    _DbContext.SaveChanges();

                    saveEmployeePrincipals(updateData, thisManager);       //新增所屬主管
                    var newQuery = _DbContext.employeeprincipals.Where(b=>b.employeeID==updateData.ID);
                    toNameFn.GetEmployeePrincipalName(newQuery.ToList(), ref nDic);
                }
            }
            //employeeDetail
            if(detail != null){
                var diffStartDate = detail.startWorkDate != upDetail.startWorkDate? true : false;

                detail.sex = upDetail.sex;
                detail.birthday = upDetail.birthday;
                detail.humanID = upDetail.humanID;
                detail.myAgentID = upDetail.myAgentID;
                detail.agentEnable = upDetail.agentEnable;
                detail.startWorkDate = upDetail.startWorkDate;
                detail.lastOperaAccID = upDetail.lastOperaAccID;
                detail.updateTime = upDetail.updateTime;
                _DbContext.SaveChanges();   //更新細項

                //record operateLog
                toNameFn.AddUpEmployee_covertToDic(ref nDic, context, detail);
                opLog.content = toNameFn.AddUpEmployee_covertToText(nDic, oDic);
                saveOperateLog(opLog);  //紀錄操作紀錄

                setPrincipalAgent(detail.accountID, detail.myAgentID, detail.agentEnable);
                if(diffStartDate){
                    upDetail.accountID = updateData.ID;
                    calAnnualDays.calThisEmployeeAnnualDays(upDetail, true); //計算此人的年假
                }
            }
            return count;
        }

        public void delPrincipal(ref int[] thisManager, int departID){
            var depart = _DbContext.departments.FirstOrDefault(b=>b.ID==departID);
            if(depart != null){
                thisManager = thisManager.Where(val => val != depart.principalID).ToArray();
            }                              
        }

        public void saveEmployeePrincipals(Account account, int[] thisManager){
            var depart = _DbContext.departments.FirstOrDefault(b=>b.ID==account.departmentID);
            var departIsNull = depart == null? true : false;
            depart = depart == null? new Department(){} : depart;
            if(!departIsNull){
                var departAgent = (from a in _DbContext.employeedetails
                            join b in _DbContext.departments on a.accountID equals b.principalID
                            where b.ID == depart.ID && a.agentEnable == true
                            select a.myAgentID).FirstOrDefault();

                var downWithUp = new EmployeePrincipal(){
                    employeeID = account.ID,    principalID = depart.principalID,
                    principalAgentID = departAgent,   
                    lastOperaAccID = account.lastOperaAccID,  createTime = definePara.dtNow()
                };
                if(depart.principalID >0){
                    _DbContext.employeeprincipals.Add(downWithUp);  //所屬部門主管一定會新增
                    _DbContext.SaveChanges();
                }
            }
            foreach(var id in thisManager){
                if(id >0 && id != depart.principalID){
                    var thisAgent = _DbContext.employeedetails.Where(b=>b.accountID == id && b.agentEnable == true)
                                                            .Select(b=>b.myAgentID).FirstOrDefault();
                    var downWithUp02 = new EmployeePrincipal(){
                        employeeID = account.ID,    principalID = id,
                        principalAgentID = thisAgent,   
                        lastOperaAccID = account.lastOperaAccID,    createTime = definePara.dtNow()
                    };
                    _DbContext.employeeprincipals.Add(downWithUp02);
                    _DbContext.SaveChanges();
                }
            }
        }
        
        public Account GetEmployeeAccByID(int ID){
            Account context = _DbContext.accounts.FirstOrDefault(b=>b.ID == ID);
            return context;
        }

        public object GetDepartOption(int loginID, bool cross){
            object result = new List<string>(){};
            if(!cross){
                var query = (from a in _DbContext.accounts
                            join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                            join c in _DbContext.departments on a.departmentID equals c.ID into noDepart
                            from d in noDepart.DefaultIfEmpty()
                            where b.principalID == loginID || b.principalAgentID == loginID
                            select new{
                                department=(d==null? "未指派":d.department)
                            });
                var myself = (from a in _DbContext.accounts
                            join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                            from c in noDepart.DefaultIfEmpty()
                            where a.ID == loginID
                            select new{
                                department=(c==null? "未指派" : c.department)
                            });
                result = (query.Union(myself)).ToList();
            }else{
                result = (from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                        from c in noDepart.DefaultIfEmpty()
                        select new{
                            department=(c==null? "未指派":c.department)
                        }).Distinct().ToList();
            }
            return result;
        }

        public object GetDepartPosition(int loginID, bool cross){
            object query = new List<string>(){};
            if(!cross){
                query = (from a in _DbContext.accounts
                        join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                        join c in _DbContext.departments on a.departmentID equals c.ID into noDepart
                        from d in noDepart.DefaultIfEmpty()
                        where b.principalID == loginID || b.principalAgentID == loginID || a.ID == loginID
                        select new{
                            department=(d==null? "未指派":d.department),
                            position=(d==null? null:d.position)
                        }).Distinct().ToList();
            }else{
                query = (from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                        from c in noDepart.DefaultIfEmpty()
                        select new{
                            department=(c==null? "未指派":c.department),
                            position=(c==null? null:c.position)
                        }).Distinct().ToList();
            }
            return query;
        }

        public object GetPositionOption(int loginID, bool cross){
            object query = new List<string>(){};
            if(!cross){
                query = ((from a in _DbContext.accounts
                        join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                        join c in _DbContext.departments on a.departmentID equals c.ID
                        where b.principalID == loginID || b.principalAgentID == loginID
                        select c.position).Union(
                            from a in _DbContext.accounts
                            join b in _DbContext.departments on a.departmentID equals b.ID
                            where a.ID == loginID
                            select b.position
                        )).ToList();
            }else{
                query = _DbContext.departments.Select(b=>b.position).Distinct().ToList();
            }
            return query;
        }
        
        public dynamic GetThisAllManager(int employeeID){
            var query = from a in _DbContext.employeeprincipals
                        join b in _DbContext.accounts on a.principalID equals b.ID
                        where a.employeeID == employeeID
                        select new{b.ID, b.userName};
            return query.ToList();
        }

        public object GetMyAnnualLeave(int employeeID){
            var query = _DbContext.employeeannualleaves
                            .Where(b=>b.employeeID == employeeID && b.deadLine > definePara.dtNow())
                            .Select(b=>new{b.ID, b.specialDays, b.remainHours, b.deadLine})
                            .OrderBy(b=>b.deadLine);
            return query.ToList();
        }

        #endregion  //employee CRUD


        #region department CRUD

        public int CreateDepartment(Department newData){
            int count = 0;
            try{
                _DbContext.departments.Add(newData);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }

            if(count ==1){
                var dic = new Dictionary<string,string>{};
                var opLog = new OperateLog(){
                    operateID=newData.lastOperaAccID, active="新增", 
                    category="部門職位", createTime=definePara.dtNow()
                };
                toNameFn.AddUpDepartPosition_covertToDic(ref dic, newData);
                opLog.content = toNameFn.AddUpDepartPosition_covertToText(dic);
                saveOperateLog(opLog);    //紀錄操作紀錄
            }

            return count;
        }

        public int DelDepartment(int id, int operateID){
            int count = 0;
            var dic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=operateID, active="刪除", 
                category="部門職位", createTime=definePara.dtNow()
            };
            var context = _DbContext.departments.FirstOrDefault(b=>b.ID == id);
            var principalID = 0;
            if(context != null)
            {
                toNameFn.AddUpDepartPosition_covertToDic(ref dic, context);

                principalID = context.principalID;
                _DbContext.departments.Remove(context);
                count = _DbContext.SaveChanges();
                if(count == 1)
                {
                    opLog.content = toNameFn.AddUpDepartPosition_covertToText(dic);
                    saveOperateLog(opLog);    //紀錄操作紀錄

                    var emQuery = _DbContext.accounts.Where(b=>b.departmentID == id).ToList();
                    if(emQuery.Count >0)
                    {
                        foreach(var em in emQuery){
                            var emDepartPrincipal = _DbContext.employeeprincipals
                                                .Where(b=>b.employeeID==em.ID && b.principalID==principalID);
                            _DbContext.RemoveRange(emDepartPrincipal);      //刪除舊所屬主管

                            em.departmentID = 0;
                            if(_DbContext.SaveChanges() == 1){
                                var opLog02 = new OperateLog(){
                                    operateID=0, employeeID=em.ID, active="更新", 
                                    category="員工資料", createTime=definePara.dtNow()
                                };
                                opLog02.content = $"部門職位：{dic["depart"]}{dic["position"]}=>未指派";
                                saveOperateLog(opLog02);    //紀錄操作紀錄
                            }
                        }
                    }
                }
            }
            return count;
        }

        public int UpdateDepartment(Department updateDate){
            var oDic = new Dictionary<string,string>{};
            var nDic = new Dictionary<string,string>{};
            var opLog = new OperateLog(){
                operateID=updateDate.lastOperaAccID, active="更新", 
                category="部門職位", createTime=definePara.dtNow()
            };
            int count = 0;
            try{
                var context = _DbContext.departments.FirstOrDefault(b=>b.ID == updateDate.ID);
                if(context != null)
                {
                    toNameFn.AddUpDepartPosition_covertToDic(ref oDic, context);
                    var principalIsChange = context.principalID == updateDate.principalID? false : true;
                    var oldPrincipalID = context.principalID;

                    context.department = updateDate.department;
                    context.position = updateDate.position;
                    context.principalID = updateDate.principalID;
                    context.lastOperaAccID = updateDate.lastOperaAccID;
                    context.updateTime = updateDate.updateTime;
                    count = _DbContext.SaveChanges();
                    if(count == 1)
                    {
                        toNameFn.AddUpDepartPosition_covertToDic(ref nDic, context);
                        opLog.content = toNameFn.AddUpDepartPosition_covertToText(nDic, oDic);
                        saveOperateLog(opLog);    //紀錄操作紀錄

                        if(principalIsChange)   //更動該部門底下員工的負責人
                        {
                            var emQuery = _DbContext.accounts.Where(b=>b.departmentID==context.ID).ToList();  //取得底下員工
                            if(emQuery.Count >0){
                                foreach(var em in emQuery){
                                    oDic.Clear(); nDic.Clear();
                                    var emPrincipal = _DbContext.employeeprincipals.Where(b=>b.employeeID==em.ID);
                                    var thisManager = emPrincipal.Where(b=>b.principalID != oldPrincipalID)
                                                                    .Select(b=>b.principalID).ToArray();

                                    toNameFn.GetEmployeePrincipalName(emPrincipal.ToList(), ref oDic);

                                    _DbContext.RemoveRange(emPrincipal);      //刪除舊所屬主管
                                    _DbContext.SaveChanges();
                                    saveEmployeePrincipals(em, thisManager);

                                    var newEmPri = _DbContext.employeeprincipals.Where(b=>b.employeeID==em.ID);
                                    toNameFn.GetEmployeePrincipalName(newEmPri.ToList(), ref nDic);
                                    var opLog02 = new OperateLog(){
                                        operateID=0, employeeID=em.ID, active="更新", 
                                        category="員工資料", createTime=definePara.dtNow()
                                    };
                                    opLog02.content = $"負責人:{oDic["principal"]} => {nDic["principal"]}";
                                    saveOperateLog(opLog02);    //紀錄操作紀錄
                                }
                            }
                        }
                    }
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        public object GetThisDepartPosition(int loginID){
            object result = null;
            var query =  from a in _DbContext.departments
                         where (from b in _DbContext.accounts 
                                join c in _DbContext.departments on b.departmentID equals c.ID
                                where b.ID == loginID select c.department
                                ).Contains(a.department) select new{
                                    a.ID, a.department, a.position
                                };

            result = query.ToList();
            return result;
        }

        public object GetAllDepartPosition(){
            object result = null;
            var query = from a in _DbContext.departments
                        join b in _DbContext.accounts on a.principalID equals b.ID into tmp
                        from c in tmp.DefaultIfEmpty()
                        orderby a.department
                        select new {
                            a.ID, a.department, a.position, accID=a.principalID, 
                            userName=(c==null? null:c.userName), 
                        };
            result = query.ToList();
            return result;
        }

        public object GetAllPrincipal(){
            var query = from a in _DbContext.accounts
                        where a.accLV < definePara.getDIMALV()
                        orderby a.accLV descending
                        select new{
                            a.ID, a.userName, a.accLV
                        };
            return query.ToList();
        }

        public object GetAllPosition(string department){
            object result = null;
            var query = (from a in _DbContext.departments
                        where a.department == department
                        orderby a.principalID
                        select new {
                            a.ID, a.position, a.principalID
                        });
            result = query.ToList();
            return result;
        }

        #endregion  //department CRUD

        
  
        
    }
}