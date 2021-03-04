
var myObj = new MyObj();

$(document).ready(function() {  
    init();

    $("input[name='searchFilterBtn']").on("click", function(){
        if($("#punchWarnDiv").length >0){
            getPunchLogWarn();
        }else if($("#leaveSignDiv").length >0){
            getEmployeeApplyLeave();
        }else if($("#overtimeSignDiv").length >0){
            getEmployeeApplyOvertime();
        }
    });

    $("#selAllChkBox").on("click", function(){
        if($(this).prop("checked")){
            $("#punchLogWarnList").find("input[type='checkbox']").prop("checked", true);
        }else{
            $("#punchLogWarnList").find("input[type='checkbox']").prop("checked", false);
        }
    });

});//.ready function


function selPageContext(type){
    window.location.href = "/ApplicationSign/Index?type="+type;
}

function init(){
    if($("#filterDepart").length >0){
        getDepartFilterOption();
    }
    if($("#punchWarnDiv").length >0){
        getPunchLogWarn();
    }else{
        if($("#searchFilterDiv").length >0){
            var date = new Date();
            var newDate = date.setDate(date.getDate() - 30);
            var dtStart = myObj.dateTimeFormat(newDate);
            $("#filter_sDate").val(dtStart.ymdHtml);
            var dtEnd = myObj.dateTimeFormat();
            $("#filter_eDate").val(dtEnd.ymdHtml);
        }
        if($("#leaveSignDiv").length >0){
            getEmployeeApplyLeave();
        }else if($("#overtimeSignDiv").length >0){
            getEmployeeApplyOvertime();
        } 
    }
}

function getDepartFilterOption(){
    var successFn = function(res){
        var depart=[];
        res.forEach(value=>{
            depart.push(value.department);
        });
        depart = depart.filter((value, key, arr)=>arr.indexOf(value)===key);
        depart.forEach(function(value){
            $("#filterDepart").append(new Option(value, value));
        });
    };
    myObj.rAjaxFn("get", "/ApplicationSign/getFilterOption", null, successFn);
}
