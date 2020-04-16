using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;

namespace practice_mvc02.Models
{
    public class CalAnnualLeave
    {
        private AnnualLeaveRepository Repository {get;}
        private const int normalDeadLineMonth = 12;

        public CalAnnualLeave(AnnualLeaveRepository repository){
            this.Repository = repository;
        }

        public void start(){
            startCalAnnualLeave();
        }

        private void startCalAnnualLeave(){
            var allEmployeeDetail = Repository.GetAllEmployeeDetail();
            var spDaysRule = Repository.GetAnnualLeaveRule();
            calSeniorityAndLeave(allEmployeeDetail, spDaysRule);
        }

        private void calSeniorityAndLeave(List<EmployeeDetail> allDetail, List<AnnualLeaveRule> spDaysRule){
            foreach(var detail in allDetail)
            {
                var dtNow = DateTime.Now;
                var dtStart = detail.startWorkDate; //報到日
                if(dtStart.Year==1 || dtStart > dtNow){
                    continue;
                }  
                dtNow = dtNow.Day>=dtStart.Day? dtNow : dtNow.AddMonths(-1);
                var year = (dtNow.Month >= dtStart.Month)? (dtNow.Year - dtStart.Year):(dtNow.Year - dtStart.Year -1);
                var month = (dtNow.Month >= dtStart.Month)? (dtNow.Month - dtStart.Month):(dtNow.Month - dtStart.Month +12);
                var totalMonth = 12*year + month;   //該員工總年資 unit:month
                var useIndex = -1;
                for(var i = 0; i<spDaysRule.Count; i++){    //list<spDaysRule> 有排序 從小到大
                    if(totalMonth >= (spDaysRule[i].seniority)*12){ //年資有到規則設定的年資
                        useIndex = i;
                    }
                }
                var data = new EmployeeAnnualLeave(){employeeID=detail.accountID};
                data.deadLine = dtStart; //特休期限
                if(useIndex >-1){
                    data.ruleID = spDaysRule[useIndex].ID;
                    data.specialDays = spDaysRule[useIndex].specialDays;
                    var buffMonth = (spDaysRule[useIndex].buffDays)/30;  //緩衝天數 days to month
                    var seniorityMon = (int)((spDaysRule[useIndex].seniority)*12); //特休規則年資 unit:month
                    var dlMonth = seniorityMon == 6? seniorityMon : normalDeadLineMonth;    //特休基本期限
                    var addYear = 0;
                    if((totalMonth - seniorityMon) >= dlMonth){ 
                        addYear = (totalMonth - seniorityMon)/12; //滿n年以上但只能用舊規則(無新規則)
                    }
                    var addMon = seniorityMon + dlMonth + buffMonth;    //年資+基本期限+緩衝天數
                    data.deadLine = data.deadLine.AddYears(addYear).AddMonths(addMon);    //從報到日開始算
                    data.remainHours = data.specialDays*8 - Repository.GetSpLeaveTotalHours(
                                        data.employeeID, data.deadLine.AddMonths(-(dlMonth+buffMonth)), data.deadLine);
                    Repository.RecordEmployeeSpDays(data);

                    if(useIndex >=1 || addYear >0){     //大部分員工有2條紀錄(今年的期限與明年的期限)
                        var data02 = new EmployeeAnnualLeave(){
                            employeeID=data.employeeID, ruleID=data.ruleID, 
                            specialDays=data.specialDays, deadLine=data.deadLine
                        };
                        if(addYear >0){ //需優先判斷
                            data02.deadLine = data02.deadLine.AddYears(-1); //使用同規則 直接-1年
                        }else{
                            data02.deadLine = data02.deadLine.AddYears(-1).AddMonths(-buffMonth);   //滿N年
                            data02.ruleID = spDaysRule[useIndex-1].ID;
                            data02.specialDays = spDaysRule[useIndex-1].specialDays;
                            buffMonth = (spDaysRule[useIndex-1].buffDays)/30;   //days to month
                            data02.deadLine = data02.deadLine.AddMonths(buffMonth);
                            dlMonth = spDaysRule[useIndex-1].seniority == 0.5? 6 : 12;   
                        }
                        data02.remainHours = data02.specialDays*8 - Repository.GetSpLeaveTotalHours(
                                        data02.employeeID, data02.deadLine.AddMonths(-(dlMonth+buffMonth)), data02.deadLine);
                        if(data02.deadLine >= DateTime.Now)
                            Repository.RecordEmployeeSpDays(data02);
                    }
                }
            }//foreach
        }

        /*
            var nextLength = 0; //該特休年資與下個特休年資的時間長度
            if(useIndex < spDaysRule.Count-1){
                nextLength = (int)(spDaysRule[useIndex+1].seniority - spDaysRule[useIndex].seniority)*12;  //unit:month
            }else{
                nextLength = normalDeadLineMonth;
            }
            nextLength = nextLength > normalDeadLineMonth? normalDeadLineMonth: nextLength;
        */

        

    }
}