
var myObj = new MyObj();

$(document).ready(function() {  
    
    init();

    $("ul[group='table']").on("click", "a", function(){
        $(".add_timeRule").show();
        $('.btnActive').css('pointer-events', "");
    });

    $("#timeRuleDiv").on("input", "input[name='newElasticityMin']", function(){
        var val = $(this).val();
        val = val.replace(/[^\d]/g, ""); //把非數字的都替換掉，除了數字
        $(this).val(val);
    });
});//.ready function

function showSetRulePage(page){
    window.location.href = "/SetRule/Index?page="+page;  
}



function init(){

    if($("#timeRuleDiv").length > 0){
        getAllTimeRule();
    }else if($("#groupRuleDiv").length > 0){
        showGroupRule();
    }else if($("#specialDateDiv").length > 0){
        showSpecialDate();
        var successFn = function(res){
            res.forEach(function(value){
                $(".template").find("select[name='needClassVal']").append(new Option(value, value));
            });
        };
        myObj.rAjaxFn("get", "/SetRule/getClassDepart", null, successFn); 
    }else if($("#leaveTimeDiv").length >0){
        showLeaveRule();
    }else if($("#annualLeaveDiv").length >0) {
        showSpLeaveRule();
        setOption();
    }
}


//#region  timeRule

function showTimeRule(){
    $('.btnActive').css('pointer-events', "");
    $(".add_timeRule").show();
    getAllTimeRule();
}

function getAllTimeRule(){
    var successFn = function(res){
        refreshTimeRuleList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllTimeRule", null, successFn);
}

function refreshTimeRuleList(res){
    $("#timeRuleList").empty();
    res.forEach(function(value){
        myObj.workTimeSpanToTime(value);
        var row = $(".template").find("[name='timeRuleRow']").clone();
        row.find("[name='name']").text(value.name);
        row.find("[name='startTime']").text(value.startTime);
        row.find("[name='endTime']").text(value.endTime);
        row.find("[name='elasticityMin']").text(value.elasticityMin + "分鐘");
        row.find("[name='sRestTime']").text(value.sRestTime);
        row.find("[name='eRestTime']").text(value.eRestTime);
        row.find(".edit_timeRule").attr("onclick", `editTimeRule(this, ${value.id});`);
        row.find(".del_timeRule").attr("onclick", `delTimeRule(${value.id});`);
        $("#timeRuleList").append(row);
     });
}

function showAddTimeRuleRow(){
    $("#timeRuleDiv").find("a.add_timeRule").hide();
    $('.btnActive').css('pointer-events', "none"); 
    var addTimeRuleRow = $(".template").find("[name='addTimeRuleRow']").clone();
    var dt = myObj.dateTimeFormat();
    addTimeRuleRow.find("input[type='time']").val(dt.hmText);
    addTimeRuleRow.find("a.update_timeRule").remove();
    addTimeRuleRow.find("a.create_timeRule").attr("onclick", "addUpTimeRule(this);");
    addTimeRuleRow.find("a.cancel_timeRule").attr("onclick", "showTimeRule(this);");
    $('#timeRuleList').append(addTimeRuleRow);
}

function addUpTimeRule(thisBtn, ID=0){
    var thisRow =  $(thisBtn).closest("tr[name='addTimeRuleRow']");
    var name = thisRow.find("[name='newName']").val();
    var startTime = thisRow.find("[name='newStartTime']").val();
    var endTime = thisRow.find("[name='newEndTime']").val();
    var sRestTime = thisRow.find("[name='newSRestTime']").val();
    var eRestTime = thisRow.find("[name='newERestTime']").val();
    var elasticityMin = thisRow.find("[name='newElasticityMin']").val();

    if(elasticityMin >60 || isNaN(parseInt(elasticityMin))){
        alert("彈性時間不合法或者超過60分鐘");
        return;
    }
    if(startTime =="" || endTime == "" || sRestTime == "" || 
        eRestTime == "" || name == "" || elasticityMin=="" ){
        alert("欄位值不能為空");
        return;
    }
    var data = {
        ID, name, startTime, endTime, sRestTime, eRestTime, elasticityMin 
    };

    var successFn = function(res){
        showTimeRule();
    }
    myObj.cudAjaxFn("/SetRule/addUpTimeRule", data, successFn);
}

function delTimeRule(timeRuleID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showTimeRule();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delTimeRule", {timeRuleID}, successFn);
}

function editTimeRule(thisBtn, timeRuleID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='timeRuleRow']").hide();
    var thisName = thisRow.find("[name='name']").text();
    var thisStartTime = thisRow.find("[name='startTime']").text();
    var thisEndTime = thisRow.find("[name='endTime']").text();
    var thisSRestTime = thisRow.find("[name='sRestTime']").text();
    var thisERestTime = thisRow.find("[name='eRestTime']").text();
    var thisElasticityMin = thisRow.find("[name='elasticityMin']").text();
    thisElasticityMin = parseInt(thisElasticityMin);

    var updateRuleRow = $(".template").find("[name='addTimeRuleRow']").clone();
    updateRuleRow.find("input[name='newName']").val(thisName);
    updateRuleRow.find("input[name='newStartTime']").val(thisStartTime);
    updateRuleRow.find("input[name='newEndTime']").val(thisEndTime);
    updateRuleRow.find("input[name='newElasticityMin']").val(thisElasticityMin);
    updateRuleRow.find("input[name='newSRestTime']").val(thisSRestTime);
    updateRuleRow.find("input[name='newERestTime']").val(thisERestTime);
    updateRuleRow.find("a.create_timeRule").remove();
    updateRuleRow.find("a.update_timeRule").attr("onclick", `addUpTimeRule(this, ${timeRuleID})`);
    updateRuleRow.find("a.cancel_timeRule").attr("onclick", `showTimeRule(this)`);

    $(thisRow).after(updateRuleRow);
}

