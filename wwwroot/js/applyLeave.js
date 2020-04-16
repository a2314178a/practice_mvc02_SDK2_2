
var myObj = new MyObj();

$(document).ready(function() {  
    
    init();

    $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
        getApplyLeave();
    });

    $("#applyLeaveList").on("change", "select[name='newApplyType']", function(){
        var unitOption = $("#applyLeaveList").find("select[name='newTimeUnit']").empty().append(new Option("天", 1));
        var useRule = myObj.leaveOption[$(this).val()];
        if(useRule == 2){
            $(unitOption).append(new Option("半天", 2));
        }else if(useRule == 3){
            $(unitOption).append(new Option("半天", 2));
            $(unitOption).append(new Option("小時", 3));
        }
    });

    $("#applyLeaveList").on("change", "select[name='newTimeUnit']", function(){
        var selUnit = $(this).val();
        var inputVal = $("#applyLeaveList").find("input[name='newTimeLength']").val();
        var useRule = myObj.leaveOption[$("#applyLeaveList").find("select[name='newApplyType']").val()];
        if(useRule == 1 || (selUnit != 1)){
            $("#applyLeaveList").find("input[name='newTimeLength']").val(isNaN(parseInt(inputVal))? "":parseInt(inputVal));
        }
    });

    $("#applyLeaveList").on("focus", "input[name='newTimeLength']", function(){
        var selUnit = $("#applyLeaveList").find("select[name='newTimeUnit']").val();
        var useRule = myObj.leaveOption[$("#applyLeaveList").find("select[name='newApplyType']").val()];
        if(useRule == 1 || selUnit !=1){
            $(this).attr("oninput", "value=value.replace(/[^\\d]/g,'')");   //只能整數
        }else{
            $(this).attr("oninput", "value=value.replace(/[^\\d.]/g,'');");  //可以有小數
        }
    });

});//.ready function


function init(){
    var date = new Date();
    var newDate = date.setDate(date.getDate() - 30);
    var dtStart = myObj.dateTimeFormat(newDate);
    $("#filter_sDate").val(dtStart.ymdHtml);
    var dtEnd = myObj.dateTimeFormat();
    $("#filter_eDate").val(dtEnd.ymdHtml);
    getApplyLeave();
    getLeaveOption();
}

function showApplyLeavePage(page=""){
    window.location.href = "/ApplyLeave/Index?page="+page;  
}


//#region applyLeave

function getApplyLeave(){
    var page = $("#applyLeaveDiv").length > 0 ? 0 : 1;
    var sDate = $("#filter_sDate").val();
    var eDate = $("#filter_eDate").val();
    if(((sDate == "" && eDate == "") || (sDate != "" && eDate != "")) && sDate <= eDate){
        var successFn = function(res){
            if(page==0){
                refreshApplyLeaveIng(res);
            }else{
                refreshApplyLeaveLog(res);
            } 
        };
        var data ={
            page, sDate, eDate
        };
        myObj.rAjaxFn("post", "/ApplyLeave/getMyApplyLeave", data, successFn);
    }else{
        alert("搜尋日期格式有誤");  return;
    }
}

function getLeaveOption(){
    var successFn = function(res){
        var sel = $(".template").find("[name='addApplyLeaveRow']").find("select[name='newApplyType']");
        myObj.leaveOption = {};
        res.forEach(function(value){
            $(sel).append(new Option(value.leaveName, value.id));
            myObj.leaveOption[value.id] = value.timeUnit;
        });
    };
    myObj.rAjaxFn("post", "/ApplyLeave/getLeaveOption", null, successFn);
}

function refreshApplyLeaveIng(res){
    $("#applyLeaveDiv").find("a.add_applyLeave").show();
    $('.btnActive').css('pointer-events', "");
    $("#applyLeaveList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='applyLeaveRow']").clone();
        var addTime = myObj.dateTimeFormat(value.createTime);
        var dateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
        dateTD.html(dateTD.html().replace(/\n/g, "<br/>"));
        row.find("input[name='applyTypeVal']").val(value.leaveID);
        row.find("[name='applyType']").text(value.leaveName);
        row.find("[name='note']").text(value.note);

        var sTime = myObj.dateTimeFormat(value.startTime);
        var sTimeTD = row.find("[name='startTime']").text(sTime.ymdText + "\n" + sTime.hmText);
        sTimeTD.html(sTimeTD.html().replace(/\n/g, "<br/>"));
        var eTime = myObj.dateTimeFormat(value.endTime);
        var eTimeTD = row.find("[name='endTime']").text(eTime.ymdText + "\n" + eTime.hmText);
        eTimeTD.html(eTimeTD.html().replace(/\n/g, "<br/>"));
        row.find("[name='endTime']").attr({"data-value":value.unitVal, "data-unit": value.unit});
    
        row.find(".edit_applyLeave").attr("onclick",`editApplyLeave(this, ${value.id});`);
        row.find(".del_applyLeave").attr("onclick",`delApplyLeave(${value.id});`);
        $("#applyLeaveList").append(row);
    });
}

