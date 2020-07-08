

//#region new all backup message
function getReceiveMessage(read){
    var successFn = function(res){
        refreshMessage(res);     
    };
    myObj.rAjaxFn("post", "/Message/getReceiveMessage?readStatus="+read, null, successFn);
}

function getSendMessage(){
    var successFn = function(res){
        refreshMessage(res);
    };
    myObj.rAjaxFn("post", "/Message/getSendMessage", null, successFn);
}

function refreshMessage(res){
    $("#selAllChkBox").prop("checked", false);
    $("#msgProcess").find("option[value='']").prop("selected", true);
    var msgID = 0;
    var receiveName = "";
    $("#messageList").empty();
    res.forEach(function(value){
        if(value.messageID != msgID){   //若同時寄很多人 也只要出現一次該信件
            msgID = value.messageID;
            var row = $(".template").find("[name='receiveMsgRow']").clone();
            var dt = myObj.dateTimeFormat(value.createTime);
            var dtTD = row.find("[name='dateTime']").text(dt.ymdText + "\n" + dt.hmText);
            dtTD.html(dtTD.html().replace(/\n/g, "<br/>"));
         
            row.find("[name='title']").find('a').attr("onclick", `showContent(this, ${value.id});`)
                                                .data("read", value.read).text(value.title);
            row.find("[name='sendName']").text(value.userName);

            var receiveTD = $("#messageList").find("td[data-tmp='tmp']");
            if($(receiveTD).length >0){
                receiveTD.text(receiveName);
                receiveTD.removeAttr("data-tmp");
            }
            
            row.find("[name='receiveName']").attr("data-tmp","tmp");
            receiveName = value.userName;

            row.find("input[name='msgChkBox']").val(value.id);
            $("#messageList").append(row);

            var textRow = $(".template").find("[name='msgContentRow']").clone();
            textRow.find("[name='content']").attr("data-id", value.id).text(value.content).hide();
            $("#messageList").append(textRow);
        }else{
           receiveName += ", " + value.userName;    //串收件人
        }
    });
    var receiveTD = $("#messageList").find("td[data-tmp='tmp']");
    if($(receiveTD).length >0){
        receiveTD.text(receiveName);
        receiveTD.removeAttr("data-tmp");
    }
}

function showContent(thisLink, relatedID){    //relatedID = msgID
    $("#messageList").find(`[data-id='${relatedID}']`).toggle();
    if($(thisLink).data("read") == 1 || $("#backupMsgDiv").length >0){
        return;
    }
    var msgID = [relatedID];
    var successFn = function(res){
        if(res==1){
            $(thisLink).data("read", 1);
        }
    };
    myObj.cudAjaxFn("/Message/setHadReadMsg", {msgID}, successFn);  //標示已讀
}

function setHadRead(){
    var msgID = [];
    var hasChkMsg = $("#messageList").find("input[name='msgChkBox']:checked");
    $.each(hasChkMsg, function(key, obj){
        msgID.push($(obj).val());
    });
    if(msgID.length == 0){
        $("#msgProcess").find("option[value='']").prop("selected", true);
        return;
    }

    var msg = "您確定要把訊息設為已讀嗎？\n\n請確認！";
    if(confirm(msg)==false){ 
        $("#msgProcess").find("option[value='']").prop("selected", true);
        return;
    }
    var successFn = function(res){
        getReceiveMessage(0);
    };
    myObj.cudAjaxFn("/Message/setHadReadMsg", {msgID}, successFn);
}

function delMsg(){
    var msgID = [];
    var hasChkMsg = $("#messageList").find("input[name='msgChkBox']:checked");
    $.each(hasChkMsg, function(key, obj){
        msgID.push($(obj).val());
    });
    if(msgID.length == 0){
        $("#msgProcess").find("option[value='']").prop("selected", true);
        return;
    }

    var msg = "您真的確定要刪除訊息嗎？\n\n請確認！";
    if(confirm(msg)==false){ 
        $("#msgProcess").find("option[value='']").prop("selected", true);
        return;
    }
    var successFn = function(res){
        if($("#allMsgDiv").length >0){
            getReceiveMessage(1);
        }else if($("#newMsgDiv").length >0){
            getReceiveMessage(0);
        }else if($("#backupMsgDiv").length >0){
            getSendMessage();
        }
    };
    if($("#allMsgDiv").length >0 || $("#newMsgDiv").length >0){
        var sel="rDel"
    }else if($("#backupMsgDiv").length >0){
        var sel="sDel"
    }
    myObj.cudAjaxFn("/Message/delMessage", {msgID, sel}, successFn);
}

//#endregion new all message

//------------------------------------------------------------------------------------------------------------

