﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using practice_mvc02.Repositories;

namespace practice_mvc02.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20200708075840_WTRuleDelUniqueAddType")]
    partial class WTRuleDelUniqueAddType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("practice_mvc02.Models.dataTable.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("accLV");

                    b.Property<string>("account")
                        .IsRequired();

                    b.Property<DateTime>("createTime");

                    b.Property<int>("departmentID");

                    b.Property<int>("groupID");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("loginTime");

                    b.Property<string>("password")
                        .IsRequired();

                    b.Property<int>("timeRuleID");

                    b.Property<DateTime>("updateTime");

                    b.Property<string>("userName");

                    b.HasKey("ID");

                    b.HasIndex("account")
                        .IsUnique();

                    b.ToTable("accounts");

                    b.HasData(
                        new
                        {
                            ID = 1,
                            accLV = 999,
                            account = "DIMA",
                            createTime = new DateTime(2020, 7, 8, 15, 58, 39, 929, DateTimeKind.Utc).AddTicks(3186),
                            departmentID = 0,
                            groupID = 0,
                            lastOperaAccID = 0,
                            password = "B72E07D7CA9ACD1262286FD6A3101E3A",
                            timeRuleID = 0,
                            updateTime = new DateTime(2020, 7, 8, 15, 58, 39, 929, DateTimeKind.Utc).AddTicks(3558),
                            userName = "DIMA_Admin"
                        });
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.AnnualDaysOffset", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<int>("emAnnualID");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("reason");

                    b.Property<DateTime>("updateTime");

                    b.Property<int>("value");

                    b.HasKey("ID");

                    b.ToTable("annualdaysoffset");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.AnnualLeaveRule", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("buffDays");

                    b.Property<DateTime>("createTime");

                    b.Property<int>("lastOperaAccID");

                    b.Property<float>("seniority");

                    b.Property<int>("specialDays");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.HasIndex("seniority")
                        .IsUnique();

                    b.ToTable("annualleaverule");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.Department", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<string>("department")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("position")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("principalID");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.HasIndex("department", "position", "principalID")
                        .IsUnique();

                    b.ToTable("departments");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.EmployeeAnnualLeave", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<DateTime>("deadLine");

                    b.Property<int>("employeeID");

                    b.Property<int>("lastOperaAccID");

                    b.Property<float>("remainHours");

                    b.Property<int>("ruleID");

                    b.Property<int>("specialDays");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("employeeannualleaves");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.EmployeeDetail", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("accountID");

                    b.Property<sbyte>("agentEnable")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("birthday");

                    b.Property<DateTime>("createTime");

                    b.Property<string>("humanID");

                    b.Property<int>("lastOperaAccID");

                    b.Property<int>("myAgentID");

                    b.Property<int>("sex")
                        .HasColumnType("int(1)");

                    b.Property<DateTime>("startWorkDate");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.HasIndex("accountID")
                        .IsUnique();

                    b.ToTable("employeedetails");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.EmployeePrincipal", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<int>("employeeID");

                    b.Property<int>("lastOperaAccID");

                    b.Property<int>("principalAgentID");

                    b.Property<int>("principalID");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("employeeprincipals");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.GroupRule", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<string>("groupName");

                    b.Property<int>("lastOperaAccID");

                    b.Property<int>("ruleParameter");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("grouprules");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.LeaveName", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<sbyte>("enable")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("leaveName");

                    b.Property<int>("timeUnit");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("leavenames");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.LeaveOfficeApply", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("accountID");

                    b.Property<int>("applyStatus");

                    b.Property<DateTime>("createTime");

                    b.Property<DateTime>("endTime");

                    b.Property<int>("lastOperaAccID");

                    b.Property<int>("leaveID");

                    b.Property<string>("note");

                    b.Property<int>("optionType");

                    b.Property<int>("principalID");

                    b.Property<DateTime>("startTime");

                    b.Property<byte>("unit");

                    b.Property<float>("unitVal");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("leaveofficeapplys");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.Message", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("content");

                    b.Property<DateTime>("createTime");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("title");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("message");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.MsgSendReceive", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<int>("lastOperaAccID");

                    b.Property<int>("messageID");

                    b.Property<sbyte>("rDelete")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("read")
                        .HasColumnType("int(1)");

                    b.Property<int>("receiveID");

                    b.Property<sbyte>("sDelete")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("sendID");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("msgsendreceive");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.OperateLog", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("active");

                    b.Property<string>("category");

                    b.Property<string>("content");

                    b.Property<DateTime>("createTime");

                    b.Property<int>("employeeID");

                    b.Property<int>("operateID");

                    b.Property<string>("remark");

                    b.HasKey("ID");

                    b.ToTable("operateLogs");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.PunchCardLog", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("accountID");

                    b.Property<DateTime>("createTime");

                    b.Property<int>("departmentID");

                    b.Property<int>("lastOperaAccID");

                    b.Property<DateTime>("logDate");

                    b.Property<DateTime>("offlineTime");

                    b.Property<DateTime>("onlineTime");

                    b.Property<int>("punchStatus");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.HasIndex("accountID", "logDate")
                        .IsUnique();

                    b.ToTable("punchcardlogs");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.PunchLogWarn", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("accountID");

                    b.Property<DateTime>("createTime");

                    b.Property<int>("principalID");

                    b.Property<int>("punchLogID");

                    b.Property<DateTime>("updateTime");

                    b.Property<int>("warnStatus");

                    b.HasKey("ID");

                    b.HasIndex("punchLogID")
                        .IsUnique();

                    b.ToTable("punchlogwarns");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.SpecialDate", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<DateTime>("date");

                    b.Property<string>("departClass")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("note");

                    b.Property<int>("status");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("specialdate");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.WorkTimeRule", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("createTime");

                    b.Property<TimeSpan>("eRestTime")
                        .HasColumnType("time");

                    b.Property<int>("elasticityMin");

                    b.Property<TimeSpan>("endTime")
                        .HasColumnType("time");

                    b.Property<int>("lastOperaAccID");

                    b.Property<string>("name")
                        .HasColumnType("varchar(255)");

                    b.Property<TimeSpan>("sRestTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("startTime")
                        .HasColumnType("time");

                    b.Property<int>("type")
                        .HasColumnType("int(1)");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.ToTable("worktimerules");
                });

            modelBuilder.Entity("practice_mvc02.Models.dataTable.workTimeTotal", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("accountID");

                    b.Property<DateTime>("createTime");

                    b.Property<DateTime>("dateMonth");

                    b.Property<double>("totalTime");

                    b.Property<DateTime>("updateTime");

                    b.HasKey("ID");

                    b.HasIndex("accountID", "dateMonth")
                        .IsUnique();

                    b.ToTable("worktimetotals");
                });
#pragma warning restore 612, 618
        }
    }
}
