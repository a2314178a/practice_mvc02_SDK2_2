using System;
using Microsoft.EntityFrameworkCore;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;
using System.Security.Cryptography;
using System.Text;

namespace practice_mvc02.Repositories
{
    public class DBContext : DbContext
    {
        public DBContext (DbContextOptions options) : base(options){}
        public DbSet<Account> accounts {get; set;}
        public DbSet<GroupRule> grouprules {get; set;}
        public DbSet<Department> departments {get; set;}
        public DbSet<PunchCardLog> punchcardlogs {get; set;}
        public DbSet<WorkTimeRule> worktimerules {get; set;}
        public DbSet<PunchLogWarn> punchlogwarns {get; set;}
        public DbSet<LeaveOfficeApply> leaveofficeapplys {get; set;}
        public DbSet<SpecialDate> specialdate {get; set;}
        public DbSet<EmployeeDetail> employeedetails {get; set;}
        public DbSet<Message> message {get; set;}
        public DbSet<MsgSendReceive> msgsendreceive {get; set;}
        public DbSet<workTimeTotal> worktimetotals {get; set;}
        public DbSet<EmployeePrincipal> employeeprincipals {get; set;}
        public DbSet<LeaveName> leavenames {get; set;}
        public DbSet<AnnualLeaveRule> annualleaverule {get; set;}
        public DbSet<EmployeeAnnualLeave> employeeannualleaves{get; set;}
        public DbSet<AnnualDaysOffset> annualdaysoffset{get; set;}
		public DbSet<OperateLog> operateLogs{get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            modelBuilder.Entity<Account>(entity=>{
                entity.HasIndex(b=>b.account).IsUnique();
                //entity.Property(b=>b.accLV).HasComment("1職員2單位主管3部門主管4處長廠長5協理6總經理7董事");
            });

            //modelBuilder.Entity<AnnualDaysOffset>().Property(b=>b.value).HasComment("unit:hour");

            modelBuilder.Entity<AnnualLeaveRule>(entity=>{
                entity.HasIndex(b=>b.seniority).IsUnique();
                //entity.Property(b=>b.seniority).HasComment("unit:year");
            });

            modelBuilder.Entity<Department>(entity =>{
                entity.Property(b => b.department).HasColumnType("varchar(255)");
                entity.Property(b => b.position).HasColumnType("varchar(255)");
                entity.HasIndex(b => new{b.department, b.position, b.principalID}).IsUnique();
            });

            modelBuilder.Entity<EmployeeDetail>(entity=>{
                entity.HasIndex(b=>b.accountID).IsUnique();
                entity.Property(b=>b.sex).HasColumnType("int(1)");
                entity.Property(b=>b.agentEnable).HasColumnType("tinyint(1)");
            });

            modelBuilder.Entity<EmployeePrincipal>(entity=>{
                //entity.Property(b=>b.employeeID).HasComment("員工ID");
                //entity.Property(b=>b.principalID).HasComment("主管ID");
                //entity.Property(b=>b.principalAgentID).HasComment("主管代理人ID");
            });

            modelBuilder.Entity<LeaveName>(entity=>{
                entity.Property(b=>b.enable).HasColumnType("tinyint(1)");
				entity.Property(b=>b.halfVal).HasColumnType("tinyint(1)");
                //entity.Property(b=>b.timeUnit).HasComment("1:全天 2:半天 3:小時");
            });

            modelBuilder.Entity<LeaveOfficeApply>(entity=>{
                //entity.Property(b=>b.applyStatus).HasComment("0:申請中 1:通過 2:沒通過");
                //entity.Property(b=>b.unit).HasComment("1:全天 2:半天 3:小時");
            });

            modelBuilder.Entity<MsgSendReceive>(entity=>{
                entity.Property(b => b.read).HasColumnType("int(1)");
                entity.Property(b => b.sDelete).HasColumnType("tinyint(1)");
                entity.Property(b => b.rDelete).HasColumnType("tinyint(1)");
                //entity.Property(b=>b.read).HasComment("0:未讀 1:已讀");
            });

            modelBuilder.Entity<PunchCardLog>(entity=>{
                entity.HasIndex(b=>new{b.accountID, b.logDate}).IsUnique();
                //entity.Property(b=>b.punchStatus).HasComment("0x01:正常 0x02:遲到 0x04:早退 0x08:加班 0x10:缺卡 0x20:曠職 //0x40:請假");
            });

            modelBuilder.Entity<PunchLogWarn>(entity=>{
                entity.HasIndex(b=>b.punchLogID).IsUnique();
                //entity.Property(b=>b.warnStatus).HasComment("0:未處理 1:已處理 2:忽略");
            });
            
            modelBuilder.Entity<SpecialDate>(entity=>{
                entity.Property(b=>b.departClass).HasColumnType("varchar(255)");
                //entity.Property(b=>b.status).HasComment("1:休假 2:上班");
            });

            modelBuilder.Entity<WorkTimeRule>(entity=>{
                entity.Property(b=>b.name).HasColumnType("varchar(255)");
				entity.Property(b=>b.type).HasColumnType("int(1)");
                //entity.HasIndex(b=> new{b.startTime, b.endTime}).IsUnique();
            });    

            modelBuilder.Entity<workTimeTotal>(entity=>{
                entity.HasIndex(b=>new{b.accountID, b.dateMonth}).IsUnique();
                //entity.Property(b=>b.totalTime).HasComment("unit : minute");
            }); 
            
            

            createDefaultData(modelBuilder); 
        }

        private void createDefaultData(ModelBuilder modelBuilder){
            string defUid = "DIMA", defPw = "DIMA";
            var md5password = GetMD5(defUid + defPw);
            modelBuilder.Entity<Account>().HasData(new Account{
                ID = 1, account = defUid, password = md5password, userName = "DIMA_Admin",
                accLV=definePara.getDIMALV(), departmentID = 0, groupID = 0, timeRuleID = 0, createTime = definePara.dtNow(), updateTime = definePara.dtNow()
            });
                        
        }

        public string GetMD5(string original) 
        { 
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider(); 
            byte[] b = md5.ComputeHash(Encoding.UTF8.GetBytes(original)); 
            return BitConverter.ToString(b).Replace("-", string.Empty); 
        }
    }
}