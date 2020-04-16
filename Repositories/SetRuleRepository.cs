using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;

namespace practice_mvc02.Repositories
{
    public class SetRuleRepository : BaseRepository
    {
        private AnnualLeaveRepository AnnualLeave {get;}
        public SetRuleRepository(AnnualLeaveRepository repository, DBContext dbContext):base(dbContext)
        {
            this.AnnualLeave = repository;
        }

        #region timeRule CRUD
        //in baseRepository
        /*public object GetAllTimeRule(){
            object result = null;
            var query = (from a in _DbContext.worktimerules
                        orderby a.startTime
                        select new {
                            a.ID, a.name, a.startTime, a.endTime, a.restTime
                        });
            result = query.ToList();
            return result;
        }*/

        public int AddTimeRule(WorkTimeRule newRule){
            int count = 0;
            try{
                _DbContext.worktimerules.Add(newRule);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        public int DelTimeRule(int id){
            int count = 0;
            var context = _DbContext.worktimerules.FirstOrDefault(b=>b.ID == id);
            if(context != null){
                _DbContext.worktimerules.Remove(context);
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        public int UpdateTimeRule(WorkTimeRule updateData){
            int count = 0;
            try{
                var context = _DbContext.worktimerules.FirstOrDefault(b=>b.ID == updateData.ID);
                if(context != null){
                    context.name = updateData.name;
                    context.startTime = updateData.startTime;
                    context.endTime = updateData.endTime;
                    context.sRestTime = updateData.sRestTime;
                    context.eRestTime = updateData.eRestTime;
                    context.lastOperaAccID = updateData.lastOperaAccID;
                    context.updateTime = updateData.updateTime;
                    count = _DbContext.SaveChanges();
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion  //timeRule

        //-----------------------------------------------------------------------------------------------------------

        #region Group
        //in baseRepository
        /*public object GetAllGroup(){
            object result = null;
            var query = from a in _DbContext.grouprules
                        select new {
                            a.ID, a.groupName, a.ruleParameter
                        };
            result = query.ToList();
            return result;
        }*/

        public int AddGroup(GroupRule newGroup){
            int count = 0;
            try{
                _DbContext.grouprules.Add(newGroup);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        public int DelGroup(int id){
            int count = 0;
            var context = _DbContext.grouprules.FirstOrDefault(b=>b.ID == id);
            if(context != null){
                _DbContext.grouprules.Remove(context);
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        public int UpdateGroup(GroupRule updateGroup){
            int count = 0;
            try{
                var context = _DbContext.grouprules.FirstOrDefault(b=>b.ID == updateGroup.ID);
                if(context != null){
                    context.groupName = updateGroup.groupName;
                    context.ruleParameter = updateGroup.ruleParameter;
                    context.lastOperaAccID = updateGroup.lastOperaAccID;
                    context.updateTime = updateGroup.updateTime;
                    count = _DbContext.SaveChanges();
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion  //group
        
        //-----------------------------------------------------------------------------------------------------------

        #region sp date

        public object GetAllSpecialDate(){
            var query = _DbContext.specialdate.Select(b=> new{
                b.ID, b.departClass, b.date, b.status, b.note
            });
            return query.ToList();
        }

        public object GetClassDepart(){
            var query = ((from a in _DbContext.departments
                            select a.department)).Union(
                        (from b in _DbContext.worktimerules
                            select b.name));
            return query.ToList();
        }

        public int AddSpecialTime(SpecialDate newData){
            int count = 0;
            try{
                _DbContext.specialdate.Add(newData);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        public int DelSpecialDate(int spDateID){
            int count = 0;
            var context = _DbContext.specialdate.FirstOrDefault(b=>b.ID == spDateID);
            if(context != null){
                _DbContext.specialdate.Remove(context);
                count = _DbContext.SaveChanges();
            }
            return count;
        }

        public int UpdateSpecialTime(SpecialDate updateData){
            int count = 0;
            try{
                var context = _DbContext.specialdate.FirstOrDefault(b=>b.ID == updateData.ID);
                if(context != null){
                    context.date = updateData.date;
                    context.departClass = updateData.departClass;
                    context.status = updateData.status;
                    context.note = updateData.note;
                    context.lastOperaAccID = updateData.lastOperaAccID;
                    context.updateTime = updateData.updateTime;
                    count = _DbContext.SaveChanges();
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion  //sp date

        //-----------------------------------------------------------------------------------------------------

        #region leaveRule

        public object GetAllLeaveRule(){
            var query = _DbContext.leavenames.Select(b=>new{
                b.ID, b.leaveName, b.timeUnit
            });
            return query.ToList();
        }

        public int AddLeave(LeaveName data){
            int count = 0;
            try{
                _DbContext.leavenames.Add(data);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        public int DelLeave(int leaveID){
            var context = _DbContext.leavenames.FirstOrDefault(b=>b.ID == leaveID);
            if(context != null){
                _DbContext.leavenames.Remove(context);
            }
            return _DbContext.SaveChanges() >0 ? 1: 0;
        }

        public int UpdateLeave(LeaveName data){
            int count = 0;
            try{
                var context = _DbContext.leavenames.FirstOrDefault(b=>b.ID == data.ID);
                if(context != null){
                    context.leaveName = data.leaveName;
                    context.timeUnit = data.timeUnit;
                    context.lastOperaAccID = data.lastOperaAccID;
                    context.updateTime = data.updateTime;
                    count = _DbContext.SaveChanges();
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            return count;
        }

        #endregion //leaveRule

        //--------------------------------------------------------------------------------------------------

        #region spLeaveRule

        public object GetAllSpLeaveRule(){
            var query = _DbContext.annualleaverule.Select(
                b=>new{b.ID, b.seniority, b.specialDays, b.buffDays}
            ).OrderBy(b=>b.seniority);
            return query.ToList();
        }

        public int AddSpLeaveRule(AnnualLeaveRule data){
            int count = 0;
            try{
                _DbContext.annualleaverule.Add(data);
                count = _DbContext.SaveChanges();
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(data.ID >0){
                var targetID = AnnualLeave.FindLowOneThanThisRule(data.ID);
                AnnualLeave.DelSomeAnnualLeaveRecord(new int[]{targetID});
                AnnualLeave.StartCalAnnualLeave();
            }
            return count;
        }

        public int UpdateSpLeaveRule(AnnualLeaveRule data){
            int count = 0;
            float diffSeniority=0;
            int diffSpecialDays=0, diffBuffDays=0;
            try{
                var context = _DbContext.annualleaverule.FirstOrDefault(b=>b.ID == data.ID);
                if(context != null){
                    diffSeniority =  data.seniority - context.seniority;
                    diffSpecialDays = data.specialDays - context.specialDays;
                    diffBuffDays = data.buffDays - context.buffDays;

                    context.seniority = data.seniority;
                    context.specialDays = data.specialDays;
                    context.buffDays = data.buffDays;
                    context.lastOperaAccID = data.lastOperaAccID;
                    context.updateTime = data.updateTime;
                    count = _DbContext.SaveChanges();
                }
            }catch(Exception e){
                count = ((MySqlException)e.InnerException).Number;
            }
            if(count == 1){
                if(diffSeniority !=0){
                    var anotherID = AnnualLeave.FindLowOneThanThisRule(data.ID);
                    AnnualLeave.DelSomeAnnualLeaveRecord(new int[]{data.ID, anotherID});
                    AnnualLeave.StartCalAnnualLeave();
                }else{
                    AnnualLeave.UpEmployeeSpLeave(data.ID, diffSpecialDays, diffBuffDays);
                } 
            }
//新增:找使用比她小1規則的年資移除重計算, 刪除:移除後重計算, 
//修改:修改對應資料,若是修改年資, 找使用它與比它小1規則的年資移除重計算
            return count;
        }

        public int DelSpLeaveRule(int ruleID){
            int count = 0;
            var context = _DbContext.annualleaverule.FirstOrDefault(b=>b.ID == ruleID);
            if(context != null){
                _DbContext.annualleaverule.Remove(context);
                count = _DbContext.SaveChanges();
            }
            if(count == 1){
                AnnualLeave.DelSomeAnnualLeaveRecord(new int[]{ruleID});
                AnnualLeave.StartCalAnnualLeave();
            }
            return count;
        }

        #endregion  //spLeaveRule

        //--------------------------------------------------------------------------------------------------
    }
}