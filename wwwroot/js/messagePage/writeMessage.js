


//#region write message

function getReceiveOption(){
    var successFn = function(res){
        refreshSelOption(res);
    };
    myObj.rAjaxFn("post", "/Message/getReceiveOption", null, successFn);
}

function refreshSelOption(res){
    var departName = "";
    myObj.departAndEmployee = res;
    res.forEach(function(value){
        if(value.department != departName){
            $("#selReceiveDepart").append(new Option(value.department, value.department));
            departName = value.department;
        }
    });
}

function setReceiveOption(departSel){
    myObj.departAndEmployee.forEach(function(value){
        if(value.department == departSel && value.userName != null){
            $("#selReceiveID").append(new Option(value.userName, value.accID));
        }
    });
}

function sendMsg(){
    var title = $("#msgTitle").val();
    var content = $("#content").val();
    var depart = $("#selReceiveDepart").val();
    var receiveID = $("#selReceiveID").val();
    
    if((depart == "" || depart != -1) && receiveID == ""){
        alert("請選擇收信人");
        return;
    }
    var data = {
        title, content, depart, receiveID
    };
    var successFn = function(res){
        if(res >0){
            alert("寄信成功");
            clearMsg();
        }else{
            alert("寄信失敗");
        }    
    };
    myObj.rAjaxFn("post", "/Message/sendMsg", data, successFn);
}

function clearMsg(){
    $("#msgTitle,#content").val("");
    $("#selReceiveDepart").find("option[value='']").prop("selected", true);
    $("#selReceiveID").empty().append(new Option("請選擇", "")).append(new Option("全體", "-1"));
}


//#endregion write message






