
var myObj = new MyObj();

$(document).ready(function() {  

    init();

});//.ready function

function init(){
    myObj.agent = {myAgentID: 0, agentEnable: false};
    $("[group='canEdit']").css("display","none");
    if($("select[name='agent']").length > 0){
        getSelOption();
    }else{
        getMyDetail();
    }
}

function getSelOption(){
    var successFn = function(res){
        setAgent(res);
        getMyDetail();
    };
    myObj.rAjaxFn("get", "/EmployeeDetail/getSelOption", null, successFn);
}

function setAgent(res){
    var agent = $("select[name='agent']");
    res.forEach(function(value){
        agent.append(new Option(value.userName, value.id));
    });
}

function getMyDetail(){
    var getAccInfoSuccessFn = function(res){
        if(res.myDetail != null)
            refreshMyDetail(res.myDetail);
        showMyAnnualLeave(res);
    };
    myObj.rAjaxFn("get", "/EmployeeDetail/getMyDetail", null, getAccInfoSuccessFn);
}

function refreshMyDetail(res){
    $("[name='userName']").text(res.userName);
    $("[name='sex']").text((res.sex)==0? "女" : "男");
    $("[name='birthday']").text((res.birthday).split("T")[0]);
    var birth = myObj.dateTimeFormat(res.birthday);
    var now = myObj.dateTimeFormat();
    var birthMonth = parseInt(birth.month+birth.day);
    var nowMonth = parseInt(now.month+now.day);
    var age = (nowMonth >= birthMonth)? (now.year - birth.year) : (now.year - birth.year)-1;
    $("[name='age']").text(age);
    $("[name='department']").text(res.department);
    $("[name='position']").text(res.position);
    $("[name='humanID']").text(res.humanID);
    $("[name='startWorkDate']").text((res.startWorkDate).split("T")[0]);
    $("select[name='agent']").find(`option[value='${res.myAgentID}']`).prop("selected", true);
    $("input[name='agentEnable']").prop("checked", res.agentEnable);
    myObj.agent = {myAgentID: res.myAgentID, agentEnable: res.agentEnable};
    hideRow();
}

function hideRow(){
    $("[group='canEdit']").hide();
    $("#pwRow").find("input").val("");
    $("select[name='agent']").find(`option[value='${myObj.agent.myAgentID}']`).prop("selected", true);
    $("input[name='agentEnable']").prop("checked", myObj.agent.agentEnable);
}

function showRow(sel){
    if(sel == 1){
        $("#pwRow,#btnDiv").show();
    }else if(sel ==2){
        $("#agentRow,#btnDiv").show();
    }
}

function showMyAnnualLeave(res){
    $("[name='spLeaveDeadLine']").empty();
    var unit = res.annualLeaveUnit["timeUnit"];
    var halfVal = res.annualLeaveUnit["halfVal"];
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    var sumSpDays = 0;
    var sumSpHours = 0;
    res.myAnnualLeave.forEach((value)=>{
        var dlDt = myObj.dateTimeFormat(value.deadLine);

        if(unit == 1 || unit == 2){ //1:天 2:半天
            if(unit == 1){
                var remainDays = parseInt((value.remainHours)/hourToDay);
                var otherHours = (value.remainHours)%hourToDay;
            }else{
                var remainDays = parseInt((value.remainHours)/(hourToDay*0.5)); //mod 半天(4hour)
                remainDays *= 0.5;  //轉成天 ex:37hours /4 = 9個半天 ...9/2 = 4.5天
                var otherHours = (value.remainHours)%(hourToDay*0.5);
            }
            sumSpDays += remainDays;
            sumSpHours += otherHours;
            var txt = `${remainDays} 天` + (otherHours>0? ` ${otherHours}小時` : "") + `於 ${dlDt.ymdText} 到期`;
        }else{
            var remainHours = value.remainHours;
            var txt = `${remainHours} 小時於 ${dlDt.ymdText} 到期`;
            sumSpHours += remainHours;
        }
        $("[name='spLeaveDeadLine']").append(`<div>${txt}</div>`);
    });

    if(unit==1 || unit==2){
        var sumSpText = `${sumSpDays} 天` + (sumSpHours >0? ` ${sumSpHours} 小時` : "");
    }else{
        var sumSpText = `${sumSpHours} 小時`;
    }
    $("[name='mySpLeave']").text(sumSpText); 
}


function updateMyDetail(){
    var pw = $("[name='password']").val();
    var rePw = $("[name='rePassword']").val();
    var myAgentID = $("select[name='agent']").length >0? $("select[name='agent']").val() : 0;
    var agentEnable = $("input[name='agentEnable']").length>0? $("input[name='agentEnable']").prop("checked") : false;
    if(pw == "" && rePw =="" && myAgentID == myObj.agent.myAgentID && agentEnable == myObj.agent.agentEnable){
        hideRow();
        return;
    }
    if(pw != rePw){
        alert("密碼並不一致，請重新輸入");return;
    }

    var data = {password: pw, myAgentID, agentEnable};
    var successFn = ()=>{
        $("#pwRow,#btnDiv").hide();
        $("#agentRow,#btnDiv").hide();
        getMyDetail();
    };
    myObj.cudAjaxFn("/EmployeeDetail/updateMyDetail", data, successFn);
}