//#endregion  timeRule

//--------------------------------------------------------------------------------------------------------------------------------

//#region groupRule

function showGroupRule(){
    $('.btnActive').css('pointer-events', "");
    $(".add_group").show();
    getAllGroup();
}

function getAllGroup(){
    var successFn = function(res){
        refreshGroupList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllGroup", null, successFn);
}

function refreshGroupList(res){
    $("#groupList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='groupRow']").clone();
        var chkBox = $(".template").find("div[name='chkBox']").clone();
        row.find("[name='groupName']").text(value.groupName);
        var para = value.ruleParameter;
        var allChkBox = chkBox.find("input[type='checkbox']");
        $.each(allChkBox, function(key, obj){
            var thisVal = $(obj).val();
            if(para & thisVal){
                $(obj).prop("checked", "checked");
            }
            $(obj).prop("disabled", "disabled");
        });
        row.find("[name='groupAuthority']").append(chkBox);
        row.find(".edit_group").attr("onclick", `editGroup(this, ${value.id});`);
        row.find(".del_group").attr("onclick", `delGroup(${value.id});`);
        $("#groupList").append(row);
     });
}

function showAddGroupRow(){
    $("#groupRuleDiv").find("a.add_group").hide();
    $('.btnActive').css('pointer-events', "none"); 
    var addGroupRow = $(".template").find("[name='addGroupRow']").clone();
    var chkBox = $(".template").find("div[name='chkBox']").clone();
    addGroupRow.find("[name='newGroupAuthority']").append(chkBox);
    addGroupRow.find("a.update_group").remove();
    addGroupRow.find("a.create_group").attr("onclick", "addGroup(this);");
    addGroupRow.find("a.cancel_group").attr("onclick", "cancelAddGroup(this);");
    $('#groupList').append(addGroupRow);
}

