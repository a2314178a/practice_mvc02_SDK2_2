
var myObj = new MyObj();

$(document).ready(function() {  

    showTime();
    getTodayPunchStatus();

    $("#punchCardBtn").on("click", function(){
        punchCardFn();
    });

});//.ready function

function showPunchCardPage(){
    window.location.href = "/PunchCard/";
}
function showPunchLogPage(targetID){
    window.location.href = "/PunchCard/Index?page=log&target="+targetID;
}
function showTimeTotalPage(targetID){
    window.location.href = "/PunchCard/Index?page=total&target="+targetID;
}


function showTime(){
    var dt = myObj.dateTimeFormat();
    var dateText = dt.year + "年" + dt.month + "月" + dt.day + "日" + " " + dt.twWeek;
    var timeText = dt.hour + '時' + dt.minute + '分' + dt.second + '秒';
    $("#dateYMDW").text(dateText);
    $("#dateHMS").text(timeText);
    setTimeout("showTime()", 1000);
}


function getTodayPunchStatus(){
    var successFn = function(res){
        myObj.onlineStatus = false;
        myObj.offlineStatus = false;
        if(res.onlineTime){
            myObj.onlineStatus = true;
            var dt = myObj.dateTimeFormat(res.onlineTime);
            var dateTimeText = dt.year + "-" + dt.month + "-" + dt.day + " " + dt.hour + ":" + dt.minute + ":" + dt.second;
            myObj.onlineTime = dateTimeText;
        }
        if(res.offlineTime){
            myObj.offlineStatus = true;
            var dt = myObj.dateTimeFormat(res.offlineTime);
            var dateTimeText = dt.year + "-" + dt.month + "-" + dt.day + " " + dt.hour + ":" + dt.minute + ":" + dt.second;
            myObj.offlineTime = dateTimeText;
        }
        chkPunchOptionStatus();
    }
    myObj.rAjaxFn("get", "/PunchCard/getTodayPunchStatus", null, successFn);
}

function chkPunchOptionStatus(){
    $("#punchTimeText_on,#punchTimeText_off").text("");
    $("#punchCardBtn").text(myObj.onlineStatus? "打下班卡" : "打上班卡");
    if(myObj.onlineStatus){
        $("#punchTimeText_on").text("今日上班打卡時間 : " + myObj.onlineTime);
    }else{
        $("#punchTimeText_on").text("今日上班打卡時間 : 尚未打卡");
    }
    if(myObj.offlineStatus){
        $("#punchTimeText_off").text("今日下班打卡時間 : " + myObj.offlineTime);
    }else{
        $("#punchTimeText_off").text("今日下班打卡時間 : 尚未打卡");
    }
}

function punchCardFn(){
    if(!myObj.onlineStatus){
        var optionVal = 1;
    }else{
        var optionVal = 0;
    }
    var successFn = function(res){
        switch(res){
            case 1: getTodayPunchStatus(); break;
            case 2: alert("上班已打卡"); break;
            case 3: alert("現在時段不能打下班卡"); break;
            case 4: alert("不能補打上班時間"); break;
        }
    };
    myObj.cudAjaxFn("/PunchCard/addPunchCardLog", {action: optionVal}, successFn);
}

function rePunchCardFn(){
    var optionVal = $('input[name=punchOption]:checked').val();
    if(isNaN(optionVal) || optionVal == 1 || !myObj.offlineStatus){
        alert("打卡操作有誤");
        return;
    }
    
    var successFn = function(res){
        switch(res){
            case 1: getTodayPunchStatus(); break;
        }
    }
    myObj.cudAjaxFn("/PunchCard/addPunchCardLog", {action: optionVal}, successFn);
}