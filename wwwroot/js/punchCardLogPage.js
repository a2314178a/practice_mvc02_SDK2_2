
var myObj = new MyObj();

$(document).ready(function() {  

    init();
    $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
        getPunchLogByIDByMonth(myObj.lookEmployeeID);
    });

    $("#calendar").on("click", "td", function(){
        getClickPunchLog(this);
    });

    $("#calendar").on("mouseover", "td", function(){
        if($(this).hasClass("tdRed")){
            $(this).addClass("tdHighRed");
        }else if($(this).hasClass("tdGreen")){
            $(this).addClass("tdHighGreen");
        }else if($(this).hasClass("tdBlue")){
            $(this).addClass("tdHighBlue");
        }else if($(this).hasClass("tdRedBlue")){
            $(this).addClass("tdHighRedBlue");
        }else{
            $(this).addClass("tdGray");
        }
    });

    $("#calendar").on("mouseleave", "td", function(){
        if($(this).hasClass("tdHighRed")){
            $(this).removeClass("tdHighRed");
        }else if($(this).hasClass("tdHighGreen")){
            $(this).removeClass("tdHighGreen");
        }else if($(this).hasClass("tdHighBlue")){
            $(this).removeClass("tdHighBlue");
        }else if($(this).hasClass("tdHighRedBlue")){
            $(this).removeClass("tdHighRedBlue");
        }else{
            $(this).removeClass("tdGray");
        }
    });

});//.ready function

function init(){
    var dtNow = myObj.dateTimeFormat();
    $("#filter_Month").val(dtNow.year + "-" + dtNow.month);
}

function showPunchCardPage(){
    window.location.href = "/PunchCard/Index";
}
function showPunchLogPage(targetID){
    window.location.href = "/PunchCard/Index?page=log&target="+targetID;
}
function showTimeTotalPage(targetID){
    window.location.href = "/PunchCard/Index?page=total&target="+targetID;
}

function getPunchLogByIDByMonth(employeeID=0){
    var qMonth = $("#filter_Month").val();
    printCalendar(qMonth);
    if(qMonth == ""){
        var dt = myObj.dateTimeFormat();
        qMonth = dt.year + "-" + dt.month;
    }
    var targetYear = qMonth.split("-")[0];
    var targetMonth = qMonth.split("-")[1];
    var tableFirstTd = $("#calendar").find("tr[name='row1']").find("td:first");
    var tableLastTd = $("#calendar").find("tr[name='row6']").find("td:last");
    var tbStartMonth = (tableFirstTd.attr("id")).split("_")[0];
    if(tbStartMonth == "previous"){
        var sMonth = (parseInt(targetMonth)-1) ==0? "12" : (parseInt(targetMonth)-1);
        var sYear = (parseInt(targetMonth)-1) ==0? (parseInt(targetYear)-1) : targetYear;
    }else{
        var sMonth = targetMonth;
        var sYear = targetYear;
    }
    var eMonth = (parseInt(targetMonth)+1)==13? "1" :　(parseInt(targetMonth)+1);
    var eYear = (parseInt(targetMonth)+1)==13? (parseInt(targetYear)+1) : targetYear;
   
    var tableStart = sYear + "-" + sMonth + "-" + tableFirstTd.text();
    var tableEnd = eYear + "-" + eMonth + "-" + tableLastTd.text();


    myObj.lookEmployeeID = employeeID;
    var data = {
        employeeID: employeeID,
        qMonth: qMonth,
        queryStart: tableStart,
        queryEnd: tableEnd,
    };
    var successFn = function(res){
        calendarWithPunchLog(res, qMonth);
    };
    myObj.rAjaxFn("post", "/PunchCard/getPunchLogByIDByMonth", data, successFn);
}



//#region punchLog detail

function refreshPunchLogList(res){
    $("#punchLogList").empty();
    res.punchLog.forEach(function(value){
        var row = $(".template").find("[name='punchLogRow']").clone();
        if((value.punchStatus & 0x40) > 0 && res.takeLeave.length >0){
            var textRow = $(".template").find("[name='takeLeaveText']").clone();
            var text = "";
            res.takeLeave.forEach(function(time){
                var sTime = myObj.dateTimeFormat(time.startTime);
                var eTime = myObj.dateTimeFormat(time.endTime);
                text += sTime.ymdHtml + " " +  sTime.hmText + " ~ ";
                text += eTime.ymdHtml + " " +  eTime.hmText + " 請假 ";
            });
            $(textRow).find("td").text(text);
        }
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
        status = (value.punchStatus & 0x20) ? status+="曠職" : status;
        status = (value.punchStatus & 0x01) && status == "" ? status+="正常" : status;
        status = status.charAt(status.length-1) == "/" ? status.substring(0, status.length -1) :status;
        
        row.find("[name='logDate']").text(logDate);
        row.find("[name='logOnlineTime']").text(dtOnTime);
        row.find("[name='logOfflineTime']").text(dtOffTime);
        row.find("[name='logPunchStatus']").text(status);
        row.find(".edit_punchLog").attr("onclick", `editPunchLog(this, ${value.id});`);
        row.find(".del_punchLog").attr("onclick", `delPunchLog(${value.id});`);
        $("#punchLogList").append(row).append(textRow);
    });
}