function addGroup(thisBtn){
    var thisRow =  $(thisBtn).closest("tr[name='addGroupRow']");
    var groupName = thisRow.find("[name='newGroupName']").val();
    var chkBox = thisRow.find("input[type='checkbox']:checked");
    var paraVal = 0x0000;
    $.each(chkBox, function(key, obj){
        paraVal = paraVal | $(obj).val();
    });

    if(groupName == ""){
        alert("群組名稱不可為空");
        return;
    }
    var data = {
        groupName : groupName,
        ruleParameter : paraVal,
    };

    var successFn = function(res){
        showGroupRule();
    }
    myObj.cudAjaxFn("/SetRule/addGroup", data, successFn);
}

function cancelAddGroup(thisBtn){
    showGroupRule();
}

function delGroup(groupID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showGroupRule();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delGroup",{groupID},successFn);
}

function editGroup(thisBtn, groupID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='groupRow']").hide();
    var thisGroupName = thisRow.find("[name='groupName']").text();
    var chkBox = thisRow.find("div[name='chkBox']").clone();
    $.each(chkBox.find("[type='checkbox']"), function(key, obj){
        $(obj).prop("disabled", false);
    });

    var updateGroupRow = $(".template").find("[name='addGroupRow']").clone();
    updateGroupRow.find("input[name='newGroupName']").val(thisGroupName);
    updateGroupRow.find("[name='newGroupAuthority']").append(chkBox);

    updateGroupRow.find("a.create_group").remove();
    updateGroupRow.find("a.update_group").attr("onclick", `updateGroup(this, ${groupID})`);
    updateGroupRow.find("a.cancel_group").attr("onclick", `cancelAddGroup(this)`);

    $(thisRow).after(updateGroupRow);
}

function updateGroup(thisBtn, groupID){
    var thisRow =  $(thisBtn).closest("tr[name='addGroupRow']");
    var groupName = thisRow.find("[name='newGroupName']").val();
    var chkBox = thisRow.find("input[type='checkbox']:checked");
    var paraVal = 0x0000;
    $.each(chkBox, function(key, obj){
        paraVal = paraVal | $(obj).val();
    });

    if(groupName == ""){
        alert("群組名稱不可為空");
        return;
    }
    var data = {
        ID : groupID,
        groupName : groupName,
        ruleParameter : paraVal,
    };
    
    var successFn = function(res){
        showGroupRule();
    }
    myObj.cudAjaxFn("/SetRule/updateGroup", data, successFn);
}

//#endregion groupRule

//-----------------------------------------------------------------------------------------------------------------------------------------

//#region specialDate

function showSpecialDate(){
    $('.btnActive').css('pointer-events', "");
    $(".add_spDate").show();
    getAllSpecialDate();
}

function getAllSpecialDate(){
    var successFn = function(res){
        refreshSpDateList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllSpecialDate", null, successFn);
}

function refreshSpDateList(res){
    $("#specialDateList").empty();
    res.forEach(function(value){
        var dt = myObj.dateTimeFormat(value.date);
        var row = $(".template").find("[name='specialDateRow']").clone();
        row.find("[name='date']").text(dt.ymdText + " " + dt.worldWeek);
        row.find("[name='needClass']").text(value.departClass);
        row.find("input[name='statusVal']").val(value.status);
        row.find("[name='status']").text((value.status == 1? "休假" : "上班"));
        row.find("[name='note']").text(value.note);
        row.find(".edit_spDate").attr("onclick", `editSpecialDate(this, ${value.id});`);
        row.find(".del_spDate").attr("onclick", `delSpecialDate(${value.id});`);
        $("#specialDateList").append(row);
     });
}

function showAddSpDateRow(){
    $("#specialDateDiv").find("a.add_spDate").hide();
    $('.btnActive').css('pointer-events', "none"); 
    var addSpDateRow = $(".template").find("[name='addSpecialDateRow']").clone();
    var dt = myObj.dateTimeFormat();
    addSpDateRow.find("input[type='date']").val(dt.ymdHtml);
    addSpDateRow.find("a.update_spDate").remove();
    addSpDateRow.find("a.create_spDate").attr("onclick", "addUpSpecialDate(this);");
    addSpDateRow.find("a.cancel_spDate").attr("onclick", "celSpecialDate(this);");
    $('#specialDateList').append(addSpDateRow);
}

function celSpecialDate(){
    showSpecialDate();
}

function delSpecialDate(spDateID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showSpecialDate();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delSpecialDate", {spDateID}, successFn);
}

function editSpecialDate(thisBtn, spDateID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='specialDateRow']").hide();
    var thisDate = thisRow.find("[name='date']").text();
    var dateVal = thisDate.split(" ")[0].replace(new RegExp("/", "g"), "-");
    var thisClass = thisRow.find("[name='needClass']").text();
    var thisStatus = thisRow.find("input[name='statusVal']").val();
    var thisNote = thisRow.find("[name='note']").text();

    var updateSpDate = $(".template").find("[name='addSpecialDateRow']").clone();
    updateSpDate.find("input[name='newDate']").val(dateVal);
    updateSpDate.find("select[name='needClassVal']").find(`option[value='${thisClass}']`).prop("selected", true);
    updateSpDate.find("select[name='status']").find(`option[value='${thisStatus}']`).prop("selected", true);
    updateSpDate.find("input[name='newNote']").val(thisNote);

    updateSpDate.find("a.create_spDate").remove();
    updateSpDate.find("a.update_spDate").attr("onclick", `addUpSpecialDate(this, ${spDateID})`);
    updateSpDate.find("a.cancel_spDate").attr("onclick", "celSpecialDate()");

    $(thisRow).after(updateSpDate);
}

