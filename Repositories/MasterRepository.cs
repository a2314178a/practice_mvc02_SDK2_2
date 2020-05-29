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
            if(crossDepart){
                var query = from a in _DbContext.accounts
                            join b in _DbContext.departments on a.departmentID equals b.ID
                            where a.userName.Contains(fName) && b.department.Contains(fDepart) && 
                                  b.position.Contains(fPosition) && a.accLV <= loginAccLV 
                            orderby b.department
                            select new {
                                a.ID, a.account, a.userName,
                                b.department, b.position,  
                            };
                result = query.ToList();
            }else{
                var query = from a in _DbContext.accounts
                            join b in _DbContext.departments on a.departmentID equals b.ID
                            join c in _DbContext.employeeprincipals on a.ID equals c.employeeID
                            where c.principalID == loginID && a.userName.Contains(fName) && 
                                  b.department.Contains(fDepart) && b.position.Contains(fPosition)                                  
                            orderby b.department
                            select new {
                                a.ID, a.account, a.userName,
                                b.department, b.position,  
                            };
                result = query.ToList();
            }

            /*if(!crossDepart){
                var query01 = from a in _DbContext.departments
                            join b in _DbContext.accounts on a.ID equals b.departmentID
                            where b.ID == loginID select a.department;
                departName = query01.Count()>0? query01.ToList()[0] : "有問題但不能回傳跨部門結果";
            }
            var query = from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        join c in _DbContext.worktimerules on a.timeRuleID equals c.ID into tmp
                        from d in tmp.DefaultIfEmpty()
                        where a.userName.Contains(fName) &&  b.department.Contains(departName) && 
                                b.position.Contains(fPosition) && a.accLV <= loginAccLV 
                        orderby b.department
                        select new {
                            a.ID, a.account, a.userName,
                            b.department, b.position,  
                            //d.startTime, d.endTime
                        };
            result = query.ToList();*/
            
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
                }
                var depart = _DbContext.departments.FirstOrDefault(b=>b.ID == newEmployee.departmentID);
                saveEmployeePrincipals(newEmployee, depart, thisManager);   //新增所屬主管
            }
            return count;
        }

        public int DelEmployee(int employeeID){
            int count = 0;
            var context = _DbContext.accounts.FirstOrDefault(b=>b.ID == employeeID);
            if(context != null){
                _DbContext.Remove(context);
                count = _DbContext.SaveChanges();
                if(count == 1){
                    var detail = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == employeeID);
                    if(detail != null){
                        _DbContext.Remove(detail);
                        _DbContext.SaveChanges();
                    }
                }
            }
            return count;
        }

        public int UpdateEmployee(Account updateData, EmployeeDetail upDetail, int[] thisManager){
            int count = 0;
            var oldDic = new Dictionary<string,string>{};
            var newDic = new Dictionary<string,string>{};
            var opLog = definePara.dtNow().ToString("yyyy-MM-dd HH:mm:ss");
            var compareDic = new Dictionary<string, bool>{};
            var context = _DbContext.accounts.FirstOrDefault(b=>b.ID == updateData.ID);
            var context2 = _DbContext.employeedetails.FirstOrDefault(b=>b.accountID == updateData.ID);
            //var oldContext = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(context));
            //var oldContext2 = JsonConvert.DeserializeObject<EmployeeDetail>(JsonConvert.SerializeObject(context2));

            //employee base data
            if(context != null){
                // oldDic = toNameFn.UpdateEmployee_IDtoName(context, context2);
                // oldDic.Add("accLV", context.accLV.ToString());

                // compareDic.Add("userName", context.userName != updateData.userName? true: false);
                // compareDic.Add("depart", context.departmentID != updateData.departmentID? true: false);
                // compareDic.Add("accLV", context.accLV != updateData.accLV? true: false);
                // compareDic.Add("timeRuleID", context.timeRuleID != updateData.timeRuleID? true: false);
                // compareDic.Add("groupID", context.groupID != updateData.groupID? true: false);
                
                if(updateData.password != null){
                    //compareDic.Add("password", context.password != updateData.password? true: false);
                    //oldDic.Add("password", context.password);
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
                if(count ==1){
                    var query = _DbContext.employeeprincipals.Where(b=>b.employeeID==updateData.ID);
                    _DbContext.RemoveRange(query);      //刪除舊所屬主管
                    _DbContext.SaveChanges();
                    var depart = _DbContext.departments.FirstOrDefault(b=>b.ID==updateData.departmentID);
                    saveEmployeePrincipals(updateData, depart, thisManager);       //新增所屬主管
                }
            }
            //employeeDetail
            if(context2 != null){
                // oldDic.Add("sex", context2.sex==0?"女":"男");
                // oldDic.Add("birth", context2.birthday.ToString("yyyy-MM-dd"));
                // oldDic.Add("humanID", context2.humanID);
                // oldDic.Add("agentEnable", context2.agentEnable?"啟用":"停用");
                // oldDic.Add("startWorkDate", context2.startWorkDate.ToString("yyyy-MM-dd"));

                // compareDic.Add("sex", context2.sex != upDetail.sex? true: false);
                // compareDic.Add("birth", context2.birthday != upDetail.birthday? true: false);
                // compareDic.Add("humanID", context2.humanID != upDetail.humanID? true: false);
                // compareDic.Add("myAgentID", context2.myAgentID != upDetail.myAgentID? true: false);
                // compareDic.Add("agentEnable", context2.agentEnable != upDetail.agentEnable? true: false);
                // compareDic.Add("startWorkDate", context2.startWorkDate != upDetail.startWorkDate? true: false);

                var diffStartDate = context2.startWorkDate != upDetail.startWorkDate? true : false;

                context2.sex = upDetail.sex;
                context2.birthday = upDetail.birthday;
                context2.humanID = upDetail.humanID;
                context2.myAgentID = upDetail.myAgentID;
                context2.agentEnable = upDetail.agentEnable;
                context2.startWorkDate = upDetail.startWorkDate;
                context2.lastOperaAccID = upDetail.lastOperaAccID;
                context2.updateTime = upDetail.updateTime;
                _DbContext.SaveChanges();   //更新細項

                // newDic = toNameFn.UpdateEmployee_IDtoName(updateData, upDetail);
                // newDic.Add("accLV", context.accLV.ToString());
                // newDic.Add("password", context.password);
                // newDic.Add("sex", context2.sex==0?"女":"男");
                // newDic.Add("birth", context2.birthday.ToString("yyyy-MM-dd"));
                // newDic.Add("humanID", context2.humanID);
                // newDic.Add("agentEnable", context2.agentEnable?"啟用":"停用");
                // newDic.Add("startWorkDate", context2.startWorkDate.ToString("yyyy-MM-dd"));
    

                // opLog += $" {newDic["operateName"]}對{context.userName}個人資料進行編輯「";
                // foreach(var tmp in compareDic){
                //     if(tmp.Value == true){
                //         switch(tmp.Key){
                //             case "userName": opLog+= $"姓名: {oldDic["userName"]} => {newDic["userName"]}，";break;
                //             case "password": opLog+= $"更改密碼，";break;
                //             case "depart": 
                //             opLog+= $"部門職位: {oldDic["departName"]}{oldDic["position"]} => {newDic["departName"]}{newDic["position"]}，";
                //                 break;
                //             case "accLV": opLog+= $"帳號等級: {oldDic["accLV"]} => {newDic["accLV"]}，";break;
                //             case "timeRuleID": opLog+= $"工作班別: {oldDic["timeRuleName"]} => {newDic["timeRuleName"]}，";break;
                //             case "groupID": opLog+= $"動作權限: {oldDic["groupName"]} => {newDic["groupName"]}，";break;
                //             case "sex": opLog+= $"性別: {oldDic["sex"]} => {newDic["sex"]}，";break;
                //             case "birth": opLog+= $"生日: {oldDic["birth"]} => {newDic["birth"]}，";break;
                //             case "humanID": opLog+= $"身分證號: {oldDic["humanID"]} => {newDic["humanID"]}，";break;
                //             case "myAgentID": opLog+= $"職務代理人: {oldDic["myAgentName"]} => {newDic["myAgentName"]}，";break;
                //             case "agentEnable": opLog+= $"代理人授權: {oldDic["agentEnable"]} => {newDic["agentEnable"]}，";break;
                //             case "startWorkDate": opLog+= $"報到日: {oldDic["startWorkDate"]} => {newDic["startWorkDate"]}，";break;
                //         }
                //     }
                // }
                // opLog+="」";
                // Console.WriteLine(opLog);

                

                setPrincipalAgent(context2.accountID, context2.myAgentID, context2.agentEnable);
                if(diffStartDate){
                    upDetail.accountID = updateData.ID;
                    calAnnualDays.calThisEmployeeAnnualDays(upDetail, true); //計算此人的年假
                }
            }
            return count;
        }

        public void saveEmployeePrincipals(Account account, Department depart, int[] thisManager){
            var departAgent = (from a in _DbContext.employeedetails
                            join b in _DbContext.departments on a.accountID equals b.principalID
                            where b.ID == depart.ID && a.agentEnable == true
                            select a.myAgentID).FirstOrDefault();

            var downWithUp = new EmployeePrincipal(){
                employeeID = account.ID,    principalID = depart.principalID,
                principalAgentID = departAgent,   
                lastOperaAccID = account.lastOperaAccID,    createTime = definePara.dtNow()
            };
            _DbContext.employeeprincipals.Add(downWithUp);  //所屬部門主管一定會新增
            _DbContext.SaveChanges();
            foreach(var id in thisManager){
                if(id >0 && id != depart.principalID){
                    var thisAgent = _DbContext.employeedetails.Where(b=>b.accountID == id && b.agentEnable == true)
                                                            .Select(b=>b.myAgentID).FirstOrDefault();
                    downWithUp.ID = 0;
                    downWithUp.principalID = id;
                    downWithUp.principalAgentID = thisAgent;
                    _DbContext.employeeprincipals.Add(downWithUp);
                    _DbContext.SaveChanges();
                }
            }
        }
        
        public Account GetEmployeeAccByID(int ID){
            Account context = _DbContext.accounts.FirstOrDefault(b=>b.ID == ID);
            return context;
        }

        public object GetDepartOption(){
            var query = _DbContext.departments.Select(b=>b.department).Distinct();
            return query.ToList();
        }

        public object GetPositionOption(){
            var query = _DbContext.departments.Select(b=>b.position).Distinct();
            return query.ToList();
        }
        
        public object GetThisAllManager(int employeeID){
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
            return count;
        }

        public int DelDepartment(int id){
            int count = 0;
            var context = _DbContext.departments.FirstOrDefault(b=>b.ID == id);
            if(context != null){
                _DbContext.departments.Remove(context);
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        public int UpdateDepartment(Department updateDate){
            int count = 0;
            try{
                var context = _DbContext.departments.FirstOrDefault(b=>b.ID == updateDate.ID);
                context.department = updateDate.department;
                context.position = updateDate.position;
                context.principalID = updateDate.principalID;
                context.lastOperaAccID = updateDate.lastOperaAccID;
                context.updateTime = updateDate.updateTime;
                count = _DbContext.SaveChanges();
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
                            a.ID, a.department, a.position, accID=a.principalID, c.userName, 
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