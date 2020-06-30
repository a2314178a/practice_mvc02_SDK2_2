
var myObj = new MyObj();

$(document).ready(function() {  

    init();

    $("#selDepart").on("change", function(){
        $("#selEmployee").empty();
        if($(this).val() =="所有部門"){
            $("#selEmployeeSpan").hide();
        }else{
            getDepartmentEmployee($(this).val());
        }
    });

    $("#queryDateOption").on("change", function(){
        setQueryDate(parseInt($(this).val()));
    });
    
});//.ready function

function showEmployeePage(page){
    window.location.href = "/EmployeeList/Index?page="+page; 
}

function init(){
    var date = myObj.dateTimeFormat();
    $("#reportStartDate,#reportEndDate").val(date.ymdHtml);
    if($("#punchReportDiv").length > 0){
        getDepartment();
    }
}

function setQueryDate(sel){
    var dtNow = new Date();
    var dtNowObj = myObj.dateTimeFormat(dtNow);
    var setDate = function(start, end){
        $("#reportStartDate").val(start);
        $("#reportEndDate").val(end);
    };
    switch(sel){
        //today
        case 1: setDate(dtNowObj.ymdHtml, dtNowObj.ymdHtml);
                break;
        //yesterday
        case 2: var dtYesterday = new Date(dtNow.getTime() - 24*60*60*1000);
                var dt = myObj.dateTimeFormat(dtYesterday);
                setDate(dt.ymdHtml, dt.ymdHtml);
                break;
        //this week
        case 3: var dayOfWeek = dtNow.getDay() ;   //今天是本周的第幾天 0（周日）到 6（周六）
                var dtStart = new Date(dtNow.getFullYear(), dtNow.getMonth(), (dtNow.getDate()-dayOfWeek));
                var dtEnd = new Date(dtNow.getFullYear(), dtNow.getMonth(), (dtNow.getDate() + (6- dayOfWeek)));
                var dtStartObj = myObj.dateTimeFormat(dtStart);
                var dtEndObj = myObj.dateTimeFormat(dtEnd);
                setDate(dtStartObj.ymdHtml, dtEndObj.ymdHtml);
                break;
        //last week
        case 4: var dayOfWeek = dtNow.getDay() ;   
                var dtStart = new Date(dtNow.getFullYear(), dtNow.getMonth(), (dtNow.getDate()-dayOfWeek -7));
                var dtEnd = new Date(dtNow.getFullYear(), dtNow.getMonth(), (dtNow.getDate() - dayOfWeek -1));
                var dtStartObj = myObj.dateTimeFormat(dtStart);
                var dtEndObj = myObj.dateTimeFormat(dtEnd);
                setDate(dtStartObj.ymdHtml, dtEndObj.ymdHtml);
                break;
        //this month
        case 5: var day1 = dtNowObj.year + "-" + dtNowObj.month + "-" + "01";
                var dayEnd = new Date(dtNowObj.year, dtNowObj.month, 0).getDate();
                var dayLast = dtNowObj.year + "-" + dtNowObj.month + "-" + dayEnd;
                setDate(day1, dayLast);
                break;
        //last month
        case 6: var lastMonthLastDay = new Date(dtNow.getFullYear(), dtNow.getMonth(), 0);
                var lastMonthFirstDay = new Date(lastMonthLastDay.getFullYear(), lastMonthLastDay.getMonth(), 1);
                var dtStartObj = myObj.dateTimeFormat(lastMonthFirstDay);
                var dtEndObj = myObj.dateTimeFormat(lastMonthLastDay);
                setDate(dtStartObj.ymdHtml, dtEndObj.ymdHtml);
                break;
    }
}

function getDepartment(){
    var seeAll = ($("#searchFilterDiv").data("all"));
    var seeDepartEm = ($("#searchFilterDiv").data("depart"));
    var successFn = function(res){
        if(res.length >1){
            $("#selDepart").append(new Option("所有部門", "所有部門")); 
        }
        res.forEach(function(value){  
            $("#selDepart").append(new Option(value.department, value.department));
        });
        if(seeAll == 1){
            $("#selDepart").append(new Option("未指派", "未指派"));
        }
        if(($("#selDepart").find("option")).length == 1){
            getDepartmentEmployee($("#selDepart").val());
        }
    };
    myObj.rAjaxFn("get", "/ExportXlsx/getDepartment", null, successFn);
}