function showAddPunchLogRow(employeeID, employeeDepartID){
    if(($("#punchLogList").find("tr")).length >0){
        alert("該天已有紀錄 無法新增");
        return;
    }
    $("#punchLogDiv").find("a.add_punchLog").hide();
    $('.btnActive').css('pointer-events', "none"); 
    var addPunchLogRow = $(".template").find("[name='addPunchLogRow']").clone();
    addPunchLogRow.find("td[name='dateTime']").text(myObj.qDateStr);
    addPunchLogRow.find("a.update_punchLog").remove();
    addPunchLogRow.find("a.create_punchLog").attr("onclick", `createPunchLog(this, ${employeeID}, ${employeeDepartID});`);
    addPunchLogRow.find("a.cancel_punchLog").attr("onclick", `cancelPunchLog(${employeeID});`);
    $('#punchLogList').append(addPunchLogRow);
}

function cancelPunchLog(){
    $('.btnActive').css('pointer-events', ""); 
    $("#punchLogDiv").find("a.add_punchLog").show();
    getPunchLogByIDByDate(myObj.qDateStr);
}

function createPunchLog(thisBtn, employeeID, employeeDepartID){
    var thisRow =  $(thisBtn).closest("tr[name='addPunchLogRow']");
    var dateTime = thisRow.find("td[name='dateTime']").text();
    var onlineTime = thisRow.find("input[name='newOnlineTime']").val();
    var offlineTime = thisRow.find("input[name='newOfflineTime']").val();
    if(dateTime =="" || (onlineTime =="" && offlineTime =="")){
        alert("此打卡紀錄不合法");return;
    }
    onlineTime = onlineTime != "" ? (dateTime + "T" + onlineTime) : "";
    offlineTime = offlineTime != "" ? (dateTime + "T" + offlineTime) : "";

    var newPunchLog = {
        accountID : employeeID,
        departmentID : employeeDepartID,
        logDate : dateTime,
        onlineTime : onlineTime,
        offlineTime : offlineTime,
    };
    var successFn = function(res){
        if(res == 2){
            alert("此打卡紀錄不合法");return;
        }
        cancelPunchLog(employeeID);
    }
    myObj.cudAjaxFn("/PunchCard/forceAddPunchCardLog", newPunchLog, successFn);
}

function editPunchLog(thisBtn, logID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='punchLogRow']").hide();
    var thisDate = thisRow.find("[name='logDate']").text();
    var thisOnlineTime = thisRow.find("[name='logOnlineTime']").text();
    var thisOfflineTime = thisRow.find("[name='logOfflineTime']").text();

    var updateLogRow = $(".template").find("[name='addPunchLogRow']").clone();
    updateLogRow.find("td[name='dateTime']").text(myObj.qDateStr);
    updateLogRow.find("input[name='newOnlineTime']").val(thisOnlineTime);
    updateLogRow.find("input[name='newOfflineTime']").val(thisOfflineTime);

    updateLogRow.find("a.create_punchLog").remove();
    updateLogRow.find("a.update_punchLog").attr("onclick", `updatePunchLog(this, ${logID})`);
    updateLogRow.find("a.cancel_punchLog").attr("onclick", `cancelPunchLog(${myObj.lookEmployeeID})`);

    $(thisRow).after(updateLogRow);
}

function updatePunchLog(thisBtn, punchLogID){
    var thisRow =  $(thisBtn).closest("tr[name='addPunchLogRow']");
    var dateTime = thisRow.find("td[name='dateTime']").text();
    var onlineTime = thisRow.find("input[name='newOnlineTime']").val();
    var offlineTime = thisRow.find("input[name='newOfflineTime']").val();
    if(dateTime =="" || (onlineTime =="" && offlineTime =="")){
        alert("此打卡紀錄不合法");return;
    }
    onlineTime = onlineTime != "" ? (dateTime + "T" + onlineTime) : "";
    offlineTime = offlineTime != "" ? (dateTime + "T" + offlineTime) : "";

    var updatePunchLog = {
        ID : punchLogID,
        logDate : dateTime,
        onlineTime : onlineTime,
        offlineTime : offlineTime,
    };
    var successFn = function(res){
        if(res == 2){
            alert("此打卡紀錄不合法");return;
        }
        cancelPunchLog(myObj.lookEmployeeID);
    }
    myObj.cudAjaxFn("/PunchCard/forceUpdatePunchCardLog", updatePunchLog, successFn);
}

function delPunchLog(logID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            cancelPunchLog(myObj.lookEmployeeID);
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/PunchCard/delPunchCardLog",{punchLogID: logID},successFn);
}

//#endregion punchLog detail

//--------------------------------------------------------------------------------------------------------

//#region time Total

