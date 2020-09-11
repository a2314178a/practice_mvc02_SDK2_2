
var myObj = new MyObj();

$(document).ready(function() {  

    init();

    $("#selDepart").on("change", function(){
        $("#selEmployee").empty().append(new Option("請選擇", ""));
        var departSel = $(this).val();
        if(departSel ==""){
            return;
        }
        getDepartmentEmployee(departSel);
    });

    $("#selEmployee").on("click", function(){
        if($("#selDepart").val() == ""){
            alert("請先選擇部門");
            return;
        }
    });

    $("#annualDaysTable").on("input", "input[name='editRemainDays']", function(){
        var annualUnit = $("#annualDaysTable").data("annualUnit");  //1:天 2:半天 3:小時
        var halfVal = $("#annualDaysTable").data("halfVal");    //可否0.5小時
        var val = $(this).val();
        if(annualUnit == 2 || (annualUnit == 3 && halfVal)){
            val = val.replace(/[^\d.]/g, ""); //先把非數字的都替換掉，除了數字和.
            val = val.replace(/^\./g, ""); //必須保證第一個為數字而不是.
            val = val.replace(".", "$#$").replace(/\./g, "").replace("$#$", "."); //保證.只出現一次，而不能出現兩次以上
        }else{
            val = val.replace(/[^\d]/g, ""); //先把非數字的都替換掉，除了數字
        }  
        $(this).val(val);
    });
    
});//.ready function

function showEmployeePage(page){
    window.location.href = "/EmployeeList/Index?page="+page; 
}

function init(){
    var date = myObj.dateTimeFormat();
    $("#startDate,#endDate").val(date.ymdHtml);
    if($("#displayTable").length > 0){
        getDepartment();
    }
}

function getDepartment(){
    var seeAll = ($("#searchFilterDiv").data("all"));
    var seeDepartEm = ($("#searchFilterDiv").data("depart"));
    var successFn = function(res){
        if(res.length >1){
            $("#selDepart").append(new Option("請選擇", "")); 
        }
        res.forEach(function(value){
            $("#selDepart").append(new Option(value.department, value.department));
        });
        if(seeAll == 1){
            $("#selDepart").append(new Option("未指派", "未指派"));
        }
        if(($("#selDepart").find("option")).length == 1){
            getDepartmentEmployee($("#selDepart").val());
        }
    };
    myObj.rAjaxFn("get", "/AnnualLog/getDepartment", null, successFn);
}

function getDepartmentEmployee(depart){
    var successFn = function(res){
        res.forEach(function(value){
            $("#selEmployee").append(new Option(value.userName, value.id));
        });
    };
    myObj.rAjaxFn("get", "/AnnualLog/getDepartmentEmployee", {depart}, successFn);
}

function searchAnnualLog(){
    var sDate = $("#startDate").val();
    var eDate = $("#endDate").val();
    var selID = $("#selEmployee").val();
    if(selID == ""){
        alert("請選擇員工");
        return;
    }
    if(sDate =="" || eDate =="" || sDate > eDate){
        alert("請選擇日期或者日期格式有誤");
        return;
    }
    var data = {selID, sDate, eDate};
    var successFn = function(res){
        refreshTable(res);
        refreshAnnualStatus(res);
        $("#annualTable").show();
    };
    myObj.rAjaxFn("post", "/AnnualLog/getAnnualLog", data, successFn);
}

