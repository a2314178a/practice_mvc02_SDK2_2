
var myObj = new MyObj();

$(document).ready(function() {  
    init();

    $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
        getEmployeeApplyLeave();
    });
});//.ready function


function selPageContext(type){
    window.location.href = "/ApplicationSign/Index?type="+type;
}

function init(){
    if($("#punchWarnDiv").length >0){
        getPunchLogWarn();
    }else if($("#leaveSignDiv").length >0){
        if($("#searchFilterDiv").length >0){
            var date = new Date();
            var newDate = date.setDate(date.getDate() - 30);
            var dtStart = myObj.dateTimeFormat(newDate);
            $("#filter_sDate").val(dtStart.ymdHtml);
            var dtEnd = myObj.dateTimeFormat();
            $("#filter_eDate").val(dtEnd.ymdHtml);
        }
        getEmployeeApplyLeave();
    }
}
//------------------------------------------------------------------------------------------------------------

//#region punchWarn

function getPunchLogWarn(){
    $('.btnActive').css('pointer-events', "");
    $("#punchLogWarnList").empty();
    var successFn = function(res){
        res.forEach(function(value){
            var dtOn = myObj.dateTimeFormat(value.onlineTime);
            var dtOff = myObj.dateTimeFormat(value.offlineTime);
            var workDate = myObj.dateTimeFormat(value.logDate);
            var logDate = workDate.year + "-" + workDate.month + "-" + workDate.day;
            var dtOnTime = dtOn.year == 1? "" : (dtOn.hour + ":" + dtOn.minute + ":" + dtOn.second);
            var dtOffTime = dtOff.year == 1? "" : (dtOff.hour + ":" + dtOff.minute + ":" + dtOff.second);

            var status = "";
            status = (value.punchStatus & 0x02) ? status+="遲到/" : status;
            status = (value.punchStatus & 0x04) ? status+="早退/" : status;
            status = (value.punchStatus & 0x08) ? status+="加班/" : status;
            status = (value.punchStatus & 0x10) ? status+="缺卡/" : status;
            status = (value.punchStatus & 0x40) ? status+="請假/" : status;
            status = (value.punchStatus & 0x20) ? "曠職" : status;
            status = (value.punchStatus & 0x01) && status == "" ? status+="正常" : status;
            status = status.charAt(status.length-1) == "/" ? status.substring(0, status.length -1) :status;

            var row = $(".template").find("[name='punchLogWarnRow']").clone();
            row.find("[name='employeeName']").text(value.userName);
            row.find("[name='logDate']").text(logDate);
            row.find("[name='logOnlineTime']").text(dtOnTime);
            row.find("[name='logOfflineTime']").text(dtOffTime);
            row.find("[name='logPunchStatus']").text(status);
            row.find("[name='signStatus']").text(value.warnStatus==1? "已處理":"未處理");
            row.find(".edit_punchLog").attr("onclick","editPunchLogWarn(this, "+value.id+");");
            row.find(".ignore_punchLog").attr("onclick","ignorePunchLogWarn(this, "+value.id+");");
            $("#punchLogWarnList").append(row);
        });
    };
    myObj.rAjaxFn("get", "/ApplicationSign/getPunchLogWarn", null, successFn);
}

function editPunchLogWarn(thisBtn, logID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='punchLogWarnRow']").hide();
    var thisName = thisRow.find("[name='employeeName']").text();
    var thisDate = thisRow.find("[name='logDate']").text();
    var thisOnlineTime = thisRow.find("[name='logOnlineTime']").text();
    var thisOfflineTime = thisRow.find("[name='logOfflineTime']").text();

    var updateLogRow = $(".template").find("[name='upPunchLogWarnRow']").clone();
    updateLogRow.find("td[name='employeeName']").text(thisName);
    updateLogRow.find("input[name='newDateTime']").val(thisDate);
    updateLogRow.find("input[name='newOnlineTime']").val(thisOnlineTime);
    updateLogRow.find("input[name='newOfflineTime']").val(thisOfflineTime);

    updateLogRow.find("a.up_punchLogWarn").attr("onclick", "updatePunchLog(this, "+ logID +")");
    updateLogRow.find("a.cel_punchLogWarn").attr("onclick", "cancelPunchLog()");

    $(thisRow).after(updateLogRow);
}

