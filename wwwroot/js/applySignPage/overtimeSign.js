

//#region leaveSign

function getEmployeeApplyOvertime(){
    var sDate = null;
    var eDate = null;
    var fDepart = $("#filterDepart").val();
    if($("#searchFilterDiv").length >0){
        sDate = $("#filter_sDate").val();
        eDate = $("#filter_eDate").val();
    }else{
        sDate = new Date(2000, 0, 1);   //=2000-01-01
        eDate = new Date(); //
    }
    
    var page = $("#overtimeSignDiv").attr("name") == "overtime"? 0 : 1;
    if(((sDate == "" && eDate == "") || (sDate != "" && eDate != "")) && sDate <= eDate){
        var successFn = function(res){ 
            refreshApplyOvertime(res);   
        };
        var data ={
            page, sDate, eDate, fDepart
        };
        myObj.rAjaxFn("post", "/ApplicationSign/getEmployeeApplyOvertime", data, successFn);
    }else{
        alert("搜尋日期格式有誤");  return;
    }
}

function refreshApplyOvertime(res){
    $('.btnActive').css('pointer-events', "");
    $("#applyOvertimeList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='applyOvertimeRow']").clone();
        row.find("[name='employeeName']").text(value.userName);

        var addTime = myObj.dateTimeFormat(value.createTime);
        var applyDateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
        applyDateTD.html(applyDateTD.html().replace(/\n/g, "<br/>"));

        row.find("[name='note']").text(value.note);

        var sTime = myObj.dateTimeFormat(value.workDate);
        var sTimeTD = row.find("[name='workDate']").text(sTime.ymdText);

        row.find("[name='timeLength']").text((parseInt(value.timeLength)/10) + " 小時");
        
        var status = "";
        switch(value.applyStatus){
            case 0: status="待審核"; break;
            case 1: status="通過"; break;
            case 2: status="不通過"; break;
        };
        row.find("[name='applyStatus']").text(status);

        row.find(".yes_applyOvertime").attr("onclick", `isAgreeApplyOvertime(this, ${value.id}, 1);`);
        row.find(".no_applyOvertime").attr("onclick", `isAgreeApplyOvertime(this, ${value.id}, 2);`);
        if(value.applyStatus == 1){
            row.find(".cancel_applyOvertime").attr("onclick", `isAgreeApplyOvertime(this, ${value.id}, 3);`);
        }else{
            row.find(".cancel_applyOvertime").remove();
        }
        $("#applyOvertimeList").append(row);
    });
}

function isAgreeApplyOvertime(thisBtn, applyLeaveID, isAgree){
    if(isAgree == 3){
        var msg = "您確定要取消該筆加班申請嗎？\n\n請確認！";
        if(confirm(msg)==false){ 
            return;
        }
        isAgree = 2;
    }
    var thisRow = $(thisBtn).closest("tr[name='applyOvertimeRow']");
    var successFn = function(res){
        if(res == 1){
            thisRow.find("[name='applyStatus']").text(isAgree ==1? "通過" : "不通過");
            thisRow.find(".cancel_applyOvertime").hide();
        }
    }
    myObj.cudAjaxFn("/ApplicationSign/isAgreeApplyOvertime", {applyLeaveID, isAgree}, successFn);
}


//#endregion leaveSign