function refreshTable(res){ //請假TABLE
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    $("#annualLogsTable").empty();
    if(res.log.length == 0 && res.offset.length == 0){
        var noData = $("<tr><td colspan='4'>無資料</td></tr>");
        $("#annualLogsTable").append(noData);
        return;
    }
    res.log.forEach(function(value){
        var logRow = $(".template").find("tr[name='annualLogRow']").clone();

        var sTime = myObj.dateTimeFormat(value.startTime);
        var eTime = myObj.dateTimeFormat(value.endTime);
        var sDateTD = logRow.find("td[name='sDate']").text(sTime.ymdHtml+"T"+sTime.hmText);
        sDateTD.html(sDateTD.html().replace(/T/g, "<br/>"));
        var eDateTD = logRow.find("td[name='eDate']").text(eTime.ymdHtml+"T"+eTime.hmText);
        eDateTD.html(eDateTD.html().replace(/T/g, "<br/>"));

        logRow.find("td[name='note']").text(value.note);
        
        var unit = value.unit;
        var unitVal = value.unitVal;
        var txt = "";
        switch(unit){
            case 1: txt = unitVal+"天"; break;
            case 2: txt = `${unitVal/2}天`; break;
            case 3: txt = unitVal+"小時"; break;
        }
        logRow.find("td[name='totalTime']").text(txt);

        $("#annualLogsTable").append(logRow);
    });

    res.offset.forEach((value)=>{       //有異動特休的紀錄
        var unit = res.annualLeaveUnit.timeUnit;
        var newTr = $("<tr><td colspan='4' style='text-align:left;'></td></tr>");
        var dt = myObj.dateTimeFormat(value.createTime);
        var offsetVal = value.value;
        if(unit == 1 || unit == 2){
            offsetVal = offsetVal>=0? `+${offsetVal/hourToDay}天` : `${offsetVal/hourToDay}天`;   //調整時數 轉換 天數 
        }else{
            offsetVal = offsetVal>=0? `+${offsetVal}小時` : `${offsetVal}小時`;    //多 "+" 符號
        }
        
        var txt = `${value.userName}在${dt.ymdText} ${dt.hmText} 調整特休: ${offsetVal} 原因: ${value.reason}`;
        newTr.find("td").text(txt);
        $("#annualLogsTable").append(newTr);
    });
}

function refreshAnnualStatus(res){  //當前特休狀態TABLE
    $("#annualDaysTable").find("tr[name='annualStatusRow'],tr[name='addUpAnnualStatusRow']").empty();
    $('.btnActive').css('pointer-events', "");
    if(res.day.length == 0){
        var noData = $("<tr name='annualStatusRow'><td colspan='4'>無特休</td></tr>");
        $("#annualDaysTable").append(noData);
        return;
    }

    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    var unit = res.annualLeaveUnit.timeUnit;
    var halfVal = res.annualLeaveUnit.halfVal;
    $("#annualDaysTable").data({"annualUnit":unit, "halfVal":halfVal});

    res.day.forEach(function(value){
        var dayRow = $(".template").find("tr[name='annualStatusRow']").clone();
        dayRow.find("td[name='spDays']").text(value.specialDays + "天");

        if(unit == 1 || unit == 2){ //1:天 2:半天
            if(unit == 1){
                var remainDays = parseInt((value.remainHours)/hourToDay);
                var otherHours = (value.remainHours)%hourToDay;
            }else{
                var remainDays = parseInt((value.remainHours)/(hourToDay*0.5)); //mod 半天(4hour)
                remainDays *= 0.5;  //轉成天 ex:37hours /4 = 9個半天 ...9/2 = 4.5天
                var otherHours = (value.remainHours)%(hourToDay*0.5);
            }
            var txt = `${remainDays} 天` + (otherHours>0? ` ${otherHours}小時` : "");
        }else{
            var remainHours = value.remainHours;
            var txt = `${remainHours} 小時`;
        }
        dayRow.find("td[name='remainDays']").data("remainHours", value.remainHours).text(txt);
        var dTime = myObj.dateTimeFormat(value.deadLine);
        dayRow.find("td[name='deadLine']").text(dTime.ymdHtml);
        dayRow.find(".edit_spDays").attr("onclick", `editSpDays(this, ${value.id});`);

        $("#annualDaysTable").append(dayRow);
    });

}

function celSpDays(){
    $("#annualDaysTable").find("tr[name='addUpAnnualStatusRow']").remove();
    $('.btnActive').css('pointer-events', "").show(); 
}

