
var leaveApp = {
    
    isPassive: (location.pathname.match("ApplyLeave")? false : true),  //是否被請假(主管幫員工請假)
    
    init(){
        this.getLeaveOption();
        this.addListenEvent();
        if(!this.isPassive){
            var date = new Date();
            var newDate = date.setDate(date.getDate() - 30);
            var dtStart = myObj.dateTimeFormat(newDate);
            $("#filter_sDate").val(dtStart.ymdHtml);
            var dtEnd = myObj.dateTimeFormat();
            $("#filter_eDate").val(dtEnd.ymdHtml);
            this.getApplyLeave();
        }
    },

    addListenEvent(){
        var app = this;
        if(!this.isPassive){
            $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
                app.getApplyLeave();
            });
        }
    
        $("#applyLeaveList").on("change", "select[name='newApplyType']", function(){
            var unitOption = $("#applyLeaveList").find("select[name='newTimeUnit']").empty().append(new Option("天", 1));
            var useRule = myObj.leaveOption[$(this).val()]["timeUnit"];
            if(useRule == 2){
                $(unitOption).append(new Option("半天", 2));
            }else if(useRule == 3){
                $(unitOption).append(new Option("半天", 2));
                $(unitOption).append(new Option("小時", 3));
            }
            $("#applyLeaveList select[name='newTimeUnit']").change();
        });
    
        $("#applyLeaveList").on("change", "select[name='newTimeUnit']", function(){
            var selUnit = $(this).val();
            var inputVal = $("#applyLeaveList").find("input[name='newTimeLength']").val();
            var useRule = myObj.leaveOption[$("#applyLeaveList").find("select[name='newApplyType']").val()]["timeUnit"];
            if(selUnit == 1 || selUnit == 2){
                $("#applyLeaveList").find("input[name='newStartTime']").hide();
                $("#applyLeaveList").find("input[name='newTimeLength']").val(isNaN(parseInt(inputVal))? "":parseInt(inputVal));
            }else if(selUnit == 3){
                $("#applyLeaveList").find("input[name='newStartTime']").show();
            }
            if(selUnit == 2){
                $("#applyLeaveList").find("input[name='newTimeLength']").hide();
                $("#applyLeaveList").find("select[name='newHalfSel']").show();
            }else{
                $("#applyLeaveList").find("input[name='newTimeLength']").show();
                $("#applyLeaveList").find("select[name='newHalfSel']").hide();
                if(app.isPassive){
                    if(selUnit == 1 ){
                        $("#applyLeaveList").find("input[name='newTimeLength']").prop("readonly", true).val("1");
                    }else{
                        $("#applyLeaveList").find("input[name='newTimeLength']").prop("readonly", false).val("");
                    }
                }
            }
        });
    
        $("#applyLeaveList").on("input", "input[name='newTimeLength']", function(){
            var halfVal = myObj.leaveOption[$("#applyLeaveList").find("select[name='newApplyType']").val()]["halfVal"];
            var val = $(this).val();
            if(halfVal && $("#applyLeaveList").find("select[name='newTimeUnit']").val() == 3){
                val = val.replace(/[^\d.]/g, ""); //先把非數字的都替換掉，除了數字和.
                val = val.replace(/^\./g, ""); //必須保證第一個為數字而不是.
                val = val.replace(".", "$#$").replace(/\./g, "").replace("$#$", "."); //保證.只出現一次，而不能出現兩次以上
            }else{
                val = val.replace(/[^\d]/g, ""); //把非數字的都替換掉，除了數字
            }
            $(this).val(val);
        });
    },

    showApplyLeavePage(page=""){
        window.location.href = "/ApplyLeave/Index?page="+page; 
    },

    //#region applyLeave
    getApplyLeave(){
        var app = this;
        var page = $("#applyLeaveDiv").length > 0 ? 0 : 1;
        var sDate = $("#filter_sDate").val();
        var eDate = $("#filter_eDate").val();
        if(((sDate == "" && eDate == "") || (sDate != "" && eDate != "")) && sDate <= eDate){
            var successFn = function(res){
                if(page==0){
                    app.refreshApplyLeaveIng(res);
                }else{
                    app.refreshApplyLeaveLog(res);
                } 
            };
            var data ={
                page, sDate, eDate
            };
            myObj.rAjaxFn("post", "/ApplyLeave/getMyApplyLeave", data, successFn);
        }else{
            alert("搜尋日期格式有誤");  return;
        }
    },

    getLeaveOption(){
        var app = this;
        var successFn = function(res){
            var sel = app.isPassive? $("#applyLeaveList") : $(".template");
            sel = sel.find("[name='addApplyLeaveRow']").find("select[name='newApplyType']");
            myObj.leaveOption = {};
            res.forEach(function(value){
                $(sel).append(new Option(value.leaveName, value.id));
                myObj.leaveOption[value.id] = {timeUnit:value.timeUnit, halfVal:value.halfVal};
            });
        };
        myObj.rAjaxFn("post", "/ApplyLeave/getLeaveOption", null, successFn);
    },

    refreshApplyLeaveIng(res){
        $("#applyLeaveDiv").find("a.add_applyLeave").show();
        $('.btnActive').css('pointer-events', "");
        $("#applyLeaveList").empty();
        res.forEach(function(value){
            var row = $(".template").find("[name='applyLeaveRow']").clone();
            var addTime = myObj.dateTimeFormat(value.createTime);
            var dateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
            dateTD.html(dateTD.html().replace(/\n/g, "<br/>"));
            row.find("input[name='applyTypeVal']").val(value.leaveID);
            row.find("[name='applyType']").text(value.leaveName);
            row.find("[name='note']").text(value.note);
    
            var sTime = myObj.dateTimeFormat(value.startTime);
            var sTimeTD = row.find("[name='startTime']").text(sTime.ymdText + "\n" + sTime.hmText);
            sTimeTD.html(sTimeTD.html().replace(/\n/g, "<br/>"));
            var eTime = myObj.dateTimeFormat(value.endTime);
            var eTimeTD = row.find("[name='endTime']").text(eTime.ymdText + "\n" + eTime.hmText);
            eTimeTD.html(eTimeTD.html().replace(/\n/g, "<br/>"));
            row.find("[name='endTime']").attr({"data-value":value.unitVal, "data-unit": value.unit});
        
            row.find(".edit_applyLeave").attr("onclick", `leaveApp.editApplyLeave(this, ${value.id});`);
            row.find(".del_applyLeave").attr("onclick", `leaveApp.delApplyLeave(${value.id});`);
            $("#applyLeaveList").append(row);
        });
    },

    showAddApplyLeaveRow(){ //主動申請
        $("#applyLeaveDiv").find("a.add_applyLeave").hide();
        $('.btnActive').css('pointer-events', "none"); 
        var addApplyLeaveRow = $(".template").find("[name='addApplyLeaveRow']").clone();
        var dt = myObj.dateTimeFormat();
        addApplyLeaveRow.find("input[type='date']").val(dt.ymdHtml);
        addApplyLeaveRow.find("input[type='time']").val(dt.hour + ":00").hide();    //預設單位為天 所以隱藏
        addApplyLeaveRow.find("select[name='newHalfSel']").hide();
        addApplyLeaveRow.find("a.up_applyLeave").remove();
        addApplyLeaveRow.find("a.add_applyLeave").attr("onclick", `leaveApp.addUpApplyLeave(this);`);
        addApplyLeaveRow.find("a.cel_applyLeave").attr("onclick", `leaveApp.cancelApplyLeave();`);
        $('#applyLeaveList').append(addApplyLeaveRow);
        $("#applyLeaveList select[name='newApplyType']").change();
    },

    showAddLeave(employeeID){   //被申請
        $("#punchLogDiv").find("a.add_punchLog, a.add_leave, .punchLog_table").hide();
        $(".passiveLeave_table").show();
        $('.btnActive').css('pointer-events', "none"); 
        $("#applyLeaveList").find("select[name='newApplyType']").change();
    
        var dt = myObj.dateTimeFormat();
        //$("#applyLeaveList").find("td[name='newApplyDate']").text(dt.ymdHtml);
        $("#applyLeaveList").find("span[name='newStartDate']").text(myObj.qDateStr);
        $("#applyLeaveList").find("input[name='newStartTime']").val(dt.hour + ":00").hide();    //預設單位為天 所以隱藏
        $("#applyLeaveList").find("a.add_applyLeave").attr("onclick", `leaveApp.addUpApplyLeave(this, ${employeeID});`);
        $("#applyLeaveList").find("a.cel_applyLeave").attr("onclick", "leaveApp.cancelApplyLeave();");
    },

    addUpApplyLeave(thisBtn, employeeID=0, applyingID=0){
        var app = this;
        var thisRow =  $(thisBtn).closest("tr[name='addApplyLeaveRow']");
        var timeObj = this.getApplyLeaveTime(thisRow);
    
        if(timeObj == null){
            alert("請假時間不符合請假規定"); return;
        }
    
        var data = {
            ID : applyingID,
            accountID: employeeID,
            leaveID : timeObj.applyTypeVal,
            note : thisRow.find("[name='newApplyNote']").val(), 
            startTime : timeObj.startTime,
            unitVal: timeObj.inputVal,
            unit: timeObj.selUnit
        };
        
        var successFn = function(res){
            if(res == "notEnough"){
                alert("剩餘的特休時數不足"); return;
            }else if(res == "noPrincipal"){
                alert("很抱歉，無法進行請假手續，請洽人事人員，謝謝!"); return;
            }else if(res == "data_illegal"){
                alert("請假時間不符合請假規定"); return;
            }else if(res == "hadSameTimeLeave"){
                alert("已有同樣的請假時間紀錄"); return;
            }else if(res == "overEndWorkTime"){
                alert("注意! 請假時長不得超過下班時間");
            }
            app.cancelApplyLeave();
        }
        //return;
        myObj.cudAjaxFn("/ApplyLeave/addUpApplyLeave", data, successFn);
    },

    cancelApplyLeave(){
        if(!this.isPassive){
            this.getApplyLeave();
        }
        else{
            getPunchLogByIDByDate(myObj.qDateStr);
            $("#punchLogDiv").find("a.add_punchLog, a.add_leave, .punchLog_table").show();
            $(".passiveLeave_table").hide();
            $('.btnActive').css('pointer-events', ""); 
        }
    },

    editApplyLeave(thisBtn, applyingID){    //編輯更新 目前沒啟用
        $('.btnActive').css('pointer-events', "none");

        var thisRow = $(thisBtn).closest("tr[name='applyLeaveRow']").hide();
        var thisApplyDate = $(thisRow).find("[name='applyDate']").html();
        var thisApplyTypeVal = $(thisRow).find("input[name='applyTypeVal']").val();
        var thisNote = $(thisRow).find("[name='note']").text();
        var thisValue = $(thisRow).find("[name='endTime']").data("value");
        var thisUnit = $(thisRow).find("[name='endTime']").data("unit");
        var thisStartTime = $(thisRow).find("[name='startTime']").html();
        var thisEndTime = $(thisRow).find("[name='endTime']").html();
    
        var updateRow = $(".template").find("[name='addApplyLeaveRow']").clone();
        updateRow.find("[name='newApplyDate']").append(thisApplyDate);
        updateRow.find("select[name='newApplyType']").find(`option[value='${thisApplyTypeVal}']`).prop("selected", true);
        updateRow.find("[name='newApplyNote']").val(thisNote);
        updateRow.find("[name='newStartDate']").val((thisStartTime.split("<br>")[0]).replace(new RegExp("/", "g"), "-"));
        updateRow.find("[name='newStartTime']").val(thisStartTime.split("<br>")[1]);
        //updateRow.find("[name='newEndDate']").val((thisEndTime.split("<br>")[0]).replace(new RegExp("/", "g"), "-"));
        //updateRow.find("[name='newEndTime']").val(thisEndTime.split("<br>")[1]);
    
        updateRow.find("[name='newTimeLength']").val(thisValue);
        updateRow.find("select[name='newTimeUnit']").find(`option[value='${thisUnit}']`).prop("selected", true);
    
        updateRow.find("a.add_applyLeave").remove();
        updateRow.find("a.up_applyLeave").attr("onclick", `leaveApp.addUpApplyLeave(this, ${applyingID})`);
        updateRow.find("a.cel_applyLeave").attr("onclick", "leaveApp.cancelApplyLeave()");
        $(thisRow).after(updateRow);
    },

    delApplyLeave(applyingID){
        var app = this;
        var msg = "您真的確定要取消申請嗎？\n\n請確認！";
        if(confirm(msg)==false) 
            return;
        var successFn = function(res){
            if(res > 0){
                app.cancelApplyLeave();
            }else{
                alert('fail');
            }     
        };
        myObj.cudAjaxFn("/ApplyLeave/delApplyLeave", {applyingID}, successFn);
    },

    getApplyLeaveTime(thisRow){
        if(this.isPassive){
            var sDate = thisRow.find("[name='newStartDate']").text();
        }else{
            var sDate = thisRow.find("[name='newStartDate']").val();
        }
        
        var sTime = thisRow.find("[name='newStartTime']").val();
        var startTime = (sDate + "T" + sTime);
        
        var applyTypeVal = thisRow.find("[name='newApplyType']").val();
        //var useRule = myObj.leaveOption[applyTypeVal];
        var selUnit = thisRow.find("[name='newTimeUnit']").val();
        if(selUnit ==2){
            var inputVal = thisRow.find("select[name='newHalfSel']").val();
        }else{
            var inputVal = thisRow.find("input[name='newTimeLength']").val();
        }
    
        if(inputVal=="" || isNaN(parseInt(inputVal)) || inputVal % 0.5 !=0 || isNaN((new Date(startTime)).valueOf())){
            return null;
        }
        return {applyTypeVal, startTime, inputVal, selUnit};
    },
    //#endregion applyLeave

    //------------------------------------------------------------------------------------------------------------

    //#region applyLog
    refreshApplyLeaveLog(res){
        $("#applyLeaveLogList").empty();
        res.forEach(function(value){
            var row = $(".template").find("[name='applyLeaveLogRow']").clone();
            var addTime = myObj.dateTimeFormat(value.createTime);
            var dateTD = row.find("[name='applyDate']").text(addTime.ymdText + "\n" + addTime.hmText);
            dateTD.html(dateTD.html().replace(/\n/g, "<br/>"));
    
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
            $("#applyLeaveLogList").append(row);
        });
    }
    //#endregion applyLog
}//class

var myObj = new MyObj();

$(function() {
    leaveApp.init();
});

