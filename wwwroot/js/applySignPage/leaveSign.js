

//#region leaveSign

function getEmployeeApplyLeave(){
    if($("#searchFilterDiv").length >0){
        var sDate = $("#filter_sDate").val();
        var eDate = $("#filter_eDate").val();
    }else{
        var sDate = new Date(2000, 0, 1);   //=2000-01-01
        var eDate = new Date(); //
    }
    
    var page = $("#leaveSignDiv").attr("name") == "leave"? 0 : 1;
    if(((sDate == "" && eDate == "") || (sDate != "" && eDate != "")) && sDate <= eDate){
        var successFn = function(res){ 
            refreshApplyLeaveIng(res);   
        };
        var data ={
            page, sDate, eDate
        };
        myObj.rAjaxFn("post", "/ApplicationSign/getEmployeeApplyLeave", data, successFn);
    }else{
        alert("搜尋日期格式有誤");  return;
    }
}

function refreshApplyLeaveIng(res){
    $('.btnActive').css('pointer-events', "");
    $("#applyLeaveList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='applyLeaveRow']").clone();
        row.find("[name='employeeName']").text(value.userName);

        var addTime = myObj.dateTimeFormat(value.createTime);
        var applyDateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
        applyDateTD.html(applyDateTD.html().replace(/\n/g, "<br/>"));

        row.find("[name='applyType']").text(value.leaveName);
        row.find("[name='note']").text(value.note);

        var sTime = myObj.dateTimeFormat(value.startTime);
        var sTimeTD = row.find("[name='startTime']").text(sTime.ymdText + "\n" + sTime.hmText);
        sTimeTD.html(sTimeTD.html().replace(/\n/g, "<br/>"));
        var eTime = myObj.dateTimeFormat(value.endTime);
        var eTimeTD = row.find("[name='endTime']").text(eTime.ymdText + "\n" + eTime.hmText);
        eTimeTD.html(eTimeTD.html().replace(/\n/g, "<br/>"));
        
        var status = "";
        switch(value.applyStatus){
            case 0: status="待審核"; break;
            case 1: status="通過"; break;
            case 2: status="不通過"; break;
        };
        row.find("[name='applyStatus']").text(status);

        row.find(".yes_applyLeave").attr("onclick",`isAgreeApplyLeave(this, ${value.id}, 1);`);
        row.find(".no_applyLeave").attr("onclick",`isAgreeApplyLeave(this, ${value.id}, 2);`);
        $("#applyLeaveList").append(row);
    });
}

function isAgreeApplyLeave(thisBtn, applyLeaveID, isAgree){
    var thisRow = $(thisBtn).closest("tr[name='applyLeaveRow']");
    var successFn = function(res){
        if(res == 1){
            thisRow.find("[name='applyStatus']").text(isAgree ==1? "通過" : "不通過");
        }
    }
    myObj.cudAjaxFn("/ApplicationSign/isAgreeApplyLeave", {applyLeaveID, isAgree}, successFn);
}


//#endregion leaveSign







