

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

