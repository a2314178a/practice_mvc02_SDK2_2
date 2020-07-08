using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using practice_mvc02.Models;
using practice_mvc02.Models.dataTable;

namespace practice_mvc02.Repositories
{
    public class ExportXlsxRepository : BaseRepository
    {
        public ExportXlsxRepository(DBContext dbContext):base(dbContext)
        {
            
        }

        public List<PunchCardLog> GetLogs(){
            var query = _DbContext.punchcardlogs.ToList();
            return query;
        }

        public List<LeaveName> GetLeaveName(){
            var query = _DbContext.leavenames;
            return query.ToList();
        }
        
        public List<exportXlsxData> GetNormalDetail_Month(exportPunchLogXlsxPara qPara){
            var query = new List<exportXlsxData>();

            if(!qPara.crossDepart)
            {
                var baseQu = (from a in _DbContext.accounts
                            join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                            join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                            where b.principalID == qPara.loginID || b.principalAgentID == qPara.loginID
                            select new {a, b, c}).ToList();
                if(qPara.departName == "未指派"){
                    query = baseQu.Where(d=>d.a.departmentID==0)
                                    .Select(d=>new exportXlsxData{
                                        accID=d.a.ID, name=d.a.userName, 
                                        workClass=(d.c.name+ (d.c.type==1? "(排休制)":"(固定制)")),
                                        department="未指派", position=""
                                    }).ToList();
                }else{  //XX部門  所有部門
                    var myData = (from a in _DbContext.accounts
                                join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                                from bb in noDepart.DefaultIfEmpty()
                                join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                                where a.ID == qPara.loginID
                                select new {
                                    acc=a, depart=bb, wt=c
                                }).ToList();

                    if(qPara.departName != "所有部門"){
                        var tmp = (from d in baseQu
                            join e in _DbContext.departments on d.a.departmentID equals e.ID
                            where (e.department.Contains(qPara.departName))
                            select new{acc=d.a, depart=e, wt=d.c}).ToList();
                        myData = myData.Where(k=>k.depart.department.Contains(qPara.departName)).Union(tmp).ToList();
                    }else{
                        var tmp = (from d in baseQu
                                join e in _DbContext.departments on d.a.departmentID equals e.ID into depart
                                from f in depart.DefaultIfEmpty()
                                select new {acc=d.a, depart=f, wt=d.c}).ToList();
                        myData = myData.Union(tmp).ToList();                
                    }
                    query = myData.Select(k=>new exportXlsxData{
                                accID=k.acc.ID, name=k.acc.userName, 
                                workClass=(k.wt.name+ (k.wt.type==1? "(排休制)":"(固定制)")),
                                department=(k.depart==null? "未指派":k.depart.department), 
                                position=(k.depart==null? "":k.depart.position)
                    }).ToList(); 
                }
            }
            else    //所有部門
            {
                if(qPara.departName == "未指派"){
                    query = (from a in _DbContext.accounts
                            join b in _DbContext.worktimerules on a.timeRuleID equals b.ID
                            where a.departmentID == 0 && a.accLV != definePara.getDIMALV()
                            select new exportXlsxData{
                                accID=a.ID, name=a.userName, 
                                workClass=(b.name + (b.type==1? "(排休制)":"(固定制)")), 
                                department="未指派", position=""
                            }).ToList();
                }else{
                    var tmp = (from a in _DbContext.accounts
                            join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                            from bb in noDepart.DefaultIfEmpty()
                            join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                            select new { acc=a, depart=bb, wt=c}).ToList();
                    if(qPara.departName != "所有部門"){
                        tmp = tmp.Where(k=>k.depart != null && k.depart.department.Contains(qPara.departName)).ToList();
                    }
                    query = tmp.Select(k=>new exportXlsxData{
                        accID=k.acc.ID, name=k.acc.userName, 
                        workClass=(k.wt.name + (k.wt.type==1? "(排休制)":"(固定制)")), 
                        department=(k.depart==null? "未指派":k.depart.department),
                        position=(k.depart==null? "":k.depart.position)
                    }).ToList();
                }
            }
            return query;
        }

