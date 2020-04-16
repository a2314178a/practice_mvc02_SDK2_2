
var myObj = new MyObj();

$(document).ready(function() {  

    showTime();
    getTodayPunchStatus();

    $("#punchCardBtn").on("click", function(){
        punchCardFn();
    });

    $("input[name='punchOption']").on("click", function(){
        chkPunchOptionStatus();
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
    $("#punchCardBtn").addClass("btn-primary").removeClass("btn-success");
    $("#punchCardBtn").css('pointer-events', "").text("打卡");
    $("#punchTimeText").text("");
    $("#rePunchBtn").hide();
    var optionVal = $('input[name=punchOption]:checked').val();
    if(optionVal == 1){
        if(myObj.onlineStatus){
            $("#punchCardBtn").addClass("btn-success").removeClass("btn-primary");
            $("#punchCardBtn").css('pointer-events', "none").text("上班已打卡");
            $("#punchTimeText").text("打卡時間 : " + myObj.onlineTime);
        }
    }else if(optionVal == 0){
        if(myObj.offlineStatus){
            $("#punchCardBtn").addClass("btn-success").removeClass("btn-primary");
            $("#punchCardBtn").text("");
            var msg = "下班已打卡<br/><span style='font-size:10px'>(點選再次打卡)</span>";
            var btn = $("#punchCardBtn").append(msg);
            //btn.html(btn.html().replace(/\n/g, "<br/>"));
            $("#punchTimeText").text("打卡時間 : " + myObj.offlineTime);
            //$("#rePunchBtn").show();
        }
    }
}

function punchCardFn(){
    var optionVal = $('input[name=punchOption]:checked').val();
    if(isNaN(optionVal) || (optionVal == 1 && myObj.onlineStatus)){
        alert("請點選打卡選項");
        return;
    }
    
    var successFn = function(res){
        switch(res){
            case 1: getTodayPunchStatus(); break;
            case 2: alert("上班已打卡"); break;
            case 3: alert("現在時段不能打下班卡"); break;
            case 4: alert("不能補打上班時間"); break;
        }
    }
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