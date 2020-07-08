
var myObj = new MyObj();

$(document).ready(function() {  
    init();

    $("#searchFilterDiv").on("click", "[name='searchFilterBtn']", function(){
        getEmployeeApplyLeave();
    });
});//.ready function


function selPageContext(type){
    window.location.href = "/ApplicationSign/Index?type="+type;
}

function init(){
    if($("#punchWarnDiv").length >0){
        getPunchLogWarn();
    }else if($("#leaveSignDiv").length >0){
        if($("#searchFilterDiv").length >0){
            var date = new Date();
            var newDate = date.setDate(date.getDate() - 30);
            var dtStart = myObj.dateTimeFormat(newDate);
            $("#filter_sDate").val(dtStart.ymdHtml);
            var dtEnd = myObj.dateTimeFormat();
            $("#filter_eDate").val(dtEnd.ymdHtml);
        }
        getEmployeeApplyLeave();
    }
}
