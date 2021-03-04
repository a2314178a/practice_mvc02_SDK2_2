
var leaveApp = {
    
    //isPassive: (location.pathname.match("ApplyLeave")? false : true),  //是否被請假(主管幫員工請假)
    
    init(){
        this.addListenEvent();
        var date = new Date();
        var newDate = date.setDate(date.getDate() - 30);
        var dtStart = myObj.dateTimeFormat(newDate);
        $("#filter_sDate").val(dtStart.ymdHtml);
        var dtEnd = myObj.dateTimeFormat();
        $("#filter_eDate").val(dtEnd.ymdHtml);
        this.getApplyOvertime();
    },

    addListenEvent(){
        var app = this;
        $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
            app.getApplyOvertime();
        });       
    },

    showApplyOvertimePage(page=""){
        window.location.href = "/ApplyOvertime/Index?page="+page; 
    },

    //#region applyLeave
    getApplyOvertime(){
        var app = this;
        var page = $("#applyOvertimeDiv").length > 0 ? 0 : 1;
        var sDate = $("#filter_sDate").val();
        var eDate = $("#filter_eDate").val();
        if(((sDate == "" && eDate == "") || (sDate != "" && eDate != "")) && sDate <= eDate){
            var successFn = function(res){
                if(page==0){
                    app.refreshApplyOvertimeIng(res);
                }else{
                    app.refreshApplyOvertimeLog(res);
                } 
            };
            var data ={
                page, sDate, eDate
            };
            myObj.rAjaxFn("post", "/ApplyOvertime/getMyApplyOvertime", data, successFn);
        }else{
            alert("搜尋日期格式有誤");  return;
        }
    },

    refreshApplyOvertimeIng(res){
        $("#applyOvertimeDiv").find("a.add_overtime").show();
        $('.btnActive').css('pointer-events', "");
        $("#applyOvertimeList").empty();
        res.forEach(function(value){
            var row = $(".template").find("[name='applyOvertimeRow']").clone();
            var addTime = myObj.dateTimeFormat(value.createTime);
            var dateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
            dateTD.html(dateTD.html().replace(/\n/g, "<br/>"));

            row.find("[name='note']").text(value.note);
            var sTime = myObj.dateTimeFormat(value.workDate);
            row.find("[name='workDate']").text(sTime.ymdText);
            row.find("[name='timeLength']").text((parseInt(value.timeLength)/10) + " 小時");
            //row.find(".edit_applyOvertime").attr("onclick", `leaveApp.editApplyOvertime(this, ${value.id});`);
            row.find(".del_applyOvertime").attr("onclick", `leaveApp.delApplyOvertime(${value.id});`);
            $("#applyOvertimeList").append(row);
        });
    },

    showAddApplyOvertimeRow(){ //主動申請
        $("#applyOvertimeDiv").find("a.add_overtime").hide();
        $('.btnActive').css('pointer-events', "none"); 
        var addApplyOvertimeRow = $(".template").find("[name='addApplyOvertimeRow']").clone();
        var dt = myObj.dateTimeFormat();
        addApplyOvertimeRow.find("input[type='date']").val(dt.ymdHtml);
        //addApplyOvertimeRow.find("a.up_applyOvertime").remove();
        addApplyOvertimeRow.find("a.add_applyOvertime").attr("onclick", `leaveApp.addUpApplyOvertime(this);`);
        addApplyOvertimeRow.find("a.cel_applyOvertime").attr("onclick", `leaveApp.cancelApplyOvertime();`);
        $('#applyOvertimeList').append(addApplyOvertimeRow);
    },

    addUpApplyOvertime(thisBtn, employeeID=0, applyingID=0){
        var app = this;
        var thisRow =  $(thisBtn).closest("tr[name='addApplyOvertimeRow']");
    
        var data = {
            ID : applyingID,
            accountID: employeeID,
            note : thisRow.find("[name='newApplyNote']").val(), 
            workDate : thisRow.find("input[name='newStartDate']").val(),
            timeLength: thisRow.find("select[name='overtimeLength']").val(), 
        };
        
        var successFn = function(res){
            if(res == "data_illegal"){
                alert("加班時間不符合加班規定"); return;
            }
            app.cancelApplyOvertime();
        }
        //return;
        myObj.cudAjaxFn("/ApplyOvertime/addUpApplyOvertime", data, successFn);
    },

    cancelApplyOvertime(){
        this.getApplyOvertime();
    },

    editApplyLeave(thisBtn, applyingID){    //編輯更新 目前沒啟用
        $('.btnActive').css('pointer-events', "none");

        var thisRow = $(thisBtn).closest("tr[name='applyOvertimeRow']").hide();
        var thisApplyDate = $(thisRow).find("[name='applyDate']").html();
        var thisNote = $(thisRow).find("[name='note']").text();
        var thisWorkDate = $(thisRow).find("[name='workDate']").html();
        var timeLength = $(thisRow).find("[name='timeLength']").html();
    
        var updateRow = $(".template").find("[name='addApplyOvertimeRow']").clone();
        updateRow.find("[name='newApplyDate']").append(thisApplyDate);
        updateRow.find("[name='newApplyNote']").val(thisNote);
        updateRow.find("[name='newStartDate']").val((thisWorkDate.split("<br>")[0]).replace(new RegExp("/", "g"), "-"));
        updateRow.find("select[name='overtimeLength']").find(`option[value='${timeLength}']`).prop("selected", true);
        updateRow.find("a.add_applyOvertime").remove();
        updateRow.find("a.up_applyOvertime").attr("onclick", `leaveApp.addUpApplyOvertime(this, ${applyingID})`);
        updateRow.find("a.cel_applyOvertime").attr("onclick", "leaveApp.cancelApplyOvertime()");
        $(thisRow).after(updateRow);
    },

    delApplyOvertime(applyingID){
        var app = this;
        var msg = "您真的確定要取消申請嗎？\n\n請確認！";
        if(confirm(msg)==false) 
            return;
        var successFn = function(res){
            if(res > 0){
                app.cancelApplyOvertime();
            }else{
                alert('fail');
            }     
        };
        myObj.cudAjaxFn("/ApplyOvertime/delApplyOvertime", {applyingID}, successFn);
    },

    
    //#endregion applyLeave

    //------------------------------------------------------------------------------------------------------------

    //#region applyLog
    refreshApplyOvertimeLog(res){
        $("#applyOvertimeLogList").empty();
        res.forEach(function(value){
            var row = $(".template").find("[name='applyOvertimeLogRow']").clone();
            var addTime = myObj.dateTimeFormat(value.createTime);
            var dateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
            dateTD.html(dateTD.html().replace(/\n/g, "<br/>"));
    
            row.find("[name='note']").text(value.note);
            var sTime = myObj.dateTimeFormat(value.workDate);
            row.find("[name='workDate']").text(sTime.ymdText);
            row.find("[name='timeLength']").text((parseInt(value.timeLength)/10) + " 小時");
    
            var status = "";
            switch(value.applyStatus){
                case 0: status="待審核"; break;
                case 1: status="通過"; break;
                case 2: status="不通過"; break;
            };
            row.find("[name='applyStatus']").text(status);
            $("#applyOvertimeLogList").append(row);
        });
    }
    //#endregion applyLog
}//class

var myObj = new MyObj();

$(function() {
    leaveApp.init();
});

