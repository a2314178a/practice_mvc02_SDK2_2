using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using practice_mvc02.Repositories;
using System.IO;
using OfficeOpenXml;
using practice_mvc02.Models.dataTable;
using OfficeOpenXml.Style;
using System.Drawing;

namespace practice_mvc02.Models
{
    public class exportPunchLogFunction
    {
        private ExportXlsxRepository Repository { get; }
        private punchStatusCode psCode {get;}
        private punchCardFunction punchCardFn {get;}
        private Dictionary<string, int> dCol;   //標題欄位定義
        private int rowIndex = 3, lastColumnIndex;   //標題起始列, 紀錄最後的欄位index
        private Dictionary<int, string> title_leaveName;
        private exportPunchLogXlsxPara qPara;   //查詢用參數
        private ExcelWorksheet ws;

        private Dictionary<string, object> objectList = new Dictionary<string, object>();    //傳給前端畫表格用
        private List<object> detailDataList = new List<object>(){}; //傳給前端畫表格用(儲存多列資料)
        private Dictionary<string, object> detailData;  //傳給前端畫表格用的(每一列資料)

        public exportPunchLogFunction(ExportXlsxRepository repository,
                    PunchCardRepository repository02, IHttpContextAccessor httpContextAccessor){
            this.Repository = repository;
            this.punchCardFn = new punchCardFunction(repository02, httpContextAccessor);
            this.psCode = new punchStatusCode();
        }

        public object createXlsxPunchLog(FileInfo file, exportPunchLogXlsxPara exportPara){
            qPara = exportPara;
            using (ExcelPackage package = new ExcelPackage(file))
            {
                if(qPara.accID == 0){
                    ws = package.Workbook.Worksheets.Add("月度總結");  //新增worksheet 
                    xlsxColumnDef(1);
                    ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells.Style.Font.Size = 16;
                    setTitle(1);
                    setRowData_month(Repository.GetNormalDetail_Month(qPara));
                }else{
                    ws = package.Workbook.Worksheets.Add("每日統計");  //新增worksheet 
                    xlsxColumnDef(2);
                    ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells.Style.Font.Size = 16;
                    setTitle(2);
                    setRowData_day(Repository.GetNormalDetail_Day(qPara));
                }    
                ws.Cells[ 1,1 , rowIndex-1,lastColumnIndex ].Style.Border.Top.Style = 
                ws.Cells[ 1,1 , rowIndex-1,lastColumnIndex ].Style.Border.Bottom.Style = 
                ws.Cells[ 1,1 , rowIndex-1,lastColumnIndex ].Style.Border.Left.Style = 
                ws.Cells[ 1,1 , rowIndex-1,lastColumnIndex ].Style.Border.Right.Style = 
                ExcelBorderStyle.Thin;
                ws.Cells.AutoFitColumns();
                setSomeCellsWidth();
                package.Save(); 
            }
            return new{
                    type = qPara.accID == 0?"month":"day",
                    columnTotal = lastColumnIndex,
                    rowTotal = rowIndex - 4,
                    colDef = objectList["colDef"],
                    titleData = objectList["titleData"],
                    leaveStartIndex = objectList["leaveStartIndex"],
                    dayStartIndex = objectList["dayStartIndex"],
                    detail = detailDataList,
            };
        }

        private void setSomeCellsWidth(){
            if(qPara.accID == 0){
                ws.Column(dCol["workDays"]).Width = 16;
                ws.Column(dCol["restDays"]).Width = 16;
                ws.Column(dCol["sumWorkMinute"]).Width = 16;
                ws.Column(dCol["lateCount"]).Width = 16;
                ws.Column(dCol["sumLateMinute"]).Width = 16;
                ws.Column(dCol["earlyCount"]).Width = 16;
                ws.Column(dCol["sumEarlyMinute"]).Width = 16;
                ws.Column(dCol["onNoPunchCount"]).Width = 16;
                ws.Column(dCol["offNoPunchCount"]).Width = 16;
                ws.Column(dCol["allNoPunchCount"]).Width = 16;
            }else{
                ws.Column(dCol["date"]).Width = 18;
                ws.Column(dCol["onlineTime"]).Width = 16;
                ws.Column(dCol["offlineTime"]).Width = 16;
                ws.Column(dCol["punchStatus"]).Width = 16;
                ws.Column(dCol["sumWorkMinute"]).Width = 16;
            }
        }