function ignorePunchLogWarn(thisBtn, logID){
    var msg = "您真的確定要忽略嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            cancelPunchLog();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/ApplicationSign/ignorePunchLogWarn", {punchLogID: logID}, successFn);
}

function updatePunchLog(thisBtn, logID){
    var thisRow =  $(thisBtn).closest("tr[name='upPunchLogWarnRow']");
    var dateTime = thisRow.find("input[name='newDateTime']").val();
    var onlineTime = thisRow.find("input[name='newOnlineTime']").val();
    var offlineTime = thisRow.find("input[name='newOfflineTime']").val();
    if(dateTime =="" || (onlineTime =="" && offlineTime =="")){
        alert("此打卡紀錄不合法");return;
    }
    onlineTime = onlineTime != "" ? (dateTime + "T" + onlineTime) : "";
    offlineTime = offlineTime != "" ? (dateTime + "T" + offlineTime) : "";

    var updatePunchLog = {
        ID : logID,
        logDate : dateTime,
        onlineTime : onlineTime,
        offlineTime : offlineTime,
    };
    var successFn = function(res){
        if(res == 2){
            alert("此打卡紀錄不合法");return;
        }
        cancelPunchLog();
    }
    myObj.cudAjaxFn("/PunchCard/forceUpdatePunchCardLog", {updatePunchLog: updatePunchLog, from:"applySign"}, successFn);
}

function cancelPunchLog(){
    getPunchLogWarn();
}

//#endregion punchWarn

//---------------------------------------------------------------------------------------------


//#region leaveSign

function getEmployeeApplyLeave(){
    if($("#searchFilterDiv").length >0){
        var sDate = $("#filter_sDate").val();
        var eDate = $("#filter_eDate").val();
    }else{
        var sDate = new Date(2000, 0, 1);   //=2000-01-01
        var eDate = new Date(); //
    }
    
    var page = $("#leaveSignDiv").attr("name") == "leave"? 0 : 1;
    if(((sDate == "" && eDate == "") || (sDate != "" && eDate != "")) && sDate <= eDate){
        var successFn = function(res){ 
            refreshApplyLeaveIng(res);   
        };
        var data ={
            page, sDate, eDate
        };
        myObj.rAjaxFn("post", "/ApplicationSign/getEmployeeApplyLeave", data, successFn);
    }else{
        alert("搜尋日期格式有誤");  return;
    }
}

function refreshApplyLeaveIng(res){
    $('.btnActive').css('pointer-events', "");
    $("#applyLeaveList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='applyLeaveRow']").clone();
        row.find("[name='employeeName']").text(value.userName);

        var addTime = myObj.dateTimeFormat(value.createTime);
        var applyDateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
        applyDateTD.html(applyDateTD.html().replace(/\n/g, "<br/>"));

        row.find("[name='applyType']").text(value.leaveName);
        row.find("[name='note']").text(value.note);

        var sTime = myObj.dateTimeFormat(value.startTime);
        var sTimeTD = row.find("[name='startTime']").text(sTime.ymdText + "\n" + sTime.hmText);
        sTimeTD.html(sTimeTD.html().replace(/\n/g, "<br/>"));
        var eTime = myObj.dateTimeFormat(value.endTime);
        var eTimeTD = row.find("[name='endTime']").text(eTime.ymdText + "\n" + eTime.hmText);
        eTimeTD.html(eTimeTD.html().replace(/\n/g, "<br/>"));
        
        var status = "";
        switch(value.applyStatus){
            case 0: status="待審核"; break;
            case 1: status="通過"; break;
            case 2: status="不通過"; break;
        };
        row.find("[name='applyStatus']").text(status);

        row.find(".yes_applyLeave").attr("onclick",`isAgreeApplyLeave(this, ${value.id}, 1);`);
        row.find(".no_applyLeave").attr("onclick",`isAgreeApplyLeave(this, ${value.id}, 2);`);
        $("#applyLeaveList").append(row);
    });
}

function isAgreeApplyLeave(thisBtn, applyLeaveID, isAgree){
    var thisRow = $(thisBtn).closest("tr[name='applyLeaveRow']");
    var successFn = function(res){
        if(res == 1){
            thisRow.find("[name='applyStatus']").text(isAgree ==1? "通過" : "不通過");
        }
    }
    myObj.cudAjaxFn("/ApplicationSign/isAgreeApplyLeave", {applyLeaveID, isAgree}, successFn);
}




//#endregion leaveSign







