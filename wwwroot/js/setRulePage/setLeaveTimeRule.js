

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
    var leaveName = ["公差", "特休", "事假", "病假", "公假", "調休", "喪假", "婚假", "產假", "陪產假", "其他", "排休", "補休"];
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
            case 3: var unit = (value.halfVal==true? "半小時" : "小時"); break;
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
    addLeaveRow.find("span[name='chkHalf']").hide();
    addLeaveRow.find("a.update_leave").remove();
    addLeaveRow.find("a.create_leave").attr("onclick", "addUpLeave(this);");
    addLeaveRow.find("a.cancel_leave").attr("onclick", "cancelAddLeave(this);");
    $('#leaveList').append(addLeaveRow);
    $("#leaveTimeDiv").find("a.add_leave").hide();
    $('.btnActive').css('pointer-events', "none"); 
}

function addUpLeave(thisBtn, leaveID=0){
    var thisRow =  $(thisBtn).closest("tr[name='addLeaveRow']");
    var leaveName = thisRow.find("[name='newLeaveName']").val();
    leaveName = leaveID ==0? leaveName : thisRow.find("td[name='name']").text();
    var timeUnit = thisRow.find("[name='unit']").val();
    var halfVal = thisRow.find("[name='newChkHalf']").prop("checked");
    halfVal = timeUnit==3? halfVal : false;
    
    var data = {
        ID:leaveID, leaveName, timeUnit, halfVal
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
    var halfVal = unit=="半小時"? true : false;
    unit = unit=="半小時"? "小時" : unit;

    var updateLeaveRow = $(".template").find("[name='addLeaveRow']").clone();
    updateLeaveRow.find("td[name='name']").text(thisName);
    $.each((updateLeaveRow.find("select[name='unit']").find("option")), function(key, value){
        if($(this).text()==unit){
            $(this).prop("selected", true);
        }
    });
    if(unit == "小時"){
        updateLeaveRow.find("input[name='newChkHalf']").prop("checked", halfVal);
    }else{
        updateLeaveRow.find("span[name='chkHalf']").hide();
    }    
    updateLeaveRow.find("a.create_leave").remove();
    updateLeaveRow.find("a.update_leave").attr("onclick", `addUpLeave(this, ${leaveID})`);
    updateLeaveRow.find("a.cancel_leave").attr("onclick", "cancelAddLeave(this)");
    $(thisRow).after(updateLeaveRow);
}

//#endregion leaveTimeRule