        private void xlsxColumnDef(int type){   //定義欄位名對應的欄位號
            set_title_leaveName();
            dCol = new Dictionary<string, int>(){};
            int colCount = 1;
            if(type == 1){
                dCol.Add("name", colCount++);           //姓名
                dCol.Add("workClass", colCount++);      //班別
                dCol.Add("department", colCount++);     //部門
                dCol.Add("position", colCount++);       //職位
                dCol.Add("workDays", colCount++);       //出勤天數
                dCol.Add("restDays", colCount++);       //休息天數
                dCol.Add("sumWorkMinute", colCount++);  //工作時長(小時)
                dCol.Add("lateCount", colCount++);      //遲到次數
                dCol.Add("sumLateMinute", colCount++);  //遲到時長(分鐘)
                dCol.Add("earlyCount", colCount++);     //早退次數
                dCol.Add("sumEarlyMinute", colCount++);    //早退時長(分鐘)
                dCol.Add("onNoPunchCount", colCount++);    //上班缺卡次數
                dCol.Add("offNoPunchCount", colCount++);   //下班缺卡次數
                dCol.Add("allNoPunchCount", colCount++);   //曠職天數

                objectList.Add("leaveStartIndex", colCount);   //leave start col index
                foreach(var leave in title_leaveName){     //新增請假名稱
                    dCol.Add(leave.Value, colCount++);
                }

                objectList.Add("dayStartIndex", colCount);    //day start col index

                var sDay = qPara.sDate;
                var eDay = qPara.eDate;
                while(sDay <= eDay){                //新增日期
                    dCol.Add(sDay.ToString("M/d"), colCount++);
                    sDay=sDay.AddDays(1);
                }
            }
            else
            {
                dCol.Add("name", colCount++);           //姓名
                dCol.Add("workClass", colCount++);      //班別
                dCol.Add("department", colCount++);     //部門
                dCol.Add("position", colCount++);       //職位
                dCol.Add("date", colCount++);       //日期
                dCol.Add("workTime", colCount++);       //上班時間
                dCol.Add("onlineTime", colCount++);  //上班打卡時間
                dCol.Add("offlineTime", colCount++);      //下班打卡時間
                dCol.Add("punchStatus", colCount++);  //今日狀態 
                dCol.Add("sumWorkMinute", colCount++);
                
                objectList.Add("leaveStartIndex", colCount);   //leave start col index
                foreach(var leave in title_leaveName){     //新增請假名稱
                    dCol.Add(leave.Value, colCount++);
                }
                objectList.Add("dayStartIndex", null);  //no set null
            }
            objectList.Add("colDef", dCol);  //colDef name:index   
            lastColumnIndex = colCount -1; 
        }

        private void set_title_leaveName(){ //設定標頭-請假名稱
            title_leaveName = new Dictionary<int, string>(){};
            var leaves = Repository.GetLeaveName();
            foreach(var leave in leaves){
                var sName = leave.timeUnit ==1?"(天)":leave.timeUnit==2?"(半天)":"(小時)";
                title_leaveName.Add(leave.ID, (leave.leaveName+sName));
            }
        }

