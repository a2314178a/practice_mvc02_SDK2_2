using System;
namespace practice_mvc02.Models
{
    public class groupRuleCode
    {
        public readonly int baseActive = 0x0001; //打卡/紀錄/請假/外出
        public readonly int departEmployeeList = 0x0004;  //查看部門員工 
        public readonly int allEmployeeList = 0x0008; //查看所有員工
        public readonly int employeeEdit = 0x0010;  //編輯員工
        public readonly int editPunchLog = 0x0002;
        public readonly int departmentList = 0x0020;  //部門職位相關 
        public readonly int setRule = 0x0040; //設定規則 
        public readonly int applySign = 0x0080;    //相關審核 
        public readonly int adminFn = 0x0100;   //管理功能
    
    
        public groupRuleCode(){
            /*this.baseActive = 0x0001; //打卡/紀錄/請假/外出
            this.editPunchLog = 0x0002;    //編輯出勤紀錄
            this.departEmployeeList = 0x0004;  //查看部門員工
            this.allEmployeeList = 0x0008; //查看所有員工
            this.employeeEdit = 0x0010;  //編輯員工
            this.departmentList = 0x0020;  //部門職位相關
            this.setRule = 0x0040; //設定規則
            this.applySign = 0x0080;    //相關審核*/
        }

        public int getTotalValue(){
            var val = baseActive | editPunchLog | departEmployeeList | allEmployeeList | employeeEdit |
                        departmentList | setRule | applySign;
            return val;
        }
    }
    
    public class punchStatusCode
    {
        public readonly int normal = 0x0001;   //正常
        public readonly int hadLost = 0x0010;  //缺卡
        public readonly int lateIn = 0x0002;   //遲到
        public readonly int earlyOut = 0x0004; //早退
        public readonly int overtime = 0x0008; //加班
        public readonly int noWork = 0x0020;   //曠職
        public readonly int takeLeave = 0x0040; //請假
    
        public punchStatusCode(){
            /*this.normal = 0x0001;   //正常
            this.lateIn = 0x0002;   //遲到
            this.earlyOut = 0x0004; //早退
            this.overtime = 0x0008; //加班
            this.hadLost = 0x0010;  //缺卡
            this.noWork = 0x0020;   //曠職
            this.takeLeave = 0x0040; //請假*/
        } 
    }

    



}