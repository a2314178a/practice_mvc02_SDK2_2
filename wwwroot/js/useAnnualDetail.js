
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
        var val = $(this).val();
        if(annualUnit == 2){
            val = val.replace(/[^\d.]/g, ""); //先把非數字的都替換掉，除了數字和.
            val = val.replace(/^\./g, ""); //必須保證第一個為數字而不是.
            //val = val.replace(/\.{2,}/g, ""); //保證只有出現一個.而沒有多個.
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
        refreshAnnualStatus(res);
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
        var unit = res.annualLeaveUnit;
        var newTr = $("<tr><td colspan='4' style='text-align:left;'></td></tr>");
        var dt = myObj.dateTimeFormat(value.createTime);
        var offsetVal = value.value;
        if(unit == 1 || unit == 2){
            offsetVal = offsetVal>=0? `+${offsetVal/hourToDay}天` : `${offsetVal/hourToDay}天`;   //調整時數 轉換 天數 
        }else{
            offsetVal = offsetVal>=0? `+${offsetVal}小時` : `${offsetVal}小時`;
        }
        
        var txt = `${value.userName}在${dt.ymdText} ${dt.hmText} 調整特休: ${offsetVal} 原因: ${value.reason}`;
        newTr.find("td").text(txt);
        $("#annualLogsTable").append(newTr);
    });
}

function refreshAnnualStatus(res){
    $("#annualDaysTable").find("tr[name='annualStatusRow'],tr[name='addUpAnnualStatusRow']").empty();
    $('.btnActive').css('pointer-events', "");
    if(res.day.length == 0){
        var noData = $("<tr name='annualStatusRow'><td colspan='4'>無特休</td></tr>");
        $("#annualDaysTable").append(noData);
        return;
    }

    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    var unit = res.annualLeaveUnit;
    $("#annualDaysTable").data("annualUnit", unit);

    res.day.forEach(function(value){
        var dayRow = $(".template").find("tr[name='annualStatusRow']").clone();
        dayRow.find("td[name='spDays']").text(value.specialDays + "天");

        if(unit == 1 || unit == 2){
            var remainDays = unit==1?parseInt((value.remainHours)/hourToDay) : parseFloat((value.remainHours)/hourToDay);
            var txt = `${remainDays} 天`;
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
    var hourToDay = myObj.workHoursToDay;  //工作幾小時算一天
    $('.btnActive').css('pointer-events', "none"); 
    $(thisBtn).hide();
    var thisRow = $(thisBtn).closest("tr[name='annualStatusRow']");
    var spDays = thisRow.find("td[name='spDays']").text();
    spDays = spDays.replace(/[^\d.]/g, "");
    var remainHours = thisRow.find("td[name='remainDays']").data("remainHours");
    var deadLine = thisRow.find("td[name='deadLine']").text();

    if(annualUnit == 1 || annualUnit == 2){//   1:整天 2:半天
        var remainVal = annualUnit==1?parseInt((remainHours)/hourToDay) : parseFloat((remainHours)/hourToDay);
    }else{
        var remainVal = remainHours;
    }

    var addUpRow = $(".template").find("tr[name='addUpAnnualStatusRow']").clone();
    addUpRow.find("input[name='editRemainDays']").attr("step", annualUnit==2? "0.5" : "1");
    addUpRow.find("input[name='editRemainDays']").data({"spDays":spDays, "oldRemainHours":remainHours}).val(remainVal);
    addUpRow.find("span[name='remainHours']").text((annualUnit==1||annualUnit==2? "天" : "小時"));
    addUpRow.find("input[name='editDeadLine']").val(deadLine);
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
    var newDeadLine = thisRow.find("input[name='editDeadLine']").val();
    var reason = thisRow.find("input[name='editReason']").val();
    if(newRemainHours =="" || newDeadLine=="" || reason==""){
        alert("欄位皆須填寫");
        return;
    } 
    if(newRemainHours % 0.5 !=0 ){
        alert("小數值須為0.5的倍數");
        return;
    }
    newRemainHours = (annualUnit==3? newRemainHours : newRemainHours*hourToDay);  //轉換成hour
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