        private void setTitle(int type){  //Cell[row, column] 設置標頭
            var tmpList_leave = new List<string>(){};
            var titleData = new Dictionary<string, string>(){};

            ws.Cells[1,1].Value = 
            $"{qPara.sDate.Year}年{qPara.sDate.Month}月{qPara.sDate.Day}日 - {qPara.eDate.Year}年{qPara.eDate.Month}月{qPara.eDate.Day}日 打卡紀錄"; 
            ws.Cells[ 1,1 , 2,lastColumnIndex ].Merge = true;
            ws.Cells[ 1,1 , 2,lastColumnIndex ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            
            ws.Cells[ rowIndex, dCol["name"] ].Value = "姓名"; 
            ws.Cells[ rowIndex,dCol["name"] , rowIndex+1,dCol["name"]].Merge = true;
            titleData.Add(dCol["name"].ToString(), "姓名");

            ws.Cells[ rowIndex, dCol["workClass"] ].Value = "班別";
            ws.Cells[ rowIndex,dCol["workClass"] , rowIndex+1,dCol["workClass"]].Merge = true;
            titleData.Add(dCol["workClass"].ToString(), "班別");

            ws.Cells[ rowIndex, dCol["department"] ].Value = "部門";   
            ws.Cells[ rowIndex,dCol["department"] , rowIndex+1,dCol["department"]].Merge = true;
            titleData.Add(dCol["department"].ToString(), "部門");

            ws.Cells[ rowIndex, dCol["position"] ].Value = "職位";
            ws.Cells[ rowIndex,dCol["position"] , rowIndex+1,dCol["position"]].Merge = true;
            titleData.Add(dCol["position"].ToString(), "職位");

            if(type==1){    //月報表
                ws.Cells[ rowIndex, dCol["workDays"] ].Value = "出勤天數";    
                ws.Cells[ rowIndex,dCol["workDays"] , rowIndex+1,dCol["workDays"]].Merge = true;
                titleData.Add(dCol["workDays"].ToString(), "出勤天數");

                ws.Cells[ rowIndex, dCol["restDays"] ].Value = "休息天數";
                ws.Cells[ rowIndex,dCol["restDays"] , rowIndex+1,dCol["restDays"]].Merge = true;
                titleData.Add(dCol["restDays"].ToString(), "休息天數");
                
                ws.Cells[ rowIndex, dCol["sumWorkMinute"] ].Value = "工作時長\n(分鐘)";
                ws.Cells[ rowIndex, dCol["sumWorkMinute"] ].Style.WrapText = true;  
                ws.Cells[ rowIndex,dCol["sumWorkMinute"] , rowIndex+1,dCol["sumWorkMinute"]].Merge = true;
                titleData.Add(dCol["sumWorkMinute"].ToString(), "工作時長\n(分鐘)");

                ws.Cells[ rowIndex, dCol["lateCount"] ].Value = "遲到次數"; 
                ws.Cells[ rowIndex,dCol["lateCount"] , rowIndex+1,dCol["lateCount"]].Merge = true;
                titleData.Add(dCol["lateCount"].ToString(), "遲到次數");

                ws.Cells[ rowIndex, dCol["sumLateMinute"] ].Value = "遲到時長\n(分鐘)"; 
                ws.Cells[ rowIndex, dCol["sumLateMinute"] ].Style.WrapText = true;
                ws.Cells[ rowIndex,dCol["sumLateMinute"] , rowIndex+1,dCol["sumLateMinute"]].Merge = true;
                titleData.Add(dCol["sumLateMinute"].ToString(), "遲到時長\n(分鐘)");
                
                ws.Cells[ rowIndex, dCol["earlyCount"] ].Value = "早退次數"; 
                ws.Cells[ rowIndex,dCol["earlyCount"] , rowIndex+1,dCol["earlyCount"]].Merge = true;
                titleData.Add(dCol["earlyCount"].ToString(), "早退次數");

                ws.Cells[ rowIndex, dCol["sumEarlyMinute"] ].Value = "早退時長\n(分鐘)";
                ws.Cells[ rowIndex, dCol["sumEarlyMinute"] ].Style.WrapText = true;
                ws.Cells[ rowIndex,dCol["sumEarlyMinute"] , rowIndex+1,dCol["sumEarlyMinute"]].Merge = true;
                titleData.Add(dCol["sumEarlyMinute"].ToString(), "早退時長\n(分鐘)");

                ws.Cells[ rowIndex, dCol["onNoPunchCount"] ].Value = "上班\n缺卡次數"; 
                ws.Cells[ rowIndex, dCol["onNoPunchCount"] ].Style.WrapText = true;   
                ws.Cells[ rowIndex,dCol["onNoPunchCount"] , rowIndex+1,dCol["onNoPunchCount"]].Merge = true;
                titleData.Add(dCol["onNoPunchCount"].ToString(), "上班\n缺卡次數");

                ws.Cells[ rowIndex, dCol["offNoPunchCount"] ].Value = "下班\n缺卡次數";
                ws.Cells[ rowIndex, dCol["offNoPunchCount"] ].Style.WrapText = true;
                ws.Cells[ rowIndex,dCol["offNoPunchCount"] , rowIndex+1,dCol["offNoPunchCount"]].Merge = true;
                titleData.Add(dCol["offNoPunchCount"].ToString(), "下班\n缺卡次數");

                ws.Cells[ rowIndex, dCol["allNoPunchCount"] ].Value = "曠職天數";
                ws.Cells[ rowIndex,dCol["allNoPunchCount"] , rowIndex+1,dCol["allNoPunchCount"]].Merge = true;
                titleData.Add(dCol["allNoPunchCount"].ToString(), "曠職天數");

                var once = false;
                foreach(var leave in title_leaveName){
                    if(!once){
                        ws.Cells[ rowIndex, dCol[leave.Value] ].Value = "請假相關";
                        ws.Cells[ rowIndex,dCol[leave.Value] , rowIndex,dCol[leave.Value]+title_leaveName.Count-1].Merge = true;
                        once = true;
                    }
                    ws.Cells[ rowIndex+1, dCol[leave.Value] ].Value = leave.Value;
                    titleData.Add(dCol[leave.Value].ToString(), leave.Value);
                    tmpList_leave.Add(leave.Value);
                }

                var sDay = qPara.sDate;
                var eDay = qPara.eDate;
                while(sDay <= eDay){
                    if(sDay == qPara.sDate){
                        ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Value = "打卡結果";
                        ws.Cells[ rowIndex,dCol[sDay.ToString("M/d")], rowIndex,dCol[eDay.ToString("M/d")]].Merge = true;
                    }
                    ws.Cells[ rowIndex+1, dCol[sDay.ToString("M/d")] ].Value = sDay.ToString("M/d");
                    titleData.Add(dCol[sDay.ToString("M/d")].ToString(), sDay.ToString("M/d"));
                    sDay=sDay.AddDays(1);
                }       
            }
            else{    //日報表
                ws.Cells[ rowIndex, dCol["date"] ].Value = "日期"; 
                ws.Cells[ rowIndex,dCol["date"] , rowIndex+1,dCol["date"]].Merge = true;
                titleData.Add(dCol["date"].ToString(), "日期");

                ws.Cells[ rowIndex, dCol["workTime"] ].Value = "上班時間"; 
                ws.Cells[ rowIndex,dCol["workTime"] , rowIndex+1,dCol["workTime"]].Merge = true;
                titleData.Add(dCol["workTime"].ToString(), "上班時間");

                ws.Cells[ rowIndex, dCol["onlineTime"] ].Value = "上班\n打卡時間"; 
                ws.Cells[ rowIndex, dCol["onlineTime"] ].Style.WrapText = true;
                ws.Cells[ rowIndex,dCol["onlineTime"] , rowIndex+1,dCol["onlineTime"]].Merge = true;
                titleData.Add(dCol["onlineTime"].ToString(), "上班\n打卡時間");

                ws.Cells[ rowIndex, dCol["offlineTime"] ].Value = "下班\n打卡時間"; 
                ws.Cells[ rowIndex, dCol["offlineTime"] ].Style.WrapText = true;
                ws.Cells[ rowIndex,dCol["offlineTime"] , rowIndex+1,dCol["offlineTime"]].Merge = true;
                titleData.Add(dCol["offlineTime"].ToString(), "下班\n打卡時間");

                ws.Cells[ rowIndex, dCol["punchStatus"] ].Value = "打卡結果"; 
                ws.Cells[ rowIndex,dCol["punchStatus"] , rowIndex+1,dCol["punchStatus"]].Merge = true;
                titleData.Add(dCol["punchStatus"].ToString(), "打卡結果");

                ws.Cells[ rowIndex, dCol["sumWorkMinute"] ].Value = "工作時長\n(分鐘)"; 
                ws.Cells[ rowIndex, dCol["sumWorkMinute"] ].Style.WrapText = true;
                ws.Cells[ rowIndex,dCol["sumWorkMinute"] , rowIndex+1,dCol["sumWorkMinute"]].Merge = true;
                titleData.Add(dCol["sumWorkMinute"].ToString(), "工作時長\n(分鐘)");  

                var once = false;
                foreach(var leave in title_leaveName){
                    if(!once){
                        ws.Cells[ rowIndex, dCol[leave.Value] ].Value = "請假相關";
                        ws.Cells[ rowIndex,dCol[leave.Value] , rowIndex,dCol[leave.Value]+title_leaveName.Count-1].Merge = true;
                        once = true;
                    }
                    ws.Cells[ rowIndex+1, dCol[leave.Value] ].Value = leave.Value;
                    titleData.Add(dCol[leave.Value].ToString(), leave.Value);
                    tmpList_leave.Add(leave.Value);
                }
            }
            
            objectList.Add("titleData", titleData);

            ws.Cells[ 1,1 , rowIndex+1,lastColumnIndex ].Style.Font.Bold = true;
            //背景要上色 一定要加這行..不然會報錯
            ws.Cells[ 1,1 , rowIndex+1,lastColumnIndex ].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //上色
            ws.Cells[ 1,1 , 2,lastColumnIndex ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(225,225,250));
            ws.Cells[ rowIndex,1 , rowIndex+1,lastColumnIndex ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(250,225,100));
            
            rowIndex+=2;        
        }


#region month report

        private void setRowData_month(List<exportXlsxData> normalData){ //寫入每個人基本與打卡資料
            foreach(var tmp in normalData){
                detailData = new Dictionary<string, object>(){};    //給前端用的
                ws.Cells[ rowIndex, dCol["name"] ].Value = tmp.name;
                ws.Cells[ rowIndex, dCol["workClass"] ].Value = tmp.workClass;
                ws.Cells[ rowIndex, dCol["department"] ].Value = tmp.department;
                ws.Cells[ rowIndex, dCol["position"] ].Value = tmp.position;

                detailData.Add(dCol["name"].ToString(), tmp.name);
                detailData.Add(dCol["workClass"].ToString(), tmp.workClass);
                detailData.Add(dCol["department"].ToString(), tmp.department);
                detailData.Add(dCol["position"].ToString(), tmp.position);
                setPunchLogData_month(tmp.accID); //計算打卡資料

                detailDataList.Add(detailData);
                rowIndex++;
            }
        }

        private void setPunchLogData_month(int id){   //計算所需的相關資料
            List<PunchCardLog> logs = Repository.GetRequestPunchLog(qPara, id);
            WorkTimeRule wtRule = Repository.GetThisWorkTimeRule(id);
            logDataCalUse ct = new logDataCalUse();
            Dictionary<string, string> logStatus = new Dictionary<string, string>(){};  
            var leaveCellsVal = new Dictionary<string, double>(){};

            calRestWorkTimeMinute(ct, wtRule);  //計算休息與工作時間長度
            calLeaveVal(id, ct, leaveCellsVal, wtRule); //計算請假時間

            foreach(var log in logs){   //計算相關需要的次數
                if(log.logDate > definePara.dtNow().Date){
                    continue;
                }
                
                WorkDateTime workTime = punchCardFn.workTimeProcess(wtRule, log);

                ct.workDays += (log.onlineTime.Year>1 || log.offlineTime.Year>1) ? 1 : 0;
                ct.offNoPunchCount += ((log.punchStatus & psCode.hadLost) >0 && log.onlineTime.Year>1 && log.offlineTime.Year==1) ? 1 : 0;
                ct.onNoPunchCount += ((log.punchStatus & psCode.hadLost) >0 && log.onlineTime.Year ==1 && log.offlineTime.Year >1) ? 1 : 0;
                ct.allNoPunchCount += (log.punchStatus & psCode.noWork)>0 ? 1 : 0;

                if((log.punchStatus & psCode.lateIn)>0){    //計算遲到次數與時間
                    ct.lateCount++;
                    var onTime = new TimeSpan(log.onlineTime.Hour, log.onlineTime.Minute, log.onlineTime.Second);
                    if(onTime < wtRule.startTime){
                        ct.sumLateMinute += (int)(onTime.Add(new TimeSpan(24,0,0)) - wtRule.startTime).TotalMinutes;
                    }else{
                        ct.sumLateMinute += (int)(onTime - wtRule.startTime).TotalMinutes;
                    }
                }

                if((log.punchStatus & psCode.earlyOut)>0){  //計算早退次數與時間
                    ct.earlyCount++;
                    var offTime = new TimeSpan(log.offlineTime.Hour, log.offlineTime.Minute, log.offlineTime.Second);
                    if(wtRule.endTime < offTime){
                        ct.sumEarlyMinute += (int)(wtRule.endTime.Add(new TimeSpan(24,0,0)) - offTime).TotalMinutes;
                    }else{
                        ct.sumEarlyMinute += (int)(wtRule.endTime - offTime).TotalMinutes;
                    }
                }

                if(log.onlineTime.Year >1 && log.offlineTime.Year >1){  //計算工作時間
                    if(log.onlineTime <= workTime.sRestDt && log.offlineTime >= workTime.eRestDt){
                        ct.sumWorkMinute += (int)((log.offlineTime - log.onlineTime).TotalMinutes) - ct.restMinute;
                    }else if(log.onlineTime < workTime.sRestDt && log.offlineTime <= workTime.eRestDt){
                        if(log.offlineTime < workTime.sRestDt){
                            ct.sumWorkMinute += (int)(log.offlineTime - log.onlineTime).TotalMinutes;
                        }else{
                            ct.sumWorkMinute += (int)(workTime.sRestDt - log.onlineTime).TotalMinutes;
                        }
                    }else if(log.onlineTime >= workTime.sRestDt && log.offlineTime > workTime.eRestDt){
                        if(log.onlineTime > workTime.eRestDt){
                            ct.sumWorkMinute += (int)(log.offlineTime - log.onlineTime).TotalMinutes;
                        }else{
                            ct.sumWorkMinute += (int)(log.offlineTime - workTime.eRestDt).TotalMinutes;
                        }
                    }
                }
                setDictionary_daysStatus_month(logStatus, log); //判斷應顯示的打卡狀態
            }//foreach(var log in logs)

            var useDate = qPara.eDate > definePara.dtNow().Date? definePara.dtNow().Date : qPara.eDate;
            var add0or1 = (qPara.eDate >= definePara.dtNow().Date)? 0 : 1;  //如搜尋日含今日 先不包含今日
            add0or1 = (logStatus.ContainsKey(definePara.dtNow().Date.ToString("M/d")))? 1 : add0or1;
            ct.restDays = (int)(useDate - qPara.sDate).TotalDays + add0or1 - ct.workDays - ct.allNoPunchCount;
            setCellsValue_month(ct, logStatus, leaveCellsVal);  //寫入相關值
        }//void setPunchLogData_month

        private void calLeaveVal(int id, logDataCalUse ct, Dictionary<string, double> leaveCellsVal, WorkTimeRule wtRule){
            
            var thisApplyLeave = Repository.GetApplyLeaveLogs(qPara, id);
            foreach(var log in thisApplyLeave){
                var leaveVal = 0.0;
                var str = title_leaveName[log.leaveID];
                var leftC = str.IndexOf('(');
                var rightC = str.IndexOf(')');
                var unit = str.Substring(leftC+1, rightC-(leftC+1));

                //以下為請假時間可能有超出報表要求時間所做的處理
                if(log.startTime >= qPara.sDate && log.endTime <= qPara.eDate){ //請假時間範圍在報表時間範圍內
                    if(unit =="天"){
                        leaveVal = log.unitVal; 
                    }else if(unit =="半天"){
                        leaveVal = log.unit == 1? log.unitVal*2 : log.unitVal;
                    }else{  //小時
                        if(log.unit == 1){  //full day
                            leaveVal = log.unitVal*(ct.workNoRestMinute)/60;
                        }else if(log.unit == 2){    //half day
                            leaveVal = log.unitVal*(ct.workNoRestMinute)/(60*2);
                        }else{  //hour
                            leaveVal = log.unitVal;
                        }
                    }
                }
                else{   //請假時間範圍有超出報表時間
                    WorkDateTime workTime = new WorkDateTime();
                    DateTime sCalTime, eCalTime;
                    if(log.startTime < qPara.sDate && log.endTime <= qPara.eDate){
                        workTime = punchCardFn.workTimeProcess(wtRule, new PunchCardLog(){logDate=qPara.sDate});
                        sCalTime = workTime.sWorkDt;
                        eCalTime = log.endTime;
                    }else if(log.startTime >= qPara.sDate && log.endTime > qPara.eDate){
                        workTime = punchCardFn.workTimeProcess(wtRule, new PunchCardLog(){logDate=qPara.eDate});
                        sCalTime = log.startTime;
                        eCalTime = workTime.eWorkDt;
                    }else{  //請假開始時間與結束時間皆超出報表要求範圍
                        workTime = punchCardFn.workTimeProcess(wtRule, new PunchCardLog(){logDate=qPara.sDate});
                        sCalTime = workTime.sWorkDt;
                        workTime = punchCardFn.workTimeProcess(wtRule, new PunchCardLog(){logDate=qPara.eDate});
                        eCalTime = workTime.eWorkDt;
                    }

                    while((eCalTime - sCalTime).TotalMinutes > ct.endStartMinute){
                        leaveVal += ct.workNoRestMinute;
                        sCalTime.AddDays(1);
                    }
                    if((eCalTime - sCalTime).TotalMinutes > (ct.workNoRestMinute)/2){//大於工作時間的一半認定有包含休息時間
                        leaveVal += (eCalTime - sCalTime).TotalMinutes - ct.restMinute;
                    }else{
                        leaveVal += (eCalTime - sCalTime).TotalMinutes;
                    }

                    leaveVal /= ct.workNoRestMinute; //convert day
                    if(unit == "半天"){
                        leaveVal *= 2;
                    }else if(unit == "小時"){
                        leaveVal *= (ct.workNoRestMinute/60); 
                    }
                }

                if(!leaveCellsVal.ContainsKey(title_leaveName[log.leaveID])){
                    leaveCellsVal.Add(title_leaveName[log.leaveID], leaveVal);
                }else{
                    leaveCellsVal[title_leaveName[log.leaveID]] += leaveVal;
                }
            }
        }
        
        private void setDictionary_daysStatus_month(Dictionary<string, string> logStatus, PunchCardLog log){
            var key = log.logDate.ToString("M/d");
            logStatus.Add(key, getPunchLogStatusToText(log.punchStatus));
        }

        private void setCellsValue_month(logDataCalUse ct, Dictionary<string, string> logStatus, Dictionary<string, double> leaves){
            ws.Cells[ rowIndex, dCol["workDays"] ].Value = ct.workDays;
            ws.Cells[ rowIndex, dCol["restDays"] ].Value = ct.restDays;
            ws.Cells[ rowIndex, dCol["sumWorkMinute"] ].Value = ct.sumWorkMinute;
            ws.Cells[ rowIndex, dCol["lateCount"] ].Value = ct.lateCount;
            ws.Cells[ rowIndex, dCol["sumLateMinute"] ].Value = ct.sumLateMinute;
            ws.Cells[ rowIndex, dCol["earlyCount"] ].Value = ct.earlyCount;
            ws.Cells[ rowIndex, dCol["sumEarlyMinute"] ].Value = ct.sumEarlyMinute;
            ws.Cells[ rowIndex, dCol["onNoPunchCount"] ].Value = ct.onNoPunchCount;
            ws.Cells[ rowIndex, dCol["offNoPunchCount"] ].Value = ct.offNoPunchCount;
            ws.Cells[ rowIndex, dCol["allNoPunchCount"] ].Value = ct.allNoPunchCount;

            detailData.Add(dCol["workDays"].ToString(), ct.workDays);
            detailData.Add(dCol["restDays"].ToString(), ct.restDays);
            detailData.Add(dCol["sumWorkMinute"].ToString(), ct.sumWorkMinute);
            detailData.Add(dCol["lateCount"].ToString(), ct.lateCount);
            detailData.Add(dCol["sumLateMinute"].ToString(), ct.sumLateMinute);
            detailData.Add(dCol["earlyCount"].ToString(), ct.earlyCount);
            detailData.Add(dCol["sumEarlyMinute"].ToString(), ct.sumEarlyMinute);
            detailData.Add(dCol["onNoPunchCount"].ToString(), ct.onNoPunchCount);
            detailData.Add(dCol["offNoPunchCount"].ToString(), ct.offNoPunchCount);
            detailData.Add(dCol["allNoPunchCount"].ToString(), ct.allNoPunchCount);

            foreach(var leave in leaves){
                ws.Cells[ rowIndex, dCol[leave.Key] ].Value = leave.Value;
                detailData.Add(dCol[leave.Key].ToString(), leave.Value);
            }

            var sDay = qPara.sDate;
            var eDay = qPara.eDate;
            while(sDay <= eDay){
                if(logStatus.ContainsKey(sDay.ToString("M/d"))){
                    var status = logStatus[sDay.ToString("M/d")];
                    ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Value = status;
                    detailData.Add(dCol[sDay.ToString("M/d")].ToString(), status);

                    if(status != "正常"){
                        ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        if(status == "請假"){  
                            ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200,250,200));
                        }else{
                            ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(250,200,200));
                        }
                    }
                }else{
                    if(sDay < definePara.dtNow().Date){
                        ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Value = "休息";
                        detailData.Add(dCol[sDay.ToString("M/d")].ToString(), "休息");
                        ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[ rowIndex, dCol[sDay.ToString("M/d")] ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240,240,240));
                    }
                }
                sDay = sDay.AddDays(1);
                if(sDay > definePara.dtNow().Date){
                    break;
                }
            }
        }

