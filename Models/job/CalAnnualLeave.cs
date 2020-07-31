using System;
using System.Collections.Generic;
using System.Linq;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Repositories;
using practice_mvc02.Models;

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
            calSeniorityAndLeave(allEmployeeDetail);
        }

        private void calSeniorityAndLeave(List<EmployeeDetail> emDetail){
            var spDaysRule = Repository.GetAnnualLeaveRule();
            foreach(var detail in emDetail)
            {
                var dtNow = definePara.dtNow();
                var dtStart = detail.startWorkDate; //報到日
                if(dtStart.Year==1 || dtStart > dtNow){
                    continue;
                }                   
                var totalMonth = Repository.calThisEmployeeSeniority(detail);   //該員工總年資 unit:month
                var useIndex = -1;
                for(var i = 0; i<spDaysRule.Count; i++){    //list<spDaysRule> 有排序 從小到大
                    if(totalMonth >= (spDaysRule[i].seniority)*12){ //年資有到規則設定的年資
                        useIndex = i;
                    }
                }
                var data = new EmployeeAnnualLeave(){employeeID=detail.accountID};
                data.deadLine = dtStart; //特休期限
                if(useIndex == -1){ //沒有符合其中一項特休
                    continue;
                }
                var dayToHour = definePara.dayToHour();
                data.ruleID = spDaysRule[useIndex].ID;
                data.specialDays = spDaysRule[useIndex].specialDays;
                var buffMonth = (spDaysRule[useIndex].buffDays)/30;  //緩衝天數 days to month
                var seniorityMon = (int)((spDaysRule[useIndex].seniority)*12); //特休規則年資 unit:month
                var dlMonth = seniorityMon == 6? seniorityMon : normalDeadLineMonth;    //特休基本期限
                var addYear = 0;
                if((totalMonth - seniorityMon) >= dlMonth){ 
                    addYear = (totalMonth - seniorityMon)/12; //滿n年以上但只能用舊規則(無新規則)
                }
                var addMon = seniorityMon + dlMonth + buffMonth;    //規則年資+基本期限+緩衝天數
                data.deadLine = data.deadLine.AddYears(addYear).AddMonths(addMon);    //從報到日開始算
                data.remainHours = data.specialDays*dayToHour;
                
                Repository.RecordEmployeeSpDays(data);

                if(useIndex >=1 || addYear >0){  //大部分員工有2條紀錄(今年的期限與明年的期限)
                    var data02 = new EmployeeAnnualLeave(){
                        employeeID=data.employeeID, ruleID=data.ruleID, 
                        specialDays=data.specialDays, deadLine=data.deadLine
                    };

                    if(addYear >0){ //需優先判斷 不然若useIndex=0 下面的[useIndex-1]會報錯
                        data02.deadLine = data02.deadLine.AddYears(-1); //使用同規則 直接-1年
                    }else{
                        data02.deadLine = data02.deadLine.AddYears(-1).AddMonths(-buffMonth);   //滿N年
                        data02.ruleID = spDaysRule[useIndex-1].ID;
                        data02.specialDays = spDaysRule[useIndex-1].specialDays;
                        buffMonth = (spDaysRule[useIndex-1].buffDays)/30;   //days to month
                        data02.deadLine = data02.deadLine.AddMonths(buffMonth);   
                    }
                    data02.remainHours = data02.specialDays*dayToHour;
                    
                    Repository.RecordEmployeeSpDays(data02);
                }
            }//foreach
        }
        
        public void calThisEmployeeAnnualDays(EmployeeDetail thisDetail, bool clearOld = false){
            var detail = new List<EmployeeDetail>{thisDetail};
            if(clearOld){
                Repository.DelAnnualDaysByEmployeeID(thisDetail.accountID);
            }
            calSeniorityAndLeave(detail);
        }

    }
}