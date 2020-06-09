
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
        showMyAnnualLeave(res.myAnnualLeave);
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
    var hourToDay = 8;  //工作幾小時算一天
    var spDays = 0;
    var spHours = 0;
    res.forEach((value)=>{
        let remainDays = parseInt((value.remainHours)/hourToDay);
        let remainHours = (value.remainHours)%hourToDay;
        let dlDt = myObj.dateTimeFormat(value.deadLine);
        let deadLine = `${remainDays} 天`+ (remainHours>0? `又 ${remainHours} 小時於 `:"於 ") + `${dlDt.ymdText} 到期`;
        /*if(remainDays == 0 && remainHours == 0){
            return;     //期限還沒到但已沒有特休餘額 不顯示該列
        }*/
        $("[name='spLeaveDeadLine']").append(`<div>${deadLine}</div>`);
        spDays += remainDays;
        spHours += remainHours;
    });
    if(spHours >= hourToDay){
        spDays +=1;
        spHours -= hourToDay;
    }
    var spText = `${spDays} 天` + (spHours >0? `又 ${spHours} 小時` : "");
    $("[name='mySpLeave']").text(spText); 
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






