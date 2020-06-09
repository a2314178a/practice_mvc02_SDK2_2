
var myObj = new MyObj();

$(document).ready(function() {  
    init();

    $("ul[group='table']").on("click", "a", function(){
        $(".add_depart").show();
        $('.btnActive').css('pointer-events', "");
    });
});//.ready function

function showAdminFnPage(page){
    window.location.href = "/AdminFn/Index?page="+page;  
}

function init(){

    if($("#operateLogDiv").length > 0){
        getFilterOption();
    }
}

function getFilterOption(){
    var dt = myObj.dateTimeFormat();
    $("#startDate,#endDate").val(dt.ymdHtml);
    var successFn = (res)=>{
        setFilterOption(res);
    };
    myObj.rAjaxFn("get", "/AdminFn/getFilterOption", null, successFn);
}

function setFilterOption(res){
    res.category.forEach((value)=>{
        $("#selCategory").append(new Option(value, value));
    });
    res.userName.forEach((value)=>{
        $("#selOpName,#selEmName").append(new Option(value.userName, value.id));
    });
}

function getOperateLog(){
    var sDate = $("#startDate").val();
    var eDate = $("#endDate").val();
    var active = $("#selActive").val();
    var category = $("#selCategory").val();
    var opID = $("#selOpName").val();
    var emID = $("#selEmName").val();
    var filter = {
        sDate, eDate, active, category, opID, emID, 
    };
    var successFn = function(res){
        refreshOpLogList(res);
    };
    myObj.rAjaxFn("get", "/AdminFn/getOperateLog", filter, successFn);
}

function refreshOpLogList(res){
    $("#opLogList").empty();
    res.forEach(function(value){
        var row = $(".template").find("[name='opLogRow']").clone();
        row.find("[name='opTime']").text(value.opTime);
        row.find("[name='active']").text(value.active);
        row.find("[name='category']").text(value.category);
        row.find("[name='opName']").text(value.opName);
        row.find("[name='emName']").text(value.emName);
        row.find("[name='content']").text(value.content);
        $("#opLogList").append(row);
     });
}