function editSpDays(thisBtn, id){
    var annualUnit = $("#annualDaysTable").data("annualUnit");
    var halfVal = $("#annualDaysTable").data("halfVal");
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    $('.btnActive').css('pointer-events', "none"); 
    $(thisBtn).hide();
    var thisRow = $(thisBtn).closest("tr[name='annualStatusRow']");
    var spDays = thisRow.find("td[name='spDays']").text();
    spDays = spDays.replace(/[^\d.]/g, "");
    var remainHours = thisRow.find("td[name='remainDays']").data("remainHours");
    var deadLine = thisRow.find("td[name='deadLine']").text();

    if(annualUnit == 1){    // 1:整天 2:半天 3:小時
        var remainVal = parseInt((remainHours)/hourToDay);
        var otherHours = (remainHours)%hourToDay;
    }else if(annualUnit == 2){
        var remainVal = parseInt((remainHours)/(hourToDay*0.5)); //mod 半天(4hour)
        remainVal *= 0.5;  //轉成天 ex:37hours /4 = 9個半天 ...9/2 = 4.5天
        var otherHours = (remainHours)%(hourToDay*0.5);
    }else{
        if(halfVal){    //可否半小時
            var remainVal = remainHours;
            var otherHours = 0;
        }else{
            var remainVal = parseInt(remainHours);
            var otherHours = remainHours - remainVal;
        }
    }
    var txt = (annualUnit==1 || annualUnit==2? "天":"小時") + (otherHours >0? ` 又${otherHours}小時` : "");

    var addUpRow = $(".template").find("tr[name='addUpAnnualStatusRow']").clone();
    var step05 = (annualUnit==2 || (annualUnit==3 && halfVal))? true : false;
    addUpRow.find("input[name='editRemainDays']").attr("step", step05? "0.5" : "1");
    addUpRow.find("input[name='editRemainDays']").data({"spDays":spDays, "oldRemainHours":remainHours}).val(remainVal);
    addUpRow.find("span[name='remainHours']").text(txt);
    addUpRow.find("input[name='editDeadLine']").val(deadLine).data("deadLine", deadLine);
    addUpRow.find(".addUp_spDays").attr("onclick", `addUpSpDays(${id});`);
    addUpRow.find(".cel_spDays").attr("onclick", "celSpDays();");
    thisRow.after(addUpRow);
}

function addUpSpDays(emAnnualID=0){
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    var annualUnit = $("#annualDaysTable").data("annualUnit");
    var thisRow = $("#annualDaysTable").find("tr[name='addUpAnnualStatusRow']");
    var spDays = thisRow.find("input[name='editRemainDays']").data("spDays");
    var oldRemainHours = thisRow.find("input[name='editRemainDays']").data("oldRemainHours");
    var newRemainHours = thisRow.find("input[name='editRemainDays']").val();
    var otherHours = thisRow.find("span[name='remainHours']").text();
    otherHours = otherHours.replace(/[^\d.]/g, "");
    otherHours = otherHours==""? 0: otherHours;
    var newDeadLine = thisRow.find("input[name='editDeadLine']").val();
    var oldDeadLine = thisRow.find("input[name='editDeadLine']").data("deadLine");
    var reason = thisRow.find("input[name='editReason']").val();
    if(newRemainHours =="" || newDeadLine=="" || reason==""){
        alert("欄位皆須填寫");
        return;
    } 
    if(newRemainHours % 0.5 !=0 ){
        alert("小數值須為0.5的倍數");
        return;
    }
    newRemainHours = (annualUnit==3? parseFloat(newRemainHours) : parseFloat(newRemainHours*hourToDay));  //轉換成hour
    newRemainHours += parseFloat(otherHours);
    if(isNaN(newRemainHours)){
        alert("更新失敗");
        return;
    }
    if(newRemainHours > spDays*hourToDay){
        alert("調整後剩餘天數不可超過特休天數");
        return;
    }
    if(DateDiff(newDeadLine, oldDeadLine) > 120){
        alert("調整後特休期限不可與原先相差超過120天");
        return;
    }
    var annualData = {
        ID: emAnnualID,
        remainHours: newRemainHours,
        deadLine: newDeadLine,
    };
    var offsetData = {
        emAnnualID, reason, value:(newRemainHours-oldRemainHours),
    };
    var successFn = ()=>{
        searchAnnualLog();
    };
    myObj.rAjaxFn("post", "/AnnualLog/addUpAnnualStatus", {annualData, offsetData}, successFn);
}

function DateDiff(sDate1, sDate2){
    var oDate1 = new Date(sDate1);
    var oDate2 = new Date(sDate2);
    var iDays = parseInt(Math.abs(oDate1 - oDate2)/1000/60/60/24); // 把相差的毫秒數轉換為天數
    return iDays;
}
