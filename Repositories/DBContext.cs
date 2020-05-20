using System;
using Microsoft.EntityFrameworkCore;
using practice_mvc02.Models.dataTable;
using practice_mvc02.Models;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {       
            modelBuilder.Entity<Account>().HasIndex(b=>b.account).IsUnique();
            modelBuilder.Entity<Department>(entity =>{
                entity.Property(b => b.department).HasColumnType("varchar(255)");
                entity.Property(b => b.position).HasColumnType("varchar(255)");
            });
            modelBuilder.Entity<Department>()
                .HasIndex(b => new{b.department, b.position, b.principalID}).IsUnique();
            
            modelBuilder.Entity<WorkTimeRule>().HasIndex(b=> new{b.startTime, b.endTime}).IsUnique();
            modelBuilder.Entity<PunchLogWarn>().HasIndex(b=>b.punchLogID).IsUnique();
            modelBuilder.Entity<PunchCardLog>().HasIndex(b=>new{b.accountID, b.logDate}).IsUnique();
            modelBuilder.Entity<EmployeeDetail>().HasIndex(b=>b.accountID).IsUnique();
            modelBuilder.Entity<EmployeeDetail>(entity=>{
                entity.Property(b=>b.sex).HasColumnType("int(1)");
                entity.Property(b=>b.agentEnable).HasColumnType("tinyint(1)");
            });
            modelBuilder.Entity<MsgSendReceive>(entity=>{
                entity.Property(b => b.read).HasColumnType("int(1)");
                entity.Property(b => b.sDelete).HasColumnType("tinyint(1)");
                entity.Property(b => b.rDelete).HasColumnType("tinyint(1)");
            });
            modelBuilder.Entity<workTimeTotal>().HasIndex(b=>new{b.accountID, b.dateMonth}).IsUnique();
            modelBuilder.Entity<WorkTimeRule>().Property(b=>b.name).HasColumnType("varchar(255)");
            modelBuilder.Entity<SpecialDate>().Property(b=>b.departClass).HasColumnType("varchar(255)");
            modelBuilder.Entity<AnnualLeaveRule>().HasIndex(b=>b.seniority).IsUnique();

             
            //createDefaultData(modelBuilder);
        }

        private void createDefaultData(ModelBuilder modelBuilder){
            modelBuilder.Entity<Account>().HasData(new Account{
                ID = 1, account = "DIMA", password = "B72E07D7CA9ACD1262286FD6A3101E3A", userName = "DIMA_Admin",
                accLV=999, departmentID = 0, groupID = 0, timeRuleID = 0, createTime = definePara.dtNow(), updateTime = definePara.dtNow()
            });
                        
        }
    }
}