function getDepartmentEmployee(depart){
    $("#selEmployee").append(new Option("所有人員", "")).prop("disabled", true);
    var successFn = function(res){
        res.forEach(function(value){
            $("#selEmployee").append(new Option(value.userName, value.id));
        });
        $("#selEmployee").prop("disabled", false);
        $("#selEmployeeSpan").show();
    };
    myObj.rAjaxFn("get", "/ExportXlsx/getDepartmentEmployee", {depart:depart}, successFn);
}

function processReport(){
    var sDate = $("#reportStartDate").val();
    var chk_sDate = new Date(sDate);
    var eDate = $("#reportEndDate").val();
    var chk_eDate = new Date(eDate);
    var departName = $("#selDepart").val();
    var accID = $("#selEmployee").val();
    if(chk_eDate <chk_sDate){
        alert("時間範圍有誤");
        return;
    }
    
    var exportPara = {
        sDate, eDate, departName, accID
    };
    var successFn = (res)=>{
        printTable(res);
        $(".exportXlsx").show();
    };
    myObj.rAjaxFn("get", "/ExportXlsx/getPrintTableData", exportPara, successFn);
}

function printTable(res){
    initTable(res);
    printTitle(res);
    printDetail(res);
}

function initTable(res){
    $(".exportXlsx").hide();
    $("#reportTable").empty();
    var colTotal = res.columnTotal;
    var rowTotal = res.rowTotal;
    var leaveStart = res.leaveStartIndex;
    if(res.type == "day"){
        var leaveColSpan = colTotal - res.leaveStartIndex +1;
    }
    else{
        var dayStart = res.dayStartIndex;
        var leaveColSpan = res.dayStartIndex - res.leaveStartIndex;
        var dayColSpan = colTotal - res.dayStartIndex + 1;
    }
    for(let i=0; i<=rowTotal; i++){
        let row = $(`<tr name='row_${i}'></tr>`);
        for(let j=1; j<=colTotal; j++){
            if(i==0){
                if(j <leaveStart){
                    row.append($(`<td rowspan='2' name='col_${j}'></td>`));
                }else if(j == leaveStart){
                    row.append($(`<td colspan='${leaveColSpan}' name='col_${j}'></td>`));
                }else if(j == dayStart){
                    row.append($(`<td colspan='${dayColSpan}' name='col_${j}'></td>`));
                }else{
                    continue;
                }
            }else if(i==1 && j<leaveStart){
                continue;
            }else{
                row.append($(`<td name='col_${j}'></td>`));
            }
        }
        $("#reportTable").append(row);
    }
}

function printTitle(res){
    for(let i=1; i<= res.columnTotal; i++){
        if(res.titleData[i] != undefined){
            if(i >= res.leaveStartIndex){
                if(i == res.leaveStartIndex){
                    $(`tr[name='row_0'] td[name='col_${i}']`).text("請假相關");
                }else if(i == res.dayStartIndex){
                    $(`tr[name='row_0'] td[name='col_${i}']`).text("打卡結果");
                }
                $(`tr[name='row_1'] td[name='col_${i}']`).text(res.titleData[i]);
            }else{
                $(`tr[name='row_0'] td[name='col_${i}']`).text(res.titleData[i]);
            }
        }
    }
    $("tr[name='row_0'],tr[name='row_1']").addClass("title");
}

function printDetail(res){
    var rowIndex = 2;
    res.detail.forEach(function(rowData){
        for(let i=1; i<=res.columnTotal; i++){
            if(rowData[i] == undefined){
                continue;
            }
            $(`tr[name='row_${rowIndex}'] td[name='col_${i}']`).text(rowData[i]);

            if((res.type=="day" && i == res.colDef.punchStatus) || (res.type=="month" && i >= res.dayStartIndex)){
                if(rowData[i] != "正常" && rowData[i] != "休息"){
                    if(rowData[i] == "請假"){
                        $(`tr[name='row_${rowIndex}'] td[name='col_${i}']`).addClass('takeRest');
                    }else{
                        $(`tr[name='row_${rowIndex}'] td[name='col_${i}']`).addClass('abnormal');
                    }
                }
            }
        }
        rowIndex++;
    });
}

function exportXlsx(){
    var successFn = (res)=>{
        if(res === 1){
            window.location.href = "/ExportXlsx/Export";
        }else{
            alert("檔案下載失敗");
        }
    };
    myObj.rAjaxFn("get", "/ExportXlsx/chkFileStatus", null, successFn);
}