        public exportXlsxData GetNormalDetail_Day(exportPunchLogXlsxPara qPara){
            var query = from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID into noDepart
                        from bb in noDepart.DefaultIfEmpty()
                        join c in _DbContext.worktimerules on a.timeRuleID equals c.ID
                        where a.ID == qPara.accID
                        select new exportXlsxData{
                            accID=a.ID, name=a.userName, 
                            workClass=(c.name + (c.type==1? "(排休制)":"(固定制)")),
                            startTime=c.startTime, endTime=c.endTime, 
                            department=(bb==null? "未指派":bb.department),
                            position=(bb==null? "":bb.position)
                        };
            return query.FirstOrDefault();
        }

        public List<PunchCardLog> GetRequestPunchLog(exportPunchLogXlsxPara qPara, int id){
            var query = _DbContext.punchcardlogs.Where(
                                b=>b.accountID == id &&
                                b.logDate >= qPara.sDate && b.logDate <= qPara.eDate)
                                .OrderBy(b=>b.logDate);
            return query.ToList();
        }

        public WorkTimeRule GetThisWorkTimeRule(int accID){
            var query = (from a in _DbContext.accounts
                        join b in _DbContext.worktimerules on a.timeRuleID equals b.ID
                        where a.ID == accID 
                        select b).FirstOrDefault();
            return query;
        }

        public List<LeaveOfficeApply> GetApplyLeaveLogs(exportPunchLogXlsxPara qPara, int id){
            var query = _DbContext.leaveofficeapplys.Where(a=>
                            a.accountID == id && a.applyStatus == 1 &&
                            ((a.startTime >= qPara.sDate && a.startTime < qPara.eDate) || 
                             (a.endTime > qPara.sDate && a.endTime <= qPara.eDate)
                            )
                        );
            return query.ToList();
        }

        public object GetDepartment(int loginID, bool cross){
            object result = new List<string>();
            if(!cross){
                var query = (from a in _DbContext.accounts
                        join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                        join c in _DbContext.departments on a.departmentID equals c.ID into noDepart
                        from d in noDepart.DefaultIfEmpty()
                        where b.principalID == loginID || b.principalAgentID == loginID
                        select new{
                            department=(d==null? "未指派" : d.department)
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
                result = _DbContext.departments.Select(b=>new {department=b.department}).Distinct().ToList();
            }
            return result;
        }

        public object GetDepartmentEmployee(string depart, int loginID, bool cross){
            
            if(!cross)
            {
                if(depart == "未指派"){
                    var noDepart = (from a in _DbContext.accounts
                                join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                                where (b.principalID == loginID || b.principalAgentID == loginID) 
                                        && a.departmentID == 0 && a.accLV != definePara.getDIMALV()
                                select new{ a.ID, a.userName }).ToList();
                    return noDepart;
                }
                var query = (from a in _DbContext.accounts
                            join b in _DbContext.employeeprincipals on a.ID equals b.employeeID
                            join c in _DbContext.departments on a.departmentID equals c.ID
                            where c.department == depart && 
                                (b.principalID == loginID || b.principalAgentID == loginID)
                            select new {
                                a.ID, a.userName
                            }).Union(
                                from a in _DbContext.accounts
                                join b in _DbContext.departments on a.departmentID equals b.ID
                                where a.ID == loginID && b.department == depart
                                select new {
                                    a.ID, a.userName
                                }
                            );
                return query.ToList();
            }
            else
            {
                if(depart == "未指派"){
                    var noDepart = _DbContext.accounts.Where(b=>b.departmentID == 0 && b.accLV != definePara.getDIMALV())
                                                        .Select(b=> new{b.ID, b.userName}).ToList();
                    return noDepart;
                }
                var query = from a in _DbContext.accounts
                        join b in _DbContext.departments on a.departmentID equals b.ID
                        where b.department == depart
                        select new{
                            a.ID, a.userName
                        };
                return query.ToList();
            }
        }
    }
}