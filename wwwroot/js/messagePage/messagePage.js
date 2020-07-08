
var myObj = new MyObj();

$(document).ready(function() {   
    init();
    $("#writeMsgDiv").on("change", "#selReceiveDepart", function(){
        $("#selReceiveID").empty().append(new Option("請選擇", "")).append(new Option("全體", "-1"));
        var departSel = $("#selReceiveDepart").val();
        if(departSel == "" || departSel == 0){  
            return;
        }
        setReceiveOption(departSel);
    });

    $(".message_table").on("change", "#msgProcess", function(){
        if($(this).val()=="read"){
            setHadRead();
        }else if($(this).val()=="del"){
            delMsg();
        }
    });

    $("#selAllChkBox").on("click", function(){
        if($(this).prop("checked")){
            $("#messageList").find("input[type='checkbox']").prop("checked", true);
        }else{
            $("#messageList").find("input[type='checkbox']").prop("checked", false);
        }
    });

});//.ready function


function showMessagePage(page){
    window.location.href = "/Message/Index?page="+page;
}

function init(){
    if($("#newMsgDiv").length >0){
        getReceiveMessage(0);
    }else if($("#allMsgDiv").length >0){
        getReceiveMessage(1);
    }else if($("#backupMsgDiv").length >0){
        getSendMessage();
    }else if($("#writeMsgDiv").length >0){
        getReceiveOption();
    }
}