function addUpSpecialDate(thisBtn, spDateID=0){
    var thisRow =  $(thisBtn).closest("tr[name='addSpecialDateRow']");
    var date = thisRow.find("[name='newDate']").val();
    var departClass = thisRow.find("select[name='needClassVal']").val();
    var status = thisRow.find("select[name='status']").val();
    var note = thisRow.find("[name='newNote']").val();

    if(date == "" || status ==""){
        alert("日期與狀態欄位不可為空");
        return;
    }
    var data = {
        ID : spDateID,
        date : date,
        departClass : departClass,
        status : status,
        note : note,
    };

    var successFn = function(res){
        showSpecialDate();
    };
    myObj.cudAjaxFn("/SetRule/addUpSpecialTime", data, successFn);
}

//#endregion specialDate

//---------------------------------------------------------------------------------------------------------------------

//#region leaveTimeRule

function showLeaveRule(){
    $('.btnActive').css('pointer-events', "");
    $(".add_leave").show();
    getAllLeaveRule();
}

function getAllLeaveRule(){
    var successFn = function(res){
        refreshLeaveRuleList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllLeaveRule", null, successFn);
}

function refreshLeaveRuleList(res){
    $("#leaveList").empty();
    var leaveName = ["公差", "特休", "事假", "病假", "公假", "調休", "喪假", "婚假", "產假", "陪產假", "其他"];
    res.forEach(function(value){
        if(!value.enable){
            return;
        }
        var row = $(".template").find("[name='leaveRow']").clone();
        row.find("[name='name']").text(value.leaveName);
        leaveName = leaveName.filter(function(val, key){return val != value.leaveName});
        switch(value.timeUnit){
            case 1: var unit = "全天"; break;
            case 2: var unit = "半天"; break;
            case 3: var unit = "小時"; break;
            default : var unit =""; break;
        }
        row.find("[name='timeUnit']").text(unit);
        row.find(".edit_leave").attr("onclick", `editLeave(this, ${value.id});`);
        row.find(".del_leave").attr("onclick", `delLeave(${value.id});`);
        $("#leaveList").append(row);
    });
    var sel = $(".template").find("select[name='newLeaveName']").empty();
    leaveName.forEach((val)=>{
        sel.append(new Option(val, val));
    });
}

function showAddLeaveRow(){
    var addLeaveRow = $(".template").find("[name='addLeaveRow']").clone();
    if(addLeaveRow.find("select[name='newLeaveName']").find("option").length == 0){
        alert("已無可新增的項目");
        return;
    }
    addLeaveRow.find("a.update_leave").remove();
    addLeaveRow.find("a.create_leave").attr("onclick", "addUpLeave(this);");
    addLeaveRow.find("a.cancel_leave").attr("onclick", "cancelAddLeave(this);");
    $('#leaveList').append(addLeaveRow);
    $("#leaveTimeDiv").find("a.add_leave").hide();
    $('.btnActive').css('pointer-events', "none"); 
}

function addUpLeave(thisBtn, leaveID=0){
    var thisRow =  $(thisBtn).closest("tr[name='addLeaveRow']");
    var name = thisRow.find("[name='newLeaveName']").val();
    var timeUnit = thisRow.find("[name='unit']").val();

    var data = {
        ID: leaveID,
        leaveName : name,
        timeUnit : timeUnit
    };
    var successFn = function(res){
        showLeaveRule();
    };
    myObj.cudAjaxFn("/SetRule/addUpLeave", data, successFn);
}

function cancelAddLeave(thisBtn){
    showLeaveRule();
}

function delLeave(leaveID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showLeaveRule();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delLeave", {leaveID}, successFn);
}

function editLeave(thisBtn, leaveID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='leaveRow']").hide();
    var thisName = thisRow.find("[name='name']").text();
    var unit = thisRow.find("[name='timeUnit']").text();

    var updateLeaveRow = $(".template").find("[name='addLeaveRow']").clone();
    updateLeaveRow.find("select[name='newLeaveName']").prepend(new Option(thisName, thisName));
    updateLeaveRow.find("select[name='newLeaveName']").find(`option[value='${thisName}']`).prop("selected", true);
    $.each((updateLeaveRow.find("select[name='unit']").find("option")), function(key, value){
        if($(this).text()==unit){
            $(this).prop("selected", true);
        }
    });
    updateLeaveRow.find("a.create_leave").remove();
    updateLeaveRow.find("a.update_leave").attr("onclick", `addUpLeave(this, ${leaveID})`);
    updateLeaveRow.find("a.cancel_leave").attr("onclick", "cancelAddLeave(this)");

    $(thisRow).after(updateLeaveRow);
}

