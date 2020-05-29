
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
    var successFn = function(res){
        res.forEach(function(value){
            $("#selDepart").append(new Option(value, value));
        });
    };
    myObj.rAjaxFn("get", "/AnnualLog/getDepartment", null, successFn);
}

function getDepartmentEmployee(depart){
    var successFn = function(res){
        res.forEach(function(value){
            $("#selEmployee").append(new Option(value.userName, value.id));
        });
    };
    myObj.rAjaxFn("get", "/AnnualLog/getDepartmentEmployee", {depart:depart}, successFn);
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
        refreshAnnualStatus(res.day);
        $("#annualTable").show();
    };
    myObj.rAjaxFn("post", "/AnnualLog/getAnnualLog", data, successFn);
}

function refreshTable(res){
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

    res.offset.forEach((value)=>{
        var newTr = $("<tr><td colspan='4' style='text-align:left;'></td></tr>");
        var dt = myObj.dateTimeFormat(value.createTime);
        value.value  = value.value>=0? `+${value.value/hourToDay}` : value.value/hourToDay;   //調整時數 轉換 天數 
        var txt = `${value.userName}在${dt.ymdText} ${dt.hmText} 調整特休: ${value.value}天 原因: ${value.reason}`;
        newTr.find("td").text(txt);
        $("#annualLogsTable").append(newTr);
    });
}

function refreshAnnualStatus(res){
    $("#annualDaysTable").find("tr[name='annualStatusRow'],tr[name='addUpAnnualStatusRow']").empty();
    $('.btnActive').css('pointer-events', "");
    if(res.length == 0){
        var noData = $("<tr name='annualStatusRow'><td colspan='4'>無特休</td></tr>");
        $("#annualDaysTable").append(noData);
        return;
    }

    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天

    res.forEach(function(value){
        var dayRow = $(".template").find("tr[name='annualStatusRow']").clone();
        dayRow.find("td[name='spDays']").text(value.specialDays + "天");

        let remainDays = parseInt((value.remainHours)/hourToDay);
        let remainHours = (value.remainHours)%hourToDay;
        /*if(remainDays ==0 && remainHours ==0){    
            return;     //期限還沒到但已沒有特休餘額 不顯示該列
        }*/
        let txt = `${remainDays} 天`+ (remainHours>0? `又 ${remainHours} 小時 `:"");
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
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    $('.btnActive').css('pointer-events', "none"); 
    $(thisBtn).hide();
    var thisRow = $(thisBtn).closest("tr[name='annualStatusRow']");
    var spDays = thisRow.find("td[name='spDays']").text();
    var remainHours = thisRow.find("td[name='remainDays']").data("remainHours");
    var deadLine = thisRow.find("td[name='deadLine']").text();

    var addUpRow = $(".template").find("tr[name='addUpAnnualStatusRow']").clone();
    addUpRow.find("input[name='editRemainDays']").data({"spDays":spDays, "oldRemainHours":remainHours}).val(parseInt(remainHours/hourToDay));
    addUpRow.find("span[name='remainHours']").text(remainHours%hourToDay >0 ? `又 ${remainHours%8} 小時`:"");
    addUpRow.find("input[name='editDeadLine']").val(deadLine);
    addUpRow.find(".addUp_spDays").attr("onclick", `addUpSpDays(${id});`);
    addUpRow.find(".cel_spDays").attr("onclick", "celSpDays();");
    thisRow.after(addUpRow);
}



function addUpSpDays(emAnnualID=0){
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    var thisRow = $("#annualDaysTable").find("tr[name='addUpAnnualStatusRow']");
    var spDays = thisRow.find("input[name='editRemainDays']").data("spDays");
    var oldRemainHours = thisRow.find("input[name='editRemainDays']").data("oldRemainHours");
    var newRemainHours = thisRow.find("input[name='editRemainDays']").val();
    var newDeadLine = thisRow.find("input[name='editDeadLine']").val();
    var reason = thisRow.find("input[name='editReason']").val();
    if(newRemainHours =="" || newDeadLine=="" || reason==""){
        alert("欄位皆須填寫");
        return;
    }
    newRemainHours = newRemainHours*hourToDay + oldRemainHours%8;
    if(newRemainHours > spDays*hourToDay){
        alert("調整後剩餘天數不可超過特休天數");
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