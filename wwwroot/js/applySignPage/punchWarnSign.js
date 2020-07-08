

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