function showAddApplyLeaveRow(){
    $("#applyLeaveDiv").find("a.add_applyLeave").hide();
    $('.btnActive').css('pointer-events', "none"); 
    var addApplyLeaveRow = $(".template").find("[name='addApplyLeaveRow']").clone();
    var dt = myObj.dateTimeFormat();
    addApplyLeaveRow.find("input[type='date']").val(dt.ymdHtml);
    addApplyLeaveRow.find("input[type='time']").val(dt.hour + ":00");
    addApplyLeaveRow.find("a.up_applyLeave").remove();
    addApplyLeaveRow.find("a.add_applyLeave").attr("onclick", "addUpApplyLeave(this);");
    addApplyLeaveRow.find("a.cel_applyLeave").attr("onclick", "cancelApplyLeave();");
    $('#applyLeaveList').append(addApplyLeaveRow);
}

function addUpApplyLeave(thisBtn, applyingID=0){
    var thisRow =  $(thisBtn).closest("tr[name='addApplyLeaveRow']");
    var timeObj = getApplyLeaveTime(thisRow);

    if(timeObj == null){
        alert("請假時間不符合請假規定"); return;
    }

    var data = {
        ID : applyingID,
        leaveID : timeObj.applyTypeVal,
        note : thisRow.find("[name='newApplyNote']").val(), 
        startTime : timeObj.startTime,
        unitVal: timeObj.inputVal,
        unit: timeObj.selUnit
    };
    
    var successFn = function(res){
        cancelApplyLeave();
    }
    myObj.cudAjaxFn("/ApplyLeave/addUpApplyLeave", data, successFn);
}

function cancelApplyLeave(){
    getApplyLeave();
}

function editApplyLeave(thisBtn, applyingID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='applyLeaveRow']").hide();
    var thisApplyDate = $(thisRow).find("[name='applyDate']").html();
    var thisApplyTypeVal = $(thisRow).find("input[name='applyTypeVal']").val();
    var thisNote = $(thisRow).find("[name='note']").text();
    var thisValue = $(thisRow).find("[name='endTime']").data("value");
    var thisUnit = $(thisRow).find("[name='endTime']").data("unit");
    var thisStartTime = $(thisRow).find("[name='startTime']").html();
    var thisEndTime = $(thisRow).find("[name='endTime']").html();

    var updateRow = $(".template").find("[name='addApplyLeaveRow']").clone();
    updateRow.find("[name='newApplyDate']").append(thisApplyDate);
    updateRow.find("select[name='newApplyType']").find(`option[value='${thisApplyTypeVal}']`).prop("selected", true);
    updateRow.find("[name='newApplyNote']").val(thisNote);
    updateRow.find("[name='newStartDate']").val((thisStartTime.split("<br>")[0]).replace(new RegExp("/", "g"), "-"));
    updateRow.find("[name='newStartTime']").val(thisStartTime.split("<br>")[1]);
    //updateRow.find("[name='newEndDate']").val((thisEndTime.split("<br>")[0]).replace(new RegExp("/", "g"), "-"));
    //updateRow.find("[name='newEndTime']").val(thisEndTime.split("<br>")[1]);

    updateRow.find("[name='newTimeLength']").val(thisValue);
    updateRow.find("select[name='newTimeUnit']").find(`option[value='${thisUnit}']`).prop("selected", true);

    updateRow.find("a.add_applyLeave").remove();
    updateRow.find("a.up_applyLeave").attr("onclick", `addUpApplyLeave(this, ${applyingID})`);
    updateRow.find("a.cel_applyLeave").attr("onclick", "cancelApplyLeave()");
    $(thisRow).after(updateRow);
}

function delApplyLeave(applyingID){
    var msg = "您真的確定要取消申請嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            cancelApplyLeave();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/ApplyLeave/delApplyLeave", {applyingID}, successFn);
}

function getApplyLeaveTime(thisRow){
    var sDate = thisRow.find("[name='newStartDate']").val();
    var sTime = thisRow.find("[name='newStartTime']").val();
    var startTime = (sDate + "T" + sTime);
    
    var applyTypeVal = thisRow.find("[name='newApplyType']").val();
    var useRule = myObj.leaveOption[applyTypeVal];
    var selUnit = thisRow.find("[name='newTimeUnit']").val();
    var inputVal = thisRow.find("input[name='newTimeLength']").val();

    if(inputVal=="" || isNaN(parseInt(inputVal)) || inputVal[0]=="." || inputVal % 0.5 !=0 || isNaN((new Date(startTime)).valueOf())){
        return null;
    }else if((useRule == 1 || selUnit !=1) && inputVal % 1 != 0){  //只能輸入整數
        return null;
    }
    return {applyTypeVal, startTime, inputVal, selUnit};
}


//#endregion applyLeave


//------------------------------------------------------------------------------------------------------------

//#region applyLog

function refreshApplyLeaveLog(res){
    $("#applyLeaveLogList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='applyLeaveLogRow']").clone();
        var addTime = myObj.dateTimeFormat(value.createTime);
        var dateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
        dateTD.html(dateTD.html().replace(/\n/g, "<br/>"));

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
        $("#applyLeaveLogList").append(row);
    });
}

//#endregion applyLog