//#endregion leaveTimeRule

//------------------------------------------------------------------------------------------------

//#region spLeaveRule

function setOption(){
    var yearSel = $(".template").find("select[name='yearSel']").append(new Option("半年", 0.5));
    var spDaySel = $(".template").find("select[name='daySel']");
    for(let i=1; i<=30; i++){
        //yearSel.append(new Option(`${i}年`, i));
        spDaySel.append(new Option(`${i}天`, i));
    }

    var buffSel = $(".template").find("select[name='buffDaySel']").append(new Option("0天", 0));
    for(let i=1; i<=12; i++){
        buffSel.append(new Option(`${i}個月`, i));
    }
    buffSel.append(new Option("無期限", 1200)); //100year = 1200month
}

function showSpLeaveRule(){
    $('.btnActive').css('pointer-events', "");
    $(".add_spLeave").show();
    getAllSpLeaveRule();
}

function getAllSpLeaveRule(){
    var successFn = function(res){
        refreshSpLeaveRuleList(res);
    };
    myObj.rAjaxFn("get", "/SetRule/getAllSpLeaveRule", null, successFn);
}

function refreshSpLeaveRuleList(res){
    var seniority = [0.5];
    for(let i =1; i<=30;i++){
        seniority.push(i);
    }
    $("#spLeaveRuleList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='spLeaveRow']").clone();
        var yText = value.seniority == 0.5 ? "6個月" : value.seniority + "年";
        seniority = seniority.filter(function(val, key){return val != value.seniority});
        row.find("[name='years']").text(yText);
        row.find("[name='days']").text(value.specialDays + "天");
        var buffTest = (value.buffDays/30) + "個月";
        buffTest = value.buffDays==0? "0天" : value.buffDays==36000 ? "無期限" : buffTest;
        row.find("[name='buffDays']").text(buffTest);
        row.find(".edit_spLeave").attr("onclick",`editSpLeaveRule(this, ${value.id});`);
        row.find(".del_spLeave").attr("onclick",`delSpLeaveRule(${value.id});`);
        $("#spLeaveRuleList").append(row);
    });
    var yearSel = $(".template").find("select[name='yearSel']").empty();
    seniority.forEach((val)=>{
        if(val==0.5){
            yearSel.append(new Option("6個月", val));
        }else{
            yearSel.append(new Option(val+"年", val));
        }
    });
}