#endregion //month report

     
#region day report

        private void setRowData_day(exportXlsxData normalData){ //寫入基本與打卡資料
            if(normalData == null){
                return;
            }
            var punchLogs = Repository.GetRequestPunchLog(qPara, normalData.accID);
            var wtRule = Repository.GetThisWorkTimeRule(normalData.accID);
            var thisApplyLeave = Repository.GetApplyLeaveLogs(qPara, normalData.accID);
            var ct = new logDataCalUse();
            string[] week = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };

            var workTime = normalData.startTime.ToString(@"hh\:mm") + "-" + normalData.endTime.ToString(@"hh\:mm");
            var sDate = qPara.sDate;
            var eDate = qPara.eDate;
            while(sDate <= eDate){
                detailData = new Dictionary<string, object>(){};   //給前端用的
                var sDateText = sDate.ToString("yyyy-MM-dd") + week[Convert.ToInt16(sDate.DayOfWeek.ToString("d"))];
                var isRest = (sDate >= definePara.dtNow().Date)? false: true;
                ws.Cells[ rowIndex, dCol["name"] ].Value = normalData.name;
                ws.Cells[ rowIndex, dCol["workClass"] ].Value = normalData.workClass;
                ws.Cells[ rowIndex, dCol["department"] ].Value = normalData.department;
                ws.Cells[ rowIndex, dCol["position"] ].Value = normalData.position;
                ws.Cells[ rowIndex, dCol["date"] ].Value = sDateText;
                ws.Cells[ rowIndex, dCol["date"] ].Style.WrapText = true;

                detailData.Add(dCol["name"].ToString(), normalData.name);
                detailData.Add(dCol["workClass"].ToString(), normalData.workClass);
                detailData.Add(dCol["department"].ToString(), normalData.department);
                detailData.Add(dCol["position"].ToString(), normalData.position);
                detailData.Add(dCol["date"].ToString(), sDateText);

                foreach(var log in punchLogs){
                    if(log.logDate == sDate){
                        ws.Cells[ rowIndex, dCol["workTime"] ].Value = workTime;
                        detailData.Add(dCol["workTime"].ToString(), workTime);

                        if(log.onlineTime.Year >1){
                            ws.Cells[ rowIndex, dCol["onlineTime"] ].Value = log.onlineTime.TimeOfDay.ToString(@"hh\:mm\:ss");
                            detailData.Add(dCol["onlineTime"].ToString(), log.onlineTime.TimeOfDay.ToString(@"hh\:mm\:ss"));
                        }
                        if(log.offlineTime.Year >1){
                            ws.Cells[ rowIndex, dCol["offlineTime"] ].Value = log.offlineTime.TimeOfDay.ToString(@"hh\:mm\:ss");
                            detailData.Add(dCol["offlineTime"].ToString(), log.offlineTime.TimeOfDay.ToString(@"hh\:mm\:ss"));
                        }
                        var status = getPunchLogStatusToText(log.punchStatus);
                        ws.Cells[ rowIndex, dCol["punchStatus"] ].Value = status;
                        detailData.Add(dCol["punchStatus"].ToString(), status);

                        if(status != "正常"){
                            ws.Cells[ rowIndex, dCol["punchStatus"] ].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            if(status == "請假"){  
                                ws.Cells[ rowIndex, dCol["punchStatus"] ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200,250,200));
                            }else{
                                ws.Cells[ rowIndex, dCol["punchStatus"] ].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(250,200,200));
                            }
                        }
                        setPunchLogData_day(normalData.accID, log, thisApplyLeave);
                        isRest = false;
                    }
                }
                if(isRest){
                    ws.Cells[ rowIndex, dCol["workTime"] ].Value = "例假日";
                    detailData.Add(dCol["workTime"].ToString(), "例假日");
                }
                detailDataList.Add(detailData);
                rowIndex++;
                sDate = sDate.AddDays(1);
            }//while(sDate <= eDate)    
        }
        
        private void setPunchLogData_day(int id, PunchCardLog log, List<LeaveOfficeApply> leaves){
            WorkTimeRule wtRule = Repository.GetThisWorkTimeRule(id);
            logDataCalUse ct = new logDataCalUse();  
            var leaveCellsVal = new Dictionary<string, double>(){};
            
            calRestWorkTimeMinute(ct, wtRule);  //計算休息與工作時間長度

            WorkDateTime workTime = punchCardFn.workTimeProcess(wtRule, log);
            if(log.onlineTime.Year >1 && log.offlineTime.Year >1){  //計算工作時間
                if(log.onlineTime <= workTime.sRestDt && log.offlineTime >= workTime.eRestDt){
                    ct.sumWorkMinute += (int)((log.offlineTime - log.onlineTime).TotalMinutes) - ct.restMinute;
                }else if(log.onlineTime < workTime.sRestDt && log.offlineTime <= workTime.eRestDt){
                    if(log.offlineTime < workTime.sRestDt){
                        ct.sumWorkMinute += (int)(log.offlineTime - log.onlineTime).TotalMinutes;
                    }else{
                        ct.sumWorkMinute += (int)(workTime.sRestDt - log.onlineTime).TotalMinutes;
                    }
                }else if(log.onlineTime >= workTime.sRestDt && log.offlineTime > workTime.eRestDt){
                    if(log.onlineTime > workTime.eRestDt){
                        ct.sumWorkMinute += (int)(log.offlineTime - log.onlineTime).TotalMinutes;
                    }else{
                        ct.sumWorkMinute += (int)(log.offlineTime - workTime.eRestDt).TotalMinutes;
                    }
                }
            }
            ws.Cells[ rowIndex, dCol["sumWorkMinute"] ].Value = ct.sumWorkMinute >0? ct.sumWorkMinute.ToString() : "";
            detailData.Add(dCol["sumWorkMinute"].ToString(), (ct.sumWorkMinute >0? ct.sumWorkMinute.ToString() : ""));
         
            foreach(var leave in leaves){   //計算請假時間
                var leaveVal = 0.0;
                if(log.logDate >= leave.startTime.Date && log.logDate <= leave.endTime){
                    if(workTime.sWorkDt >= leave.startTime && workTime.eWorkDt <= leave.endTime){
                        leaveVal = ct.workNoRestMinute;
                    }else if(workTime.sWorkDt < leave.startTime && workTime.eWorkDt <= leave.endTime){
                        leaveVal = ((workTime.eWorkDt - leave.startTime).TotalMinutes);
                        leaveVal = leaveVal > (ct.workNoRestMinute)*0.5? leaveVal - ct.restMinute : leaveVal; //大於工作時間的一半認定有包含休息時間
                    }else if(workTime.sWorkDt >= leave.startTime && workTime.eWorkDt > leave.endTime){
                        leaveVal = ((leave.endTime - workTime.sWorkDt).TotalMinutes);
                        leaveVal = leaveVal > (ct.workNoRestMinute)*0.5? leaveVal - ct.restMinute : leaveVal;
                    }else{
                        leaveVal = ((leave.endTime - leave.startTime).TotalMinutes);
                        leaveVal = leaveVal > (ct.workNoRestMinute)*0.5? leaveVal - ct.restMinute : leaveVal;
                    }
                    var leaveName = title_leaveName[leave.leaveID];
                    var leftC = leaveName.IndexOf('(');
                    var rightC = leaveName.IndexOf(')');
                    var unit = leaveName.Substring(leftC+1, rightC-(leftC+1));
                    leaveVal /= ct.workNoRestMinute; //convert day
                    if(unit == "半天"){
                        leaveVal *= 2;
                    }else if(unit == "小時"){
                        leaveVal *= (ct.workNoRestMinute/60); 
                    }
                    ws.Cells[ rowIndex, dCol[leaveName] ].Value = leaveVal;
                    detailData.Add(dCol[leaveName].ToString(), leaveVal);
                }  
            }
        }
 