function getTimeTotalByID(targetID){
    var successFn = function(res){
        refreshTimeTotalList(res);
    };
    myObj.rAjaxFn("post", "/PunchCard/getTimeTotalByID", {targetID}, successFn);
}

function refreshTimeTotalList(res){
    $("#timeTotalList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='timeTotalRow']").clone();
        var dt = myObj.dateTimeFormat(value.dateMonth);
        var dateText = dt.year + "-" + dt.month;
        row.find("[name='logDate']").text(dateText);
        row.find("[name='timeTotal']").text(value.totalTime);
        $("#timeTotalList").append(row);
    });
}

//#endregion time Total 

//------------------------------------------------------------------------------------------------------------

//#region calendar

function printCalendar(qMonth){
    $("#calendar").empty();

    if(qMonth != ""){
        var tmpYear = qMonth.split("-")[0];
        var tmpMonth = qMonth.split("-")[1];
        var queryMonth = new Date(tmpYear, tmpMonth-1, 1);
    }else{
        var queryMonth = new Date();
    }
    var firstDayWeek = new Date(queryMonth.getFullYear(), queryMonth.getMonth(), 1).getDay();   //第一天星期幾
    var lastDay = new Date(queryMonth.getFullYear(), queryMonth.getMonth()+1, 0).getDate(); //這個月有幾日
    var lastMonthLastDay = new Date(queryMonth.getFullYear(), queryMonth.getMonth(), 0).getDate(); //上個月有幾日
    //firstDayWeek = 6;
    var lastMonthStartCount = lastMonthLastDay - firstDayWeek +1;   
    var startCount = 1;
    var startPrint_flag = false;
    var idName = "this_";
    var calTable = $(".template").find("[name='calendarTB']").clone();
    var calTD = calTable.find("td");
    $.each(calTD, function(key, value){
        startPrint_flag = firstDayWeek == key? true : startPrint_flag;
        if(!startPrint_flag){
            $(value).attr("id", "previous_"+lastMonthStartCount).text(lastMonthStartCount++);
            return;
        }
        if(startCount > lastDay){
            startCount = 1;
            idName = "next_";
        }
        $(value).attr("id", idName+startCount).text(startCount++);
    });

    var tmp = calTable.find("tr[name='row6']").find("td:first").text();
    if( tmp > 1 && tmp < 8){
        calTable.find("tr[name='row6']").hide();
    }
    $("#calendar").append(calTable.html());
}

function calendarWithPunchLog(res ,qMonth){
    var thisMonth = qMonth.split("-")[1];

    res.forEach(function(value){
        var logMonth = (myObj.dateTimeFormat(value.logDate)).month;
        var logDate = (myObj.dateTimeFormat(value.logDate)).day;
        logDate = parseInt(logDate);

        var idStr = logMonth < thisMonth ? "#previous_" : logMonth > thisMonth ? "#next_" : "#this_";
        if(value.punchStatus != 1){
            if(value.punchStatus == 0x40){
                $("#calendar").find(idStr+logDate).addClass("tdBlue");
            }
            else if((value.punchStatus & 0x40)>0){
                $("#calendar").find(idStr+logDate).addClass("tdRedBlue");
            }
            else{
                $("#calendar").find(idStr+logDate).addClass("tdRed");
            }
                
        }else{
            $("#calendar").find(idStr+logDate).addClass("tdGreen");
        }
    });
}

function getClickPunchLog(thisTD){  
    var thisTdID = $(thisTD).attr("id");
    var qMonth = thisTdID.split("_")[0];
    var qDay = thisTdID.split("_")[1];
    var filterMonth = $("#filter_Month").val();
    var dt = myObj.dateTimeFormat();
    var nowArrMonth = filterMonth==""? (dt.month -1) : ((filterMonth.split("-")[1])-1) ;
    var qArrMonth = qMonth == "previous"? nowArrMonth-1 : qMonth == "next"? nowArrMonth+1 : nowArrMonth;
    var qDate = myObj.dateTimeFormat(new Date((filterMonth.split("-")[0]), qArrMonth, qDay));
    var qDateStr = qDate.year + "-" + qDate.month + "-" + qDate.day;
    getPunchLogByIDByDate(qDateStr);
}

function getPunchLogByIDByDate(qDateStr){
    myObj.qDateStr = qDateStr;
    var data = {
        employeeID : myObj.lookEmployeeID,
        qDateStr : qDateStr,
    };
    var successFn = function(res){
        refreshPunchLogList(res);
        $(".calendar_table").hide();
        $(".noDisplay").show();
    };
    myObj.rAjaxFn("post", "/PunchCard/getPunchLogByIDByDate", data, successFn);
}

function showCalendar(){
    getPunchLogByIDByMonth(myObj.lookEmployeeID);
    $(".calendar_table").show();
    $("#punchLogList").empty();
    $(".noDisplay").hide();
    $('.btnActive').css('pointer-events', ""); 
    $("#punchLogDiv").find("a.add_punchLog").show();
}

//#endregion calendar