function showAddSpLeaveRow(){
    var addSpLeaveRow = $(".template").find("[name='addSpLeaveRow']").clone();
    if(addSpLeaveRow.find("select[name='yearSel']").find("option").length == 0){
        alert("已無可新增的項目");
        return;
    }
    addSpLeaveRow.find("a.update_spLeave").remove();
    addSpLeaveRow.find("a.create_spLeave").attr("onclick", "addUpSpLeaveRule(this);");
    addSpLeaveRow.find("a.cancel_spLeave").attr("onclick", "cancelAddSpLeave(this);");
    $("#annualLeaveDiv").find("a.add_spLeave").hide();
    $('.btnActive').css('pointer-events', "none"); 
    $('#spLeaveRuleList').append(addSpLeaveRow);
}

function addUpSpLeaveRule(thisBtn, ruleID=0){
    var thisRow = $(thisBtn).closest("tr[name='addSpLeaveRow']");
    var seniority = $(thisRow).find("select[name='yearSel']").val();
    var specialDays = $(thisRow).find("select[name='daySel']").val();
    var buffDays = $(thisRow).find("select[name='buffDaySel']").val();
    buffDays*=30;

    var data={
        ID:ruleID, seniority, specialDays, buffDays
    };

    var successFn = ()=>{
        showSpLeaveRule();
    }
    myObj.cudAjaxFn("/SetRule/addUpSpLeaveRule", data, successFn);
}

function cancelAddSpLeave(thisBtn){
    showSpLeaveRule();
}

function editSpLeaveRule(thisBtn, ruleID){
    $('.btnActive').css('pointer-events', "none");

    var thisRow = $(thisBtn).closest("tr[name='spLeaveRow']").hide();
    var thisYear = thisRow.find("[name='years']").text();
    var thisYearVal = thisYear == "6個月"? 0.5 : thisYear.substring(0, thisYear.length-1);
    var thisSpDay = thisRow.find("[name='days']").text();
    var thisBuffDay = thisRow.find("[name='buffDays']").text();

    var upSpLeaveRow = $(".template").find("[name='addSpLeaveRow']").clone();
    upSpLeaveRow.find("select[name='yearSel']").prepend(new Option(thisYear, thisYearVal));
    $.each(upSpLeaveRow.find("[name='yearSel']").find("option"), function(){
        if($(this).text() == thisYear){
            $(this).prop("selected", true);
        }
    });
    $.each(upSpLeaveRow.find("[name='daySel']").find("option"), function(){
        if($(this).text() == thisSpDay){
            $(this).prop("selected", true);
        }
    });
    $.each(upSpLeaveRow.find("[name='buffDaySel']").find("option"), function(){
        if($(this).text() == thisBuffDay){
            $(this).prop("selected", true);
        }
    });

    upSpLeaveRow.find("a.create_spLeave").remove();
    upSpLeaveRow.find("a.update_spLeave").attr("onclick", `addUpSpLeaveRule(this, ${ruleID})`);
    upSpLeaveRow.find("a.cancel_spLeave").attr("onclick", "cancelAddSpLeave(this)");
    $(thisRow).after(upSpLeaveRow);
}

function delSpLeaveRule(ruleID){
    var msg = "您真的確定要刪除嗎？\n\n請確認！";
    if(confirm(msg)==false) 
        return;
    var successFn = function(res){
        if(res > 0){
            showSpLeaveRule();
        }else{
            alert('fail');
        }     
    };
    myObj.cudAjaxFn("/SetRule/delSpLeaveRule", {ruleID}, successFn);
}

//#endregion spLeaveRule

