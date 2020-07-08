

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
        row.find("[name='type']").text(value.type==1? "排休制":"固定制");
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
    var type = thisRow.find("[name='newType']").val();
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
        ID, name, type, startTime, endTime, sRestTime, eRestTime, elasticityMin 
    };

    var successFn = function(res){
        if(res == "same"){
            alert("注意! 已有相同的上班與下班時間");
        }
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
    var thisType = thisRow.find("[name='type']").text();
    thisType = (thisType =="排休制"? 1: 0);
    var thisStartTime = thisRow.find("[name='startTime']").text();
    var thisEndTime = thisRow.find("[name='endTime']").text();
    var thisSRestTime = thisRow.find("[name='sRestTime']").text();
    var thisERestTime = thisRow.find("[name='eRestTime']").text();
    var thisElasticityMin = thisRow.find("[name='elasticityMin']").text();
    thisElasticityMin = parseInt(thisElasticityMin);

    var updateRuleRow = $(".template").find("[name='addTimeRuleRow']").clone();
    updateRuleRow.find("input[name='newName']").val(thisName);
    updateRuleRow.find("select[name='newType']").find(`option[value='${thisType}']`).prop("selected", true);
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

