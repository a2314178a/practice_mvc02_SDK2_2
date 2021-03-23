
var myObj = new MyObj();

$(document).ready(function() {  
    init();

    $("ul[group='table']").on("click", "a", function(){
        $(".add_depart").show();
        $('.btnActive').css('pointer-events', "");
    });

    $("#oneKeyList").on("input", "input[type='text']", function(){ 
        var val = $(this).val();
        val = val.replace(/[^\d]/g, ""); //把非數字的都替換掉
        $(this).val(val);
    });

});//.ready function

function showAdminFnPage(page){
    window.location.href = "/AdminFn/Index?page="+page;  
}

function init(){

    if($("#operateLogDiv").length > 0){
        getFilterOption();
    }
}

function getFilterOption(){
    var dt = myObj.dateTimeFormat();
    $("#startDate,#endDate").val(dt.ymdHtml);
    var successFn = (res)=>{
        setFilterOption(res);
    };
    myObj.rAjaxFn("get", "/AdminFn/getFilterOption", null, successFn);
}

function setFilterOption(res){
    res.category.forEach((value)=>{
        $("#selCategory").append(new Option(value, value));
    });
    res.userName.forEach((value)=>{
        $("#selOpName,#selEmName").append(new Option(value.userName, value.id));
    });
}

function getOperateLog(){
    var sDate = $("#startDate").val();
    var eDate = $("#endDate").val();
    var active = $("#selActive").val();
    var category = $("#selCategory").val();
    var opID = $("#selOpName").val();
    var emID = $("#selEmName").val();
    var filter = {
        sDate, eDate, active, category, opID, emID, 
    };
    var successFn = function(res){
        refreshOpLogList(res);
    };
    myObj.rAjaxFn("get", "/AdminFn/getOperateLog", filter, successFn);
}

function refreshOpLogList(res){
    $("#opLogList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='opLogRow']").clone();
        row.find("[name='opTime']").text(value.opTime);
        row.find("[name='active']").text(value.active);
        row.find("[name='category']").text(value.category);
        row.find("[name='opName']").text(value.opName);
        row.find("[name='emName']").text(value.emName);
        row.find("[name='content']").text(value.content);
        $("#opLogList").append(row);
     });
}

function punchCheck(){
    var successFn = function(res){
        alert((res===1)? "成功" : "失敗");
    };
    myObj.rAjaxFn("get", "/AdminFn/manual_refreshPunchLogWarn", {}, successFn);
}

function countWorkTime(){
    var successFn = function(res){
        alert((res===1)? "成功" : "失敗");
    };
    myObj.rAjaxFn("get", "/AdminFn/manual_calWorkTime", {}, successFn);
}

function annualCheck(){
    var successFn = function(res){
        alert((res===1)? "成功" : "失敗");
    };
    myObj.rAjaxFn("get", "/AdminFn/manual_calAnnualLeave", {}, successFn);
}

function delDataBaseOldLog(sel){
    var delMonth = 0;
    var url = "";
    var confirmMsg = "";
    switch (sel) {
        case 1:
            delMonth = $("input[name='delApplyLeaveMonth']").val(); 
            url = "/AdminFn/clearEmployeeLeaveOfficeApply";
            confirmMsg = "您真的確定要清除 請假紀錄 嗎？\n\n請確認！";
            break;
        case 2:
            delMonth = $("input[name='delPunchCardLogMonth']").val(); 
            url = "/AdminFn/clearPunchCardLogs";
            confirmMsg = "您真的確定要清除 打卡紀錄 嗎？\n\n請確認！";
            break;   
        case 3:
            delMonth = $("input[name='delOperateLogMonth']").val(); 
            url = "/AdminFn/clearOperateLogs";
            confirmMsg = "您真的確定要清除 操作紀錄 嗎？\n\n請確認！";
            break;   
        case 4:
            delMonth = $("input[name='delAnnualDeadLineMonth']").val(); 
            url = "/AdminFn/clearEmployeeAnnualLeaves";
            confirmMsg = "您真的確定要清除 已過期的特休 嗎？\n\n請確認！";
            break; 
        default:return; 
    }
    if(confirm(confirmMsg)==false){
        return;
    }
    delMonth = delMonth == ""? 12 : parseInt(delMonth);
    if(delMonth <=5){
        alert("該參數值須大於等於6"); return;
    }
    var successFn = function(res){
        if(res.statusCode===1){
            alert(`已完成，共刪除 ${res.count} 條紀錄`);
        }else{
            alert("清除失敗");
        }
    };
    myObj.rAjaxFn("post", url, {delMonth}, successFn);
}

function delUselessMessage(){
    var confirmMsg = "您真的確定要清除 訊息 嗎？\n\n請確認！";
    if(confirm(confirmMsg)==false){
        return;
    }
    var successFn = function(res){
        if(res.statusCode===1){
            alert(`已完成，共刪除 ${res.count} 條紀錄`);
        }else{
            alert("清除失敗");
        }
    };
    myObj.rAjaxFn("get", "/AdminFn/clearMessageAndMsgSendReceive", {}, successFn);
}