#endregion //month report
        

        private void calRestWorkTimeMinute(logDataCalUse ct, WorkTimeRule wtRule){  //計算工作與休息長度
            var restLength = TimeSpan.Zero;
            if(wtRule.eRestTime < wtRule.sRestTime){
                restLength = wtRule.eRestTime.Add(new TimeSpan(24,0,0)) - wtRule.sRestTime;
            }else{
                restLength = wtRule.eRestTime - wtRule.sRestTime;
            }
            ct.restMinute = (int)restLength.TotalMinutes;

            var workLength = TimeSpan.Zero;
            if(wtRule.endTime < wtRule.startTime){
                workLength = wtRule.endTime.Add(new TimeSpan(24,0,0)) - wtRule.startTime;
            }else{
                workLength = wtRule.endTime - wtRule.startTime;
            }
            ct.endStartMinute = (int)workLength.TotalMinutes;

            ct.workNoRestMinute = ct.endStartMinute - ct.restMinute;
        }

        private string getPunchLogStatusToText(int status){ //打卡狀態轉文字
            var text = " "; //避免下面的text[text.Length-1]出錯
            text = (status & psCode.lateIn)>0 ? text+="遲到/" : text;
            text = (status & psCode.earlyOut)>0 ? text+="早退/" : text;
            text = (status & psCode.overtime)>0 ? text+="加班/" : text;
            text = (status & psCode.hadLost)>0 ? text+="缺卡/" : text;
            text = (status & psCode.takeLeave)>0 ? text+="請假/" : text;
            text = (status & psCode.noWork)>0 ? text+="曠職/" : text;
            text = (status & psCode.normal)>0 && text==" " ? text="正常" : text;
            text = text[text.Length-1]=='/'? text.Substring(0, text.Length-1) : text;
            text = text.Trim();
            return text;
        }
  
        private class logDataCalUse{
            public int workDays = 0; 
            public int restDays = 0; 
            public int sumWorkMinute = 0;
            public int lateCount = 0; 
            public int sumLateMinute = 0;
            public int earlyCount = 0; 
            public int sumEarlyMinute = 0;
            public int onNoPunchCount = 0;
            public int offNoPunchCount = 0;
            public int allNoPunchCount = 0; 

            public int restMinute = 0;
            public int endStartMinute = 0;
            public int workNoRestMinute = 0;
        }

    }

    public class exportPunchLogXlsxPara{
        public DateTime sDate {get; set;}
        public DateTime eDate {get; set;}
        public string departName {get; set;}
        public int accID {get; set;}
    }

    public class exportXlsxData{
        public int accID {get; set;}
        public string name {get; set;}
        public string workClass {get; set;}
        public TimeSpan startTime {get; set;}
        public TimeSpan endTime {get; set;}
        public string department {get; set;}
        public string position {get; set;}
    }


}